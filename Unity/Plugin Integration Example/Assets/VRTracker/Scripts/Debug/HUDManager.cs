using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour {

    public static HUDManager instance;                  //the instance of the MessageCounter singleton

    public Text FPSDisplay;// Reference to the component that displays the fps.
    public Text MPSDisplay;
                            
    public int tickTime = 3;                                //the time (period) in between each calculation 
    private float time;                                     //the curent time counter
    private int frameNumber = 0;                            //the number of frame in the current period
    private int messageNumber = 0;                          //the number of messages received during the current period
    [SerializeField]
    private float average;                 //the average number of messages received per second (mps)

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one MessageCounter in the scene");
        }
        else
        {
            instance = this;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    private void Update()
    {
        //increment the current number of frames and the current time
        frameNumber++;

        time += Time.deltaTime;

        if (time >= tickTime)
        {
            //calculate and shown the fps on the virtual console
            ShowAverageFPS(CalculateAverageFPS());
            ShowAverageMPS(CalculateAverageMPS());

        }

    }

    /// <summary>
    /// calculate the average fps of the game
    /// </summary>
    /// <returns>the average fps of the game</returns>
    private float CalculateAverageFPS()
    {
        float average = frameNumber / time;

        //reset the time and frame counter 
        time = 0;
        frameNumber = 0;

        return average;

    }

    /// <summary>
    /// calculate the mps
    /// </summary>
    /// <returns>the average mps</returns>
    private float CalculateAverageMPS()
    {
        average = messageNumber / time;

        //reset the time counter and the message counter
        time = 0;
        messageNumber = 0;

        //to keep the console simple, do not keep precision numbers
        float roundedAverage = Mathf.Round(average);
        return roundedAverage;
    }

    /// <summary>
    /// show the average fps on the virtual console
    /// </summary>
    /// <param name="newAverage">the new average fps</param>
    private void ShowAverageFPS(float newAverage)
    {
        FPSDisplay.text = Mathf.FloorToInt(newAverage).ToString();
    }

    /// <summary>
    /// show the mps on the virtual console
    /// </summary>
    /// <param name="newAverage">the new mps</param>
    private void ShowAverageMPS(float newAverage)
    {
        MPSDisplay.text = newAverage.ToString();
    }

    /// <summary>
    /// increment the message counter
    /// </summary>
    public void AddMessage()
    {
        messageNumber++;
    }
}
