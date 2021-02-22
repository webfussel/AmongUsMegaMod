using System;
using System.Collections.Generic;
using Hazel;
using Reactor.Extensions;
using UnityEngine;
using static MegaMod.MegaModManager; // TODO: wtf?

namespace MegaMod.Roles
{
    public class Tracker : Role
    {
        public static readonly byte RoleID = 105;
        
        private bool markTrapUsed;

        private readonly Sprite _specialButton;

        private bool sabotageActive;

        private SystemTypes? markedSystem;

        public Tracker(PlayerControl player) : base(player)
        {
            name = "Engineer";
            color = new Color(255f / 255f, 165f / 255f, 10f / 255f, 1); // TODO: Give unique color
            startText = "Track down the [FF0000FF]Impostors";
            _specialButton = markTrapButton;
        }

        /**
     * Sets the Role if spawn chance is reached.
     * Can only set Role if crew still has space for Role.
     * Removes crew free space on successful assignment.
     */
        public static void SetRole(List<PlayerControl> crew)
        {
            float spawnChance = MainConfig.OptTrackerSpawnChance.GetValue();
            if (spawnChance < 1) return;
            bool spawnChanceAchieved = Rng.Next(1, 101) <= spawnChance;
            if ((crew.Count <= 0 || !spawnChanceAchieved)) return;

            int random = Rng.Next(0, crew.Count);
            Tracker tracker = new Tracker(crew[random]);
            AddSpecialRole(tracker);
            crew.RemoveAt(random);
            
            MessageWriter writer = GetWriter(RPC.SetRole);
            writer.Write(RoleID);
            writer.Write(tracker.player.PlayerId);
            CloseWriter(writer);
        }

        public override void ClearSettings()
        {
            player = null;
            markTrapUsed = false;
            sabotageActive = false;
            markedSystem = null;
        }

        protected override void SetConfigSettings()
        {
            // do nothing
        }

        public void CheckMarkButton(HudManager instance)
        {
            if (player == null || player.PlayerId != PlayerControl.LocalPlayer.PlayerId ||
                !instance.UseButton.isActiveAndEnabled || player.Data.IsDead || !sabotageActive) return;
        
            KillButtonManager killButton = instance.KillButton;
            killButton.gameObject.SetActive(true);
            killButton.isActive = true;
            killButton.renderer.enabled = true;
            killButton.SetCoolDown(0f, 1f);
            killButton.renderer.sprite = _specialButton;
            killButton.renderer.color = Palette.EnabledColor;
            killButton.renderer.material.SetFloat("_Desat", 0f);
        }

        public override void CheckDead(HudManager instance)
        {
            if (!player.Data.IsDead) return;
            
            KillButtonManager killButton = instance.KillButton;
            killButton.gameObject.SetActive(false);
            killButton.renderer.enabled = false;
        }

        public void SetSabotageState(bool active)
        {
            if(active == sabotageActive) return;
            
            if(active == true)
            {
                // Check if the tracker marked correctly
                // If yes and the saboteur is alive, tell the tracker their name
                // If the saboteur isn't alive, tell the tracker that
            }
            else
            {
                markTrapUsed = false;
                markedSystem = null;
            }

            sabotageActive = active;
        }

        public bool ShowMarkTrapMap()
        {
            DestroyableSingleton<HudManager>.Instance.ShowMap((Action<MapBehaviour>)delegate (MapBehaviour m)
            {
                m.ShowInfectedMap();
                m.ColorControl.baseColor = !sabotageActive ? Color.gray : color;
            });
            return false;
        }

        public void OpenMap(MapBehaviour instance)
        {
            if (!instance.IsOpen) return;

            instance.ColorControl.baseColor = color;
            foreach (MapRoom room in instance.infectedOverlay.rooms)
            {
                if (room.door == null) continue;
                
                room.door.enabled = false;
                room.door.gameObject.SetActive(false);
            }
        }

        public void UpdateMap(MapBehaviour instance)
        {
            if (!instance.IsOpen || !instance.infectedOverlay.gameObject.active) return;

            instance.ColorControl.baseColor = sabotageActive ? Color.gray : color;
            float percentage = _repairUsed ? 1f : 0f;
            
            foreach (MapRoom room in instance.infectedOverlay.rooms)
            {
                if (room.special == null) continue;
                
                room.special.material.SetFloat("_Desat", sabotageActive ? 1f : 0f);
                room.special.enabled = true;
                room.special.gameObject.SetActive(true);
                room.special.material.SetFloat("_Percent", !PlayerControl.LocalPlayer.Data.IsDead ? percentage : 1f);
            }
        }
        
        public bool SetMarkButtonsActive()
        {
            return false;
        }

        private bool CanMark()
        {
            return !markTrapUsed && sabotageActive && !player.Data.IsDead;
        }

        public bool MarkReactor()
        {
            if (!CanMark()) return false;
            
            markTrapUsed = true;
            markedSystem = SystemTypes.Reactor;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
            return false;
        }

        public bool MarkLight()
        {
            if (!CanMark()) return false;
            
            markTrapUsed = true;
            markedSystem = SystemTypes.Electrical;
            SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
            WriteImmediately(RPC.FixLights);
            return false;
        }

        public bool MarkComms()
        {
            if (!CanMark()) return false;
            
            markTrapUsed = true;
            markedSystem = SystemTypes.Comms;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
            return false;
        }

        public bool MarkOxy()
        {
            
            if (!CanMark()) return false;
            
            markTrapUsed = true;
            markedSystem = SystemTypes.LifeSupp;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
            return false;
        }

        public bool MarkSeismic()
        {
            if (!CanMark()) return false;
            
            markTrapUsed = true;
            markedSystem = SystemTypes.Laboratory;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);
            return false;
        }
    }
}