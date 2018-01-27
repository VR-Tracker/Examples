using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CollisionDetector : MonoBehaviour {

	private VRTrackerPickup pickupScript;
	private NetworkIdentity netId;

	// Use this for initialization
	void Start () {
		netId = transform.GetComponentInParent<NetworkIdentity> ();
	}
		
	void OnTriggerEnter (Collider col)
	{
		if (!pickupScript)
			return;
		
		// Trigger Collision detection in Server only
		if (netId != null && !netId.isServer)
			return;
		
		if (col.gameObject.name != "Body" && col != null) {
			pickupScript.collisionsArray.Add (col.gameObject);
		}
	}

	void OnTriggerExit (Collider col) {
		// Trigger Collision detection in Server only

		if (!pickupScript)
			return;

		// Trigger Collision detection in Server only
		if (netId != null && !netId.isServer)
			return;

		if(col != null)
			pickupScript.collisionsArray.Remove (col.gameObject);
	}

	public void setTagObject(VRTrackerPickup script){
		pickupScript = script;
	}
}
