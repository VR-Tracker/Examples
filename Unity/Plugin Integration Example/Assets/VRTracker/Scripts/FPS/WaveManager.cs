// Manages the waves of zombies during the game as well as the game state
// Last modified: 2017-07-27

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


//Contains information about a zombie wave
[System.Serializable]
public class Wave
{
    public int quantity = 0;                        //Number of zombies spawned during the wave
    public int duration = 0;                        //Duration of the wave
    public float spawnRate = 0f;                    //Time between the aparition of zombies
    //public bool[] openDoors = new bool[5];          //Which doors should be opened during this wave
}

public class WaveManager : NetworkBehaviour
{

    public static WaveManager instance;             //The instance of the Wave Manager

    public List<Wave> waveList;                     //The List of zombie waves in order
    public int countdownTime = 5;                   //The duration of the countdown before waves
    public int intermissionTime = 10;               //The duration of the pause between waves

    private int currentWave;                        //The index of the current wave
    private EnemySpawner ESpawner;                 //The Zombie Spawner in charge of spawning the zombies
    private Announcer announcer;                    //The announcer in charge of showing messages to the player
    //private MusicManager musicManager;              //The music manager in charge of the game music
    //private AudioManager audioManager;              //The audio manager in charge of game sounds
    //private DoorManager doorManager;                //The door manager in charge of opening and closing doors
    private DoOnMainThread mainThread;              //Calls functions on the main thread

    public float time = 0;                              //The current time shown by the timer
    private bool isCounting = false;                //If the Timer is currently counting


    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one Enemy Managewr in the scene");
        }
        else
        {
            instance = this;
        }
    }

    public void Start () {
        //Adds itself to the observers list of the VRTracker instance
        //base.Start();

        ESpawner = GetComponent<EnemySpawner>();
        announcer = GetComponent<Announcer>();
        currentWave = 0;
        NextWave();

    }


    private void Update()
    {
        if(!VerifyTimer() || !VerifyEnemy())
        {
            ManageEndOfWave();
        }
        if (isCounting)
        {
            //Make the timer count and stop if it reaches 0
            time -= Time.deltaTime;
            if (time <= 0)
            {
                time = 0;
                isCounting = false;
            }
        }
    }

    /// <summary>
    /// Coordinates the transition in between waves
    /// </summary>
    private void ManageEndOfWave()
    {
        //PLay the bell sound
        //audioManager.playSound("Bell");

        //Stop the game timer
        StopTimer();

        //Disable all the spawn points
        //ESpawner.DisableSpawnPoints();

        //Make the respawn point apear for dead players
        //TagsManager.instance.EnableSpawn();

        //Try to make a healthpack appear
        //PickupSpawner.instance.TryHealthSpawn();

        //Start the next wave
        StartCoroutine(WaitForWellDone(3f));
    }

    /// <summary>
    /// Starts the zombie wave at the index of currentWave
    /// </summary>
    public void NextWave()
    {
        Debug.Log("Next wave " + currentWave);

        if (currentWave < waveList.Count)
        {
           StartWarning();
        }
    }

    private void StartWarning()
    {
        StartCoroutine(WaitForCountDown());
    }

    /// <summary>
    /// waits for "intermissionTime" before starting the countdown
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForCountDown()
    {
        Debug.Log("Wave length " + waveList.Count);
        Debug.Log("counttime " + countdownTime);

        if (currentWave < waveList.Count)
        {
            //Show the message to the player
            announcer.AddMessage("Prepare for Wave " + (currentWave + 1));

            //Wait for "intermissionTime" seconds
            yield return new WaitForSeconds(intermissionTime);

            //start the countdown
            StartCountdown(countdownTime);
        }
    }


    /// <summary>
    /// Shows a countdown to the player
    /// </summary>
    /// <param name="time">duration of the countdown</param>
    private void StartCountdown(int time)
    {
        Debug.Log("Countdown " + time);

        if (time > 0)
        {
            //Show the message to the player
            if (announcer.hasMessage)
            {
                announcer.updateMessage("Wave " + (currentWave + 1) + " starting in: " + time);
            }
            else
            {
                announcer.AddMessage("Wave " + (currentWave + 1) + " starting in: " + time);
            }

            //Wait for a second before calling itself
            StartCoroutine(WaitForSecond(time, 1f));
        }
        else
        {
            //when the countdown is over, start the enmy wave
            if(currentWave < waveList.Count)
                StartWave();
        }
    }

    /// <summary>
    /// Starts a zombie wave
    /// </summary>
    private void StartWave()
    {
        Debug.Log("Start wave" + currentWave);

        //Set the timer to the correct duration and start it
        time = waveList[currentWave].duration;
        StartTimer();

        //Set the spawnRate and quantity of the zombie spawner
        ESpawner.SetSpawnRate(waveList[currentWave].spawnRate);
        ESpawner.SpawnWave(waveList[currentWave].quantity);
		ESpawner.SetWave (currentWave);
        //Increment the current wave index
        currentWave++;
       
    }

    /// <summary>
    /// Verifies if all the enemies have been killed by the player(s)
    /// </summary>
    /// <returns></returns>
    private bool VerifyEnemy()
    {
        if (!ESpawner.isSpawning && GameObject.FindWithTag("Enemy") == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Verifies if the game timer has reached zero
    /// </summary>
    /// <returns></returns>
    private bool VerifyTimer()
    {
        if(time == 0)
        {
            Debug.Log("Stop wave");
            ESpawner.ClearEnemies();
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Waits before showing a new countdown
    /// </summary>
    /// <param name="number">time currently shown</param>
    /// <param name="time"> tme to wait</param>
    /// <returns></returns>
    IEnumerator WaitForSecond(int number, float time)
    {
        yield return new WaitForSeconds(time);
        Debug.Log("new ocunttime " + number);

        StartCountdown(--number);
    }

    /// <summary>
    /// Tells the player the wave is finished, then calls for the next wave
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator WaitForWellDone(float time)
    {
        //Show the message to the player
        announcer.AddMessage("Wave Completed");
        yield return new WaitForSeconds(time);

        //Start the next wave
        NextWave();
    }

    /// <summary>
    /// Opens the doors of the wave
    /// </summary>
    /*private void OpenDoors()
    {
        for(int i = 0; i< waveList[currentWave].openDoors.Length; i++)
        {
            if (waveList[currentWave].openDoors[i])
            {
                //doorManager.SetOpenDoor(i);
                ESpawner.EnableSpawnPoint(i);
            }
        }
    }*/

    public void Restart()
    {
        currentWave = 0;
        NextWave();
    }

    public void Stop()
    {
        //audioManager.playSound("Bell");
        //doorManager.CloseAllDoors();
        //ESpawner.DisableSpawnPoints();
        //EnemyManager.instance.ClearZombies();
       StopTimer();
    }

    /// <summary>
    /// Makes the timer start counting
    /// </summary>
    public void StartTimer()
    {
        isCounting = true;
    }

    /// <summary>
    /// Makes the timer stop counting
    /// </summary>
    public void StopTimer()
    {
        isCounting = false;
        time = (0f);
    }

}
