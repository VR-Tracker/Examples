// Manages the timer at the top left of the HUD
// Last modified: 2017-07-27

using UnityEngine;

public class GameTimer : MonoBehaviour {

    public static GameTimer instance;               //The instance of the timer

    public GameObject timerTemplate;                //The template of the game Timer object
    public float time;                              //The current time shown by the timer

    private TextMesh timer;                         //The textMesh of the timer
    private bool isCounting = false;                //If the Timer is currently counting
    private GameObject cam;                         //The main camera of the scene

    //create the single instance of the game timer
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one GameTimer in the scene");
        }
        else
        {
            instance = this;
        }
    }

    //Use this to initialise
    void Start () {
        cam = Camera.main.gameObject;
        SetTimer(0f);
        if(cam != null)
        {
            timer = Instantiate(timerTemplate, this.transform).GetComponent<TextMesh>();
        }
    }
	
	// Update is called once per frame
	void Update () {

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

        //Changes the format and color of the timer depending on its value
        if(time > 10)
        {
            timer.text = time.ToString("F0");       //0 digit of precision
            timer.color = new Color(1f, 1f, 1f);    //Color: white
        }
        else if(time > 5)
        {
            timer.text = time.ToString("F1");       //1 digit of precision
            timer.color = new Color(1f, 0.5f, 0.5f);//Color: pink
        }
        else
        {
            timer.text = time.ToString("F2");       //2 digit of precision
            timer.color = new Color(1f, 0f, 0f);    //Color: red
        }
        
    }

    /// <summary>
    /// Gives a new value to the timer
    /// </summary>
    /// <param name="newTime"></param>
    public void SetTimer(float newTime)
    {
        time = newTime;
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
        SetTimer(0f);
    }

    /// <summary>
    /// Gives a new parent transform to the game timer
    /// </summary>
    /// <param name="newParent"></param>
    public void UpdateTimerObject(Transform newParent)
    {
        timer.transform.SetParent(newParent);
    }
}
