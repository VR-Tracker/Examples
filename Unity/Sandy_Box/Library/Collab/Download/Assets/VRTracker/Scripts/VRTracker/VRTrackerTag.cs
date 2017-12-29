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

	protected Vector3 tagRotation;

	public Vector3 orientationOffset; // Offset to apply
	public Vector3 EyeTagOffset; // Difference between tag and eye position in real world
	public bool orientationEnabled = true;
	//public List<GameObject> rotatingObject; // Store the game object that will move
	private bool isHead = false;
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
	public Vector3 velocity;

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

	private float delta = 0;
	private long maxPredictionDuration = 400; // 500ms
    public int DeadReckogningDelayMs = 40; // 40ms seems to be a good value, don't go up though
	public bool enablePositionSmoothing = true;

	private Queue<KeyValuePair<long, Vector3>> orientations;
	private Vector3[] orientationSpeeds;
	private Vector3 orientationAcceleration;
	private long orientationReceptionTime = 0;
	private bool orientationMessageSaved = true; // to check if the message received was added to the Queue
	public bool enableOrientationSmoothing = true;
	private Vector3 predictedOrientation;
	private int counterFrameWithSameOrientation = 0;
	private Vector3 lastFrameOrientationReceived;

	// Use this for initialization
	protected virtual void Start () {
		if (transform.parent.GetComponent<NetworkIdentity>() != null && !transform.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
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

	protected virtual void LateUpdate(){
		if (transform.parent.GetComponent<NetworkIdentity>() != null && !transform.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
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

		if (enablePositionSmoothing && positions.Count >= 7) {
			KeyValuePair<long, Vector3>[] positionsArray = positions.ToArray();
			long deltaTimeLateUpdateSinceLastPosition = (lateUpdateTimestamp - positionsArray[positionsArray.Length - 1].Key); // Delay since last position received
		
            // If the tracking is lost we want the prediction to smoothly stop
			Vector3 accelerationDropedOverTime = Vector3.Slerp (positionAcceleration, Vector3.zero, deltaTimeLateUpdateSinceLastPosition/maxPredictionDuration);
			Vector3 speedDropedOverTime = Vector3.Slerp (positionSpeeds [0], Vector3.zero, deltaTimeLateUpdateSinceLastPosition/maxPredictionDuration);

			if (counterFrameWithSamePosition == 0) {
				// Just for calculation order
				float accOperator = (float)(0.5 * (deltaTimeLateUpdateSinceLastPosition+ DeadReckogningDelayMs) * (deltaTimeLateUpdateSinceLastPosition+ DeadReckogningDelayMs) / 1000000);
				float accOperatorLastUpdate = (float)(0.5 * deltaTimeSinceLastFrame * deltaTimeSinceLastFrame / 1000000);

				// Here is where the magic happens, we calculate the futur position based on Last Late Update position, and futur position based on last message reception
				Vector3 predictionFromLastUpdate = predictedPosition + accOperatorLastUpdate * accelerationDropedOverTime + speedDropedOverTime * deltaTimeSinceLastFrame / 1000;
				Vector3 predictedPositionFromLastReception = positionsArray [positionsArray.Length - 1].Value + accOperator * accelerationDropedOverTime + speedDropedOverTime * (deltaTimeLateUpdateSinceLastPosition+ DeadReckogningDelayMs) / 1000; 
				// And we give much more importance to the futur position based on last update, this avoids the shakes in the position
				predictedPosition = Vector3.Lerp(predictedPositionFromLastReception, predictionFromLastUpdate, 0.9f);

			} else {
				float accOperatorLastUpdate = (float)(0.5 * deltaTimeSinceLastFrame * deltaTimeSinceLastFrame / 1000000);
				predictedPosition = predictedPosition + accOperatorLastUpdate * accelerationDropedOverTime + speedDropedOverTime * deltaTimeSinceLastFrame / 1000;
			}    
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
		// Setting Orientation for Tag V2
		if (orientationUsesQuaternion) {
			//tagRotation = Quaternion.Euler (orientationOffset - orientationBegin);
			//this.transform.Rotate (0, -magneticNorthOffset, 0); //TODO what if orientation is disabled ?
			//tagRotation *= imuOrientation_quat;
			tagRotation = orientation_;
		}

		// Setting Orientation for Tag V1
		else {
			tagRotation = orientation_ + orientationOffset - orientationBegin;
		}


		// SMOOTH ORIENTATION

		// Check if the position changed since last frame, and if the prediction needs to stop because a new position was received
		if (lastFrameOrientationReceived.x == tagRotation.x && lastFrameOrientationReceived.y == tagRotation.y && lastFrameOrientationReceived.z == tagRotation.z) {
			counterFrameWithSameOrientation++;
		} else {
			counterFrameWithSameOrientation = 0;
		}

		if(!orientationMessageSaved){
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
				orientationSpeeds[1] = 1000 * ((orientationsArray[orientationsArray.Length - 4].Value - orientationsArray[orientationsArray.Length - 7].Value) / (orientationsArray[orientationsArray.Length - 4].Key - orientationsArray[orientationsArray.Length - 7].Key));
				orientationSpeeds[0] = 1000 * ((orientationsArray[orientationsArray.Length - 1].Value - orientationsArray[orientationsArray.Length - 4].Value) / (orientationsArray[orientationsArray.Length - 1].Key - orientationsArray[orientationsArray.Length - 4].Key));
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
				Vector3 predictedOrientationFromLastReception = orientationsArray [orientationsArray.Length - 1].Value + accOperator * accelerationDropedOverTime + speedDropedOverTime * (deltaTimeLateUpdateSinceLastOrientation + DeadReckogningDelayMs) / 1000; 
				// And we give much more importance to the futur orientation based on last update, this avoids the shakes in the orientation
				predictedOrientation = Vector3.Lerp (predictedOrientationFromLastReception, predictionFromLastUpdate, 0.9f);

			} else {
				float accOperatorLastUpdate = (float)(0.5 * deltaTimeSinceLastFrame * deltaTimeSinceLastFrame / 1000000);
				predictedOrientation = predictedOrientation + accOperatorLastUpdate * accelerationDropedOverTime + speedDropedOverTime * deltaTimeSinceLastFrame / 1000;
			}
		}

		lastFrameOrientationReceived = tagRotation;


		// Calculated the offset between the Tag and the user's eyes
		if(enableOrientationSmoothing)
			calcOffset = Quaternion.Euler(predictedOrientation)* EyeTagOffset;
		else
			calcOffset = Quaternion.Euler(tagRotation)* EyeTagOffset;

		// Assign tag orientation if enabled only. By default it's disabled for Camera, to use the VR Headset orientation instead
		if (orientationEnabled) {
				//Apply uniformely the rotation
			if(enableOrientationSmoothing)
				this.transform.rotation = Quaternion.Euler(predictedOrientation);
			else
				this.transform.rotation = Quaternion.Euler(tagRotation);
		}

		if(enablePositionSmoothing)
			this.transform.position = this.predictedPosition+calcOffset;
		else
			this.transform.position = this.positionReceived+calcOffset;
		
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

    public void updatePosition(Vector3 position_, int timestamp)
    {
        updatePosition(position_);
    }

    public void updatePosition(Vector3 position_){
        // PREDICTION
        this.positionReceived = position_;
        
        // Make sure the queue is always the same size. Another container would be better...
        while (positions.Count > 7)
            positions.Dequeue();

        // Get reception time and add the posiution to the queue
        long receveidTime = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - startTimestamp;
        positions.Enqueue(new KeyValuePair<long, Vector3>(receveidTime, position_));

        // Convert queue to Array for easy iteration
        KeyValuePair<long, Vector3>[] positionsArray = positions.ToArray();

       
        // Time to calculate speed and acceleration based on last positions (to be modified once we use the IMU accelerometer values)
        if (positionsArray.Length >= 7)
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
        Vector3 mrotation = Vector3.zero;
        mrotation.z += neworientation.z;
        mrotation.x -= neworientation.x;
        mrotation.y -= neworientation.y;

		//orientation_ = mrotation;
		orientation_ = neworientation;

		orientationUsesQuaternion = false;
		orientationReceptionTime = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - startTimestamp;
		orientationMessageSaved = false;
	}

	// Update the Oriention from IMU For Tag V2
	public void updateOrientationQuat(Quaternion neworientation)
	{
		Debug.Log("Update orentiation Quat : ");
		orientationUsesQuaternion = true;
		this.orientation_ = neworientation.eulerAngles;

       Vector3 mrotation = Vector3.zero;
        mrotation.z -= neworientation.z;
        mrotation.y+= neworientation.x;
        mrotation.x -= neworientation.y;
       

		//orientation_ = mrotation;
		this.imuOrientation_quat = neworientation;
		orientationReceptionTime = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - startTimestamp;
		orientationMessageSaved = false;
	}

	public void onSpecialCommand(string data){
        commandReceived = true;
		command = data;
		long receveidTime = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - startTimestamp;
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