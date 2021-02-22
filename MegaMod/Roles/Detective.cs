using System.Collections.Generic;
using Hazel;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod.Roles
{
    public class Detective : Role
    {
        private float cooldown { get; set; }
        public bool showReport { get; private set; }

        public Detective(PlayerControl player) : base(player)
        {
            name = "Detective";
            color = new Color(0, 40f / 255f, 198f / 255f, 1);
            startText = "Shoot the [FF0000FF]Impostor";
            player.SetKillTimer(10f);
        }

        /**
     * Sets the Role if spawn chance is reached.
     * Can only set Role if crew still has space for Role.
     * Removes crew free space on successful assignment.
     */
        public static void SetRole(List<PlayerControl> crew)
        {
            float spawnChance = MainConfig.OptDetectiveSpawnChance.GetValue();
            if (spawnChance < 1) return;
            bool spawnChanceAchieved = Rng.Next(1, 101) <= spawnChance;
            if ((crew.Count <= 0 || !spawnChanceAchieved)) return;
        
            int random = Rng.Next(0, crew.Count);
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
        }

        protected override void SetConfigSettings()
        {
            cooldown = MainConfig.OptDetectiveKillCooldown.GetValue();
            showReport = MainConfig.OptShowDetectiveReports.GetValue();
        }

        public override void CheckDead(HudManager instance)
        {
            if (!player.Data.IsDead) return;
            
            KillButtonManager killButton = instance.KillButton;
            killButton.gameObject.SetActive(false);
            killButton.renderer.enabled = false;
        }

        public void SetCooldown(float deltaTime)
        {
            player.SetKillTimer(Mathf.Max(0.0f, player.killTimer - deltaTime));
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

        private void KillPlayer(PlayerControl target)
        {
            MessageWriter writer = GetWriter(RPC.DetectiveKill);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(target.PlayerId);
            CloseWriter(writer);
            PlayerControl.LocalPlayer.MurderPlayer(target);
        }

        public bool KillOrCommitSuicide(KillButtonManager instance)
        {
            if (instance.isCoolingDown) return false;
            
            PlayerControl target = PlayerTools.FindClosestTarget(player);
            if (target == null) return false;

            if (SpecialRoleIsAssigned<Doctor>(out var doctorKvp) && doctorKvp.Value.protectedPlayer?.PlayerId == target.PlayerId)
            {
                // Sound effekt
                player.SetKillTimer(cooldown);
                return false;
            }
            
            
            if (
                //check if they're Maniac and the setting is configured
                (SpecialRoleIsAssigned(out KeyValuePair<byte, Maniac> maniacKvp) && target.PlayerId == maniacKvp.Key && maniacKvp.Value.maniacCanDieToDetective)
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