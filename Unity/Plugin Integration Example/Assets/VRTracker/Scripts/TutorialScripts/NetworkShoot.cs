using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkShoot : NetworkBehaviour {


    private CompleteProject.PlayerShooting shootingScript;
    public VRTrackerTag vrGun;
    private CompleteProject.PlayerHealth playerHealth;

    // Use this for initialization
    void Start () {
        shootingScript = GetComponentInChildren<CompleteProject.PlayerShooting>();
        playerHealth = gameObject.GetComponent<CompleteProject.PlayerHealth>();
        /*if(shootingScript.vrGun != null)
        {
            // Callback for Local layer, not server
            shootingScript.vrGun.OnDown += CmdShoot;
        }*/
        if(vrGun != null)
        {
            vrGun.OnDown += CmdShoot;

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
        if(!playerHealth.isDead)
            shootingScript.Shoot();
    }

    
}
