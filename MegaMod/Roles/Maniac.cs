using System.Collections.Generic;
using Hazel;
using UnityEngine;
using static MegaMod.MegaModManager; // TODO: wtf?

namespace MegaMod.Roles
{
    public class Maniac : Role
    {
        public bool showImpostorToManiac { get; private set; }
        public bool maniacCanDieToDetective { get; private set; }


        public Maniac(PlayerControl player) : base(player)
        {
            name = "Maniac";
            color = new Color(138f / 255f, 138f / 255f, 138f / 255f, 1);
            startText = "Get voted off of the ship to win";
        }
    
        public static void SetRole(List<PlayerControl> crew)
        {
            float spawnChance = MainConfig.OptManiacSpawnChance.GetValue();
            if (spawnChance < 1) return;
            bool spawnChanceAchieved = Rng.Next(1, 101) <= spawnChance;
            if ((crew.Count <= 0 || !spawnChanceAchieved)) return;
        
            int random = Rng.Next(0, crew.Count);
            Maniac maniac = new Maniac(crew[random]);
            AddSpecialRole(maniac);
            crew.RemoveAt(random);

            MessageWriter writer = GetWriter(RPC.SetManiac);
            writer.Write(maniac.player.PlayerId);
            CloseWriter(writer);
        }
        public override void ClearSettings()
        {
            player = null;
        }

        public void ClearTasks()
        {
            if (player == null) return;
            
            var tasksToRemove = new List<PlayerTask>();
            foreach (PlayerTask task in player.myTasks)
                if (task.TaskType != TaskTypes.FixComms && task.TaskType != TaskTypes.FixLights && task.TaskType != TaskTypes.ResetReactor && task.TaskType != TaskTypes.ResetSeismic && task.TaskType != TaskTypes.RestoreOxy)
                    tasksToRemove.Add(task);
            foreach (PlayerTask task in tasksToRemove)
                player.RemoveTask(task);
        }

        protected override void SetConfigSettings()
        {
            showImpostorToManiac = MainConfig.OptManiacShowImpostor.GetValue();
            maniacCanDieToDetective = MainConfig.OptManiacCanDieToDetective.GetValue();
        }

        public override void CheckDead(HudManager instance)
        {
        }

        public override void SetIntro(IntroCutscene.CoBegin__d instance)
        {
            base.SetIntro(instance);
            var maniacTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            maniacTeam.Add(player);
            instance.yourTeam = maniacTeam;
        }
    }
}