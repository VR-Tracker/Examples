using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* VR Tracker
 * This script is to be set on a Gameobject between the Camera and the Object to which the Headset Tag position is applied
 * 
 */

public class VRTracker_HeadsetRotationCancel_Y : MonoBehaviour
{

    public Camera camera;
    public VRTrackerTag tag;

    private float offsetY = 0.0f;
    private Quaternion previousOffset;
    private Quaternion destinationOffset;

    private float t;
    private float timeToReachTarget = 10.0f;
    private bool offsetSet = false;

    void OnEnable()
    {

        StartCoroutine(FixOffset());
        previousOffset = Quaternion.Euler(Vector3.zero);
        destinationOffset = Quaternion.Euler(Vector3.zero);

    }

    void OnDisable()
    {
        offsetSet = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        t += Time.deltaTime / timeToReachTarget;
        transform.localRotation = Quaternion.Lerp(previousOffset, destinationOffset, t);
        /*
        if (tag == null)
            tag = VRTracker.instance.getHeadsetTag();
        if (!offsetSet && tag != null)
        {
            Vector3 tagRotation = tag.getOrientation();
            Vector3 cameraRotation = camera.transform.localEulerAngles;


            previousOffset = destinationOffset;
            destinationOffset.y = tagRotation.y - cameraRotation.y;


            transform.rotation = Quaternion.Euler(destinationOffset);
            Debug.Log("TAG Rotation: " + tagRotation.x + "  " + tagRotation.y + "   " + tagRotation.z);
            Debug.Log("CAMERA Rotation: " + cameraRotation.x + "  " + cameraRotation.y + "   " + cameraRotation.z);
            previousOffset = Vector3.Lerp(previousOffset, offsetRotation, 0.2f);

            offsetSet = true;
        }*/

    }

    IEnumerator FixOffset()
    {
        while (true)
        {
            if (tag == null)
                tag = VRTracker.instance.getHeadsetTag();
            if (tag != null)
            {
                Vector3 tagRotation = tag.getOrientation();
                Vector3 cameraRotation = camera.transform.localEulerAngles;
                Vector3 newRotation = Vector3.zero;
                newRotation.y = tagRotation.y - cameraRotation.y;

                previousOffset = destinationOffset;
                destinationOffset = Quaternion.Euler(newRotation);
                t = 0;

                 Debug.Log("TAG Rotation: " + tagRotation.x + "  " + tagRotation.y + "   " + tagRotation.z);
                 Debug.Log("CAMERA Rotation: " + cameraRotation.x + "  " + cameraRotation.y + "   " + cameraRotation.z);
            }
            yield return new WaitForSeconds(10);
        }
    }
}
