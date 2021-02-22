using System;
using System.Collections.Generic;
using Hazel;
using Reactor.Extensions;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod.Roles
{
    public class Seer : Role
    {
        public static readonly byte RoleID = 104;
        
        public enum MessageType { EnteredVent, ExitedVent, Died }
        private readonly Dictionary<MessageType, string> messages = new Dictionary<MessageType, string>()
        {
            { MessageType.EnteredVent, "Someone entered a vent..." },
            { MessageType.ExitedVent,  "Someone exited a vent..." },
            { MessageType.Died,        "Someone died..." }
        };
        
        private bool canCallEmergency { get; set; }

        public Seer(PlayerControl player) : base(player)
        {
            name = "Seer";
            color = new Color(1f, 0.71f, 0.92f);
            // Old message was too long
            startText = "You know things...";
        }

        /*
        * Sets the Role if spawn chance is reached.
        * Can only set Role if crew still has space for Role.
        * Removes crew free space on successful assignment.
        */
        public static void SetRole(List<PlayerControl> crew)
        {
            float spawnChance = MainConfig.OptSeerSpawnChance.GetValue();
            if (spawnChance < 1) return;
            bool spawnChanceAchieved = Rng.Next(1, 101) <= spawnChance;
            if ((crew.Count <= 0 || !spawnChanceAchieved)) return;

            int random = Rng.Next(0, crew.Count);
            Seer seer = new Seer(crew[random]);
            AddSpecialRole(seer);
            crew.RemoveAt(random);
            
            MessageWriter writer = GetWriter(RPC.SetRole);
            writer.Write(RoleID);
            writer.Write(seer.player.PlayerId);
            CloseWriter(writer);
        }

        public void SetChatActive(HudManager instance)
        {
            instance.Chat.gameObject.SetActive(true);
        }

        public void SetEmergencyButtonInactive(EmergencyMinigame instance)
        {
            if (canCallEmergency) return;
            instance.StatusText.Text = "You can't call an Emergency!";
            instance.NumberText.Text = string.Empty;
            instance.ButtonActive = false;
            instance.ClosedLid.gameObject.SetActive(true);
            instance.OpenLid.gameObject.SetActive(false);
        }

        public override void ClearSettings()
        {
            player = null;
        }

        protected override void SetConfigSettings()
        {
            canCallEmergency = MainConfig.OptSeerCanPressEmergency.GetValue();
        }

        public override void CheckDead(HudManager instance)
        {

        }

        public void SendChatMessage(MessageType type)
        {
            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance && !player.Data.IsDead)
            {
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, messages[type]);
            }
        }
    }
}