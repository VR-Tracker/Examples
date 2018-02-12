namespace VRTK
{
	using UnityEngine;

	public class VRTK_AnimationController : MonoBehaviour {

		public string TriggerName = "TriggerAnim";

		[Tooltip("The animator to Trigger when buttons are pressed on the controller.")]
		public Animator animator;

		[Tooltip("The controller to listen to the controller events on.")]
		public VRTK_ControllerEvents events;

		// Update is called once per frame
		void Update () {
			if (Input.GetKeyDown ("n")) {
				animator.SetTrigger (TriggerName);
			}
		}


		protected virtual void Awake()
		{
			if(animator == null)
				animator = GetComponent<Animator>();

			Initialize();
		}

		protected virtual void Initialize()
		{
			if (events == null)
			{
				events = GetComponentInParent<VRTK_ControllerEvents>();
			}
		}

		protected virtual void OnEnable()
		{
			if (events == null)
			{
				VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_NOT_INJECTED, "RadialMenuController", "VRTK_ControllerEvents", "events", "the parent"));
				return;
			}
			else
			{
				events.ButtonOnePressed += new ControllerInteractionEventHandler(DoButtonOnePressed);
			}
		}

		protected virtual void OnDisable()
		{
			events.ButtonOnePressed -= new ControllerInteractionEventHandler(DoButtonOnePressed);
		}

		protected virtual void DoButtonOnePressed(object sender, ControllerInteractionEventArgs e)
		{
			animator.SetTrigger (TriggerName);
		}
	}
}