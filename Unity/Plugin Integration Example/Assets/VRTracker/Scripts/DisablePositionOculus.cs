using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class DisablePositionOculus : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log("Disable position tracking 1");
        UnityEngine.XR.InputTracking.disablePositionalTracking = true;
	}

    void OnEnable()
    {
        Debug.Log("Disable position tracking 2");
        UnityEngine.XR.InputTracking.disablePositionalTracking = true;
    }

    // Update is called once per frame
    void Update () {
	//	Debug.Log(Time.deltaTime);
	}
}
