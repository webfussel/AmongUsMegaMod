using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod.Roles
{
    public class Glutton : Role
    {
        public static readonly byte RoleID = 109;

        private CooldownButton cooldownButton = null;

        public Glutton(PlayerControl player) : base(player)
        {
            name = "Glutton";
            color = new Color(0.6f, 0.4f, 0.2f, 1);
            colorAsHex = "996633FF";
            startText = "Eat the evidence";
        }

        /**
         * Sets the Role if spawn chance is reached.
         * Can only set Role if crew still has space for Role.
         * Removes crew free space on successful assignment.
         */
        public static void SetRole(List<PlayerControl> imps)
        {
            float spawnChance = MainConfig.OptGluttonSpawnChance.GetValue();
            if (spawnChance < 1) return;
            bool spawnChanceAchieved = Rng.Next(1, 101) <= spawnChance;
            if ((imps.Count <= 0 || !spawnChanceAchieved)) return;

            int random = Rng.Next(0, imps.Count);
            Glutton glutton = new Glutton(imps[random]);
            AddSpecialRole(glutton);
            imps.RemoveAt(random);

            MessageWriter writer = GetWriter(RPC.SetRole);
            writer.Write(RoleID);
            writer.Write(glutton.player.PlayerId);
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

        public override void ClearSettings()
        {
            player = null;
            cooldownButton = null;
        }

        protected override void SetConfigSettings() { }

        public override void CheckDead(HudManager instance) { }

        public void CreateEatingButton()
        {
            if (cooldownButton != null)
                return;

            ConsoleTools.Info("Creating eating button...");

            cooldownButton = new CooldownButton
            (
                () => OnClick(),
                MainConfig.OptGluttonEatingCooldown.GetValue(),
                new Vector2(0, 2),
                HudManager.Instance,
                eatingButton,
                () => OnUpdate(cooldownButton)
            );
        }

        private static void OnClick()
        {
            ConsoleTools.Info("Glutton clicked his button!");
        }

        private static void OnUpdate(CooldownButton button)
        {
            
        }
    }
}