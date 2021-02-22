using HarmonyLib;
using Hazel;
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
                else if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Tracker tracker))
                    tracker?.OpenMap(__instance);
            }
        }
        
        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
        class MapUpdate
        {
            static void Postfix(MapBehaviour __instance)
            {
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Engineer engineer))
                    engineer?.UpdateMap(__instance);
                else if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Tracker tracker))
                    tracker?.UpdateMap(__instance);
            }
        }
        
        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.Method_41))] // SetSpecialActive
        class SetSpecialActivePatch
        {
            static bool Prefix(MapRoom __instance, float DCEFKAOFGOG)
            {
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Engineer engineer))
                    return engineer.SetRepairButtonsActive();
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Tracker tracker))
                    return tracker.SetMarkButtonsActive();

                return true;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageReactor))]
        class SabotageReactorPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if (TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Engineer engineer))
                    return engineer.RepairReactor();
                if (TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Tracker tracker))
                    return tracker.MarkSystem(SystemTypes.Reactor);
                if (SpecialRoleIsAssigned<Tracker>(out var trackerKvp) && PlayerControl.LocalPlayer.Data.IsImpostor)            
                    trackerKvp.Value.OnSabotageHappened(SystemTypes.Reactor, PlayerControl.LocalPlayer);

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
                
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Tracker tracker))
                    return tracker.MarkSystem(SystemTypes.Electrical);
                
                if (SpecialRoleIsAssigned<Tracker>(out var trackerKvp) && PlayerControl.LocalPlayer.Data.IsImpostor)
                    trackerKvp.Value.OnSabotageHappened(SystemTypes.Electrical, PlayerControl.LocalPlayer);

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
                
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Tracker tracker))
                    return tracker.MarkSystem(SystemTypes.Comms);
                
                if (SpecialRoleIsAssigned<Tracker>(out var trackerKvp) && PlayerControl.LocalPlayer.Data.IsImpostor)
                    trackerKvp.Value.OnSabotageHappened(SystemTypes.Comms, PlayerControl.LocalPlayer);

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
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Tracker tracker))
                    return tracker.MarkSystem(SystemTypes.LifeSupp);
                if (SpecialRoleIsAssigned<Tracker>(out var trackerKvp) && PlayerControl.LocalPlayer.Data.IsImpostor)
                    trackerKvp.Value.OnSabotageHappened(SystemTypes.LifeSupp, PlayerControl.LocalPlayer);

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
                if(TryGetSpecialRole(PlayerControl.LocalPlayer.PlayerId, out Tracker tracker))
                    return tracker.MarkSystem(SystemTypes.Laboratory);
                if (SpecialRoleIsAssigned<Tracker>(out var trackerKvp) && PlayerControl.LocalPlayer.Data.IsImpostor)
                    trackerKvp.Value.OnSabotageHappened(SystemTypes.Laboratory, PlayerControl.LocalPlayer);

                return true;
            }
        }
    }
}