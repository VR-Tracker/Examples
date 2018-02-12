using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;


public class CameraManager : MonoBehaviour {

    public static CameraManager instance;

    public List<Camera> cameras;
    private int index = 0;
    

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one CameraManager in the scene");
        }
        else
        {
            instance = this;
            Camera[] cams = GetComponentsInChildren<Camera>();
            foreach (Camera cam in cams)
            {
                cameras.Add(cam);
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        foreach (Camera cam in cameras)
        {
            DisableCam(cam);
        }
        if (VRTracker.instance != null && VRTracker.instance.isSpectator)
        {
            EnableCam(cameras[index]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            DisableCam(cameras[index]);
            index = (index + 1) % cameras.Count;
            EnableCam(cameras[index]);
        }

    }

    public void AddPlayerCam(GameObject player)
    {
        Camera newCam = player.transform.GetComponent<Camera>();
        DisableCam(newCam);
        cameras.Add(newCam);
    }

    private void EnableCam(Camera cam)
    {
        cam.enabled = true;
        cam.GetComponent<AudioListener>().enabled = true;
    }

    private void DisableCam(Camera cam)
    {
        cam.enabled = false;
        cam.GetComponent<AudioListener>().enabled = false;
    }
}
