using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class DisablePositionOculus : MonoBehaviour {

	// Use this for initialization
	void Start () {
        UnityEngine.XR.InputTracking.disablePositionalTracking = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
