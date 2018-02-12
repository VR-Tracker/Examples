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
    public bool[] openDoors = new bool[5];          //Which doors should be opened during this wave
}

public class WaveManager : NetworkBehaviour
{

    public static WaveManager instance;             //The instance of the Wave Manager

    public List<Wave> waveList;                     //The List of zombie waves in order
    public int countdownTime = 5;                   //The duration of the countdown before waves
    public int intermissionTime = 10;               //The duration of the pause between waves

    private int currentWave;                        //The index of the current wave
    private EnemySpawner ZSpawner;                 //The Zombie Spawner in charge of spawning the zombies
    private Announcer announcer;                    //The announcer in charge of showing messages to the player
    private MusicManager musicManager;              //The music manager in charge of the game music
    private AudioManager audioManager;              //The audio manager in charge of game sounds
    //private DoorManager doorManager;                //The door manager in charge of opening and closing doors
    private DoOnMainThread mainThread;              //Calls functions on the main thread



    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one MusicManager in the scene");
        }
        else
        {
            instance = this;
        }
    }

    public void Start () {
        //Adds itself to the observers list of the VRTracker instance
        //base.Start();

        ZSpawner = GetComponent<EnemySpawner>();
        announcer = GetComponent<Announcer>();

        /*doorManager = DoorManager.instance;
        if (doorManager == null)
        {
            Debug.LogWarning("Could not find the doorManager!");
        }*/

        musicManager = MusicManager.instance;
        if(musicManager == null)
        {
            Debug.LogWarning("Could not find the musicManager!");
        }

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("no audioManager in the scene");
        }
        mainThread = DoOnMainThread.instance;
        currentWave = 0;
	}


    private void Update()
    {
        if(!VerifyTimer() || !VerifyZombie())
        {
            ManageEndOfWave();
        }
    }

    /// <summary>
    /// Coordinates the transition in between waves
    /// </summary>
    private void ManageEndOfWave()
    {
        //PLay the bell sound
        audioManager.playSound("Bell");

        //Change the gameState to "intermission"
        musicManager.ChangeState(MusicManager.States.InterMission);

        //Stop the game timer
        GameTimer.instance.StopTimer();

        //Close all the doors
        //doorManager.CloseAllDoors();

        //Disable all the spawn points
        ZSpawner.DisableSpawnPoints();

        //Make the respawn point apear for dead players
        //TagsManager.instance.EnableSpawn();

        //Try to make a healthpack appear
        PickupSpawner.instance.TryHealthSpawn();

        //Start the next wave
        StartCoroutine(WaitForWellDone(3f));
    }

    /// <summary>
    /// Starts the zombie wave at the index of currentWave
    /// </summary>
    public void NextWave()
    {
        if(currentWave < waveList.Count)
        {
            DoOnMainThread.ExecuteOnMainThread.Enqueue(() => {  StartWarning(); } );
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
        if (currentWave < waveList.Count)
        {
            //Show the message to the player
            announcer.AddMessage("Prepare for Wave " + (currentWave + 1));

            //Wait for "intermissionTime" seconds
            yield return new WaitForSeconds(intermissionTime);

            //Open the adequate doors for the wave
            OpenDoors();

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
        if(time > 0)
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
            //when the countdown is over, start the zombie wave
            StartWave();
        }
    }

    /// <summary>
    /// Starts a zombie wave
    /// </summary>
    private void StartWave()
    {
        //Set the timer to the correct duration and start it
        GameTimer.instance.SetTimer(waveList[currentWave].duration);
        GameTimer.instance.StartTimer();

        //Set the spawnRate and quantity of the zombie spawner
        ZSpawner.SetSpawnRate(waveList[currentWave].spawnRate);
        ZSpawner.SpawnWave(waveList[currentWave].quantity);
		ZSpawner.setWave (currentWave);
        //Increment the current wave index
        currentWave++;

        //Change the game state of the music manager to "wave"
        musicManager.ChangeState(MusicManager.States.Wave);
    }

    /// <summary>
    /// Verifies if all the zombies have been killed by the player(s)
    /// </summary>
    /// <returns></returns>
    private bool VerifyZombie()
    {
        if (!ZSpawner.isSpawning && musicManager.state == MusicManager.States.Wave && GameObject.FindWithTag("Enemy") == null)
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
        if(musicManager.state == MusicManager.States.Wave && GameTimer.instance.time == 0)
        {
            Debug.Log("Stahp");
            //EnemyManager.instance.ClearZombies();
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
    private void OpenDoors()
    {
        for(int i = 0; i< waveList[currentWave].openDoors.Length; i++)
        {
            if (waveList[currentWave].openDoors[i])
            {
                //doorManager.SetOpenDoor(i);
                ZSpawner.EnableSpawnPoint(i);
            }
        }
    }

    public void Restart()
    {
        currentWave = 0;
        NextWave();
    }

    public void SendStopMessage(){
        VRTracker.instance.SendMessageToGateway("cmd=specialdata&function=stopwave");
    }

    public void Stop()
    {
        audioManager.playSound("Bell");
        musicManager.ChangeState(MusicManager.States.InterMission);
        //doorManager.CloseAllDoors();
        ZSpawner.DisableSpawnPoints();
        //EnemyManager.instance.ClearZombies();
        GameTimer.instance.StopTimer();
    }

    public void Notify(string message)
    {
        string[] datas = message.Split('&');
        string[] infos = datas[1].Split('=');
        if (infos[1] == "gamestart")
        {
            NextWave();
        }
        else if(infos[1] == "stopwave")
        {
            DoOnMainThread.ExecuteOnMainThread.Enqueue(() => { Stop(); });
        }
    }

}
