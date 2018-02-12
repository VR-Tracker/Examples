using UnityEngine;
using UnityEngine.Networking;

namespace CompleteProject
{
    public class EnemyManager : NetworkBehaviour
    {
        public PlayerHealth playerHealth;       // Reference to the player's heatlh.
        public GameObject enemyPrefab;                // The enemy prefab to be spawned.
        public float spawnTime = 3f;            // How long between each spawn.
        public Transform[] spawnPoints;         // An array of the spawn points this enemy can spawn from.
        public static EnemyManager instance;


        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("More than one TagsManager in the scene");
            }
            else
            {
                instance = this;
            }
        }

        void Start ()
        {


            // Call the Spawn function after a delay of the spawnTime and then continue to call after the same amount of time.
            InvokeRepeating ("Spawn", spawnTime, spawnTime);
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if(players.Length > 0)
            {
                Debug.Log("Number of player found " + players.Length);

                playerHealth = players[0].GetComponent<PlayerHealth>();
            }
            else
            {
                Debug.Log("No player found");
            }
        }

        void Update()
        {

            if(playerHealth == null)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                if (players.Length > 0)
                {
                    Debug.Log("Number of player found " + players.Length);

                    playerHealth = players[0].GetComponent<PlayerHealth>();
                }
                else
                {
                    Debug.Log("No player found");
                }
            }

        }

        void Spawn ()
        {
            // If the player has no health left...
            if(playerHealth.currentHealth <= 0f)
            {
                // ... exit the function.
                return;
            }

            // Find a random index between zero and one less than the number of spawn points.
            int spawnPointIndex = Random.Range (0, spawnPoints.Length);

            // Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
            var enemy = (GameObject)Instantiate(enemyPrefab, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
            NetworkServer.Spawn(enemy);
        }
    }
}