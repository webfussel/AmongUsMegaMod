using HarmonyLib;
using static MegaMod.MegaModManager; // TODO: wtf?

namespace MegaMod
{
    [HarmonyPatch(typeof(RoomTracker), nameof(RoomTracker.FixedUpdate))]
    public class RoomTrackerPatch
    {
        static void Postfix(RoomTracker __instance)
        {
            if (__instance.LastRoom == null || PlayerControl.LocalPlayer == null || !PlayerControl.LocalPlayer.Data.IsImpostor ||
                __instance.LastRoom.RoomId == currentRoomId) return;
            
            currentRoomId = __instance.LastRoom.RoomId;
        }
    }
}