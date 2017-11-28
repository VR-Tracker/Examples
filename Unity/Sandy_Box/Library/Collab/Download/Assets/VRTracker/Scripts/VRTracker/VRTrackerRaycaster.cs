using System;
using UnityEngine;
using UnityEngine.Networking;

namespace VRStandardAssets.Utils
{
	// In order to interact with objects in the scene
	// this class casts a ray into the scene and if it finds
	// a VRInteractiveItem it exposes it for other classes to use.
	// This script should be generally be placed on the camera.
	public class VRTrackerRaycaster : MonoBehaviour
	{
		public event Action<RaycastHit> OnRaycasthit;                   // This event is called every frame that the user's gaze is over a collider.


		[SerializeField] private LayerMask m_ExclusionLayers;           // Layers to exclude from the raycast.
		[SerializeField] private VRInput m_VrInput;                     // Used to call input based events on the current VRInteractiveItem.
		[SerializeField] private bool m_ShowDebugRay;                   // Optionally show the debug ray.
		[SerializeField] private float m_DebugRayLength = 5f;           // Debug ray length.
		[SerializeField] private float m_DebugRayDuration = 1f;         // How long the Debug ray will remain visible.
		[SerializeField] private float m_RayLength = 500f;              // How far into the scene the ray is cast.


		private VRInteractiveItem m_CurrentInteractible;                //The current interactive item
		private VRInteractiveItem m_LastInteractible;                   //The last interactive item


		// Utility for other classes to get the current interactive item
		public VRInteractiveItem CurrentInteractible
		{
			get { return m_CurrentInteractible; }
		}


		private void OnEnable()
		{
			if (transform.parent.parent.gameObject.GetComponent<NetworkIdentity>() != null && !transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
				return;
			}
			m_VrInput.OnClick += HandleClick;
			m_VrInput.OnDoubleClick += HandleDoubleClick;
			m_VrInput.OnUp += HandleUp;
			m_VrInput.OnDown += HandleDown;
		}


		private void OnDisable ()
		{
			if (transform.parent.parent.GetComponent<NetworkIdentity>() != null && !transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
				return;
			}
			m_VrInput.OnClick -= HandleClick;
			m_VrInput.OnDoubleClick -= HandleDoubleClick;
			m_VrInput.OnUp -= HandleUp;
			m_VrInput.OnDown -= HandleDown;
		}


		private void Update()
		{
			if (transform.parent.parent.GetComponent<NetworkIdentity>() != null && !transform.parent.parent.GetComponent<NetworkIdentity>().isLocalPlayer) {
				return;
			}

			EyeRaycast();
		}


		private void EyeRaycast()
		{
			// Show the debug ray if required
			if (m_ShowDebugRay)
			{
				Debug.DrawRay(this.transform.position, this.transform.forward * m_DebugRayLength, Color.blue, m_DebugRayDuration);
			}

			// Create a ray that points forwards from the camera.
			Ray ray = new Ray(this.transform.position, this.transform.forward);
			RaycastHit hit;

			// Do the raycast forweards to see if we hit an interactive item
			if (Physics.Raycast(ray, out hit, m_RayLength, ~m_ExclusionLayers))
			{
				VRInteractiveItem interactible = hit.collider.GetComponent<VRInteractiveItem>(); //attempt to get the VRInteractiveItem on the hit object
				m_CurrentInteractible = interactible;

				// If we hit an interactive item and it's not the same as the last interactive item, then call Over
				if (interactible && interactible != m_LastInteractible)
					interactible.Over(); 

				// Deactive the last interactive item 
				if (interactible != m_LastInteractible)
					DeactiveLastInteractible();

				m_LastInteractible = interactible;

				if (OnRaycasthit != null)
					OnRaycasthit(hit);
			}
			else
			{
				// Nothing was hit, deactive the last interactive item.
				DeactiveLastInteractible();
				m_CurrentInteractible = null;
			}
		}


		private void DeactiveLastInteractible()
		{
			if (m_LastInteractible == null)
				return;

			m_LastInteractible.Out();
			m_LastInteractible = null;
		}


		private void HandleUp()
		{
			if (m_CurrentInteractible != null)
				m_CurrentInteractible.Up();
		}


		private void HandleDown()
		{
			if (m_CurrentInteractible != null)
				m_CurrentInteractible.Down();
		}


		private void HandleClick()
		{
			if (m_CurrentInteractible != null)
				m_CurrentInteractible.Click();
		}


		private void HandleDoubleClick()
		{
			if (m_CurrentInteractible != null)
				m_CurrentInteractible.DoubleClick();

		}
	}
}