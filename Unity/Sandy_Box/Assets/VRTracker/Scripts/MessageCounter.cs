using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageCounter : MonoBehaviour {


    public static MessageCounter instance;                  //the instance of the MessageCounter singleton

    [SerializeField]
    private float average;                 //the average number of messages received per second (mps)

    public int tickTime = 3;                                //the time to wait (period) between each averages

    private int messageNumber = 0;                          //the number of messages received during the current period
    private float time;                                     //the curent time
    public Text m_Text;                             // Reference to the component that displays the mps.

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
        //m_Text = GetComponent<Text>();
    }

    void Update()
    {
        //increment the current time
        time += Time.deltaTime;
        if (time >= tickTime)
        {
            //show the mps on the console
            ShowAverage(CalculateAverage());
        }
    }

    /// <summary>
    /// calculate the mps
    /// </summary>
    /// <returns>the average mps</returns>
    private float CalculateAverage()
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
    /// show the mps on the virtual console
    /// </summary>
    /// <param name="newAverage">the new mps</param>
    private void ShowAverage(float newAverage)
    {
        m_Text.text = newAverage.ToString();
    }

    /// <summary>
    /// increment the message counter
    /// </summary>
    public void AddMessage()
    {
        messageNumber++;
    }
}
