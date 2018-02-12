namespace VRTK
{
	using UnityEngine;
	[RequireComponent(typeof(VRTK_RadialMenu))]
	public class VRTK_RadialMenuControllerTrigger : VRTK_RadialMenuController {

		protected override void OnEnable()
		{
			if (events == null)
			{
				VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_NOT_INJECTED, "RadialMenuController", "VRTK_ControllerEvents", "events", "the parent"));
				return;
			}
			else
			{
				events.TouchpadPressed += new ControllerInteractionEventHandler(DoTouchpadClicked);
				events.TouchpadReleased += new ControllerInteractionEventHandler(DoTouchpadUnclicked);
				events.TouchpadTouchStart += new ControllerInteractionEventHandler(DoTouchpadTouched);
				events.TouchpadTouchEnd += new ControllerInteractionEventHandler(DoTouchpadUntouched);
				events.TouchpadAxisChanged += new ControllerInteractionEventHandler(DoTouchpadAxisChanged);
				events.TriggerPressed += new ControllerInteractionEventHandler(DoTriggerClicked);
				events.TriggerReleased += new ControllerInteractionEventHandler(DoTriggerUnclicked);

				menu.FireHapticPulse += new HapticPulseEventHandler(AttemptHapticPulse);
			}
		}

		protected override void OnDisable()
		{
			events.TouchpadPressed -= new ControllerInteractionEventHandler(DoTouchpadClicked);
			events.TouchpadReleased -= new ControllerInteractionEventHandler(DoTouchpadUnclicked);
			events.TouchpadTouchStart -= new ControllerInteractionEventHandler(DoTouchpadTouched);
			events.TouchpadTouchEnd -= new ControllerInteractionEventHandler(DoTouchpadUntouched);
			events.TouchpadAxisChanged -= new ControllerInteractionEventHandler(DoTouchpadAxisChanged);
			events.TriggerPressed -= new ControllerInteractionEventHandler(DoTriggerClicked);
			events.TriggerReleased -= new ControllerInteractionEventHandler(DoTriggerUnclicked);

			menu.FireHapticPulse -= new HapticPulseEventHandler(AttemptHapticPulse);
		}

		protected virtual void DoTouchpadClicked(object sender, ControllerInteractionEventArgs e)
		{
			if (!menu.isShown)
				return;
			//touchpadTouched = true;
			Debug.Log ("DoTouchpadClicked");
			//DoClickButton();
		}

		protected virtual void DoTouchpadUnclicked(object sender, ControllerInteractionEventArgs e)
		{
			if (!menu.isShown)
				return;
			//touchpadTouched = false;
			//Debug.Log ("DoTouchpadUntouched");
			DoUnClickButton();
		}


		protected virtual void DoTriggerClicked(object sender, ControllerInteractionEventArgs e)
		{
			if(menu.isShown)
				DoHideMenu(false);
			else
				DoShowMenu(CalculateAngle(e));
		}

		protected virtual void DoTriggerUnclicked(object sender, ControllerInteractionEventArgs e)
		{
			
		}

		protected virtual void DoTouchpadTouched(object sender, ControllerInteractionEventArgs e)
		{
			if (!menu.isShown)
				return;

			//Debug.Log ("DoTouchpadTouched");
			if(!touchpadTouched)
				DoClickButton();
			touchpadTouched = true;
		}

		protected virtual void DoTouchpadUntouched(object sender, ControllerInteractionEventArgs e)
		{
			if (!menu.isShown)
				return;
			
			touchpadTouched = false;
		}
	}

}
