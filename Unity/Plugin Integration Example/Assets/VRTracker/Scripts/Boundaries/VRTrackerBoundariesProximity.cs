using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRTrackerBoundariesProximity : MonoBehaviour {

	public Transform player;
    public Transform controller;
	Renderer render;


	// Use this for initialization
	void Start () {
		/*if (VRTracker.instance.tags.Count > 0) {
			Debug.LogWarning ("Setting " + VRTracker.instance.tags [0]);
			player = VRTracker.instance.tags [0].transform;
		} else {
			Debug.LogWarning("Player is null");
		}*/
		render = gameObject.GetComponent<Renderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!player.Equals(null)) {
			render.sharedMaterial.SetVector ("_PlayerPosition", player.position);
            render.sharedMaterial.SetVector ("_ControllerPosition", controller.position);
        }
    }
}
