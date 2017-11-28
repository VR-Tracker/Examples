using UnityEngine;
using System.Collections;
using System;
using UnityEngine.VR;
using UnityEngine.Networking;
using System.Collections.Generic;

public class VRTrackerTag : MonoBehaviour {

	// For Quaternion orientation from Tag
	protected bool orientationUsesQuaternion = false;
	protected Quaternion imuOrientation_quat;
	public float magneticNorthOffset = 0.0f;

	// For Rotation vector orientation from Tag
	protected Vector3 orientation_;
	protected Vector3 orientationBegin;

	protected Quaternion tagRotation;

	public Vector3 orientationOffset; // Offset to apply
	public Vector3 EyeTagOffset; // Difference between tag and eye position in real world
	public bool orientationEnabled = true;
    //public List<GameObject> rotatingObject; // Store the game object that will move
    private bool isHead = false;
    private bool uniformRotation = true;
	public string status;   
	public int battery;
	[System.NonSerialized]public bool waitingForID = false;                           // if the tag is Waiting for its ID
	[System.NonSerialized]public bool IDisAssigned = false;
	protected Vector3 position;

	private float currentTime;

	protected int counter = 0; // Count for 30 frames before asking for orientation after the connection

	protected float assignationDelay = 10f; // Delay during which the User can press the red button on the Tag to assign it to one of its object in the game

	public event Action OnDown;  // Called when Fire is pressed.
	public event Action OnUp;    // Called when Fire is released.

	public string UID = "Enter Your Tag UID";
    public bool displayLog = false;

	protected Boolean commandReceived = false;
	protected String command;

	private Vector3 previousPosition;
	public Vector3 velocity;

	// Use this for initialization
	protected virtual void Start () {
		if (transform.parent.GetComponent<NetworkIdentity>() != null && !transform.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
			return;
		}
        VRTracker.instance.AddTag (this);
		//Try to assign automatically the tag
		tryAssignToPrefab ();


        //Check if there is a camera
        if(gameObject.GetComponentsInChildren<Camera>().Length > 0)
        {
            isHead = true;
        }

		if(UID != "Enter Your Tag UID")
        {
            IDisAssigned = true;
        }
    }

	protected virtual void Update(){
		if (transform.parent.GetComponent<NetworkIdentity>() != null && !transform.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
			return;
		}

		if (waitingForID)
		{
			currentTime -= Time.deltaTime;
			if(currentTime <= 0)
			{
				//Assignation time off
				currentTime = 0;
				waitingForID = false;
				IDisAssigned = false;
			}
		}

		// Wait for ID assignement before enabling Tag orientation
		if (IDisAssigned) {
			if (counter == 30) {
				if (UID != "Enter Your Tag UID") {
                    if (displayLog)
                    {
                        Debug.LogWarning("Tag " + UID + " asks for orientation");
                    }
                    VRTracker.instance.TagOrientation (UID, true);
				}
				counter++;
			} else if (counter < 30) {
				counter++;
			}
		}
			
		Vector3 calcOffset = new Vector3(0f,0f,0f); // Position offset due to distance between eyes and tag position
		// Setting Orientation for Tag V2
		if (orientationUsesQuaternion) {
			//tagRotation = Quaternion.Euler (orientationOffset - orientationBegin);
			//this.transform.Rotate (0, -magneticNorthOffset, 0); //TODO what if orientation is disabled ?
			//tagRotation *= imuOrientation_quat;
			tagRotation = imuOrientation_quat;
		}

		// Setting Orientation for Tag V1
		else {
			tagRotation = Quaternion.Euler (orientation_ + orientationOffset - orientationBegin);
		}

		// Calculated the offset between the Tag and the user's eyes
		calcOffset = tagRotation* EyeTagOffset;

		// Assign tag orientation if enabled only. By default it's disabled for Camera, to use the VR Headset orientation instead
		if (orientationEnabled) {
            if (uniformRotation)
            {
                //Apply uniformely the rotation
                this.transform.rotation = tagRotation;
            }else
            {
				//Can apply to specific part of the body
            }
        }

		this.transform.position = this.position+calcOffset; // Apply position offset due to orientation and difference between Tag position and point to track

		if (commandReceived) {
			commandReceived = false;
			if (command.Contains("triggeron"))
			{
				OnTriggerDown ();
			}
			else if (command.Contains("triggeroff"))
			{
				OnTriggerUp(); 
			}
			else if (command.Contains("buttonon"))
			{
                if (displayLog)
                {
                    Debug.Log("Update orentiation begin to : " + orientationBegin.y);
                }
                ResetOrientation();
			}
		}

		velocity = (transform.position - previousPosition) / Time.deltaTime;
		previousPosition = transform.position;
	}

	public void updatePosition(Vector3 position){
		this.position = position;
		//Debug.Log (position.x + " " + position.y + " " + position.z);
	}

	// Reset Headset orientation and Tag orientation offset
	public void ResetOrientation()
	{
		if(orientationUsesQuaternion == true)
			this.orientationBegin.y = this.imuOrientation_quat.eulerAngles.y;
		else
			this.orientationBegin.y = this.orientation_.y;

		if (UnityEngine.XR.XRSettings.isDeviceActive)
		{
			UnityEngine.XR.InputTracking.Recenter();
		}
	}

	// Update the Oriention from IMU For Tag V1
	public void updateOrientation(Vector3 neworientation)
	{
		orientationUsesQuaternion = false;
		this.orientation_ = neworientation;
	}

	// Update the Oriention from IMU For Tag V2
	public void updateOrientationQuat(Quaternion neworientation)
	{
		Debug.Log("Update orentiation Quat : ");
		orientationUsesQuaternion = true;
		this.imuOrientation_quat = neworientation;
	}

	public void onSpecialCommand(string data){
		commandReceived = true;
		command = data;
	}


	public void onSpecialCommandToAll(string tagID, string data){
		if (waitingForID && data.Contains ("buttonon")){
			UID = tagID;
			IDisAssigned = true;
			waitingForID = false;
		}
	}

	public Vector3 GetPosition()
	{
		return this.position;
	}

	public IEnumerator WaitForAssignation()
	{
		//Prepare for assignation
		currentTime = assignationDelay;
		waitingForID = true;
		while (!IDisAssigned && waitingForID)
		{
			yield return null;
		}
	}

	public void OnTriggerDown()
	{
		if (OnDown != null)
			OnDown();
	}

	public void OnTriggerUp()
	{
		if (OnUp != null)
			OnUp();
	}

	private void OnDestroy()
	{
		VRTracker.instance.RemoveTag(this);
	}

	public void assignTag(string tagID)
	{
		UID = tagID;
		IDisAssigned = true;
		waitingForID = false;
        VRTracker.instance.assignTag(tagID);
    }

    public void tryAssignToPrefab(){
		Debug.Log ("Add tag to vr tracker instance");
		//Add tag to the singleton VR Tracker
		GameObject parent = transform.parent.gameObject;
		if (parent != null) {
			NetworkIdentity netId = parent.GetComponent<NetworkIdentity> ();
			if (netId != null) {
				//If it's local identity, we assign the id
				if (netId.isLocalPlayer) {
					string tagID = VRTrackerTagAssociation.instance.getAssociatedTagID (gameObject.name);
					if (tagID != "") {
						assignTag (tagID);
                    }
                    else {
						Debug.LogError ("ID not valid : " + tagID);
					}
				}
			}
		}
	}


}
