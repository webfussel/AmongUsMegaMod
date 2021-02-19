using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using MegaMod;
using static MegaMod.MegaModManager; // TODO: wtf?
using UnityEngine;

public class Engineer : Role
{
    public bool repairUsed;
    public bool sabotageActive { get; set; }

    public Engineer(PlayerControl player) : base(player)
    {
        this.player = player;
        name = "Engineer";
        color = new Color(255f / 255f, 165f / 255f, 10f / 255f, 1);
        startText = "Maintain important systems on the ship";
    }

    /**
     * Sets the Role if spawn chance is reached.
     * Can only set Role if crew still has space for Role.
     * Removes crew free space on successful assignment.
     */
    public static void SetRole(List<PlayerControl> crew)
    {
        float spawnChance = HarmonyMain.optEngineerSpawnChance.GetValue();
        if (spawnChance < 1) return;
        ConsoleTools.Info("Try to set Engineer");
        bool spawnChanceAchieved = rng.Next(1, 101) <= spawnChance;
        if ((crew.Count <= 0 || !spawnChanceAchieved)) return;

        int random = rng.Next(0, crew.Count);
        Engineer engineer = new Engineer(crew[random]);
        AddSpecialRole(engineer);
        crew.RemoveAt(random);
            
        MessageWriter writer = GetWriter(RPC.SetEngineer);
        writer.Write(engineer.player.PlayerId);
        CloseWriter(writer);
    }

    public override void ClearSettings()
    {
        player = null;
        repairUsed = false;
    }

    public override void SetConfigSettings()
    {
        // do nothing
    }

    public void SetRepairButton(HudManager instance)
    {
        if (player == null || player.PlayerId != PlayerControl.LocalPlayer.PlayerId ||
            !instance.UseButton.isActiveAndEnabled) return;
        
        KillButtonManager killButton = instance.KillButton;
        killButton.gameObject.SetActive(true);
        killButton.isActive = true;
        killButton.SetCoolDown(0f, 1f);
        killButton.renderer.sprite = repairIco;
        killButton.renderer.color = Palette.EnabledColor;
        killButton.renderer.material.SetFloat("_Desat", 0f);
    }

    public bool ShowRepairMap()
    {
        DestroyableSingleton<HudManager>.Instance.ShowMap((Action<MapBehaviour>)delegate (MapBehaviour m)
        {
            m.ShowInfectedMap();
            m.ColorControl.baseColor = sabotageActive ? Color.gray : color;
        });
        return false;
    }

    public override void CheckDead(HudManager instance)
    {
        if (!player.Data.IsDead) return;
        // Should Engineer really be unable to solve an emergency when dead?
        KillButtonManager killButton = instance.KillButton;
        killButton.gameObject.SetActive(false);
        killButton.renderer.enabled = false;
        killButton.isActive = false;
        killButton.SetTarget(null);
        killButton.enabled = false;
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowInfectedMap))]
    class EngineerMapOpen
    {
        static void Postfix(MapBehaviour __instance)
        {
            Engineer engi = GetSpecialRole<Engineer>(PlayerControl.LocalPlayer.PlayerId);
            if (engi.player == null || engi.player.PlayerId != PlayerControl.LocalPlayer.PlayerId || !__instance.IsOpen) return;

            __instance.ColorControl.baseColor = engi.color;
            foreach (MapRoom room in __instance.infectedOverlay.rooms)
            {
                if (room.door == null) continue;
                
                room.door.enabled = false;
                room.door.gameObject.SetActive(false);
                // here was room.door.gameObject.active = false
            }
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
    class EngineerMapUpdate
    {
        static void Postfix(MapBehaviour __instance)
        {
            Engineer engi = GetSpecialRole<Engineer>(PlayerControl.LocalPlayer.PlayerId);
            if (engi.player == null || engi.player.PlayerId != PlayerControl.LocalPlayer.PlayerId || !__instance.IsOpen || !__instance.infectedOverlay.gameObject.active) return;

            __instance.ColorControl.baseColor = !engi.sabotageActive ? Color.gray : engi.color;
            float perc = engi.repairUsed ? 1f : 0f;
            
            foreach (MapRoom room in __instance.infectedOverlay.rooms)
            {
                if (room.special == null) continue;
                
                room.special.material.SetFloat("_Desat", !engi.sabotageActive ? 1f : 0f);
                room.special.enabled = true;
                room.special.gameObject.SetActive(true);
                // here was room.special.gameObject.active = true;
                room.special.material.SetFloat("_Percent", !PlayerControl.LocalPlayer.Data.IsDead ? perc : 1f);
            }
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.Method_41))]
    class SabotageButtonDeactivatePatch
    {
        static bool Prefix(MapRoom __instance, float DCEFKAOFGOG)
        {
            Engineer engi = GetSpecialRole<Engineer>(PlayerControl.LocalPlayer.PlayerId);
            if (engi.player == null) return true;
            return engi.player == null || engi.player.PlayerId != PlayerControl.LocalPlayer.PlayerId;
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageReactor))]
    class SabotageReactorPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            Engineer engi = GetSpecialRole<Engineer>(PlayerControl.LocalPlayer.PlayerId);
            if (engi.player == null || engi.player.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;
            if (engi.repairUsed || !engi.sabotageActive || PlayerControl.LocalPlayer.Data.IsDead) return false;
            
            engi.repairUsed = true;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
            return false;
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageLights))]
    class SabotageLightsPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            Engineer engi = GetSpecialRole<Engineer>(PlayerControl.LocalPlayer.PlayerId);
            if (engi.player == null || engi.player.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;
            if (engi.repairUsed || !engi.sabotageActive || PlayerControl.LocalPlayer.Data.IsDead) return false;
            
            engi.repairUsed = true;
            SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
            WriteImmediately(RPC.FixLights);
            return false;
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageComms))]
    class SabotageCommsPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            Engineer engi = GetSpecialRole<Engineer>(PlayerControl.LocalPlayer.PlayerId);
            if (engi.player == null || engi.player.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;
            if (engi.repairUsed || !engi.sabotageActive || PlayerControl.LocalPlayer.Data.IsDead) return false;
            
            engi.repairUsed = true;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageOxygen))]
    class SabotageOxyPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            Engineer engi = GetSpecialRole<Engineer>(PlayerControl.LocalPlayer.PlayerId);
            if (engi.player == null || engi.player.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;
            if (engi.repairUsed || !engi.sabotageActive || PlayerControl.LocalPlayer.Data.IsDead) return false;
            
            engi.repairUsed = true;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
            return false;
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageSeismic))]
    class SabotageSeismicPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            Engineer engi = GetSpecialRole<Engineer>(PlayerControl.LocalPlayer.PlayerId);
            if (engi.player == null || engi.player.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;
            if (engi.repairUsed || !engi.sabotageActive || PlayerControl.LocalPlayer.Data.IsDead) return false;
            
            engi.repairUsed = true;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);
            return false;
        }
    }
}