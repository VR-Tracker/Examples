using UnityEngine;
using VRTK;
using UnityEngine.Networking;

public class VRTrackerVRTKSync : MonoBehaviour {
    public GameObject AvatarHead;
    public GameObject LeftHand;
    public GameObject RightHand;
    private NetworkIdentity netId;

    private void OnEnable() {
        netId = transform.GetComponent<NetworkIdentity>();

        SetUpTransformFollow(AvatarHead, VRTK_DeviceFinder.Devices.Headset);
        SetUpTransformFollow(LeftHand, VRTK_DeviceFinder.Devices.LeftController);
        SetUpTransformFollow(RightHand, VRTK_DeviceFinder.Devices.RightController);
    }

    private static void SetUpTransformFollow(GameObject avatarComponent, VRTK_DeviceFinder.Devices device) {
        if (avatarComponent)
        {
            var transformFollow = avatarComponent.transform.GetComponent<VRTK_TransformFollow>();
            if (transformFollow)
            {
                transformFollow.gameObjectToChange = VRTK_DeviceFinder.DeviceTransform(device).gameObject;
            }
            else
                Debug.LogWarning("VRTK no VRTK_TransformFollow on avatar component " + avatarComponent.name);
        }
        else
            Debug.LogWarning("VRTK Device component " + device.ToString() + " is not linked");
        
    }
    /*
    private static void SetUpControllerHandLink(GameObject avatarComponent, VRTK_DeviceFinder.Devices device) {
        var photonView = avatarComponent.GetComponent<PhotonView>();
        if (photonView == null) {
            Debug.LogError(string.Format("The network representation '{0}' has no {1} component on it.", avatarComponent.name, typeof(PhotonView).Name));
            return;
        }

        if (!photonView.isMine) {
            return;
        }

        GameObject controller = VRTK_DeviceFinder.DeviceTransform(device).gameObject;
        GameObject actual = VRTK_DeviceFinder.GetActualController(controller);
        var link = actual.AddComponent<PhotonViewLink>();
        link.linkedView = photonView;
    }
    */
}
