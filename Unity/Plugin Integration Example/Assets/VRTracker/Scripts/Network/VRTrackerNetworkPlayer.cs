using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VRTrackerNetworkPlayer : NetworkBehaviour
{

	// Use this for initialization
	void Start () {
        /*if (isServer)
        {
            Camera cam = GetComponent<Camera>();
            if(cam != null)
            {
                CameraManager.instance.cameras.Add(cam);
                cam.enabled = false;
                cam.GetComponent<AudioListener>().enabled = false;
            }
        }*/
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void OnStartLocalPlayer()
    {
        Debug.LogWarning("On start local player client ");

        VRTrackerBoundaries.instance.localPlayer = gameObject;
        VRTrackerBoundaries.instance.LookForLocalPlayer();
    }

}
