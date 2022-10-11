using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;
using UnityEngine;

namespace Crash_Save
{
#if DEBUG
	[EnableReloading]
#endif
    public static class Main
    {
        static Harmony harmony;

        public static bool Load(UnityModManager.ModEntry entry)
        {
            harmony = new Harmony(entry.Info.Id);

            entry.OnToggle = OnToggle;
#if DEBUG
			entry.OnUnload = OnUnload;
#endif

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry entry, bool active)
        {
            if (active)
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                harmony.UnpatchAll(entry.Info.Id);
            }

            return true;
        }

#if DEBUG
		static bool OnUnload(UnityModManager.ModEntry entry) {
			return true;
		}
#endif

        [HarmonyPatch(typeof(WorldController), "HandleLogMessageReceived")]
        class HandleLogMessage
        {
            static bool _exceptionUnhandled;
            public static void Prefix(ref bool ____exceptionUnhandled)
            {
                _exceptionUnhandled = ____exceptionUnhandled;
            }

            public static void Postfix(ref bool ____exceptionUnhandled)
            {
                if(____exceptionUnhandled && !_exceptionUnhandled)
                {
                    // Guru Medititation Error
                    Debug.Log("Crash Save: Guru Meditation Error Detected");

                    foreach(Device device in Controllers.worldController.devices)
                    {
                        if (device.isSolid)
                            device.ToggleState(device.rootComponent, true);
                    }
                    GlobalControllers.storageHelperController.SaveWorldToDiskAsync(StorageHelperController.SaveActions.New, false, () => { Debug.Log("Crash Save: Saved Recovery World File");});
                }
            }
        }
    }
}
