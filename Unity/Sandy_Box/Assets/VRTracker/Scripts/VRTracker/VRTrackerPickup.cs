using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

/** The pickup script allow the user to move objects with a Wand 
 * made of a Tag and a button.
 * This script must always be on the player object as it requires a NetworkIdentity
 * The Tag to use for grabbing and moving objects must be link at TagPickup
 * The collision detector, on the object container the collider and collision detector
 * script must be linked.
 **/

public class VRTrackerPickup : NetworkBehaviour {

	[System.NonSerialized]
	public List <GameObject> collisionsArray = new List <GameObject> ();
	private List <GameObject> selectedObjectsUsingGravity = new List <GameObject> ();
	private List <GameObject> selectedObjectsNotUsingGravity = new List <GameObject> ();

	public event Action OnGrabed;  // Called when Object is grabbed by the Wand
	public event Action OnReleased;    // Called when Object is released by the Wand

	public VRTrackerTag TagPickup;
	public CollisionDetector collisionDetector;

	public Vector3 positionOffset = new Vector3 (0.3f, 0f, 0f);
	private NetworkIdentity netId;

	public VRTrackerTag TagPickup;

	// Use this for initialization
	void Start () {
		netId = transform.GetComponent<NetworkIdentity> ();

		if (!netId)
			Debug.LogWarning ("No Network Identity found");
		
		if (netId != null && !netId.isLocalPlayer) {
			Debug.Log ("Pickup return on Start");
			return;
		}

		// Callback for Local layer, not server
		if (TagPickup) {
			TagPickup.OnDown += OnButtonPressed;
			TagPickup.OnUp += OnButtonReleased;
		}

		collisionDetector.setTagObject (this);
	}

	void Update () {
		
		// Update loop on server only
		if (netId != null && !netId.isServer)
			return;

		foreach (GameObject obj in selectedObjectsUsingGravity) {
			if (obj != null) {
				Vector3 pos = TagPickup.transform.position + TagPickup.transform.rotation * positionOffset;
				obj.transform.position = pos;
				obj.transform.rotation = this.transform.rotation;
			}
		}
		foreach (GameObject obj in selectedObjectsNotUsingGravity) {
			if (obj != null) {
				Vector3 pos = TagPickup.transform.position + TagPickup.transform.rotation * positionOffset;
				obj.transform.position = pos;
				obj.transform.rotation = this.transform.rotation;
			}
		}

	}

	private void OnButtonPressed(){
		CmdSelectObject ();
	}

	private void OnButtonReleased(){
		CmdUnselectObject ();

	}

	// Execute the selection on server
	[Command] 
	void CmdSelectObject(){
		foreach (GameObject obj in collisionsArray) {
			if (obj != null) {
				if (obj.GetComponent<Rigidbody> () && obj.GetComponent<Rigidbody> ().useGravity) {
					selectedObjectsUsingGravity.Add (obj);
					obj.GetComponent<Rigidbody> ().useGravity = false;
				} else {
					selectedObjectsNotUsingGravity.Add (obj);
				}
			}
		}

		// Execute functions linked to this action
		if (OnGrabed != null)
			OnGrabed();
	}

	// Execute the unselection on server
	[Command] 
	void CmdUnselectObject(){
		// Execute functions linked to this action
		if (OnReleased != null)
			OnReleased();

		// Re-enable Gravity
		foreach (GameObject obj in selectedObjectsUsingGravity) {
			obj.GetComponent<Rigidbody> ().useGravity = true;
			obj.GetComponent<Rigidbody>().velocity = TagPickup.velocity;
		}
			
		selectedObjectsUsingGravity.Clear ();
		selectedObjectsNotUsingGravity.Clear ();
	}
}
