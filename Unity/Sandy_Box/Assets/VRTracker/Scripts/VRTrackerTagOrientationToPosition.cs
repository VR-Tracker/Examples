using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* VR Tracker
 * This scripts transforms a Tag into a 3D Rudder like controller
 * https://www.3drudder.com/
 */
public class VRTrackerTagOrientationToPosition : MonoBehaviour {

	public Transform tag;
	public float stabilityRange = 5.0f;
	public float positionOffset = 0.005f;
	public float rotationOffset = 0.005f;

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		Vector3 orientation = transform.rotation.eulerAngles;

		if (tag.rotation.eulerAngles.y > 180)
			orientation.y += (tag.rotation.eulerAngles.y - 360)*Time.deltaTime*rotationOffset;
		else
			orientation.y += tag.rotation.eulerAngles.y*Time.deltaTime*rotationOffset;

		if (Mathf.Abs(orientation.y) < stabilityRange)
			orientation.y = 0.0f;
		
		transform.rotation = Quaternion.Euler(orientation);

		Vector3 orientationFixed = new Vector3 (tag.rotation.eulerAngles.z, 0, tag.rotation.eulerAngles.x);
		if (orientationFixed.x > 180)
			orientationFixed.x -= 360;
		if (orientationFixed.z > 180)
			orientationFixed.z -= 360;
		
		if (Mathf.Abs(orientationFixed.x) < stabilityRange)
			orientationFixed.x = 0.0f;
		if (Mathf.Abs(orientationFixed.z) < stabilityRange)
			orientationFixed.z = 0.0f;

		Vector3 translation = new Vector3 (-orientationFixed.x*positionOffset*Time.deltaTime, 0, orientationFixed.z*positionOffset*Time.deltaTime);
		transform.Translate (translation);
	}
}
