using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

public class VRTrackerPickup : MonoBehaviour {

	List <GameObject> currentCollisions = new List <GameObject> ();
	List <GameObject> selectedObjectsUsingGravity = new List <GameObject> ();
	List <GameObject> selectedObjectsNotUsingGravity = new List <GameObject> ();

	public event Action OnGrabed;  // Called when Object is grabbed by the Wand
	public event Action OnReleased;    // Called when Object is released by the Wand

	public Vector3 positionOffset = new Vector3 (0.3f, 0f, 0f);
	private NetworkIdentity objNetId;
	private VRTrackerPickUpV2 pickUp;

	// Use this for initialization
	void Start () {
		if (transform.parent.parent.GetComponent<NetworkIdentity>() != null && !transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
			return;
		}
		this.transform.parent.GetComponent<VRTrackerTag> ().OnDown += OnButtonPressed;
		this.transform.parent.GetComponent<VRTrackerTag> ().OnUp += OnButtonReleased;
		pickUp = transform.parent.parent.gameObject.GetComponent<VRTrackerPickUpV2>();

	}

	// Update is called once per frame
	void Update () {
		if (transform.parent.parent.GetComponent<NetworkIdentity>() != null && !transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
			return;
		}

		foreach (GameObject obj in selectedObjectsUsingGravity) {
			if (obj != null) {
				Vector3 pos = this.transform.parent.position + this.transform.parent.rotation * positionOffset;
				//Vector3 pos = this.transform.parent.position + positionOffset;
				obj.transform.position = pos;
				obj.transform.rotation = this.transform.rotation;
			}
		}
		foreach (GameObject obj in selectedObjectsNotUsingGravity) {
			if (obj != null) {
				Vector3 pos = this.transform.parent.position + this.transform.parent.rotation * positionOffset;
				//Vector3 pos = this.transform.parent.position + positionOffset;
				obj.transform.position = pos;
				obj.transform.rotation = this.transform.rotation;
			}
		}

	}

	void OnTriggerEnter (Collider col)
	{
        /*if (col.gameObject.name != "Body") {
            //Debug.Log ("Collision with " + col.gameObject.name);
            currentCollisions.Add (col.gameObject);
		}*/
		if (col.gameObject.name != "Body" && transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer && col != null)
		{
			//Debug.Log ("Collision with " + col.gameObject.name);
			pickUp.currentCollisions.Add(col.gameObject);
		}
	}

	void OnTriggerExit (Collider col) {

        // TODO: Check that object is not being moved, if so unassigned transform parent
        //currentCollisions.Remove (col.gameObject);
		if(transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer && col != null)
			pickUp.currentCollisions.Remove(col.gameObject);
	}

	private void OnButtonPressed(){
		foreach (GameObject obj in currentCollisions) {
			
			if (obj != null) {
				//positionOffset = obj.transform.position - transform.position;
				//Debug.Log ("Offset X: " + positionOffset.x + "   Y: "  +  positionOffset.y + "   Z: "  + positionOffset.z);
				GameObject netPlayer = transform.parent.parent.gameObject;
				if(netPlayer.GetComponent<NetworkIdentity>().isLocalPlayer){
					if (obj.GetComponent<Rigidbody> ().useGravity) {
						//obj.GetComponent<Rigidbody> ().useGravity = false;
						Debug.Log (obj);	
						Debug.Log (transform.parent.gameObject);
						Debug.Log (transform.parent.parent.gameObject);
						Debug.Log (obj.GetComponent<NetworkIdentity> ());

						VRTrackerNetworking vn = transform.parent.parent.GetComponent<VRTrackerNetworking> ();
						//vn.disableGravity(obj);
						selectedObjectsUsingGravity.Add (obj);
					} else {
						selectedObjectsNotUsingGravity.Add (obj);
					}
					//Need to inform the server for that
					//obj.GetComponent<Rigidbody> ().useGravity = false;
					//CmdDisableGravity(obj);
					Debug.Log ("Gravity has been removed");
				}
			}
		}

		// Execute functions linked to this action
		if (OnGrabed != null)
			OnGrabed();
	}

	private void OnButtonReleased(){
		foreach (GameObject obj in selectedObjectsUsingGravity) {
			if (obj != null && transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
				Debug.Log ("Set Gravity Back, velocity : " + this.transform.parent.GetComponent<VRTrackerTag> ().velocity.x + "  " +  this.transform.parent.GetComponent<VRTrackerTag> ().velocity.y + " "  + this.transform.parent.GetComponent<VRTrackerTag> ().velocity.z);
				//obj.GetComponent<Rigidbody> ().useGravity = true;
				//CmdEnableGravity(obj, this.transform.parent.GetComponent<VRTrackerTag> ().velocity);
				VRTrackerNetworking vn = transform.parent.parent.GetComponent<VRTrackerNetworking> ();
				//vn.enableGravity (obj, this.transform.parent.GetComponent<VRTrackerTag> ().velocity);
				Debug.Log ("Gravity has been enabled");

				//obj.GetComponent<Rigidbody> ().velocity = this.transform.parent.GetComponent<VRTrackerTag> ().velocity;
			}
		}

		// Execute functions linked to this action
		if (OnReleased != null)
			OnReleased();

		selectedObjectsUsingGravity.Clear ();
		selectedObjectsNotUsingGravity.Clear ();
	}

	/*[Command]
	private void CmdDisableGravity(GameObject obj){
		Debug.Log ("Informing no gravity");
		if (obj != null) {
			objNetId = obj.GetComponent<NetworkIdentity> ();
			objNetId.AssignClientAuthority (connectionToClient); //Assign network authority to the current player
			RpcDisableGravity(obj);
			objNetId.RemoveClientAuthority (connectionToClient); //Remove network authority after action done
		}
	}

	[ClientRpc]
	private void RpcDisableGravity(GameObject obj){
		obj.GetComponent<Rigidbody> ().useGravity = false;
	}

	[Command]
	private void CmdEnableGravity(GameObject obj, Vector3 velocity){
		Debug.Log ("Informing gravity");
		if (obj != null) {
			objNetId = obj.GetComponent<NetworkIdentity> ();
			objNetId.AssignClientAuthority (connectionToClient); //Assign network authority to the current player
			RpcEnableGravity(obj, velocity);
			objNetId.RemoveClientAuthority (connectionToClient); //Remove network authority after action done
		}
	}

	[ClientRpc]
	private void RpcEnableGravity(GameObject obj, Vector3 velocity){
		obj.GetComponent<Rigidbody> ().useGravity = true;
		obj.GetComponent<Rigidbody> ().velocity = velocity;
	}
	*/
}
