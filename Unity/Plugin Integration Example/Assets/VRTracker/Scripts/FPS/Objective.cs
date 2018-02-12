// This class control the behaviour of an objective object in the scene
// Last modified: 2017-07-14

using UnityEngine;

public class Objective : MonoBehaviour {

    public bool reached = false;                //If the objective has been reached

    private GameObject destruction;             //The object that contains the light and particle system of the destruction animation
    private GameObject spotlight;               //The spotlight that starts at the objective and goes up
    private GameObject particleSys;             //The constant particle system that emanates from the objective
    private GameObject icon;                    //The 2D icon that floats above the obective

    private bool isFlashing = false;            //If the destruction light is getting brighter
    private bool isFading = false;              //If the destruction light is fading away

    //Called at the object's instantiation
    void Start () {
        //Finds the various gameObjects needed by the class

        destruction = transform.Find("Destruction").gameObject;
        spotlight = transform.Find("Spotlight").gameObject;
        particleSys = transform.Find("Particles").gameObject;
        icon = transform.Find("Icon").gameObject;
    }
	
	// Update is called once per frame
	void Update () {
		if (isFlashing)
        {
            //Progressively makes the destruction light brighter
            destruction.GetComponent<Light>().intensity += 0.1f;
            if(destruction.GetComponent<Light>().intensity >= 2)
            {
                //Switch to fading mode
                isFlashing = false;
                isFading = true;
            }
        }
        if (isFading)
        {
            //Progressively fades the destruction light
            destruction.GetComponent<Light>().intensity -= 0.05f;
            if (destruction.GetComponent<Light>().intensity <= 0)
            {
                //Disable the objective
                isFading = false;
                this.gameObject.SetActive(false);
            }
        }
    }

    //Disable the various parts of the objective GameObject
    void Disapear()
    {
        spotlight.GetComponent<Light>().enabled = false;
        icon.SetActive(false); ;
        particleSys.GetComponent<ParticleSystem>().Stop();
        this.GetComponent<MeshRenderer>().enabled = false;
    }

    //Starts the destruction flash
    private void Flash()
    {
        isFlashing = true;
    }

    //Starts the destruction particle animation
    private void ParticleSplash()
    {
        destruction.GetComponent<ParticleSystem>().Play();
    }

    //Activates the various parts of the objective GameObject
    public void EnableSpawnPoint()
    {
        icon.SetActive(true);
        spotlight.SetActive(true);
        spotlight.GetComponent<Light>().enabled = true;
        particleSys.SetActive(true);
        particleSys.GetComponent<ParticleSystem>().Play();
        this.GetComponent<MeshRenderer>().enabled = true;
        reached = false;
    }

    //Called when the objective is reached. Starts the destruction procedure
    public void ActivateObjective()
    {
        Flash();
        ParticleSplash();
        Disapear();
        reached = true;
    }
}
