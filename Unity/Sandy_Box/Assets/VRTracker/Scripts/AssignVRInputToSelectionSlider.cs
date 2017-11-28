using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRStandardAssets.Utils
{
	
	public class AssignVRInputToSelectionSlider : MonoBehaviour {

		// VRTRacker : search for inputs and link (as objects are spawned)
		private void OnEnable () {
			Invoke("linkVRInput", 2);
		}

		private void linkVRInput(){
			VRInput[] inputs = FindObjectsOfType(typeof(VRInput)) as VRInput[];
			foreach (VRInput input in inputs) {
			/* TODO UNDO	if (input.transform.parent.GetComponent<VRTrackerTag> ().isLocalPlayer) {
					if (transform.GetComponent<SelectionSlider> () != null) {
						transform.GetComponent<SelectionSlider> ().setVRInput(input);
						Debug.Log ("Linked");
					}
				}*/
			}
		}
		
		// Update is called once per frame
		void Update () {
			
		}
	}

}