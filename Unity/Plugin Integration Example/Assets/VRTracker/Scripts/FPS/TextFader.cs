using UnityEngine;
using UnityEngine.UI;

public class TextFader : MonoBehaviour {

    public float duration = 1f;
    public float sizeVariation = 4f;

    private Text text;
    private float alpha = 0f;
    private float aimSize;

    private bool glowIn = false;
    private bool glowOut = false;

    private bool fadingIn = false;
    private bool fadingOut = false;

    private bool growingIn = false;
    private bool growingOut = false;

    void Start () {
        text = this.GetComponent<Text>();
        aimSize = text.fontSize;
        text.fontSize = (int) (aimSize - (sizeVariation / 2f));
        GlowIn();
        FadeIn();
        GrowIn();
	}

    private void FadeIn()
    {
        fadingIn = true;
    }
	
	private void GlowIn()
    {
        glowIn = true;
    }

    private void GrowIn()
    {
        growingIn = true;
    }

    private void Update()
    {
        ManageGlow();
        ManageFade();
        ManageSize();
    }

    private void ManageGlow()
    {
        /*
        if (glowIn)
        {
            text.fontMaterial.SetFloat(ShaderUtilities.ID_GlowOuter,
                                       text.fontMaterial.GetFloat(ShaderUtilities.ID_GlowOuter) + Time.deltaTime / duration);
            if (text.fontMaterial.GetFloat(ShaderUtilities.ID_GlowOuter) >= 1f)
            {
                glowIn = false;
                glowOut = true;
            }
        }
        else if (glowOut)
        {
            text.fontMaterial.SetFloat(ShaderUtilities.ID_GlowOuter,
                                       text.fontMaterial.GetFloat(ShaderUtilities.ID_GlowOuter) - Time.deltaTime / duration);
            if (text.fontMaterial.GetFloat(ShaderUtilities.ID_GlowOuter) <= 0f)
            {
                glowOut = false;
            }
        }*/
    }

    private void ManageFade()
    {
        if (fadingIn)
        {
            alpha += Time.deltaTime / duration;
            if (alpha >= 1f)
            {
                alpha = 1f;
                fadingIn = false;
            }
            Color oldColor = text.color;
            Color newColor = new Color(oldColor.r, oldColor.b, oldColor.g, alpha);
            text.color = newColor;
        }

        else if (fadingOut)
        {
            alpha -= Time.deltaTime / duration;
            if (alpha <= 0f)
            {
                alpha = 0f;
                fadingOut = false;
                Destroy(this);
            }
            Color oldColor = text.color;
            Color newColor = new Color(oldColor.r, oldColor.b, oldColor.g, alpha);
            text.color = newColor;
        }

    }

    private void ManageSize()
    {
        /*
        if (growingIn)
        {
            text.fontSize += ((Time.deltaTime / duration) * (3 * sizeVariation / 4));
            if(text.fontSize >= aimSize + (sizeVariation / 2))
            {
                growingIn = false;
                growingOut = true;
            }
        }
        else if (growingOut)
        {
            text.fontSize -= ((Time.deltaTime / duration) * (3 * sizeVariation / 4));
            if (text.fontSize <= aimSize)
            {
                text.fontSize = aimSize;
                growingOut = false;
            }
        }
        */
    }

    public void DestroyText()
    {
        fadingOut = true;
    }
}
