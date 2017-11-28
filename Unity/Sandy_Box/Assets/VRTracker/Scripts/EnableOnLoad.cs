using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine;

public class EnableOnLoad : MonoBehaviour {

    public bool enableOnLoad = false;

	// Use this for initialization
//	void Start () {
   //     SceneManager.sceneLoaded += OnLevelFinishedLoading;
  //  }

	protected virtual void Start () {

		Debug.Log ("Level Finish");
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
			GameObject[] bodyParts = GameObject.FindGameObjectsWithTag ("Body");
			foreach(GameObject bodyPart in bodyParts)
			{
				bodyPart.layer = 9; //See layer for the number
				setLayerToChildren(bodyPart, 9);
			}
			Debug.Log (bodyParts);
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
