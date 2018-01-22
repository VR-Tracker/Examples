using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRStandardAssets.Utils;

public class SliderGroup : MonoBehaviour {

    public SelectionSlider[] sliders;
    public bool watching = false;

    private bool barFilled = false;

    public IEnumerator WaitForBarsToFill()
    {
        watching = false;
        for(int i = 0; i < sliders.Length; i++)
        {
            StartCoroutine(WaitForBarToFill(sliders[i]));
        }
        while (!barFilled)
        {
            yield return null;
        }
        barFilled = false;
    }


    private IEnumerator WaitForBarToFill(SelectionSlider slider)
    {
        yield return (StartCoroutine(slider.WaitForBarToFill()));
        barFilled = true;
        //Check if ask for assignation
        if(slider.name == "TagAssignationSlider")
        {
            //We fade the assignation steps and remove the autoassignation button
            VRTracker.instance.autoAssignation = false;
            VRTracker.instance.assignationComplete = false;
        }
    }
    /// <summary>
    /// Hide the button i'm ready if auto assignation is not possible
    /// </summary>
    public void hideSkipAssignationSlider()
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            if (sliders[i].name == "CompleteSelectionSlider")
            {
                sliders[i].gameObject.SetActive(false);
            }
        }
    }
}
