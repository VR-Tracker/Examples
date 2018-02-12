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

	private VRTracker_Network_ObjectSpawner networkObjSpawner;

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {

		if(networkObjSpawner == null){

			if(VRTracker.instance && VRTracker.instance.GetLocalPlayer() != null){
				GameObject localPlayer = VRTracker.instance.GetLocalPlayer ();
				networkObjSpawner = localPlayer.GetComponent<VRTracker_Network_ObjectSpawner> ();
			}
		}
		
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


	public void LoadNext(){
		if (networkObjSpawner != null)
			networkObjSpawner.LoadNext(gameObject);
	}


	public void LoadPrevious(){
		if (networkObjSpawner != null)
			networkObjSpawner.LoadPrevious(gameObject);
	}

	public void Hide(){
		if (networkObjSpawner != null)
			networkObjSpawner.Hide ();
	}

	public void Drop(){
	if (networkObjSpawner != null)
		networkObjSpawner.Drop();
	}
}
