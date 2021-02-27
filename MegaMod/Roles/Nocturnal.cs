﻿using System;
using System.Collections.Generic;
using Hazel;
using Reactor.Extensions;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod.Roles
{
    public class Nocturnal : Role
    {
        public static readonly byte RoleID = 106;

        public Nocturnal(PlayerControl player) : base(player)
        {
            name = "Nocturnal";
            color = new Color(0.49f, 0.8f, 1f);
            startText = "The night is your home";
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
            Nocturnal nocturnal= new Nocturnal(crew[random]);
            AddSpecialRole(nocturnal);
            crew.RemoveAt(random);

            MessageWriter writer = GetWriter(RPC.SetRole);
            writer.Write(RoleID);
            writer.Write(nocturnal.player.PlayerId);
            CloseWriter(writer);
        }

        public override void ClearSettings()
        {
            player = null;
        }

        protected override void SetConfigSettings()
        {
            
        }

        public override void CheckDead(HudManager instance)
        {

        }
    }
}