using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DisableRigidbodyOnClient : MonoBehaviour {

	private NetworkIdentity netId;

	// Use this for initialization
	void Start () {
		netId = transform.GetComponent<NetworkIdentity> ();
		if (GetComponent<NetworkIdentity> () && GetComponent<Rigidbody> () && !GetComponent<NetworkIdentity> ().isServer) {
			GetComponent<Rigidbody> ().isKinematic = true;
			GetComponent<Rigidbody> ().detectCollisions = false;
		}
	}
}
