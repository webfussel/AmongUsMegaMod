using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod.Roles
{
    public class Detective : Role
    {
        public DateTime? lastKilled { get; set; }
        public float cooldown { get; set; }

        public Detective(PlayerControl player) : base(player)
        {
            this.player = player;
            name = "Detective";
            color = new Color(0, 40f / 255f, 198f / 255f, 1);
            startText = "Shoot the [FF0000FF]Impostor";
        }

        /**
     * Sets the Role if spawn chance is reached.
     * Can only set Role if crew still has space for Role.
     * Removes crew free space on successful assignment.
     */
        public static void SetRole(List<PlayerControl> crew)
        {
            float spawnChance = HarmonyMain.optDetectiveSpawnChance.GetValue();
            if (spawnChance < 1) return;
            bool spawnChanceAchieved = rng.Next(1, 101) <= spawnChance;
            if ((crew.Count <= 0 || !spawnChanceAchieved)) return;
        
            int random = rng.Next(0, crew.Count);
            Detective detective = new Detective(crew[random]);
            AddSpecialRole(detective);
            crew.RemoveAt(random);
            
            MessageWriter writer = GetWriter(RPC.SetDetective);
            writer.Write(detective.player.PlayerId);
            CloseWriter(writer);
        }
    
        public override void ClearSettings()
        {
            player = null;
            lastKilled = null;
        }

        protected override void SetConfigSettings()
        {
            cooldown = HarmonyMain.optDetectiveKillCooldown.GetValue();
        }

        public override void CheckDead(HudManager instance)
        {
            if (!player.Data.IsDead) return;
            
            KillButtonManager killButton = instance.KillButton;
            killButton.gameObject.SetActive(false);
            killButton.renderer.enabled = false;
            killButton.isActive = false;
            killButton.SetTarget(null);
            killButton.enabled = false;
            killButton.TimerText.Text = "";
            killButton.TimerText.gameObject.SetActive(false);
            cooldown = 0;
            lastKilled = null;
        }

        public void CheckKillButton(HudManager instance)
        {
            if (instance.UseButton == null || !instance.UseButton.isActiveAndEnabled) return;
        
            KillButtonManager killButton = instance.KillButton;
            killButton.gameObject.SetActive(true);
            killButton.isActive = true;
            killButton.SetCoolDown(GetCurrentCooldown(), PlayerControl.GameOptions.KillCooldown + 15.0f);
            if (DistLocalClosest < GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance])
            {
                killButton.SetTarget(PlayerTools.closestPlayer);
                CurrentTarget = PlayerTools.closestPlayer;
            }
            else
            {
                killButton.SetTarget(null);
                CurrentTarget = null;
            }
        }

        private void KillPlayer(PlayerControl player)
        {
            MessageWriter writer = GetWriter(RPC.DetectiveKill);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(player.PlayerId);
            CloseWriter(writer);
            PlayerControl.LocalPlayer.MurderPlayer(player);
            lastKilled = DateTime.UtcNow;
        }

        public float GetCurrentCooldown()
        {
            if (lastKilled == null) return cooldown;
        
            TimeSpan diff = (TimeSpan) (DateTime.UtcNow - lastKilled);

            float cooldownMS = cooldown * 1000.0f;
            if (diff.TotalMilliseconds > cooldownMS) return 0;
            return (float) (cooldownMS - diff.TotalMilliseconds) / 1000.0f;
        }

        public override void SetIntro(IntroCutscene.CoBegin__d __instance)
        {
            base.SetIntro(__instance);
            lastKilled = DateTime.UtcNow.AddSeconds(10 - cooldown);
        }

        public void ResetCooldown(ExileController instance)
        {
            lastKilled = DateTime.UtcNow.AddMilliseconds(instance.Duration);
        }

        public bool KillOrCommitSuicide(PlayerControl target)
        {
            if (target == null || GetCurrentCooldown() != 0) return false;
            
            if (SpecialRoleIsAssigned<Doctor>(out var doctorKvp))
                if (doctorKvp.Value.protectedPlayer.PlayerId == target.PlayerId)
                {
                    // Sound effekt
                    lastKilled = DateTime.UtcNow;
                    return false;
                }
            
            
            if (
                //check if they're jester and the setting is configured
                (SpecialRoleIsAssigned(out KeyValuePair<byte, Jester> jesterKvp) && target.PlayerId == jesterKvp.Key && jesterKvp.Value.jesterCanDieToDetective)
                //or if they're an impostor
                || target.Data.IsImpostor
            )
                KillPlayer(target);
            //else, they're innocent or shielded by the doctor
            else
                KillPlayer(player);
            return false;
        }
    }
}