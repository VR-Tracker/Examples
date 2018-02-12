using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class Enemy
{
    public GameObject enemyPrefab;
    public int level;
    public int spawnTime;
}

[System.Serializable]
public class SpawnPoint
{
    public bool enabled = false;
    public GameObject spawPoint;
}


public class EnemySpawner : NetworkBehaviour
{

    public Enemy[] enemies;
    public List<SpawnPoint> spawnPoints;
    public bool isSpawning = false;

    private bool isWaiting = false;
    private float spawnRate = 5f;

    private int zombieIndex = 0;
    private int currentWave = 0;
    private PlayerHealth playerHealth;       // Reference to the player's heatlh.
    private List<GameObject> enemyList;


    public void Start()
    {
        enemyList = new List<GameObject>();
    }

    public void SetSpawnRate(float newRate)
    {
        spawnRate = newRate;
    }

    private void Spawn(int currentWave = 0)
    {
        if (this != null)
        {
            if (enemyList.Count < WaveManager.instance.waveList.Count)
            {
                // Find a random index between zero and one less than the number of spawn points.
                int spawnPointIndex = Random.Range(0, spawnPoints.Count);
                int enemyIndex = Random.Range(0, enemies.Length);

                var enemy = (GameObject)Instantiate(enemies[enemyIndex].enemyPrefab, spawnPoints[spawnPointIndex].spawPoint.transform.position, spawnPoints[spawnPointIndex].spawPoint.transform.rotation);

                enemyList.Add(enemy);
                NetworkServer.Spawn(enemy);
            }
           
        }
    }

    public void SpawnWave(int enemyNumber)
    {
        if (enemyNumber > 0)
        {
            isSpawning = true;
            if (isServer)
            {
                Spawn(currentWave);
            }
            else
            {
                Debug.Log("Not server");

            }
            StartCoroutine(WaitForSpawn(enemyNumber));
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
        yield return new WaitForSeconds(spawnRate);
        SpawnWave(--number);
    }

    public void SetWave(int waveNumber)
    {
        currentWave = waveNumber;
    }

    public void ClearEnemies()
    {
        if(enemyList != null)
        {
            foreach (GameObject enemy in enemyList)
            {
                if (enemy != null)
                {
                    NetworkServer.Destroy(enemy);
                }
            }
            enemyList.Clear();
        }

    }
    
}
