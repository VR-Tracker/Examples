using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;


public class VRTrackerPickUpV2 : NetworkBehaviour {

    [System.NonSerialized]
    public List<GameObject> currentCollisions = new List<GameObject>();
    [System.NonSerialized]
    public List<GameObject> selectedObjectsUsingGravity = new List<GameObject>();
    [System.NonSerialized]
    public List<GameObject> selectedObjectsNotUsingGravity = new List<GameObject>();

    public event Action OnGrabed;  // Called when Object is grabbed by the Wand
    public event Action OnReleased;    // Called when Object is released by the Wand

    public Vector3 positionOffset = new Vector3(0.3f, 0f, 0f);
    public GameObject input;
    private NetworkIdentity objNetId;

    // Use this for initialization
    void Start()
    {
        if (GetComponent<NetworkIdentity>() != null && !GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            return;
        }
        input.transform.parent.GetComponent<VRTrackerTag>().OnDown += OnButtonPressed;
        input.transform.parent.GetComponent<VRTrackerTag>().OnUp += OnButtonReleased;
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<NetworkIdentity>() != null && !GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            return;
        }

        foreach (GameObject obj in selectedObjectsUsingGravity)
        {
            if (obj != null)
            {
                Vector3 pos = input.transform.position + input.transform.rotation * positionOffset;
                //Vector3 pos = this.transform.parent.position + positionOffset;
                obj.transform.position = pos;
                obj.transform.rotation = input.transform.rotation;
            }
        }
        foreach (GameObject obj in selectedObjectsNotUsingGravity)
        {
            if (obj != null)
            {
                Vector3 pos = input.transform.position + input.transform.rotation * positionOffset;
                //Vector3 pos = this.transform.parent.position + positionOffset;
                obj.transform.position = pos;
                obj.transform.rotation = input.transform.rotation;
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
		if (GetComponent<NetworkIdentity>() != null && !GetComponent<NetworkIdentity>().isLocalPlayer)
		{
			return;
		}

        if (col.gameObject.name != "Body")
        {
            //Debug.Log ("Collision with " + col.gameObject.name);
            currentCollisions.Add(col.gameObject);
        }
    }

    void OnTriggerExit(Collider col)
    {
		if (GetComponent<NetworkIdentity>() != null && !GetComponent<NetworkIdentity>().isLocalPlayer)
		{
			return;
		}

        // TODO: Check that object is not being moved, if so unassigned transform parent
        currentCollisions.Remove(col.gameObject);
    }

    private void OnButtonPressed()
    {
		if (GetComponent<NetworkIdentity>() != null && !GetComponent<NetworkIdentity>().isLocalPlayer)
		{
			return;
		}

        foreach (GameObject obj in currentCollisions)
        {
            
			if (obj != null && GetComponent<NetworkIdentity>().isLocalPlayer && hasAuthority)
            {
                //positionOffset = obj.transform.position - transform.position;
                Debug.Log ("Offset X: " + positionOffset.x + "   Y: "  +  positionOffset.y + "   Z: "  + positionOffset.z);
                GameObject netPlayer = input.transform.parent.parent.gameObject;
                if (netPlayer.GetComponent<NetworkIdentity>().isLocalPlayer)
                {
                    if(obj.GetComponent<Rigidbody>() == null)
                    {
                        return;
                    }
                    if (obj.GetComponent<Rigidbody>().useGravity)
                    {
                        //obj.GetComponent<Rigidbody> ().useGravity = false;
                        Debug.Log(obj);

                        //VRTrackerNetworking vn = input.transform.parent.parent.GetComponent<VRTrackerNetworking>();
                        //vn.disableGravity(obj);
						CmdAssignLocalAuthority(obj);
                        CmdDisableGravity(obj);
                        selectedObjectsUsingGravity.Add(obj);
                    }
                    else
                    {
                        selectedObjectsNotUsingGravity.Add(obj);
                    }
                    //Need to inform the server for that
                    //obj.GetComponent<Rigidbody> ().useGravity = false;
                    //CmdDisableGravity(obj);
                    Debug.Log("Gravity has been removed");
                }
            }
        }

        // Execute functions linked to this action
        if (OnGrabed != null)
            OnGrabed();
    }

    private void OnButtonReleased()
    {
		if (GetComponent<NetworkIdentity>() != null && !GetComponent<NetworkIdentity>().isLocalPlayer)
		{
			return;
		}

        foreach (GameObject obj in selectedObjectsUsingGravity)
        {
            Debug.Log("OBR " + obj);
			if (obj != null && input.transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer && hasAuthority)
            {
                Debug.Log("Set Gravity Back, velocity : " + input.transform.parent.GetComponent<VRTrackerTag>().velocity.x + "  " + input.transform.parent.GetComponent<VRTrackerTag>().velocity.y + " " + input.transform.parent.GetComponent<VRTrackerTag>().velocity.z);
                //obj.GetComponent<Rigidbody> ().useGravity = true;
                //CmdEnableGravity(obj, this.transform.parent.GetComponent<VRTrackerTag> ().velocity);
                //VRTrackerNetworking vn = input.transform.parent.parent.GetComponent<VRTrackerNetworking>();
                CmdEnableGravity(obj, input.transform.parent.GetComponent<VRTrackerTag>().velocity);
                Debug.Log("Gravity has been enabled");
				CmdRemoveLocalAuthority (obj);
                //obj.GetComponent<Rigidbody> ().velocity = this.transform.parent.GetComponent<VRTrackerTag> ().velocity;
            }
        }
        Debug.Log("Going to release");

        // Execute functions linked to this action
        //if (OnReleased != null)
            //OnReleased();

        selectedObjectsUsingGravity.Clear();
        selectedObjectsNotUsingGravity.Clear();
    }

    [Command]
    private void CmdDisableGravity(GameObject obj)
    {
        Debug.Log("Informing no gravity");
        if (obj != null)
        {
            //NetworkIdentity localPlayer = GetComponent<NetworkIdentity>();
            //objNetId = obj.GetComponent<NetworkIdentity>();
            //localPlayer.localPlayerAuthority = true;
            /*if (hasAuthority)
            {
                Debug.LogWarning("Local player has authority");

                obj.GetComponent<NetworkIdentity>().AssignClientAuthority(localPlayer.connectionToClient); //Assign network authority to the current player
                Debug.LogWarning("Authority " + obj.GetComponent<NetworkIdentity>().clientAuthorityOwner);
            }
            else
            {
                Debug.LogWarning("Local player has not authority");

            }*/
			RpcDisableGravity (obj);

            //objNetId.localPlayerAuthority = false;
            //obj.GetComponent<NetworkIdentity>().RemoveClientAuthority(localPlayer.connectionToClient); //Remove network authority after action done
            //obj.GetComponent<NetworkIdentity>().localPlayerAuthority = false;


        }
    }

    [ClientRpc]
    public void RpcDisableGravity(GameObject obj)
    {
        obj.GetComponent<Rigidbody>().useGravity = false;

    }

    [Command]
    public void CmdEnableGravity(GameObject obj, Vector3 velocity)
    {
        Debug.Log("Informing gravity");
        if (obj != null)
        {
            /*NetworkIdentity localPlayer = GetComponent<NetworkIdentity>();
            Debug.Log(GetComponent<NetworkIdentity>().connectionToClient);
            NetworkIdentity ni = GetComponent<NetworkIdentity>();
            localPlayer.localPlayerAuthority = true;
            objNetId = obj.GetComponent<NetworkIdentity>();*/
            //objNetId.localPlayerAuthority = true;
            //objNetId.AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient); //Assign network authority to the current player
            //localPlayer.localPlayerAuthority = true;
            //if (hasAuthority)
            //{

                Debug.LogWarning(" EG Local player has authority");
                //obj.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
				RpcEnableGravity (obj, velocity);
                //obj.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
                Debug.LogWarning("Authority " + obj.GetComponent<NetworkIdentity>().clientAuthorityOwner);
				//obj.GetComponent<NetworkIdentity>().RemoveClientAuthority(localPlayer.connectionToClient); //Remove network authority after action done


            /*}
            else
            {
                Debug.LogWarning("Local player has not authority");

            }*/

            //objNetId.localPlayerAuthority = false;
        }
    }

    [ClientRpc]
    public void RpcEnableGravity(GameObject obj, Vector3 velocity)
    {
        obj.GetComponent<Rigidbody>().useGravity = true;
        obj.GetComponent<Rigidbody>().velocity = velocity;
    }

	[Command]
	void CmdAssignLocalAuthority (GameObject obj) {
		Debug.Log("Assigning authority");
		NetworkInstanceId nIns = obj.GetComponent<NetworkIdentity> ().netId;
		GameObject client = NetworkServer.FindLocalObject (nIns);
		NetworkIdentity ni = client.GetComponent<NetworkIdentity> ();
		ni.AssignClientAuthority(connectionToClient);
	}

	[Command]
	void CmdRemoveLocalAuthority (GameObject obj) {
		Debug.Log("Removing authority");
		NetworkInstanceId nIns = obj.GetComponent<NetworkIdentity> ().netId;
		GameObject client = NetworkServer.FindLocalObject (nIns);
		NetworkIdentity ni = client.GetComponent<NetworkIdentity> ();
		ni.RemoveClientAuthority (ni.clientAuthorityOwner);
	}
}
