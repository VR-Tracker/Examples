using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRTrackerAssociation{
		public string tagID = "";
		public bool isWaitingForAssignation = false;
		public bool isIDAssigned = false;

		public VRTrackerAssociation(){

		}

		public void WaitForAssignation()
		{
			//Prepare for assignation
			isWaitingForAssignation = true;

		}

		public void assign(string tagUID){
			tagID = tagUID;
			isIDAssigned = true;
			isWaitingForAssignation = false;
		}
	}


