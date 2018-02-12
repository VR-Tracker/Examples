namespace VRTK
{
	
	using UnityEngine;
	using UnityEngine.Networking;

	[RequireComponent(typeof(VRTK_InteractableObject))]
	[RequireComponent(typeof(NetworkIdentity))]
	public class VRTracker_VRTK_Network_Grab : MonoBehaviour {

		private VRTK_InteractableObject interact;
		private NetworkIdentity networkId;
		private VRTracker_Network_ObjectSpawner networkObjSpawner;

		protected void OnEnable()
		{
			networkId = GetComponent<NetworkIdentity> ();

			if (interact == null)
				interact = GetComponent<VRTK_InteractableObject> ();
			
			interact.InteractableObjectGrabbed += new InteractableObjectEventHandler(DoObjectGrabbed);
			interact.InteractableObjectUngrabbed += new InteractableObjectEventHandler(DoObjectUngrabbed);
		}

		protected void OnDisable()
		{
			interact.InteractableObjectGrabbed -= new InteractableObjectEventHandler(DoObjectGrabbed);
			interact.InteractableObjectUngrabbed -= new InteractableObjectEventHandler(DoObjectUngrabbed);
		}

		protected virtual void DoObjectGrabbed(object sender, InteractableObjectEventArgs e)
		{
		//	Debug.Log ("DoObjectGrabbed " + gameObject.name);
			if (networkObjSpawner && networkId)
				networkObjSpawner.SetAuth (networkId.netId);

		}

		protected virtual void DoObjectUngrabbed(object sender, InteractableObjectEventArgs e)
		{
		//	Debug.Log ("DoObjectUngrabbed " + gameObject.name);
			if (networkObjSpawner && networkId)
				networkObjSpawner.RemAuth (networkId.netId);
		}

		// Update is called once per frame
		void Update () {

			if(networkObjSpawner == null){

				if(VRTracker.instance && VRTracker.instance.GetLocalPlayer() != null){
					GameObject localPlayer = VRTracker.instance.GetLocalPlayer ();
					networkObjSpawner = localPlayer.GetComponent<VRTracker_Network_ObjectSpawner> ();
				}
			}
		}
	}
}
