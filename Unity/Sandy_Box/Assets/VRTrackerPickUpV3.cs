using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

public class VRTrackerPickUpV3 : MonoBehaviour {

    List<GameObject> currentCollisions = new List<GameObject>();
    List<GameObject> selectedObjectsUsingGravity = new List<GameObject>();
    List<GameObject> selectedObjectsNotUsingGravity = new List<GameObject>();

    public event Action OnGrabed;  // Called when Object is grabbed by the Wand
    public event Action OnReleased;    // Called when Object is released by the Wand

    public Vector3 positionOffset = new Vector3(0.3f, 0f, 0f);
    private VRTrackerPickUpV2 pickUp;
    // Use this for initialization
    void Start()
    {
        if (transform.parent.parent.GetComponent<NetworkIdentity>() != null && !transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            return;
        }
        pickUp = transform.parent.parent.gameObject.GetComponent<VRTrackerPickUpV2>();
        Debug.LogWarning("G " + transform.parent.parent.gameObject);
        Debug.LogWarning("Net " + transform.parent.parent.GetComponent<NetworkIdentity>());
        Debug.LogWarning("Pick Up " + pickUp);
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void OnTriggerEnter(Collider col)
    {
		if (col.gameObject.name != "Body" && transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            //Debug.Log ("Collision with " + col.gameObject.name);
            pickUp.currentCollisions.Add(col.gameObject);
        }
    }

    void OnTriggerExit(Collider col)
    {
        // TODO: Check that object is not being moved, if so unassigned transform parent
		if(transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer)
        	pickUp.currentCollisions.Remove(col.gameObject);
    }

    private void OnButtonPressed()
    {
        Debug.Log("Button Pressed");
        foreach (GameObject obj in currentCollisions)
        {
            Debug.Log("OBP " + obj);

            if (obj != null)
            {
                //positionOffset = obj.transform.position - transform.position;
                //Debug.Log ("Offset X: " + positionOffset.x + "   Y: "  +  positionOffset.y + "   Z: "  + positionOffset.z);
                GameObject netPlayer = transform.parent.parent.gameObject;
                if (netPlayer.GetComponent<NetworkIdentity>().isLocalPlayer)
                {
					if (obj.GetComponent<Rigidbody> () == null) {
						return;
					}
                    if (obj.GetComponent<Rigidbody>().useGravity)
                    {
                        //obj.GetComponent<Rigidbody> ().useGravity = false;
                        Debug.Log(obj);
                        Debug.Log(transform.parent.gameObject);
                        Debug.Log(transform.parent.parent.gameObject);
                        Debug.Log(obj.GetComponent<NetworkIdentity>());

                        VRTrackerNetworking vn = transform.parent.parent.GetComponent<VRTrackerNetworking>();
                        //vn.disableGravity(obj);
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
        foreach (GameObject obj in selectedObjectsUsingGravity)
        {
            if (obj != null && transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer)
            {
                Debug.Log("Set Gravity Back, velocity : " + this.transform.parent.GetComponent<VRTrackerTag>().velocity.x + "  " + this.transform.parent.GetComponent<VRTrackerTag>().velocity.y + " " + this.transform.parent.GetComponent<VRTrackerTag>().velocity.z);
                //obj.GetComponent<Rigidbody> ().useGravity = true;
                //CmdEnableGravity(obj, this.transform.parent.GetComponent<VRTrackerTag> ().velocity);
                VRTrackerNetworking vn = transform.parent.parent.GetComponent<VRTrackerNetworking>();
               // vn.enableGravity(obj, this.transform.parent.GetComponent<VRTrackerTag>().velocity);
                Debug.Log("Gravity has been enabled");

                //obj.GetComponent<Rigidbody> ().velocity = this.transform.parent.GetComponent<VRTrackerTag> ().velocity;
            }
        }

        // Execute functions linked to this action
        if (OnReleased != null)
            OnReleased();

        selectedObjectsUsingGravity.Clear();
        selectedObjectsNotUsingGravity.Clear();
    }

}
