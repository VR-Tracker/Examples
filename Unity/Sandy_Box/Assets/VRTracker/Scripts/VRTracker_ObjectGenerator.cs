using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/* VR Tracker
 * Ce script prend une liste de prebaf contenant des modeles 3D et 
 * les genere (spawn) en utilisant les fonctions suivant / precedant. 
 * On peut ensuite deposer ces objets dans la scene avec la fonction drop
 * Ce script est generalement attaché à un controller et les fonctions appelées par 
 * des boutons ou radial menu sur le controller et permet de generer des objets devant 
 * le controlleur e tles placer dans la scene, comme une libraire dans laquelle on vient 
 * choisir
 */
public class VRTracker_ObjectGenerator : MonoBehaviour {

	public List<GameObject> library;
	private int index;
	private GameObject currentlyShowing;

	// Use this for initialization
	void Start () {
		index = Mathf.FloorToInt(library.Count / 2);
	}

	// Update is called once per frame
	void Update () {
		
		if (Input.GetKeyDown ("l")) {
			LoadPrevious ();
		}
		if (Input.GetKeyDown ("m")) {
			LoadNext ();
		}
		if (Input.GetKeyDown ("p")) {
			Drop ();
		}
	}

	public void LoadNextClick(){
		Debug.Log ("LoadNextClick" + index.ToString());
		LoadNext ();
	}

	public void LoadNext(){
		if (index >= library.Count - 1)
			return;
		
		if (currentlyShowing){
			if (VRTracker_Network_ObjectSpawner.instance != null)
				VRTracker_Network_ObjectSpawner.instance.Delete (currentlyShowing);
			else
				Object.Destroy (currentlyShowing);
		}
		index++;
		Debug.Log ("Load Next " + index.ToString());
		CreateInstance ();
	}

	public void LoadPreviousClick(){
		Debug.Log ("LoadPreviousClick" + index.ToString());
		LoadPrevious ();
	}

	public void LoadPrevious(){
		if (index <= 0)
			return;

		if (currentlyShowing){
			if (VRTracker_Network_ObjectSpawner.instance != null)
				VRTracker_Network_ObjectSpawner.instance.Delete (currentlyShowing);
			else
				Object.Destroy (currentlyShowing);
		}
			
		index--;
		Debug.Log ("Load Prev " + index.ToString());
		CreateInstance ();
	}

	public void Reload(){
		CreateInstance ();
	}

	public void Hide(){
		if (currentlyShowing){
			if (VRTracker_Network_ObjectSpawner.instance != null)
				VRTracker_Network_ObjectSpawner.instance.Delete (currentlyShowing);
			else
				Object.Destroy (currentlyShowing);
		}
	}

	public void DropClick(){
		Debug.Log ("Drop Click" + index.ToString());
		Drop ();
	}

	public void Drop(){

		if (!currentlyShowing)
			return;
		Debug.Log ("Drop " + index.ToString());
		if (currentlyShowing.GetComponent<NetworkIdentity> () && VRTracker_Network_ObjectSpawner.instance) {
			Debug.Log ("Removing Client Auth");
			currentlyShowing.GetComponent<NetworkIdentity> ().RemoveClientAuthority (VRTracker_Network_ObjectSpawner.instance.connectionToClient);
		}

	//	if (currentlyShowing.GetComponent<Rigidbody> ())
	//		currentlyShowing.GetComponent<Rigidbody> ().isKinematic = true; // Enable collisions on drop

		currentlyShowing.transform.parent = null;
		Reload ();
	}

	private void CreateInstance(){
		Debug.Log ("CreateInstance " + index.ToString());
		currentlyShowing = (GameObject)Instantiate(library[index], transform.position, Quaternion.identity);
		//	if (currentlyShowing.GetComponent<Rigidbody> ())
		//		currentlyShowing.GetComponent<Rigidbody> ().isKinematic = true; // Enable Kinematic when hold to avoid unwanted collision ?

		currentlyShowing.transform.parent = this.gameObject.transform;
		Vector3 offset = new Vector3 (0, 0, 0.2f);
		Vector3 nullrotation = new Vector3 (0, 0, 0);
		currentlyShowing.transform.localPosition = offset;
		currentlyShowing.transform.localRotation = Quaternion.Euler(nullrotation);
		if(VRTracker_Network_ObjectSpawner.instance != null)
			VRTracker_Network_ObjectSpawner.instance.Spawn (currentlyShowing);
	}
}
