// VRTracker Defines|SDK_VRTracker|001
namespace VRTK
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Handles all the scripting define symbols for the Oculus and Avatar SDKs.
    /// </summary>
    public static class SDK_VRTrackerDefines
    {
        /// <summary>
        /// The scripting define symbol for the VRTracker SDK.
        /// </summary>
        public const string ScriptingDefineSymbol = SDK_ScriptingDefineSymbolPredicateAttribute.RemovableSymbolPrefix + "SDK_VRTRACKER";

		[SDK_ScriptingDefineSymbolPredicate(ScriptingDefineSymbol, "Standalone")]
		[SDK_ScriptingDefineSymbolPredicate(ScriptingDefineSymbol, "Android")]
		private static bool IsVRTrackerAvailable()
		{
			return true;//VRTK_SharedMethods.GetTypeUnknownAssembly("Ximmerse.InputSystem.XDevicePlugin") != null; //TODO: change to check WS state
		}
    }
}