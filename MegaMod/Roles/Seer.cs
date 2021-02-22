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
        // TODO: Der Seer soll den Notfall-Button nicht benutzen können!

        public enum MessageType { EnteredVent, ExitedVent, Died }
        private readonly Dictionary<MessageType, string> messages = new Dictionary<MessageType, string>()
        {
            { MessageType.EnteredVent, "Someone entered a vent..." },
            { MessageType.ExitedVent,  "Someone exited a vent..." },
            { MessageType.Died,        "Someone died..." }
        };

        public Seer(PlayerControl player) : base(player)
        {
            name = "Seer";
            color = new Color(255f / 255f, 165f / 255f, 10f / 255f, 1); // TODO: Set unique color!
            startText = "Listen carefully to gather valuable informations!";
        }

        /*
        * Sets the Role if spawn chance is reached.
        * Can only set Role if crew still has space for Role.
        * Removes crew free space on successful assignment.
        */
        public static void SetRole(List<PlayerControl> crew)
        {
            float spawnChance = HarmonyMain.OptSeerSpawnChance.GetValue();
            if (spawnChance < 1) return;
            bool spawnChanceAchieved = Rng.Next(1, 101) <= spawnChance;
            if ((crew.Count <= 0 || !spawnChanceAchieved)) return;

            int random = Rng.Next(0, crew.Count);
            Seer seer = new Seer(crew[random]);
            AddSpecialRole(seer);
            crew.RemoveAt(random);
            
            MessageWriter writer = GetWriter(RPC.SetSeer);
            writer.Write(seer.player.PlayerId);
            CloseWriter(writer);
        }

        public override void ClearSettings()
        {
            player = null;
        }

        protected override void SetConfigSettings()
        {
            // do nothing
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