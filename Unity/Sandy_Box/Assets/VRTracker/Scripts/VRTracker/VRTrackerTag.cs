using UnityEngine;
using System.Collections;
using System;
using UnityEngine.VR;
using UnityEngine.Networking;
using System.Collections.Generic;

public class VRTrackerTag : MonoBehaviour {

    // Type of Tag (Head, controller Left / Right for VRTK)
	public enum TagType 
	{
		Head, LeftController, RightController
	}
	public TagType tagType;

	// Button value saved here for VRTK
    [System.NonSerialized] public bool triggerPressed = false;
    [System.NonSerialized] public bool triggerUp = false;
    [System.NonSerialized] public bool triggerDown = false;
    [System.NonSerialized] public bool buttonPressed = false;
    [System.NonSerialized] public bool buttonUp = false;
    [System.NonSerialized] public bool buttonDown = false;
	[System.NonSerialized] public bool trackpadTouch = false;
	[System.NonSerialized] public bool trackpadUp = false;
	[System.NonSerialized] public bool trackpadDown = false;
	[System.NonSerialized] public Vector2 trackpadXY = Vector2.zero;

	private int trackpadMaxLeft = 0; // Max left (x) value sent by the trackpad
	private int trackpadMaxRight = 1000; // Max right (x) value sent by the trackpad
	private int trackpadMaxUp = 1000; // Max up (x) value sent by the trackpad
	private int trackpadMaxDown = 0; // Max down (x) value sent by the trackpad

    // For Quaternion orientation from Tag
    protected bool orientationUsesQuaternion = false;
	protected Quaternion imuOrientation_quat;

	// For Rotation vector orientation from Tag
	protected Vector3 orientation_;
	protected Vector3 orientationBegin;

	protected Vector3 acceleration_;

	protected Vector3 tagRotation;

	//public Vector3 orientationOffset; // Offset to apply
	//public Vector3 EyeTagOffset; // Difference between tag and eye position in real world
	public bool orientationEnabled = true;
	public string status;   
	public int battery;
	[System.NonSerialized]public bool waitingForID = false;                           // if the tag is Waiting for its ID
	[System.NonSerialized]public bool IDisAssigned = false;
	protected Vector3 positionReceived;

	private float currentTime;

	protected int counter = 0; // Count for 30 frames before asking for orientation after the connection

	protected float assignationDelay = 10f; // Delay during which the User can press the red button on the Tag to assign it to one of its object in the game

	public event Action OnDown;  // Called when Fire is pressed.
	public event Action OnUp;    // Called when Fire is released.

	public string UID = "Enter Your Tag UID";
	public bool displayLog = false;

	protected Boolean commandReceived = false;
	protected String command;


    // Simple velocity calculation for Pickup script
	private Vector3 previousPosition;
    [System.NonSerialized] public Vector3 velocity;

	// SMOOTHING
	private long lastLateUpateTimestamp = 0;
	private int counterFrameWithSamePosition = 0;
	private Vector3 lastFramePosition;
	private long startTimestamp;
    private long lastMessagePositionTimestamp;
	private Vector3 predictedPosition; // Based on last received position
	private Queue<KeyValuePair<long, Vector3>> positions;
	private Vector3[] positionSpeeds;
	private Vector3 positionAcceleration;

	private long maxPredictionDuration = 400; // 500ms
    public int DeadReckogningDelayMs = 80; // 40ms seems to be a good value, don't go up though
    public float smoothingIntensity = 0.85f;
    public bool enablePositionSmoothing = true;

	private Queue<KeyValuePair<long, Vector3>> orientations;
	private Vector3[] orientationSpeeds;
	private Vector3 orientationAcceleration;
	private long orientationReceptionTime = 0;
	private bool orientationMessageSaved = true; // to check if the message received was added to the Queue
	public bool enableOrientationSmoothing = false;
	private Vector3 predictedOrientation;
	private int counterFrameWithSameOrientation = 0;
	private Vector3 lastFrameOrientationReceived;

	private NetworkIdentity netId;

	// Use this for initialization
	protected virtual void Start () {
		//Debug.Log ("TAG " + UID + "  " + tagType.ToString ());
		//onTagData("cmd=specialdata&s=30&x=376.43&y=481&z=36&st=1&s=10&ox=190.19&oy=-49.17&oz=-22.71&ax=21.27&ay=0.78&az=-15.79");

		netId = transform.GetComponentInParent<NetworkIdentity> ();
		if (netId != null && !netId.isLocalPlayer) {
			return;
		}
		
		startTimestamp = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
		lastLateUpateTimestamp = startTimestamp;

		//enablePrediction = true;

		positionSpeeds = new Vector3[2];
		positions = new Queue<KeyValuePair<long, Vector3>>();

		orientationSpeeds = new Vector3[2];
		orientations = new Queue<KeyValuePair<long, Vector3>>();

        VRTracker.instance.AddTag (this);
		//Try to assign automatically the tag
		tryAssignToPrefab ();

		if(UID != "Enter Your Tag UID")
		{
			IDisAssigned = true;
		}
        
	}

	protected virtual void LateUpdate(){
		if (netId != null && !netId.isLocalPlayer) {
			return;
		}

		long lateUpdateTimestamp = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - startTimestamp;
		
		// PREDICTIONS
		long deltaTimeSinceLastFrame = lateUpdateTimestamp - lastLateUpateTimestamp; // Delay since last update frame
		lastLateUpateTimestamp = lateUpdateTimestamp;

		// Check if the position changed since last frame, and if the prediction needs to stop because a new position was received
		if (lastFramePosition.x == positionReceived.x && lastFramePosition.y == positionReceived.y && lastFramePosition.z == positionReceived.z) {
			counterFrameWithSamePosition++;
		} else {
			counterFrameWithSamePosition = 0;
		}

		if (enablePositionSmoothing && positions.Count == 7) {
			KeyValuePair<long, Vector3>[] positionsArray = positions.ToArray ();
			long deltaTimeLateUpdateSinceLastPosition = (lateUpdateTimestamp - positionsArray [positionsArray.Length - 1].Key); // Delay since last position received
		
			// If the tracking is lost we want the prediction to smoothly stop
			Vector3 accelerationDropedOverTime = Vector3.Slerp (positionAcceleration, Vector3.zero, deltaTimeLateUpdateSinceLastPosition / maxPredictionDuration);
			Vector3 speedDropedOverTime = Vector3.Slerp (positionSpeeds [0], Vector3.zero, deltaTimeLateUpdateSinceLastPosition / maxPredictionDuration);

			if (counterFrameWithSamePosition == 0) {
				// Just for calculation order
				float accOperator = (float)(0.5 * (deltaTimeLateUpdateSinceLastPosition + DeadReckogningDelayMs) * (deltaTimeLateUpdateSinceLastPosition + DeadReckogningDelayMs) / 1000000);
				float accOperatorLastUpdate = (float)(0.5 * deltaTimeSinceLastFrame * deltaTimeSinceLastFrame / 1000000);

				// Here is where the magic happens, we calculate the futur position based on Last Late Update position, and futur position based on last message reception
				Vector3 predictionFromLastUpdate = predictedPosition + accOperatorLastUpdate * accelerationDropedOverTime + speedDropedOverTime * deltaTimeSinceLastFrame / 1000;
				Vector3 predictedPositionFromLastReception = positionsArray [positionsArray.Length - 1].Value + accOperator * accelerationDropedOverTime + speedDropedOverTime * (deltaTimeLateUpdateSinceLastPosition + DeadReckogningDelayMs) / 1000; 
				// And we give much more importance to the futur position based on last update, this avoids the shakes in the position
				predictedPosition = Vector3.Lerp (predictedPositionFromLastReception, predictionFromLastUpdate, Mathf.Clamp (smoothingIntensity, 0.0f, 1.0f));

			} else {
				float accOperatorLastUpdate = (float)(0.5 * deltaTimeSinceLastFrame * deltaTimeSinceLastFrame / 1000000);
				predictedPosition = predictedPosition + accOperatorLastUpdate * accelerationDropedOverTime + speedDropedOverTime * deltaTimeSinceLastFrame / 1000;
			}  

		} else {
//			Debug.LogWarning("Position Length != 0 : " + positions.Count.ToString());
		}

        lastFramePosition = positionReceived;
      //  Debug.Log("positionReceived: " + positionReceived.x + "  " + positionReceived.y + "  " + positionReceived.z);
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
					Debug.LogWarning("Tag " + UID + " asks for orientation");
					VRTracker.instance.TagOrientation (UID, true);
				}
				counter++;
			} else if (counter < 30) {
				counter++;
			}
		}

		Vector3 calcOffset = new Vector3(0f,0f,0f); // Position offset due to distance between eyes and tag position

		// Setting Orientation for Tag
		//tagRotation = orientation_ + orientationOffset - orientationBegin;
		tagRotation = orientation_ - orientationBegin;
		tagRotation.y -= VRTracker.instance.RoomNorthOffset;
		//Debug.Log (VRTracker.instance.RoomNorthOffset);

		// SMOOTH ORIENTATION

		// Check if the position changed since last frame, and if the prediction needs to stop because a new position was received
		if (lastFrameOrientationReceived.x == tagRotation.x && lastFrameOrientationReceived.y == tagRotation.y && lastFrameOrientationReceived.z == tagRotation.z) {
			counterFrameWithSameOrientation++;
		} else {
			counterFrameWithSameOrientation = 0;
		}


		if(!orientationMessageSaved && enableOrientationSmoothing){
			// Make sure the queue is always the same size. Another container would be better...
			while (orientations.Count > 7)
				orientations.Dequeue();

			// Get reception time and add the posiution to the queue
			orientations.Enqueue(new KeyValuePair<long, Vector3>(orientationReceptionTime, tagRotation));

			// Convert queue to Array for easy iteration
			KeyValuePair<long, Vector3>[] orientationsArray = orientations.ToArray();


			// Time to calculate speed and acceleration based on last positions (to be modified once we use the IMU accelerometer values)
			if (orientationsArray.Length >= 7)
			{
				Vector3 orientationsOffset1 = orientationsArray[orientationsArray.Length - 4].Value - orientationsArray[orientationsArray.Length - 7].Value;
				Vector3 orientationsOffset0 = orientationsArray[orientationsArray.Length - 1].Value - orientationsArray[orientationsArray.Length - 4].Value;
				orientationsOffset1.x = orientationsOffset1.x > 250 ?  orientationsOffset1.x - 360 : orientationsOffset1.x;
				orientationsOffset1.x = orientationsOffset1.x < -250 ? orientationsOffset1.x + 360 : orientationsOffset1.x;
				orientationsOffset1.y = orientationsOffset1.y > 250 ? orientationsOffset1.y - 360 : orientationsOffset1.y;
				orientationsOffset1.y = orientationsOffset1.y < -250 ? orientationsOffset1.y + 360 : orientationsOffset1.y;
				orientationsOffset1.z = orientationsOffset1.z > 250 ? orientationsOffset1.z - 360 : orientationsOffset1.z;
				orientationsOffset1.z = orientationsOffset1.z < -250 ? orientationsOffset1.z + 360 : orientationsOffset1.z;
				orientationsOffset0.x = orientationsOffset0.x > 250 ? orientationsOffset0.x - 360 : orientationsOffset0.x;
				orientationsOffset0.x = orientationsOffset0.x < -250 ? orientationsOffset0.x + 360 : orientationsOffset0.x;
				orientationsOffset0.y = orientationsOffset0.y > 250 ? orientationsOffset0.y - 360 : orientationsOffset0.y;
				orientationsOffset0.y = orientationsOffset0.y < -250 ? orientationsOffset0.y + 360 : orientationsOffset0.y;
				orientationsOffset0.z = orientationsOffset0.z > 250 ? orientationsOffset0.z - 360 : orientationsOffset0.z;
				orientationsOffset0.z = orientationsOffset0.z < -250 ? orientationsOffset0.z + 360 : orientationsOffset0.z;

				//Debug.Log ("orientationsOffset0 " + orientationsOffset0.ToString ());
				//Debug.Log ("orientationsOffset1 " + orientationsOffset1.ToString ());

				orientationSpeeds[1] = 1000 * ((orientationsOffset1) / (orientationsArray[orientationsArray.Length - 4].Key - orientationsArray[orientationsArray.Length - 7].Key));
				orientationSpeeds[0] = 1000 * ((orientationsOffset0) / (orientationsArray[orientationsArray.Length - 1].Key - orientationsArray[orientationsArray.Length - 4].Key));
				orientationAcceleration = 2 * 1000 * ((orientationSpeeds[0] - orientationSpeeds[1]) / (orientationsArray[orientationsArray.Length - 1].Key - orientationsArray[orientationsArray.Length - 7].Key));
			}

			orientationMessageSaved = true;
		}

		if (enableOrientationSmoothing && orientations.Count >= 7) {
			KeyValuePair<long, Vector3>[] orientationsArray = orientations.ToArray ();
			long deltaTimeLateUpdateSinceLastOrientation = (lateUpdateTimestamp - orientationsArray [orientationsArray.Length - 1].Key); // Delay since last orientation received

			// If the tracking is lost we want the prediction to smoothly stop
			Vector3 accelerationDropedOverTime = Vector3.Slerp (orientationAcceleration, Vector3.zero, deltaTimeLateUpdateSinceLastOrientation / maxPredictionDuration);
			Vector3 speedDropedOverTime = Vector3.Slerp (orientationSpeeds [0], Vector3.zero, deltaTimeLateUpdateSinceLastOrientation / maxPredictionDuration);

			if (counterFrameWithSameOrientation == 0) {
				// Just for calculation order
				float accOperator = (float)(0.5 * (deltaTimeLateUpdateSinceLastOrientation + DeadReckogningDelayMs) * (deltaTimeLateUpdateSinceLastOrientation + DeadReckogningDelayMs) / 1000000);
				float accOperatorLastUpdate = (float)(0.5 * deltaTimeSinceLastFrame * deltaTimeSinceLastFrame / 1000000);

				// Here is where the magic happens, we calculate the futur orientation based on Last Late Update orientation, and futur orientation based on last message reception
				Vector3 predictionFromLastUpdate = predictedOrientation + accOperatorLastUpdate * accelerationDropedOverTime + speedDropedOverTime * deltaTimeSinceLastFrame / 1000;
				Vector3 predictedOrientationFromLastReception = orientationsArray [orientationsArray.Length - 1].Value + accOperator * accelerationDropedOverTime + speedDropedOverTime * (deltaTimeLateUpdateSinceLastOrientation+ DeadReckogningDelayMs) / 1000; 
				// And we give much more importance to the futur orientation based on last update, this avoids the shakes in the orientation


				predictionFromLastUpdate.x = ((predictionFromLastUpdate.x+180)%360)-180;
				predictionFromLastUpdate.y = ((predictionFromLastUpdate.y+180)%360)-180;
				predictionFromLastUpdate.z = ((predictionFromLastUpdate.z+180)%360)-180;
				predictionFromLastUpdate.x = predictionFromLastUpdate.x - predictedOrientationFromLastReception.x > 250 ? predictionFromLastUpdate.x - 360 : predictionFromLastUpdate.x;
				predictionFromLastUpdate.x = predictionFromLastUpdate.x - predictedOrientationFromLastReception.x < -250 ? predictionFromLastUpdate.x + 360 : predictionFromLastUpdate.x;
				predictionFromLastUpdate.y = predictionFromLastUpdate.y - predictedOrientationFromLastReception.y > 250 ? predictionFromLastUpdate.y - 360 : predictionFromLastUpdate.y;
				predictionFromLastUpdate.y = predictionFromLastUpdate.y - predictedOrientationFromLastReception.y < -250 ? predictionFromLastUpdate.y + 360 : predictionFromLastUpdate.y;

				predictionFromLastUpdate.z = predictionFromLastUpdate.z - predictedOrientationFromLastReception.z > 250 ? predictionFromLastUpdate.z - 360 : predictionFromLastUpdate.z;
				predictionFromLastUpdate.z = predictionFromLastUpdate.z - predictedOrientationFromLastReception.z < -250 ? predictionFromLastUpdate.z + 360 : predictionFromLastUpdate.z;

				predictedOrientation = Vector3.Lerp (predictedOrientationFromLastReception, predictionFromLastUpdate, Mathf.Clamp(smoothingIntensity, 0.0f, 0.95f));


				// Debug.Log ("predictedOrientation : " + predictedOrientation.ToString() + "predictionFromLastUpdate  : " + predictionFromLastUpdate.ToString () + "   predictedOrientationFromLastReception   : " + predictedOrientationFromLastReception.ToString ());
			} 
			else {
				float accOperatorLastUpdate = (float)(0.5 * deltaTimeSinceLastFrame * deltaTimeSinceLastFrame / 1000000);
				predictedOrientation = predictedOrientation + accOperatorLastUpdate * accelerationDropedOverTime + speedDropedOverTime * deltaTimeSinceLastFrame / 1000;
			}
		}

		lastFrameOrientationReceived = tagRotation;


		// Calculated the offset between the Tag and the user's eyes
		/*if(enableOrientationSmoothing)
			calcOffset = Quaternion.Euler(predictedOrientation)* EyeTagOffset;
		else
			calcOffset = Quaternion.Euler(tagRotation)* EyeTagOffset;
		*/
		// Assign tag orientation if enabled only. By default it's disabled for Camera, to use the VR Headset orientation instead
		if (orientationEnabled) {
				//Apply uniformely the rotation
			if (enableOrientationSmoothing)
				this.transform.rotation = Quaternion.Euler (predictedOrientation);
			else {
				this.transform.rotation = Quaternion.Euler (tagRotation);
			}
		}

		if(enablePositionSmoothing && positions.Count == 7)
			this.transform.position = this.predictedPosition+calcOffset;
		else
			this.transform.position = this.positionReceived+calcOffset;
		
		if (commandReceived) {
			commandReceived = false;
            if (command.Contains("triggeron"))
            {
                OnTriggerDown();
                triggerPressed = true;
                triggerDown = true; 
				triggerUp = false;

            }
            else if (command.Contains("triggeroff"))
            {
                OnTriggerUp();
                triggerPressed = false;
                triggerUp = true;
            }
            else if (command.Contains("buttonon"))
            {
                buttonPressed = true;
                buttonDown = true;
				buttonUp = false;
                if (displayLog)
                {
                    Debug.Log("Update orentiation begin to : " + orientationBegin.y);
                }
				if(transform.GetComponentInChildren<Camera>())
	                ResetOrientation();
            }
            else if (command.Contains("buttonoff"))
            {
                buttonPressed = false;
				buttonUp = true;
            }
         }

		velocity = (transform.position - previousPosition) / Time.deltaTime;
		previousPosition = transform.position;
	}

    public void updatePosition(Vector3 position_, int timestamp)
    {
        updatePosition(position_);
    }

    public void updatePosition(Vector3 position_){
        // PREDICTION
        this.positionReceived = position_;
        
        // Make sure the queue is always the same size. Another container would be better...
        while (positions.Count >= 7)
            positions.Dequeue();

        // Get reception time and add the posiution to the queue
        long receveidTime = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - startTimestamp;
        positions.Enqueue(new KeyValuePair<long, Vector3>(receveidTime, position_));

        // Convert queue to Array for easy iteration
        KeyValuePair<long, Vector3>[] positionsArray = positions.ToArray();

       
        // Time to calculate speed and acceleration based on last positions (to be modified once we use the IMU accelerometer values)
		if (positionsArray.Length == 7)
        {
			positionSpeeds[1] = 1000 * ((positionsArray[positionsArray.Length - 4].Value - positionsArray[positionsArray.Length - 7].Value) / (positionsArray[positionsArray.Length - 4].Key - positionsArray[positionsArray.Length - 7].Key));
			positionSpeeds[0] = 1000 * ((positionsArray[positionsArray.Length - 1].Value - positionsArray[positionsArray.Length - 4].Value) / (positionsArray[positionsArray.Length - 1].Key - positionsArray[positionsArray.Length - 4].Key));
			positionAcceleration = 2 * 1000 * ((positionSpeeds[0] - positionSpeeds[1]) / (positionsArray[positionsArray.Length - 1].Key - positionsArray[positionsArray.Length - 7].Key));
        }
       
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
		Vector3 flippedRotation = neworientation;
		orientation_ = flippedRotation;

		orientationUsesQuaternion = false;
		orientationReceptionTime = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - startTimestamp;
		orientationMessageSaved = false;
	}

	public void updateOrientationAndAcceleration(Vector3 neworientation, Vector3 newacceleration)
	{
		Vector3 flippedRotation = new Vector3(-neworientation.z, neworientation.x, neworientation.y);
		Quaternion quattest = Quaternion.Euler (flippedRotation);
		quattest = quattest*Quaternion.Euler(180f, 0, 0);
		quattest = quattest*Quaternion.Euler(0, -90f, 0);
		orientation_ = quattest.eulerAngles;
		acceleration_ = newacceleration;
		orientationUsesQuaternion = false;
		orientationReceptionTime = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - startTimestamp;
		orientationMessageSaved = false;
	}

	// Update the Oriention from IMU For Tag V2
	public void updateOrientationQuat(Quaternion neworientation)
	{
		orientationUsesQuaternion = true;
		this.orientation_ = neworientation.eulerAngles;
		this.imuOrientation_quat = neworientation;
		orientationReceptionTime = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - startTimestamp;
		orientationMessageSaved = false;
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

	public void onTagData(string data){
		//Debug.Log ("TAG: " + data);
		string[] sensors = data.Split(new string[] {"&s="}, System.StringSplitOptions.RemoveEmptyEntries);
		for (int i = 1; i < sensors.Length; i++) {
			string[] parameters = sensors[i].Split ('&');
			char[] sensorInfo = parameters[0].ToCharArray();
			if (sensorInfo.Length != 2)
				return;
			Dictionary<string, string> values = new Dictionary<string, string>();
			for (int j = 1; j < parameters.Length; j++) {
				string[] dict = parameters [j].Split ('=');
				values.Add (dict[0], dict[1]);
			}

			// IMU
			if (sensorInfo [0] == '1') {
				Vector3 rec_orientation;
				Vector3 rec_acceleration;

				float f;
				float.TryParse(values ["ox"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out f);
				rec_orientation.x = f;
				float.TryParse(values ["oy"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out f);
				rec_orientation.y = f;
				float.TryParse(values ["oz"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out f);
				rec_orientation.z = f;

				float.TryParse(values ["ax"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out f);
				rec_acceleration.x = f;
				float.TryParse(values ["ay"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out f);
				rec_acceleration.y = f;
				float.TryParse(values ["az"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out f);
				rec_acceleration.z = f;

				updateOrientationAndAcceleration (rec_orientation, rec_acceleration);
			}

			// Trackpad
			else if (sensorInfo [0] == '3') {
				string press = values ["st"];
				if (press == "2") {
					trackpadTouch = false;
					trackpadUp = true;
				} else if (press == "1" || press == "3") {
					trackpadTouch = true;
					trackpadDown = true;
				}
				float a,b;
				float.TryParse(values ["x"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out a);
				trackpadXY.y = -(a - (Mathf.Abs(trackpadMaxLeft - trackpadMaxRight)/2))/Mathf.Abs(trackpadMaxLeft - trackpadMaxRight);
				float.TryParse(values ["y"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out b);
				trackpadXY.x = -(b - (Mathf.Abs(trackpadMaxUp - trackpadMaxDown)/2))/Mathf.Abs(trackpadMaxUp - trackpadMaxDown);
				if (a == 0.0f && b == 0.0f)
					trackpadXY = Vector2.zero;
				//Debug.Log ("Trackpad " + trackpadXY.x + "  " + trackpadXY.y);
			}
		}
	}

	public Vector3 GetPosition()
	{
		return this.positionReceived;
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
        if(VRTracker.instance)
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
		//Add tag to the singleton VR Tracker
		GameObject parent = transform.parent.gameObject;
		if (parent != null) {
			NetworkIdentity netId = parent.GetComponent<NetworkIdentity> ();
			if (netId != null) {
				//If it's local identity, we assign the id
				if (netId.isLocalPlayer) {
					if(VRTrackerTagAssociation.instance != null){
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


}
//			Debug.LogWarning("Position Length != 0 : " + positions.Count.ToString());