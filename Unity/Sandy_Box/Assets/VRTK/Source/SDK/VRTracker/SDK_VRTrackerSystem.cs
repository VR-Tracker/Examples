// VRTracker System|SDK_VRTracker|002
namespace VRTK
{
    /// <summary>
	/// The VRTracker System SDK script provides a bridge to the Oculus SDK.
    /// </summary>
/*	[SDK_Description("VRTracker (Standalone:Oculus)", null, "Oculus", "Standalone")]
	[SDK_Description("VRTracker (Standalone:OpenVR)", null, "OpenVR", "Standalone", 1)]
	[SDK_Description("VRTracker (Android:Cardboard)", null, "Cardboard", "Android", 2)]
	[SDK_Description("VRTracker (Android:Daydream)", null, "Daydream", "Android", 3)]
	[SDK_Description("VRTracker (Android:Oculus)", null, "Oculus", "Android", 4)]
	[SDK_Description("VRTracker (Android:OpenVR)", null, "OpenVR", "Android", 5)]
	*/
	[SDK_Description("VRTracker (Standalone:Oculus)", SDK_VRTrackerDefines.ScriptingDefineSymbol, "Oculus", "Standalone")]
	[SDK_Description("VRTracker (Standalone:OpenVR)", SDK_VRTrackerDefines.ScriptingDefineSymbol, "OpenVR", "Standalone", 1)]
	[SDK_Description("VRTracker (Android:Cardboard)", SDK_VRTrackerDefines.ScriptingDefineSymbol, "Cardboard", "Android", 2)]
	[SDK_Description("VRTracker (Android:Daydream)", SDK_VRTrackerDefines.ScriptingDefineSymbol, "Daydream", "Android", 3)]
	[SDK_Description("VRTracker (Android:Oculus)", SDK_VRTrackerDefines.ScriptingDefineSymbol, "Oculus", "Android", 4)]
	[SDK_Description("VRTracker (Android:OpenVR)", SDK_VRTrackerDefines.ScriptingDefineSymbol, "OpenVR", "Android", 5)]

	public class SDK_VRTrackerSystem
#if VRTK_DEFINE_SDK_VRTRACKER
        : SDK_BaseSystem
#else
        : SDK_FallbackSystem
#endif
    {
#if VRTK_DEFINE_SDK_VRTRACKER
        /// <summary>
        /// The IsDisplayOnDesktop method returns true if the display is extending the desktop.
        /// </summary>
        /// <returns>Returns true if the display is extending the desktop</returns>
        public override bool IsDisplayOnDesktop()
        {
            return false;
        }

        /// <summary>
        /// The ShouldAppRenderWithLowResources method is used to determine if the Unity app should use low resource mode. Typically true when the dashboard is showing.
        /// </summary>
        /// <returns>Returns true if the Unity app should render with low resources.</returns>
        public override bool ShouldAppRenderWithLowResources()
        {
            return false;
        }

        /// <summary>
        /// The ForceInterleavedReprojectionOn method determines whether Interleaved Reprojection should be forced on or off.
        /// </summary>
        /// <param name="force">If true then Interleaved Reprojection will be forced on, if false it will not be forced on.</param>
        public override void ForceInterleavedReprojectionOn(bool force)
        {
        }
#endif
    }
}