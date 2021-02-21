using HarmonyLib;
using MegaMod.Roles;
using static MegaMod.MegaModManager;

namespace MegaMod
{
    public class MapPatches
    {
        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowInfectedMap))]
        class MapOpen
        {
            static void Postfix(MapBehaviour __instance)
            {
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Engineer engineer))
                    engineer?.OpenMap(__instance);
            }
        }
        
        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
        class MapUpdate
        {
            static void Postfix(MapBehaviour __instance)
            {
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Engineer engineer))
                    engineer?.UpdateMap(__instance);
            }
        }
        
        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.Method_41))] // SetSpecialActive
        class SetSpecialActivePatch
        {
            static bool Prefix(MapRoom __instance, float DCEFKAOFGOG)
            {
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Engineer engineer))
                    return engineer.SetRepairButtonsActive();

                return true;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageReactor))]
        class SabotageReactorPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Engineer engineer))
                    return engineer.RepairReactor();
                
                return true;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageLights))]
        class SabotageLightsPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Engineer engineer))
                    return engineer.RepairLight();
                
                return true;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageComms))]
        class SabotageCommsPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Engineer engineer))
                    return engineer.RepairComms();
                
                return true;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageOxygen))]
        class SabotageOxyPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Engineer engineer))
                    return engineer.RepairOxy();
                
                return true;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageSeismic))]
        class SabotageSeismicPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Engineer engineer))
                    return engineer.RepairSeismic();
                
                return true;
            }
        }
    }
}