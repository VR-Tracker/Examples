using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VRTrackerBoundaries : MonoBehaviour {

    /* 
     *VRTracker Boundaries is a singleton that will retrieve the local user
     * It will then add the player gameObject (Head + Controller) into the different boundaries
     * Each boundaries will appear when the local user will be close
     * 
     */

    public GameObject localPlayer;
    private VRTrackerTag[] vrtrackerTags;
    private Transform[] playerTransform;
    Renderer[] renders;
    Vector3[] nearestObject;
    public static VRTrackerBoundaries instance;

    // Use this for initialization
    void Start()
    {
        if (instance != null)
        {
            Debug.LogError("More than one CameraManager in the scene");
        }
        else
        {
            instance = this;
        }
    }

    /// <summary>
    /// LookForLocalPlayer will update the different boundaries with local player component (Head + controller)
    /// TODO : Update the function so that it can automatically handle different configurations for the player
    /// </summary>
    public void LookForLocalPlayer()
    {
        vrtrackerTags = localPlayer.GetComponentsInChildren<VRTrackerTag>();
        VRTrackerBoundariesProximity[] boundaries = GetComponentsInChildren<VRTrackerBoundariesProximity>();

        foreach (VRTrackerBoundariesProximity boundary in boundaries)
        {
            if(vrtrackerTags.Length > 0)
            {
                boundary.player = vrtrackerTags[0].transform;
            }
            if(vrtrackerTags.Length > 1)
            {
                boundary.controller = vrtrackerTags[1].transform;
            }
        }

    }

}
