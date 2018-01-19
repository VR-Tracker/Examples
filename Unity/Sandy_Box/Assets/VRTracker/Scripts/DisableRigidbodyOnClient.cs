using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/* VR Tracker
 * Disable collision detection and enable kinematics on the
 * Rigidbody on the Client (only keep enabled in Server)
 */

public class DisableRigidbodyOnClient : MonoBehaviour {

	void Start () {
		if (GetComponent<NetworkIdentity> () && GetComponent<Rigidbody> () && !GetComponent<NetworkIdentity> ().isServer) {
			GetComponent<Rigidbody> ().isKinematic = true;
			GetComponent<Rigidbody> ().detectCollisions = false;
		}
	}
}