using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class SpawnPoint
{
    public bool enabled = false;
    public Vector3 position;
}

public class EnemySpawner : NetworkBehaviour
{

    public GameObject zombieTemplate;
    public List<SpawnPoint> spawnPoints;
    public bool isSpawning = false;

    private bool isWaiting = false;
    private float spawnRate = 5f;

    private int zombieIndex = 0;
    private int currentWave = 0;


    public void Start()
    {

    }

    public void SetSpawnRate(float newRate)
    {
        spawnRate = newRate;
    }

    private void SpawnZombie(Vector3 position, int currentWave = 0)
    {
        if (this != null)
        {
            GameObject newZombie = Instantiate(zombieTemplate, position, transform.rotation);
            //Retrieve the health component
            //ZombieHealth zombieHealth = newZombie.GetComponent(typeof(ZombieHealth)) as ZombieHealth;
            //Update zombie health depending on the wave index
            //zombieHealth.setHealth(currentWave);
            //ZombieManager.instance.AddZombie(newZombie, "zombie" + zombieIndex++);
        }
    }

    public void SpawnWave(int zombiesNumber)
    {
        if (zombiesNumber > 0)
        {
            isSpawning = true;
            if (Network.isServer)
            {
                Vector3 positon = spawnPoints[DetermineSpawnPt()].position;
                VRTracker.instance.SendMessageToGateway("cmd=specialdata&function=spawnzombie&posx=" + positon.x + "&posy=" + positon.y + "&posz=" + positon.z);
            }
            StartCoroutine(WaitForSpawn(zombiesNumber));
        }
        else
        {
            isSpawning = false;
        }
    }

    private int DetermineSpawnPt()
    {
        int randomIndex = Random.Range(0, spawnPoints.Count);
        while (!spawnPoints[randomIndex].enabled)
        {
            randomIndex = Random.Range(0, spawnPoints.Count);
        }
        return randomIndex;
    }

    IEnumerator WaitForSpawn(int number)
    {
        isWaiting = true;
        yield return new WaitForSeconds(spawnRate);
        isWaiting = false;
        SpawnWave(--number);
    }

    public void EnableSpawnPoint(int index)
    {
        spawnPoints[index].enabled = true;
    }

    public void DisableSpawnPoints()
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            spawnPoints[i].enabled = false;
        }
    }

    public void setWave(int waveNumber)
    {
        currentWave = waveNumber;
    }
}
