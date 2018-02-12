using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {

    public float speed = 1f;
    public Transform lookat;
    public Vector3 axis;

	// Update is called once per frame
	void Update () {
        transform.RotateAround(lookat.position, axis, Time.deltaTime * speed);
	}
}
