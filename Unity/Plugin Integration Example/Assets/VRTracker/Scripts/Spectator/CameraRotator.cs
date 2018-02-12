using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour {

    public float speed = 1f;
    public Transform lookat;
    public Vector3 axis;

    // Update is called once per frame
    void Update()
    {
        if (lookat != null)
        {
            transform.RotateAround(lookat.position, axis, Time.deltaTime * speed);
        }
        else
        {
            transform.RotateAround(Vector3.zero, axis, Time.deltaTime * speed);
        }
    }
}

