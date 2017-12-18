using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VRTrackerNetworking : NetworkBehaviour {
	private NetworkIdentity objNetId;

	public void disableGravity(GameObject obj){
		if (isLocalPlayer) {
			CmdDisableGravity (obj);
		}
	}

	public void enableGravity(GameObject obj, Vector3 velocity){
		if (isLocalPlayer) {
			CmdEnableGravity (obj, velocity);
		}
	}

	[Command]
	private void CmdDisableGravity(GameObject obj){
		Debug.Log ("Informing no gravity");
		if (obj != null) {
			Debug.Log (GetComponent<NetworkIdentity> ().GetType());
			Debug.Log (connectionToClient);
			Debug.Log (GetComponent<NetworkIdentity> ().localPlayerAuthority);
            NetworkIdentity localPlayer = GetComponent<NetworkIdentity>();
            Debug.Log(localPlayer.localPlayerAuthority);
            Debug.Log(this);
            objNetId = obj.GetComponent<NetworkIdentity> ();
            localPlayer.localPlayerAuthority = true;
            if (localPlayer.localPlayerAuthority)
            {
                Debug.LogWarning("Local player has authority");
                objNetId.localPlayerAuthority = true;
                objNetId.AssignClientAuthority(localPlayer.connectionToClient); //Assign network authority to the current player
                RpcDisableGravity(obj);
                //objNetId.localPlayerAuthority = false;
                objNetId.RemoveClientAuthority(localPlayer.connectionToClient); //Remove network authority after action done
                objNetId.localPlayerAuthority = false;

            }
            else
            {
                Debug.LogWarning("Do not have authority");
            }

        }
	}

	[ClientRpc]
	public void RpcDisableGravity(GameObject obj){
		obj.GetComponent<Rigidbody> ().useGravity = false;
	}

	[Command]
	public void CmdEnableGravity(GameObject obj, Vector3 velocity){
		Debug.Log ("Informing gravity");
		if (obj != null) {
            NetworkIdentity localPlayer = GetComponent<NetworkIdentity>();
            Debug.Log (GetComponent<NetworkIdentity> ().connectionToClient);
			NetworkIdentity ni = GetComponent<NetworkIdentity> ();
            localPlayer.localPlayerAuthority = true;
            objNetId = obj.GetComponent<NetworkIdentity> ();
            objNetId.localPlayerAuthority = true;
            objNetId.AssignClientAuthority (GetComponent<NetworkIdentity>().connectionToClient); //Assign network authority to the current player
			RpcEnableGravity(obj, velocity);
			objNetId.RemoveClientAuthority (GetComponent<NetworkIdentity>().connectionToClient); //Remove network authority after action done
            objNetId.localPlayerAuthority = false;
        }
    }

	[ClientRpc]
	public void RpcEnableGravity(GameObject obj, Vector3 velocity){
		obj.GetComponent<Rigidbody> ().useGravity = true;
		obj.GetComponent<Rigidbody> ().velocity = velocity;
	}

}
