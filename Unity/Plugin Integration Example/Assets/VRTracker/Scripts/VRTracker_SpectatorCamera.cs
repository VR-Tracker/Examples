using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/* VR Tracker
 * Disable the User Camera for the local player as it will use the camera from VRTK
 * Enable the Camera if it is the Server to use it as spectator mode
 */

public class VRTracker_SpectatorCamera : MonoBehaviour {

	public NetworkIdentity networkIdentity;
	public Camera camera;

	// Use this for initialization
	void Start () {
		if (networkIdentity != null && camera != null) {
			if (networkIdentity.isServer && !VRTracker.instance.isSpectator)
            {
                CameraManager.instance.cameras.Add(camera);
                camera.enabled = false;
                if(camera.GetComponent<AudioListener>() != null)
                {
                    camera.GetComponent<AudioListener>().enabled = false;
                }
            }
        }
	}

}
