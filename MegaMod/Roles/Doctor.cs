using System.Collections.Generic;
using Hazel;
using Reactor.Extensions;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod.Roles
{
    public class Doctor : Role
    {
    
        public PlayerControl protectedPlayer { get; set; }
        public bool shieldUsed { get; set; }
        public int doctorKillerNameDuration { get; set; }
        public int doctorKillerColorDuration { get; set; }
        public bool showReport {get; set;}
        public int  showProtectedPlayer { get; set; }
        public bool shieldKillAttemptIndicator { get; set; }
        public Sprite specialButton { get; }
        public float cooldown { get; set; }

        public Doctor(PlayerControl player) : base(player)
        {
            name = "Doctor";
            color = new Color(36f / 255f, 183f / 255f, 32f / 255f, 1);
            startText = "Create a shield to protect a [8DFFFF]Crewmate";
            specialButton = bundle.LoadAsset<Sprite>("SA").DontUnload();
            player.SetKillTimer(10f);
        }

        /**
     * Sets the Role if spawn chance is reached.
     * Can only set Role if crew still has space for Role.
     * Removes crew free space on successful assignment.
     */
        public static void SetRole(List<PlayerControl> crew)
        {
            float spawnChance = HarmonyMain.optDoctorSpawnChance.GetValue();
            if (spawnChance < 1) return;
            bool spawnChanceAchieved = Rng.Next(1, 101) <= spawnChance;
            if ((crew.Count <= 0 || !spawnChanceAchieved)) return;
        
            int random = Rng.Next(0, crew.Count);
            Doctor doctor = new Doctor(crew[random]);
            AddSpecialRole(doctor);
            crew.RemoveAt(random);
            
            MessageWriter writer = GetWriter(RPC.SetDoctor);
            writer.Write(doctor.player.PlayerId);
            CloseWriter(writer);
        }

        public override void ClearSettings()
        {
            player = null;
            protectedPlayer = null;
            shieldUsed = false;
        }

        protected override void SetConfigSettings()
        {
            showProtectedPlayer = HarmonyMain.optDoctorShowShieldedPlayer.GetValue();
            showReport = HarmonyMain.optDoctorReportSwitch.GetValue();
            shieldKillAttemptIndicator = HarmonyMain.optDoctorPlayerMurderIndicator.GetValue();
            doctorKillerNameDuration = (int) HarmonyMain.optDoctorReportNameDuration.GetValue();
            doctorKillerColorDuration = (int) HarmonyMain.optDoctorReportColorDuration.GetValue();
            cooldown = (int) HarmonyMain.optDoctorShieldCooldown.GetValue();
        }

        public void SetCooldown(float deltaTime)
        {
            player.SetKillTimer(Mathf.Max(0.0f, player.killTimer - deltaTime));
        }

        public bool SetProtectedPlayer()
        {
            if (protectedPlayer != null) return false;
            protectedPlayer = PlayerTools.FindClosestTarget(player);
            shieldUsed = true;
            player.SetKillTimer(cooldown);
            MessageWriter writer = GetWriter(RPC.SetProtected);
            writer.Write(protectedPlayer.PlayerId);
            CloseWriter(writer);
            return false;
        }
        public bool CheckProtectedPlayer(byte playerId)
        {
            return protectedPlayer != null && protectedPlayer.PlayerId == playerId;
        }
    
        public void BreakShield()
        {
            WriteImmediately(RPC.ShieldBreak);
            
            protectedPlayer.myRend.material.SetColor("_VisorColor", Palette.VisorColor);
            protectedPlayer.myRend.material.SetFloat("_Outline", 0f);
            protectedPlayer = null;
        }

        public void CheckShieldButton(HudManager instance)
        {
            if (instance.UseButton == null || !instance.UseButton.isActiveAndEnabled) return;
        
            KillButtonManager killButton = instance.KillButton;
            killButton.gameObject.SetActive(true);
            killButton.isActive = true;
            killButton.renderer.sprite = specialButton;
            killButton.SetTarget(!shieldUsed && !killButton.isCoolingDown ? PlayerTools.FindClosestTarget(player) : null);
        }

        public void ShowShieldedPlayer()
        {
            if (protectedPlayer == null) return;
        
            int showShielded = showProtectedPlayer;
        
            // If everyone can see shielded
            if(showShielded == 3)
            {
                protectedPlayer.myRend.material.SetColor("_VisorColor", ModdedPalette.protectedColor);
                protectedPlayer.myRend.material.SetFloat("_Outline", 1f);
                protectedPlayer.myRend.material.SetColor("_OutlineColor", ModdedPalette.protectedColor);
            }
            // If I am protected and should see the shield
            else if (PlayerControl.LocalPlayer == protectedPlayer && (showShielded == 0 || showShielded == 2))
            {
                protectedPlayer.myRend.material.SetColor("_VisorColor", ModdedPalette.protectedColor);
                protectedPlayer.myRend.material.SetFloat("_Outline", 1f);
                protectedPlayer.myRend.material.SetColor("_OutlineColor", ModdedPalette.protectedColor);
            }
            // If I am Doctor and should see the shield
            else if(PlayerControl.LocalPlayer == player && (showShielded == 1 || showShielded == 2))
            {
                protectedPlayer.myRend.material.SetColor("_VisorColor", ModdedPalette.protectedColor);
                protectedPlayer.myRend.material.SetFloat("_Outline", 1f);
                protectedPlayer.myRend.material.SetColor("_OutlineColor", ModdedPalette.protectedColor);
            }
        }

        public override void CheckDead(HudManager instance)
        {
            if (protectedPlayer == null || !protectedPlayer.Data.IsDead && !player.Data.IsDead) return;
            BreakShield();
        }
    }
}