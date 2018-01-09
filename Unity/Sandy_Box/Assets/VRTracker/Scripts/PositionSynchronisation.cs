using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PositionSynchronisation : NetworkBehaviour {

	private Transform myTransform;
	[SerializeField] float lerpRate = 5;
	[SyncVar] private Vector3 syncPos;
	private NetworkIdentity theNetID;

	private Vector3 lastPos;
	private float threshold = 0.5f;


	void Start () {

		if (Network.isServer) {
			transform.position = new Vector3(0,0,0);
			Debug.Log ("NPC is Server");
		}
		else
			Debug.Log ("NPC is Client");
		
		myTransform = GetComponent<Transform> ();
		syncPos = GetComponent<Transform>().position;
	}


	void FixedUpdate () {
		TransmitPosition ();
		LerpPosition ();
	}

	void LerpPosition () {
		if (!hasAuthority) {
			myTransform.position = Vector3.Lerp (myTransform.position, syncPos, Time.deltaTime * lerpRate);
		}
	}

	[Command]
	void CmdProvidePositionToServer (Vector3 pos) {
		syncPos = pos;
	}

	[ClientCallback]
	void TransmitPosition () {
		if (hasAuthority  && Vector3.Distance(myTransform.position, lastPos) > threshold) {
			CmdProvidePositionToServer (myTransform.position);
			lastPos = myTransform.position;
		}
	}


}
