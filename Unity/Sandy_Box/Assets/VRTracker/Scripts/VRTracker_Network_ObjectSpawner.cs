using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VRTracker_Network_ObjectSpawner : NetworkBehaviour {

	public static VRTracker_Network_ObjectSpawner instance;

	private void Awake()
	{
		if (!isLocalPlayer)
			return;
		
		if (instance != null)
		{
			Debug.LogError("More than one VRTracker_Network_ObjectSpawner in the scene");
		}
		else
		{
			instance = this;
		}
	}
		
	public void Spawn(GameObject obj){
		CmdSpawn (obj);
	}

	public void Delete(GameObject obj){
		CmdDelete (obj);
	}

	[Command]
	void CmdSpawn(GameObject obj)
	{
		Debug.Log ("Spawn " + obj.name);

		if (connectionToClient == null)
			Debug.Log ("NO connectionToClient");
		NetworkServer.SpawnWithClientAuthority(obj, connectionToClient);
	}

	[Command]
	void CmdDelete(GameObject obj)
	{
		Debug.Log ("Delete " + obj.name);
		NetworkServer.Destroy (obj);
	}
}
