using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorDisable : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (VRTracker.instance.isSpectator)
        {
            gameObject.SetActive(false);
        }	
	}
	
}
