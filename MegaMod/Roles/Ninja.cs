﻿using System.Collections.Generic;
using Hazel;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod.Roles
{
    public class Ninja : Role
    {
        public static readonly byte RoleID = 106;
        private bool doubleKillUsed { get; set; }
        
        public Ninja(PlayerControl player) : base(player)
        {
            name = "Ninja";
            color = new Color(0, 0, 0, 1);
            borderColor = new Color(1, 1, 1, 1);
            startText = "Double kill - Triple cooldown";
        }
        
        /**
         * Sets the Role if spawn chance is reached.
         * Can only set Role if crew still has space for Role.
         * Removes crew free space on successful assignment.
         */
        public static void SetRole(List<PlayerControl> imps)
        {
            float spawnChance = MainConfig.OptNinjaSpawnChance.GetValue();
            if (spawnChance < 1) return;
            bool spawnChanceAchieved = Rng.Next(1, 101) <= spawnChance;
            if ((imps.Count <= 0 || !spawnChanceAchieved)) return;
        
            int random = Rng.Next(0, imps.Count);
            Ninja ninja = new Ninja(imps[random]);
            AddSpecialRole(ninja);
            imps.RemoveAt(random);
            
            MessageWriter writer = GetWriter(RPC.SetRole);
            writer.Write(RoleID);
            writer.Write(ninja.player.PlayerId);
            CloseWriter(writer);
        }

        public void CheckKillButton(HudManager instance)
        {
            if (instance.UseButton == null || !instance.UseButton.isActiveAndEnabled || player.Data.IsDead) return;
            
            KillButtonManager killButton = instance.KillButton;
            killButton.gameObject.SetActive(true);
            killButton.renderer.enabled = true;
            killButton.isActive = true;
            killButton.renderer.sprite = defaultKillButton;
            killButton.SetTarget(PlayerTools.FindClosestTarget(player));
        }

        public void CheckCooldown(KillButtonManager instance)
        {
            PlayerControl closest = PlayerTools.FindClosestTarget(player);
            if (!instance.isCoolingDown || doubleKillUsed || closest.Data.IsImpostor) return;
            
            doubleKillUsed = true;
            player.MurderPlayer(closest);
            player.RpcMurderPlayer(closest);
            player.SetKillTimer(player.killTimer + PlayerControl.GameOptions.KillCooldown * 2);
        }

        public override void ClearSettings()
        {
            player = null;
        }

        protected override void SetConfigSettings() {}

        public override void CheckDead(HudManager instance) {}
    }
}