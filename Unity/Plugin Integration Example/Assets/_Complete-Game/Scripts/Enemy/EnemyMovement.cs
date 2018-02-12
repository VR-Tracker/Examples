using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CompleteProject
{
    public class EnemyMovement : MonoBehaviour
    {
        Transform player;               // Reference to the player's position.
        PlayerHealth playerHealth;      // Reference to the player's health.
        EnemyHealth enemyHealth;        // Reference to this enemy's health.
        UnityEngine.AI.NavMeshAgent nav;               // Reference to the nav mesh agent.
        public bool isAttacking = false;                        // If the zombie is currently attacking

        [SerializeField] private List<Transform> targets;       // The transform of the zombie's meal
        private float distance;                                 // The distance between the zombie and its target
    
        void Awake ()
        {
            // Set up the references.
            player = GameObject.FindGameObjectWithTag ("Player").transform;
            playerHealth = player.GetComponent <PlayerHealth> ();
            enemyHealth = GetComponent <EnemyHealth> ();
            nav = GetComponent <UnityEngine.AI.NavMeshAgent> ();
            //Added
            SetAllTargets();

        }


        void Update ()
        {
            // If the enemy and the player have health left...
            if(enemyHealth.currentHealth > 0 && playerHealth.currentHealth > 0)
            {
                // ... set the destination of the nav mesh agent to the player.
                // nav.SetDestination (player.position);
                Transform chosenTarget = FindClosestTarget();
                if (chosenTarget != null)
                {
                    Vector3 ChosenPosition = new Vector3(chosenTarget.position.x, 0f, chosenTarget.position.z);
                    nav.SetDestination(ChosenPosition);
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
            foreach (Player player in TagsManager.instance.players)
            {
                if (!player.isDead && !player.isSpectator)
                {
                    GameObject playerObject = TagsManager.instance.getPlayerObject(player.ID);
                    if (playerObject != null)
                        targets.Add(playerObject.transform.Find("PlayerHead"));
                }
            }
        }

        private Transform FindClosestTarget()
        {
            if (targets.Count == 0)
            {
                Debug.Log("oy");
                return null;
            }

            float distanceTemp;
            Transform chosenTransform = targets[0];
            //Debug.Log (chosenTransform.name + ": " + chosenTransform.position);
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

    }
}