using UnityEngine;
using System.Collections.Generic;
using System;
using WebSocketSharp;
using System.Text.RegularExpressions;


public class VRTracker : MonoBehaviour {

	public static VRTracker instance;

	private WebSocket myws;
	private Vector3 position;
	private Vector3 orientation;
    private int timestamp = 0;
	protected Quaternion orientation_quat;

	[System.NonSerialized]public List<VRTrackerTag> tags;
	private List<string> messagesToSend;
	private bool connected = false;

	public string UserUID = "";
	public bool autoAssignation = true;
	public event Action OnConnected;
	public event Action OnDisconnected;
	[System.NonSerialized]public bool assignationComplete = false;
	[System.NonSerialized]public string serverIp = "";
	private bool serverIpReceived = false;
	private bool isWaitingForIp = false;
    [System.NonSerialized]
    public bool isSpectator = false;

    private void Awake()
	{
		if (instance != null)
		{
			Debug.LogError("More than one VRTracker in the scene");
		}
		else
		{
			instance = this;
		}

		// Initialize Unique User ID
		UserUID = SystemInfo.deviceUniqueIdentifier.ToLower();

		// Connect to Gateway
		openWebsocket ();
        tags = new List<VRTrackerTag>();

    }

    // Use this for initialization
    void Start () {

		DontDestroyOnLoad(this.gameObject);

	}

	// Update is called once per frame
	void Update () {

	}

	// Called when connection to Gateway is successfull
	private void OnOpenHandler(object sender, System.EventArgs e) {
		if(OnConnected != null)
			OnConnected ();

		connected = true;
		Debug.Log("VR Tracker : connection established");

		myws.SendAsync ("cmd=mac&uid="+UserUID, OnSendComplete);
        myws.SendAsync("cmd=allavailabletag", OnSendComplete);

        //Ask the server IP
        askServerIP();

        foreach (VRTrackerTag tag in tags) {
			if(tag.UID != "Enter Your Tag UID")
				assignTag(tag.UID);
		}

	}

    private void OnErrorHandler(object sender, System.EventArgs e)
    {
    
    }

    // Handler for all messages from the Gateway
    private void OnMessageHandler(object sender, MessageEventArgs e) {

		Debug.Log (e.Data);
		if (e.Data.Contains("cmd=position"))
		{
			//Debug.Log (System.DateTime.Now.Millisecond + ", " + e.Data);

			string[] datasbytag = e.Data.Split(new string[] { "&uid=" }, System.StringSplitOptions.RemoveEmptyEntries);
			for (int i = 1; i < datasbytag.Length; i++)
			{
				bool positionUpdated = false;
				bool orientationUpdated = false;
				bool orientationQuaternion = false;
                bool timestampUpdated = false;
				string[] datas = datasbytag[i].Split('&');
				string uid = datas[0];
				foreach (string data in datas)
				{
					string[] datasplit = data.Split('=');
					// Position
					if (datasplit[0] == "x")
					{
						positionUpdated = true;
						position.x = float.Parse(datasplit[1]);
					}
					else if (datasplit[0] == "z")
					{
						position.y = float.Parse(datasplit[1]);
					}
					else if (datasplit[0] == "y")
					{
						position.z = float.Parse(datasplit[1]);
					}
                    else if (datasplit[0] == "ts")
                    {
                        timestamp = int.Parse(datasplit[1]);
                        timestampUpdated = true;
                    }

                    // Orientation
                    else if (datasplit[0] == "ox")
					{
						orientationUpdated = true;
						orientation.y = -float.Parse(datasplit[1]);
						orientation_quat.y = orientation.y;
					}
					else if (datasplit[0] == "oy")
					{
						orientation.x = -float.Parse(datasplit[1]);
						orientation_quat.x = -orientation.x;
					}
					else if (datasplit[0] == "oz")
					{
						orientation.z = float.Parse(datasplit[1]);
						orientation_quat.z = orientation.z;
					}
					else if (datasplit[0] == "ow")
					{
						orientationQuaternion = true;
						orientation_quat.w = -float.Parse(datasplit[1]);
					}
				}
				foreach (VRTrackerTag tag in tags)
				{
					if (tag.UID == uid)
					{
						if (orientationUpdated) {
							if(orientationQuaternion)
								tag.updateOrientationQuat (orientation_quat);
							else 
								tag.updateOrientation (orientation);
						}
						if (positionUpdated)
						{
                            if(!timestampUpdated)
    							tag.updatePosition(position);
                            else
                                tag.updatePosition(position, timestamp);

                        }
					}
				}
			}
		}
		else if (e.Data.Contains("cmd=specialcmd"))
		{
			Debug.Log (e.Data);
			string[] datas = e.Data.Split('&');
			string uid = null;
			string command = null;
			foreach (string data in datas)
			{
				string[] datasplit = data.Split('=');

				// Tag UID sending the special command
				if (datasplit[0] == "uid")
				{
					uid = datasplit[1];
				}

				// Special Command name
				else if (datasplit[0] == "data")
				{
					command = datasplit[1];
				}
			}
			if (uid != null && command != null)
				receiveSpecialCommand(uid, command);

		}
		else if (e.Data.Contains("cmd=taginfos"))
		{

			string[] datas = e.Data.Split('&');

			string uid = null;
			string status = null;
			int battery = 0;

			foreach (string data in datas)
			{
				string[] datasplit = data.Split('=');

				// Tag UID sending its informations
				if (datasplit[0] == "uid")
				{
					uid = datasplit[1];
				}
				// Tag status (“lost”, “tracking”, “unassigned”)
				else if (datasplit[0] == "status")
				{
					status = datasplit[1];
				}
				// Tag battery
				else if (datasplit[0] == "battery")
				{
					battery = int.Parse(datasplit[1]);
				}
			}
			if (uid != null && status != null)
			{
				foreach (VRTrackerTag tag in tags)
				{
					if (tag.UID == uid)
					{
						tag.status = status;
						tag.battery = battery;
					}
				}
			}

		}
		else if (e.Data.Contains("cmd=error"))
		{
			// TODO Parse differnt kinds of errors
			Debug.LogWarning("VR Tracker : " + e.Data);
			if (e.Data.Contains("needmacadress"))
			{
				myws.SendAsync("cmd=mac&uid=" + UserUID, OnSendComplete);
				foreach (VRTrackerTag tag in tags)
				{
					if (tag.UID != "Enter Your Tag UID")
						assignTag(tag.UID);
				}
			}
		}
		else if (e.Data.Contains("function=needaddress"))
		{
			receiveAskServerIP ();
		}
		//if the message gives us an IP address, try to connect as a client to it
		else if (e.Data.Contains("function=address"))
		{

			string[] datas = e.Data.Split('&');
			foreach(string data in datas)
			{
				string[] infos = data.Split('=');
				if(infos[0] == "ip")
				{
					receiveServerIP (infos [1]);
				}
			}
		}
		else if (e.Data.Contains("cmd=availabletag"))
		{
			Debug.Log("Available tag message : " + e.Data);
			string[] datas = e.Data.Split('&');


			// Verify if Tags connected to the system can be assoicated to the User from association File
			foreach (string data in datas)
			{
				string[] datasplit = data.Split('=');
				if (datasplit[0].Contains("tag"))
				{
					VRTrackerTagAssociation.instance.addAvailableTag(datasplit[1]);
				}
			}
		}
		else if (e.Data.Contains("cmd=reoriente")){
			string uid = null;
			string[] datas = e.Data.Split('&');

			foreach (string data in datas)
			{
				string[] datasplit = data.Split('=');
				// Tag UID sending the special command
				if (datasplit[0] == "uid")
				{
					uid = datasplit[1];
				}
			}
			foreach (VRTrackerTag tag in tags)
			{
				if (tag.UID == uid)
				{
					Debug.Log ("Resetting orientation after receiving message");
					tag.ResetOrientation();
				}
			}
		}
		else
		{
			Debug.Log("VR Tracker : Unknown data received : " + e.Data);
		}
	}

	// Called when connection to Gateway is closed
	private void OnCloseHandler(object sender, CloseEventArgs e) {
		connected = false;
		if(OnDisconnected != null)
			OnDisconnected ();

        Debug.Log("VR Tracker : connection closed for this reason: " + e.Reason);
	}

	private void OnSendComplete(bool success) {

	}

	/*
	 * Opens the websocket connection with the Gateway
	 */
	private void openWebsocket(){
		//Debug.Log("VR Tracker : opening websocket connection");
		myws = new WebSocket("ws://192.168.42.1:7777/user/");
		myws.OnOpen += OnOpenHandler;
		myws.OnMessage += OnMessageHandler;
		myws.OnClose += OnCloseHandler;
        myws.OnError += OnErrorHandler;
		myws.ConnectAsync();
	}

	/*
	 * Close the ebsocket connection to the Gateway
	 */
	private void closeWebsocket(){
		connected = false;
		Debug.Log("VR Tracker : closing websocket connection");

		this.myws.Close();
	}


	/* 
	 * Send your Unique ID, it can be your MAC address for 
	 * example but avoid the IP. It will be used by the Gateway
	 * to identify you over the network. It is necessary on multi-gateway
	 * configuration 
	 */
	private void sendMyUID(string uid){
		myws.SendAsync (uid, OnSendComplete);

	}

	/* 
	 * Asks the gateway to assign a specific Tag to this device.  
	 * Assigned Tags will then send their position to this device.
	 */
	public void assignTag(string TagID){
		myws.SendAsync ("cmd=tagassign&uid=" + TagID, OnSendComplete);
    }

	/* 
	 * Asks the gateway to assign a Tag to this device.  
	 * Assigned Tags will then send their position to this device.
	 */
	public void assignATag(){
		myws.SendAsync ("cmd=assignatag", OnSendComplete);
	}

	/* 
	 * Asks the gateway to UNassign a specific Tag from this device.  
	 * You will stop receiving updates from this Tag.
	 */
	public void unAssignTag(string TagID){
		myws.SendAsync("cmd=tagunassign&uid=" + TagID, OnSendComplete);
	}

	/* 
	 * Asks the gateway to UNassign all Tags from this device.  
	 * You will stop receiving updates from any Tag.
	 */
	public void unAssignAllTags(){
		myws.SendAsync("cmd=tagunassignall", OnSendComplete);
	}

	/* 
	 * Ask for informations on a specific Tag
	 */
	public void getTagInformations(string TagID){
		myws.SendAsync("cmd=taginfos&uid=" + TagID, OnSendComplete);
	}

	/*
	 * Enable or Disable orientation detection for a Tag
	 */
	public void TagOrientation(string TagID, bool enable){
		string en = "";
		if (enable) {
			en = "true";
		} else {
			en = "false";
		}

		myws.SendAsync("cmd=orientation&orientation=" + en + "&uid=" + TagID, OnSendComplete);
	}

	/*
	 * Set a specific color on the Tag
	 * R (0-255)
	 * G (0-255)
	 * B (0-255)
	 */
	public void setTagColor(string TagID, int red, int green, int blue){
		myws.SendAsync("cmd= color&r=" + red + "&g=" + green + "&b=" + blue + "&uid=" + TagID, OnSendComplete);
	}


	/* 
	 * Send special command to a Tag
	 */
	public void sendTagCommand(string TagID, string command){
		Debug.Log("VR Tracker : " + command);
		myws.SendAsync("cmd=specialcmd&uid=" + TagID + "&data=" + command, OnSendComplete);
	}

	/* 
	 * Send special command to the gateway that will be broadcast to all others users
	 */
	public void sendSpecialData(string command)
	{
		Debug.Log("VR Tracker : " + command);
		myws.SendAsync("cmd=specialdata&data="+ command, OnSendComplete);
	}


	/* 
	 * Send User device battery level to the Gateway
	 * battery (0-100)
	 */
	public void sendUserBattery(int battery){
		myws.SendAsync("cmd=usrbattery&battery=" + battery, OnSendComplete);
	}

	// For Multiplayer, we ask all other user if the know the Server IP
	public void askServerIP(){
		Debug.Log ("Is WS connected " + connected.ToString ());
        myws.SendAsync("cmd=specialdata&function=needaddress", OnSendComplete);

	}

	public void sendServerIP(string ip){
		//Debug.Log ("cmd=specialdata&function=address&ip=" + ip);
		myws.SendAsync("cmd=specialdata&function=address&ip=" + ip, OnSendComplete);
	}

	// The server IP was sent to us by another user (typically the server)
	private void receiveServerIP(string ip){
		//Debug.Log ("Server IP received: " + ip);

		if(!serverIpReceived){
			//Debug.Log ("Receiving first IP: " + ip);
			serverIp = ip;
			serverIpReceived = true;
		}
	}

	// Another user is looking for the Server and asks if we know the IP
	private void receiveAskServerIP(){
		if (serverIp != "") {
			sendServerIP (serverIp);
		}
		//VRTrackerNetwork.instance.receiveAskServerIP ();
	}

	public void SendMessageToGateway(string message)
	{
		myws.SendAsync(message, OnSendComplete);
	}

	public static string LookForFunction(string message)
	{
		string[] datas = message.Split('&');
		foreach (string data in datas)
		{
			string[] infos = data.Split('=');
			if(infos[0] == "function")
			{
				return infos[1];
			}
		}
		return null;
	}

	/*
	 * Executed on reception of a special command 
	 */
	public void receiveSpecialCommand(string TagID, string data){
		// TODO: You can do whatever you wants with the special command, have fun !

		bool tagFound = false;
		// Search for the Tag the special command is sent to
		foreach (VRTrackerTag tag in tags)
		{
			if (tag.UID == TagID)
			{
				tagFound = true;
				tag.onSpecialCommand(data);
			}
		}
		// If the Tag was not found, the command is sent to all Tags
		if (!tagFound) {
			/*foreach (VRTrackerTag tag in tags) {
				tag.onSpecialCommandToAll (TagID, data);
			}*/
			onAssociation (TagID, data);
		}
		
	}

	/*
	 * Executed on reception of  tag informations
	 */
	public void receiveTagInformations(string TagID, string status, int battery){
		// TODO: You can do whatever you wants with the Tag informations
	}

	/* 
	 * Ensure the Websocket is correctly closed on application quit
	 */
	void OnApplicationQuit() {
		closeWebsocket ();
        if (Network.connections.Length == 1)
        {
            //Disconnection to the server
            Debug.Log("Disconnecting: " + Network.connections[0].ipAddress + ":" + Network.connections[0].port);
            Network.CloseConnection(Network.connections[0], true);
        }
        else
        {
            if (Network.connections.Length == 0)
            {
                Debug.Log("No one is connected");
            }
            else
            {
                if (Network.connections.Length > 1)
                    Debug.Log("I'm a server, there is multiple connection");
            }

        }
    }

	public void AddTag(VRTrackerTag tag)
	{
		tags.Add(tag);
	}

	public void RemoveTag(VRTrackerTag tag)
	{
	//	tags.Remove(tag);
	}

	public GameObject GetTagObject(string id)
	{
		foreach(VRTrackerTag tag in tags)
		{
			if(tag.UID == id)
			{
				return tag.gameObject;
			}
		}
		return null;
	}



	public void assignDirectlyTags()
	{
		//Assign tags from file
		Debug.Log("Number of tag : " + tags.Count);

		if (VRTrackerTagAssociation.instance.isAllTagAvailable())
		{
			//If tags are on, we associate them to the user
			foreach (KeyValuePair<string, VRTrackerAssociation> playerAssociation in VRTrackerTagAssociation.instance.prefabAssociation)
			{
				//TODO fix this part when partial assignation
				if (!assignTagToUser(playerAssociation.Key, VRTrackerTagAssociation.instance.userTagUID[playerAssociation.Key]))
				{
					Debug.Log("CALIBRATION AUTOMATIC ERROR");
					assignationComplete = false;
					break;
				}
			}

			Debug.Log("CALIBRATION AUTOMATIC FINISH");
			if (VRTrackerTagAssociation.instance.userTagUID.Count > 0) {
				assignationComplete = true;
			}
		}else
		{
			Debug.LogWarning("Some tag not available");
		}
	}

	public bool assignTagToUser(string prefabName, string tagUID)
	{
		Debug.Log("Assiging to user, tag " + tagUID);
		Dictionary<string, string>.KeyCollection keyTag = VRTrackerTagAssociation.instance.userTagUID.Keys;
		foreach (string mac in keyTag)
		{
			VRTrackerAssociation tagAssociation;
			if (VRTrackerTagAssociation.instance.prefabAssociation.TryGetValue(prefabName, out tagAssociation))
			{
				tagAssociation.tagID = tagUID;
                //assignTag(tagUID);
                return true;
			}
		}
		return false;
	}

	public bool isAssigned()
	{
		return assignationComplete;
	}

	// Save the association between the Tag and each object for this user in a file on the PC/Phone
	public void saveAssociationTagUser()
	{
		VRTrackerTagAssociation.instance.saveAssociation ();
	}

	public void askForServer(){
		isWaitingForIp = true;
		askServerIP ();
	}

	public void onAssociation(string tagID, string data){
		if (data.Contains ("buttonon")){
            //Update the assignation

            foreach (KeyValuePair<string, VRTrackerAssociation> kvp in VRTrackerTagAssociation.instance.prefabAssociation) {
                Debug.Log("Checking");
                if (kvp.Value.isWaitingForAssignation) {
                    Debug.Log("Assigning " + tagID);
                    kvp.Value.assign (tagID);
                    //Ask for the tag assignation
                    //assignTag(tagID);
					VRTrackerTagAssociation.instance.isWaitingForAssociation = false;
				}
			}
		}
	}

    /*
    public void unassignAllTag()
    {
        Dictionary<string, VRTrackerAssociation>.KeyCollection keyTag = VRTrackerTagAssociation.instance.prefabAssociation.Keys;
        foreach (string gameObjectName in keyTag)
        {
            if (VRTrackerTagAssociation.instance.prefabAssociation.TryGetValue(prefabName, out tagAssociation))
            {
                tagAssociation.tagID = tagUID;
                assignTag(tagUID);
                return true;
            }
        }
    }*/
}
