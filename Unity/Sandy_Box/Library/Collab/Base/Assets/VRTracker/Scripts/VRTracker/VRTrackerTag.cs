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

	private Vector3 previousPosition;
	public Vector3 velocity;

	// ANALYTICS
	private float messageReceptionTimestamp = 0;
	private long lastLateUpateTimestamp = 0;
	private int counterSameMessageReception = 0;
	private int counterMessagesBetweenFrame = 0;
	private int counterFrameWithSamePosition = 0;
	private Vector3 lastFramePosition;

	// PREDICTION
	private long startTimestamp;
	private long lastMessagePositionTimestamp;
	private Vector3 predictedPosition; // Based on last received position
	private Vector3 lerpingPosition; // Based on position predicted in late update when new position received
	private Vector3 finalSmoothedPosition; 
	private Vector3 lerpingOrigin; // Based on position predicted in late update when new position received
	private Vector3 acceleration;
	private Vector3[] speeds;
	private Queue<KeyValuePair<long, Vector3>> positions;
	private float delta = 0;
	private bool positionWasUpdatedSinceLastFrame = false;
	private long deltaTimeLateUpdateSinceLastPosition = 0;
	private long deltaTimeBetweenTwoLastPositions = 0;
	private long lerpingDuration = 0;
	private long lerpingTimestamp = 0;
	private bool isLerping = false;
	private long maxPredictionDuration = 500; // 500ms

	public bool enablePrediction = true;

	// Use this for initialization
	protected virtual void Start () {
		if (transform.parent.GetComponent<NetworkIdentity>() != null && !transform.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
			return;
		}
		startTimestamp = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
		lastLateUpateTimestamp = startTimestamp;

		enablePrediction = true;

		speeds = new Vector3[2];
		positions = new Queue<KeyValuePair<long, Vector3>>();
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

		positionWasUpdatedSinceLastFrame = false;
		long lateUpdateTimestamp = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - startTimestamp;
		//ANALYTICS


		// PREDICTIONS
		long deltaTimeSinceLastFrame = lateUpdateTimestamp - lastLateUpateTimestamp; // Delay since last update frame
		lastLateUpateTimestamp = lateUpdateTimestamp;

		// Check if the position changed since last frame, and if the prediction needs to stop because a new position was received
		bool stopPredictionStartLerp = false;
		if (lastFramePosition.x == positionReceived.x && lastFramePosition.y == positionReceived.y && lastFramePosition.z == positionReceived.z) {
			counterFrameWithSamePosition++;
		} else {
			if (counterFrameWithSamePosition > 2){ //TODO: Try to start lerping only if more than X frame had the same position
				stopPredictionStartLerp = true;

			}
			counterFrameWithSamePosition = 0;
		}

		if (positions.Count > 2) {
			KeyValuePair<long, Vector3>[] positionsArray = positions.ToArray();
			deltaTimeLateUpdateSinceLastPosition = (lateUpdateTimestamp - positionsArray[positionsArray.Length - 1].Key); // Delay since last position received
		
			// Save Lerp duration (the duration is equal to the prediction duration), lerp origin position
			if (stopPredictionStartLerp) {
				
				lerpingDuration = deltaTimeBetweenTwoLastPositions; // Lerp Duration
				lerpingTimestamp = lateUpdateTimestamp - deltaTimeSinceLastFrame; // Time at which Lerp starts (previous LateUpdate frame)
				isLerping = true;
				lerpingOrigin = this.transform.position; // The Lerping origin is the last frame position at LateUpdate
				Debug.Log("Start Lerp at " + lerpingOrigin.x + "  " + lerpingOrigin.y + "  " + lerpingOrigin.z + "  for: " + lerpingDuration);
			}

			// Check if still Lerping
			if (isLerping) {
				if(lateUpdateTimestamp - lerpingTimestamp > lerpingDuration)
					isLerping = false;
			}
				
		//	Debug.Log ("Drop:" + (100*deltaTimeLateUpdateSinceLastPosition/maxPredictionDuration) + "%");
			Vector3 accelerationDropedOverTime = Vector3.Slerp (acceleration, Vector3.zero, deltaTimeLateUpdateSinceLastPosition/maxPredictionDuration);
			Vector3 speedDropedOverTime = Vector3.Slerp (speeds [0], Vector3.zero, deltaTimeLateUpdateSinceLastPosition/maxPredictionDuration);


			long delta = 0;
			Vector3 origin;
			if (counterFrameWithSamePosition == 0) {
				delta = deltaTimeLateUpdateSinceLastPosition;
				origin = positionsArray [positionsArray.Length - 1].Value;
			} else {
				delta = deltaTimeSinceLastFrame;
				origin = predictedPosition;
			}

			// Debug.Log ("Delta : " + delta*1000 + "  Millis : " + lateUpdateTimestamp);
			float accOP = (float)(0.5 * delta * delta / 1000000);
			predictedPosition = origin + accOP * accelerationDropedOverTime + speedDropedOverTime * delta / 1000;
			//predictedPosition = positionReceived + speeds[0] * delta;


			if (isLerping) {
				long lerpDelta = (lateUpdateTimestamp - lerpingTimestamp);
				//Debug.Log ("Lerp delta = " + lerpDelta);

				float accOP2 = (float)(0.5 * lerpDelta * lerpDelta)/1000000;
				lerpingPosition = lerpingPosition + accOP2 * accelerationDropedOverTime + speedDropedOverTime * lerpDelta /1000; 
				finalSmoothedPosition = Vector3.Lerp (lerpingPosition, predictedPosition, lerpDelta/lerpingDuration);
			}
			else
				finalSmoothedPosition = predictedPosition;
            
		}


		String message =  deltaTimeSinceLastFrame + ";" + deltaTimeLateUpdateSinceLastPosition + ";" + counterSameMessageReception + ";" + isLerping + ";" + counterMessagesBetweenFrame + ";" + counterFrameWithSamePosition + ";" + positionReceived.x + ";" + positionReceived.y + ";" + positionReceived.z + ";" + predictedPosition.x + ";" + predictedPosition.y + ";" + predictedPosition.z  + ";" + finalSmoothedPosition.x + ";" + finalSmoothedPosition.y + ";" + finalSmoothedPosition.z + ";" + acceleration.x + ";" + acceleration.y + ";" + acceleration.z + ";" + speeds[0].x + ";" + speeds[0].y + ";" + speeds[0].z + ";" + delta;
		//Debug.LogWarning (message);
		GetComponent<LoggingSystem>().writeMessageWithTimestampToLog (message);
		lastFramePosition = positionReceived;
		counterMessagesBetweenFrame = 0;

		
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

		if(enablePrediction)
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
            
            Debug.Log("Received timestamp " + timestamp);
            updatePosition(position_);
        }

        public void updatePosition(Vector3 position_){

		positionWasUpdatedSinceLastFrame = true;


        // ANALYTICS
		long milliseconds = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - startTimestamp;
		messageReceptionTimestamp = milliseconds;
        counterMessagesBetweenFrame++;

		// PREDICTION
           // while (positions.Count >= 5)
             //   positions.Dequeue();
			
		positions.Enqueue(new KeyValuePair<long, Vector3>(milliseconds, position_));
		KeyValuePair<long, Vector3>[] positionsArray = positions.ToArray();
		deltaTimeBetweenTwoLastPositions = positionsArray[positionsArray.Length - 1].Key - positionsArray [positionsArray.Length - 2].Key;

        Debug.Log(deltaTimeBetweenTwoLastPositions);

        if (positions.Count > 1) {
			speeds[1] = 1000*((positionsArray[positionsArray.Length - 3].Value - positionsArray[positionsArray.Length - 5].Value) / (positionsArray[positionsArray.Length - 3].Key - positionsArray[positionsArray.Length - 5].Key));
			speeds[0] = 1000*((positionsArray[positionsArray.Length - 1].Value - positionsArray[positionsArray.Length - 3].Value) / (positionsArray[positionsArray.Length - 1].Key - positionsArray[positionsArray.Length - 3].Key));
            if (positions.Count > 2) {
				acceleration = 1000*((speeds[0] - speeds[1]) / (positionsArray[positionsArray.Length - 2].Key - positionsArray[positionsArray.Length - 4].Key));
            }
        }

        this.positionReceived = position_;
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