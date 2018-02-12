using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MyRespawner : MonoBehaviour {

    public GameObject respawnPoint;

    private Transform myHead;
    private CompleteProject.PlayerHealth playerHealth;



	// Use this for initialization
	void Start () {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        myHead = transform.Find("PlayerHead");
        playerHealth = GetComponent<CompleteProject.PlayerHealth>();
    }

	void Update () {
        if (VerifyPosition() && !respawnPoint.GetComponent<Objective>().reached && playerHealth.isDead)
        {
            respawnPoint.GetComponent<Objective>().ActivateObjective();
            //GetComponent<vp_PlayerRespawner>().Respawn();
            GetComponent<ScoreScript>().setScore(0);
            //Camera.main.GetComponent<GrayscaleEffect>().enabled = false;
        }
	}

    private bool VerifyPosition()
    {
        if(respawnPoint != null && respawnPoint.activeSelf)
        {
            if (Mathf.Abs(myHead.position.x - respawnPoint.transform.position.x) > 0.4)
            {
                return false;
            }
            else if (Mathf.Abs(myHead.position.z - respawnPoint.transform.position.z) > 0.4)
            {
                return false;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetActiveSpawnPoint( bool isActive)
    {
        respawnPoint.SetActive(isActive);
        respawnPoint.GetComponent<Objective>().EnableSpawnPoint();
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        respawnPoint = GameObject.Find("Objective").transform.Find("RespawnPoint").gameObject;
    }
}
