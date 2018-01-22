using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine;

public class EnableOnLoad : MonoBehaviour {

    public bool enableOnLoad = false;

	protected virtual void Start () {

		if (transform.parent.GetComponent<NetworkIdentity>() != null && !transform.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
			Debug.Log ("Disable Cam");
			Camera[] cams = GetComponentsInChildren<Camera>();
			//Desactivate camera of other player
			foreach(Camera cam in cams)
			{
				cam.enabled = false;
			}
			//Disable audio listener of other player
			if(this.GetComponentInChildren<AudioListener>() != null)
				this.GetComponentInChildren<AudioListener>().enabled = false;

			foreach(Transform child in transform)
			{
				//display all the component with tag buddy
				if (child.tag == "Body") {
					child.gameObject.layer = 9; //See layer for the number
					setLayerToChildren (child.gameObject, 9);
				}
					
			}
		}
       else
        {
			Debug.Log ("Enable Cam");
            transform.eulerAngles.Set(transform.eulerAngles.x, 0, transform.eulerAngles.z);

            Camera[] cams = GetComponentsInChildren<Camera>();
            foreach (Camera cam in cams)
            {
                cam.enabled = true;
                cam.transform.eulerAngles.Set(cam.transform.eulerAngles.x, 0, cam.transform.eulerAngles.z);
            }
            if (this.GetComponentInChildren<AudioListener>() != null)
            {
                this.GetComponentInChildren<AudioListener>().enabled = true;
            }
			foreach(Transform child in transform)
			{
				//hide all the component with for the player buddy
				if (child.tag == "Body") {
					child.gameObject.layer = 8; //See layer for the number
					setLayerToChildren (child.gameObject, 8);
				}

			}
        }
    }

	public void setLayerToChildren(GameObject gObject, int layerNumber)
	{
		foreach (Transform trans in gObject.GetComponentsInChildren<Transform>(true))
		{
			trans.gameObject.layer = layerNumber;
		}
	}
}
