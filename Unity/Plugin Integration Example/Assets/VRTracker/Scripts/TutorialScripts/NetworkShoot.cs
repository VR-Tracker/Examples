using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkShoot : NetworkBehaviour {


    private CompleteProject.PlayerShooting shootingScript;
    //public VRTrackerTag vrGun;

	// Use this for initialization
	void Start () {
        shootingScript = GetComponentInChildren<CompleteProject.PlayerShooting>();
        if(shootingScript.vrGun != null)
        {
            // Callback for Local layer, not server
            shootingScript.vrGun.OnDown += CmdShoot;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // Execute the shoot on server
    [Command]
    void CmdShoot()
    {
        // Execute functions linked to this action
        //shootingScript.Shoot();
    }

    
}
