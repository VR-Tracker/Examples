using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRTracker_FollowRotation : MonoBehaviour {

    public VRTrackerTag tag;
    private Quaternion offset;

	// Use this for initialization
	void Start () {
        offset = transform.rotation;
	}
	
	// Update is called once per frame
	void LateUpdate () {
        transform.rotation = offset*Quaternion.Euler(tag.getOrientation());
	}
}
