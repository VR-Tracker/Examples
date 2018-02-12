using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* VR Tracker
 * Ce script doit etre attaché a un objet ayant un Rigidbody dont on veut qu'il soit kinematic
 * Le kinematic sera activé à la création de l'objet s'il n'est pas en collision avec d'autres. 
 * S'il est en collision, on attendra qu'il n'y ait plus de collision.
 * Cela evite de créer un choc si on tente de spawn un object dans un autre.
 */
public class EnableKinematicWhenCollisionOff : MonoBehaviour {

	// Declare and initialize a new List of GameObjects called currentCollisions.
	List <GameObject> currentCollisions = new List <GameObject> ();

	// Use this for initialization
	void Start () {
//		Debug.Log ("Collision Count : " + currentCollisions.Count.ToString());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter (Collision col) {
		Debug.Log ("Collision with " + col.gameObject.name);

		// Add the GameObject collided with to the list.
		currentCollisions.Add (col.gameObject);
	}

	void OnCollisionExit (Collision col) {

		// Remove the GameObject collided with from the list.
		currentCollisions.Remove (col.gameObject);
	}
}
