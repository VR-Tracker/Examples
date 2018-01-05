using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/**
 * This script is to be attached on a Networked NPC
 * It will disable the rigidBody collisions / gravity 
 * so only the server take care of it (cf VRTrackerPickug.cs)
 **/

public class DisableRigidbodyOnClient : MonoBehaviour {

	private NetworkIdentity netId;

	// Use this for initialization
	void Start () {
		netId = transform.GetComponent<NetworkIdentity> ();
		if (GetComponent<NetworkIdentity> () && GetComponent<Rigidbody> () && !GetComponent<NetworkIdentity> ().isServer) {
			GetComponent<Rigidbody> ().isKinematic = true;
			GetComponent<Rigidbody> ().detectCollisions = false;
			GetComponent<Rigidbody> ().useGravity = false;
		}
	}
}
