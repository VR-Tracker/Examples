using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace CompleteProject
{
    public class EnemyMovement : NetworkBehaviour
    {
        Transform player;               // Reference to the player's position.
        PlayerHealth playerHealth;      // Reference to the player's health.
        EnemyHealth enemyHealth;        // Reference to this enemy's health.
        [SyncVar]                          //Synchronize on the network the health bar
        UnityEngine.AI.NavMeshAgent nav;               // Reference to the nav mesh agent.
        public bool isAttacking = false;                        // If the zombie is currently attacking

        [SerializeField] private List<Transform> targets;       // The transform of the zombie's meal
        private float distance;                                 // The distance between the zombie and its target
        private int refreshCount = 0;
        

        void Awake ()
        {
            // Set up the references.
            player = GameObject.FindGameObjectWithTag ("Player").transform;
            playerHealth = player.GetComponent <PlayerHealth> ();
            enemyHealth = GetComponent <EnemyHealth> ();
            nav = GetComponent <UnityEngine.AI.NavMeshAgent> ();
            //Added
            if (Network.isServer)
            {
                SetAllTargets();
            }
        }


        void Update ()
        {
            // If the enemy and the player have health left...
            if(enemyHealth.currentHealth > 0 && playerHealth.currentHealth > 0)
            {
                // ... set the destination of the nav mesh agent to the player.
                // nav.SetDestination (player.position);
                if(refreshCount == 10)
                {
                    Transform chosenTarget = FindClosestTarget();
                    if (chosenTarget != null)
                    {
                        Vector3 ChosenPosition = new Vector3(chosenTarget.position.x, 0f, chosenTarget.position.z);
                        nav.SetDestination(ChosenPosition);
                    }
                    refreshCount = 0;
                }
                else
                {
                    refreshCount++;
                }
                
            }
            // Otherwise...
            else
            {
                // ... disable the nav mesh agent.
                nav.enabled = false;
            }
        }
      
        public void SetAllTargets()
        {
            targets.Clear();
            foreach (GameObject player in VRTrackerNetwork.instance.players)
            {
                if (player != null)
                {
                    if (!player.GetComponent<PlayerHealth>().isDead)
                        targets.Add(player.transform.Find("Player"));
                }
            }
        }

        private Transform FindClosestTarget()
        {
            if (targets.Count == 0)
            {
                Debug.Log("No player");
                return null;
            }

            float distanceTemp;
            Transform chosenTransform = targets[0];
            distance = Vector3.Distance(this.transform.position, targets[0].position);
            if (targets.Count > 1)
            {
                for (int i = 1; i < targets.Count; i++)
                {
                    distanceTemp = Vector3.Distance(this.transform.position, targets[i].position);
                    if (distanceTemp < distance)
                    {
                        distance = distanceTemp;
                        chosenTransform = targets[i];
                    }
                }
            }
            return chosenTransform;
        }

        void OnChangeTarget(Vector3 ChosenPosition)
        {
            nav.SetDestination(ChosenPosition);
        }

    }
}