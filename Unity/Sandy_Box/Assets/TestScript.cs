using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnGUI() {
		if (Network.isServer)
			GUI.Label (new Rect (10, 10, 100, 20), "Running as a server");
		else
			if (Network.isClient)
				GUI.Label (new Rect (10, 10, 100, 20), "Running as a client");

	}
}
