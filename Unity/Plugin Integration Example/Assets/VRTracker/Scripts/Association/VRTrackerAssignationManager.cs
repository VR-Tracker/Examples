using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VRStandardAssets.Utils;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace VRStandardAssets.Intro
{
	// The intro scene takes users through the basics
	// of interacting through VR in the other scenes.
	// This manager controls the steps of the intro
	// scene.
	public class VRTrackerAssignationManager : MonoBehaviour
	{
		[SerializeField] private Reticle m_Reticle;                         // The scene only uses SelectionSliders so the reticle should be shown.
		[SerializeField] private SelectionRadial m_Radial;                  // Likewise, since only SelectionSliders are used, the radial should be hidden.
		[SerializeField] private UIFader m_HowToUseConfirmFader;            // Afterwards users are asked to confirm how to use sliders in this UI.
		[SerializeField] private SliderGroup m_SliderCroup;                 // They demonstrate this using this slider.
		[SerializeField] private UIFader m_AssociationFader;                // The final instructions are controlled using this fader.
		[SerializeField] private UIFader m_FailedCalibrationFader;
		[SerializeField] private SelectionSlider m_FailedCalibrationSlider;
		[SerializeField] private UIFader m_CalibrationCompleteFader;
		[SerializeField] private SelectionSlider m_CalibrationCompleteSlider;
		[SerializeField] private UIFader m_LoadingFader;

		private IEnumerator Start ()
		{
			m_Reticle.Show ();
			//m_Radial.Hide ();

			DontDestroyOnLoad (VRTracker.instance);
			Debug.Log("Instance is spectator :  " + VRTracker.instance.isSpectator);

            if (VRTracker.instance.autoAssignation)
            {
                Debug.Log("Loading association");
                VRTrackerTagAssociation.instance.LoadAssociation();
            }
            if (!VRTrackerTagAssociation.instance.isAssociationLoaded)
            {
                Debug.LogWarning("hide auto assingation");
                m_SliderCroup.hideSkipAssignationSlider();
            }



			GameObject pPrefab = VRTrackerNetwork.instance.playerPrefab;
            //Create and prepare the different prefab for assignation in the next scene
			if (pPrefab != null) {
				VRTrackerTag[] playerObject = pPrefab.GetComponentsInChildren<VRTrackerTag>();
				for (int i = 0; i < playerObject.Length; i++) {
					//Store the different player prefab name for association
					VRTrackerAssociation newAsso = new VRTrackerAssociation();
                    VRTrackerTagAssociation.instance.AddPrefabAssociation(playerObject[i].gameObject.name, newAsso);
				}
			}

			if (VRTracker.instance.autoAssignation)
			{
                yield return new WaitForSeconds(1);
				VRTracker.instance.assignDirectlyTags();
                if (!VRTracker.instance.assignationComplete)
                {
                    m_SliderCroup.hideSkipAssignationSlider();
                }
            }

			#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
				VRTracker.instance.isSpectator = false;
				Debug.Log("Setting spectator mode to  :  " + VRTracker.instance.isSpectator);
		
			#endif
			if (!VRTracker.instance.isSpectator)
			{
				m_SliderCroup.hideSpectatorMode();
                Debug.Log("Setting spectator mode to  :  " + VRTracker.instance.isSpectator);

            }
            else
            {
				//VRTracker.instance.assignationComplete = true;
			}
			Debug.Log("Instance is spectator :  " + VRTracker.instance.isSpectator);

            // In order, fade in the UI on confirming the use of sliders, wait for the slider to be filled, then fade out the UI.
            yield return StartCoroutine(m_HowToUseConfirmFader.InteruptAndFadeIn());
            yield return StartCoroutine(m_SliderCroup.WaitForBarsToFill());
            //yield return StartCoroutine(m_SliderCroup.WaitForBarsToFill());
            yield return StartCoroutine(m_HowToUseConfirmFader.InteruptAndFadeOut());


            // Assign a Tag to each Prefab instance containing a Tag in VR Tracker
            if (!VRTracker.instance.isAssigned() && !VRTracker.instance.isSpectator)
			{
				Debug.Log("Manual assignement ");
				//Assignement step
				//foreach (VRTrackerTag tagObject in VRTracker.instance.tags)
				foreach (KeyValuePair<string, VRTrackerAssociation> prefab in VRTrackerTagAssociation.instance.prefabAssociation)
				{

					Debug.Log("Assigning Tag to " + prefab.Key);

					bool associationFailed = true;
					while (associationFailed)
					{

						// Edit shown title to the Prefab name
						m_AssociationFader.transform.Find("CalibrationInstructions/Title").GetComponentInChildren<Text>().text = "Assign" + prefab.Key;

						// Start assignation
						yield return StartCoroutine(ShowMenu(m_AssociationFader, prefab.Value));

						// Check if timed out and throw an error
						if (!prefab.Value.isIDAssigned)
						{
							associationFailed = true;
							yield return StartCoroutine(ShowMenu(m_FailedCalibrationFader, m_FailedCalibrationSlider));
						}
						else
						{
							associationFailed = false;

						}
					}

				}
			}
			if(!VRTracker.instance.isSpectator)
			{
				VRTracker.instance.SaveAssociationTagUser();
				enablePlayerCameraForNextLevel();
			}
			// Load the next Level (the Game !)
			yield return StartCoroutine(m_LoadingFader.InteruptAndFadeIn());
			//LevelLoader.instance.LoadLevel(1);
			if (VRTracker.instance.serverIp == "") {
				VRTracker.instance.serverIp = Network.player.ipAddress;
				VRTracker.instance.sendServerIP (VRTracker.instance.serverIp);
                VRTrackerNetwork.instance.serverBindAddress = VRTracker.instance.serverIp;
                #if UNITY_EDITOR || UNITY_STANDALONE
                    Debug.LogWarning("Unity Editor");
				#endif

                if (VRTracker.instance.isSpectator)
                {
                    Debug.Log("Starting as Server");
                    VRTrackerNetwork.instance.StartServer();
                }
                else
                {
                    Debug.Log("Starting as Host");
                    VRTrackerNetwork.instance.StartHost();
                }
            } else {
                Debug.Log("Starting client");
                VRTrackerNetwork.instance.networkAddress = VRTracker.instance.serverIp;
				VRTrackerNetwork.instance.StartClient ();

                VRTrackerNetwork.instance.serverBindAddress = VRTracker.instance.serverIp;
                VRTrackerNetwork.instance.serverBindToIP = true;
                VRTrackerNetwork.instance.networkAddress = VRTracker.instance.serverIp;

                Debug.Log("Server Ip " + VRTrackerNetwork.instance.serverBindAddress);
                Debug.Log("Server Port " + VRTrackerNetwork.instance.networkPort);
                //after binding address, start server
                VRTrackerNetwork.instance.StartClient();

            }
        }

		private IEnumerator ShowMenu(UIFader fader, SelectionSlider slider)
		{
			yield return StartCoroutine(fader.InteruptAndFadeIn());
			yield return StartCoroutine(slider.WaitForBarToFill());
			yield return StartCoroutine(fader.InteruptAndFadeOut());
		}

		private IEnumerator ShowMenu(UIFader fader, VRTrackerTag tag)
		{
			yield return StartCoroutine(fader.InteruptAndFadeIn());
			yield return StartCoroutine(tag.WaitForAssignation());
			if (tag.IDisAssigned)
				transform.GetComponent<AudioSource> ().Play();
			yield return StartCoroutine(fader.InteruptAndFadeOut());
		}

		private IEnumerator ShowMenu(UIFader fader, VRTrackerAssociation prefab)
		{
			yield return StartCoroutine(fader.InteruptAndFadeIn());
            prefab.WaitForAssignation();
            yield return StartCoroutine(VRTrackerTagAssociation.instance.WaitForAssignation ());
			if (prefab.isIDAssigned)
				transform.GetComponent<AudioSource> ().Play();
			yield return StartCoroutine(fader.InteruptAndFadeOut());
		}

		/* In the Intro / Tag assignation scene, a camera is already there,
         * the one in our player character is currently disable but we want
         * to use it in the next scene. It's done using the "enableOnLoad" script
         */
		private void enablePlayerCameraForNextLevel(){
			// For each Tag
			foreach( VRTrackerTag tag in VRTracker.instance.tags){

				// Check the Tag has a camera in its children and an enable on load script
				if(tag.GetComponentInChildren<Camera>() && tag.GetComponent<EnableOnLoad>())
					tag.GetComponent<EnableOnLoad>().enableOnLoad = true;
			}
		}
	}
}
