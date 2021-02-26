using HarmonyLib;
using MegaMod.Roles;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetTasks))]
    public class PlayerControlPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if(TryGetSpecialRole(__instance.PlayerId, out Role role))
                role.SetRoleDescription();
        }
    }
}