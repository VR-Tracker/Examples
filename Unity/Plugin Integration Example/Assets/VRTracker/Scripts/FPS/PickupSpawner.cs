using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour {

    public static PickupSpawner instance;

    public GameObject healthPickupTemplate;
    public List<Vector3> spawnPositions;

	public float limitMinX = -1.2f;
	public float heightY = 0.5f;
	public float limitMinZ = -1.0f;
	public float limitMaxX = 2.4f;
	public float limitMaxZ = 4.0f;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one PickupSpawner in the scene");
        }
        else
        {
            instance = this;
        }
    }

    public void TryHealthSpawn()
    {
        /*if (!VerifyPlayerHealth())
            return;
            */
        SpawnPickup();
    }

    private void SpawnPickup()
    {
        Vector3 chosenPosition = ChoosePosition();
        SpawnPickupToPosition(chosenPosition);
    }

    /*
    private bool VerifyPlayerHealth()
    {
        foreach (GameObject player in TagsManager.instance.playerObjects)
        {
            if(player.GetComponent<vp_FPPlayerEventHandler>().Health.Get() < 1)
            {
                return true;
            }
        }
        return false;
    }*/

    private Vector3 ChoosePosition()
    {
        Vector3 chosenPosition = new Vector3();
        float distance = 0;
        foreach(Vector3 position in spawnPositions)
        {
            float distanceToClosest = 100f;
            /*foreach(GameObject player in TagsManager.instance.playerObjects)
            {
                float distanceToPlayer = Vector3.Distance(player.transform.position, position);
                if(distanceToPlayer < distanceToClosest)
                {
                    distanceToClosest = distanceToPlayer;
                }
            }*/
            if(distanceToClosest > distance)
            {
                distance = distanceToClosest;
                chosenPosition = position;
            }
        }
        return chosenPosition;
    }

    private void SpawnPickupToPosition( Vector3 pos)
    {
        Instantiate(healthPickupTemplate, pos, this.transform.rotation);
    }

	private Vector3 generateRamdomPosition(){
		float x;
		float y;
		float z;
		x = Random.Range(limitMinX, limitMaxX);
		y = heightY;
		z = Random.Range(limitMinZ, limitMaxZ);
		return new Vector3(x, y, z);
	}
}
