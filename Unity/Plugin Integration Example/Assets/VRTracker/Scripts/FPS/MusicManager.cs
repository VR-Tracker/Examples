using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    public static MusicManager instance;

    public enum States { InterMission, Wave, Action };
    public States state;

    private string currentSong;
    private string nextSong;
    private AudioManager audioManager;
    private bool inTransition = false;
    private float transitionSpeed = 0.0005f;

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

    // Use this for initialization
    void Start () {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("no audioManager in the scene");
        }
        ChangeState(States.InterMission);
    }
	
	// Update is called once per frame
	void Update () {
        if (!audioManager.IsPlaying("ThunderStorm"))
        {
            audioManager.playSound("ThunderStorm");
        }
        if (inTransition)
        {
            if (currentSong == null)
            {
                audioManager.ChangeVolume(nextSong, 0.2f);
                currentSong = nextSong;
                inTransition = false;
            }
            else
            {
                if (audioManager.HasVolume(currentSong))
                {
                    audioManager.ChangeVolume(currentSong, -transitionSpeed);
                    audioManager.ChangeVolume(nextSong, transitionSpeed);
                }
                else
                {
                    audioManager.StopSound(currentSong);
                    currentSong = nextSong;
                    inTransition = false;
                }
            }
        }
    }

    public void ChangeState(States newState)
    {
        state = newState;
        switch (state)
        {
            case States.InterMission:
                transitionSpeed = 0.005f;
                MakeTransition("ShadowlandsMachine");
                break;
            case States.Action:
                transitionSpeed = 0.001f;
                MakeTransition("Mechanolith");
                break;
            case States.Wave:
                transitionSpeed = 0.0005f;
                MakeTransition("ControlledChaos");
                break;
            default:
                break;
        }

    }

    private void MakeTransition(string newSong)
    {
        if (!inTransition)
        {
            inTransition = true;
            audioManager.playSound(newSong, 0f);
            nextSong = newSong;
        }
        else
        {
            audioManager.ChangeVolume(nextSong, 0.2f);
            currentSong = nextSong;
            audioManager.playSound(newSong, 0f);
            nextSong = newSong;
        }
        
    }
}
