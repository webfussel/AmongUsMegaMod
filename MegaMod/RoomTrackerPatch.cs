using HarmonyLib;

namespace MegaMod
{
    [HarmonyPatch(typeof(RoomTracker), nameof(RoomTracker.FixedUpdate))]
    public class RoomTrackerPatch
    {
        static void Postfix(RoomTracker __instance)
        {
            ConsoleTools.Info(__instance.LastRoom.name);
        }
    }
}