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

        public Doctor(PlayerControl player) : base(player)
        {
            this.player = player;
            name = "Doctor";
            color = new Color(36f / 255f, 183f / 255f, 32f / 255f, 1);
            startText = "Create a shield to protect a [8DFFFF]Crewmate";
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
            ConsoleTools.Info("Try to set Doctor");
            bool spawnChanceAchieved = rng.Next(1, 101) <= spawnChance;
            if ((crew.Count <= 0 || !spawnChanceAchieved)) return;
        
            int random = rng.Next(0, crew.Count);
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

        public override void SetConfigSettings()
        {
            showProtectedPlayer = HarmonyMain.optDoctorShowShieldedPlayer.GetValue();
            showReport = HarmonyMain.optDoctorReportSwitch.GetValue();
            shieldKillAttemptIndicator = HarmonyMain.optDoctorPlayerMurderIndicator.GetValue();
            doctorKillerNameDuration = (int) HarmonyMain.optDoctorReportNameDuration.GetValue();
            doctorKillerColorDuration = (int) HarmonyMain.optDoctorReportColorDuration.GetValue();
        }

        public bool SetProtectedPlayer(PlayerControl target)
        {
            MessageWriter writer = GetWriter(RPC.SetProtected);
            protectedPlayer = target;
            shieldUsed = true;
            writer.Write(protectedPlayer.PlayerId);
            CloseWriter(writer);
            return false;
        }
        public bool CheckProtectedPlayer(byte playerId)
        {
            return protectedPlayer != null & protectedPlayer.PlayerId == playerId;
        }
    
        public void BreakShield(bool flag)
        {
            if (flag)
            {
                Doctor doctor = GetSpecialRole<Doctor>(PlayerControl.LocalPlayer.PlayerId);
                WriteImmediately(RPC.ShieldBreak);
            
                doctor.protectedPlayer.myRend.material.SetColor("_VisorColor", Palette.VisorColor);
                doctor.protectedPlayer.myRend.material.SetFloat("_Outline", 0f);
                doctor.protectedPlayer = null;
            }
        }

        public void SetShieldButton(HudManager instance)
        {
            Sprite smallShieldIco = bundle.LoadAsset<Sprite>("RESmall").DontUnload();
            if (protectedPlayer == null || protectedPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId ||
                !instance.UseButton.isActiveAndEnabled) return;
        
            if (rend == null)
            {
                rend = new GameObject("Shield Icon");
                rend.AddComponent<SpriteRenderer>().sprite = smallShieldIco;
            }
        
            int scale = Screen.width > Screen.height ? Screen.width / 800 : Screen.height / 600;
            rend.transform.localPosition = Camera.main.ScreenToWorldPoint(new Vector3(0 + (25 * scale), 0 + (25 * scale), -50f));
            rend.SetActive(true);
        }

        public void CheckShieldButton(HudManager instance)
        {
            if (instance.UseButton == null || !instance.UseButton.isActiveAndEnabled) return;
        
            KillButtonManager killButton = instance.KillButton;
            killButton.renderer.sprite = shieldIco;
            killButton.gameObject.SetActive(true);
            killButton.isActive = true;
            killButton.SetCoolDown(0f, 1f);
            if (DistLocalClosest < GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance] && !shieldUsed)
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
            // If I am Medic and should see the shield
            else if(PlayerControl.LocalPlayer == player && (showShielded == 1 || showShielded == 2))
            {
                protectedPlayer.myRend.material.SetColor("_VisorColor", ModdedPalette.protectedColor);
                protectedPlayer.myRend.material.SetFloat("_Outline", 1f);
                protectedPlayer.myRend.material.SetColor("_OutlineColor", ModdedPalette.protectedColor);
            }
        }

        public override void CheckDead(HudManager instance)
        {
            if (protectedPlayer == null || (!protectedPlayer.Data.IsDead && !player.Data.IsDead)) return;
            BreakShield(true);
    
        }
    }
}