using UnityEngine;
using System.Collections;

public class ShadowFlasher : MonoBehaviour {

    public static ShadowFlasher instance;
    public GameObject myShadow;
    public GameObject myRedFlash;

    private bool shadowFadingIn = false;
    private bool shadowFadingOut = false;
    private Material myShadowMaterial;
    private Color currentShadowColor;

    private bool redFadingIn = false;
    private bool redFadingOut = false;
    private Material myRedMaterial;
    private Color currentRedColor;


    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one ShadowFlasher in the scene");
        }
        else
        {
            instance = this;
        }
        myShadowMaterial = myShadow.GetComponent<MeshRenderer>().material;
        currentShadowColor = myShadowMaterial.color;

        myRedMaterial = myRedFlash.GetComponent<MeshRenderer>().material;
        currentRedColor = myRedMaterial.color;
        DontDestroyOnLoad(this);
    }
	
	// Update is called once per frame
	void Update () {
		if (shadowFadingIn)
        {
            currentShadowColor.a += 0.02f;
            myShadowMaterial.color = currentShadowColor;

            if(currentShadowColor.a >= 1)
            {
                shadowFadingIn = false;
                Camera.main.GetComponent<GrayscaleEffect>().enabled = true;
                Announcer.instance.AddMessage("You Died");
                StartCoroutine(WaitForDark(1.5f));
            }
        }
        else if (shadowFadingOut)
        {
            currentShadowColor.a -= 0.02f;
            myShadowMaterial.color = currentShadowColor;

            if (currentShadowColor.a <= 0)
            {
                shadowFadingOut = false;
            }
        }

        if (redFadingIn)
        {
            currentRedColor.a += 0.06f;
            myRedMaterial.color = currentRedColor;
            if (currentRedColor.a >= 0.3)
            {
                redFadingIn = false;
                redFadingOut = true;
            }
        }else if (redFadingOut)
        {
            currentRedColor.a -= 0.06f;
            myRedMaterial.color = currentRedColor;
            if (currentRedColor.a <= 0)
            {
                redFadingOut = false;
            }
        }
	}

    public void ShadowBlink()
    {
        shadowFadingIn = true;
    }

    public void RedBlink()
    {
        redFadingIn = true;
    }

    public void SetNewShadow(GameObject plane)
    {
        myShadow = plane;
    }

    private IEnumerator WaitForDark(float time)
    {
        yield return new WaitForSeconds(time);
        shadowFadingOut = true;
    }
}
