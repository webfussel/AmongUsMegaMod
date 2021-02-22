using System;
using System.Collections.Generic;
using Hazel;
using Reactor.Extensions;
using UnityEngine;
using static MegaMod.MegaModManager; // TODO: wtf?

namespace MegaMod.Roles
{
    public class Engineer : Role
    {
        private bool _repairUsed;
        public bool sabotageActive { get; set; }
        private readonly Sprite _specialButton;

        public Engineer(PlayerControl player) : base(player)
        {
            name = "Engineer";
            color = new Color(255f / 255f, 165f / 255f, 10f / 255f, 1);
            startText = "Maintain important systems on the ship";
            _specialButton = repairButton;
        }

        /**
     * Sets the Role if spawn chance is reached.
     * Can only set Role if crew still has space for Role.
     * Removes crew free space on successful assignment.
     */
        public static void SetRole(List<PlayerControl> crew)
        {
            float spawnChance = MainConfig.OptEngineerSpawnChance.GetValue();
            if (spawnChance < 1) return;
            bool spawnChanceAchieved = Rng.Next(1, 101) <= spawnChance;
            if ((crew.Count <= 0 || !spawnChanceAchieved)) return;

            int random = Rng.Next(0, crew.Count);
            Engineer engineer = new Engineer(crew[random]);
            AddSpecialRole(engineer);
            crew.RemoveAt(random);
            
            MessageWriter writer = GetWriter(RPC.SetEngineer);
            writer.Write(engineer.player.PlayerId);
            CloseWriter(writer);
        }

        public override void ClearSettings()
        {
            player = null;
            _repairUsed = false;
        }

        protected override void SetConfigSettings()
        {
            // do nothing
        }

        public void CheckRepairButton(HudManager instance)
        {
            if (player == null || player.PlayerId != PlayerControl.LocalPlayer.PlayerId ||
                !instance.UseButton.isActiveAndEnabled || player.Data.IsDead) return;
        
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

        public bool ShowRepairMap()
        {
            DestroyableSingleton<HudManager>.Instance.ShowMap((Action<MapBehaviour>)delegate (MapBehaviour m)
            {
                m.ShowInfectedMap();
                m.ColorControl.baseColor = sabotageActive ? Color.gray : color;
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
                // here was room.door.gameObject.active = false
            }
        }

        public void UpdateMap(MapBehaviour instance)
        {
            if (!instance.IsOpen || !instance.infectedOverlay.gameObject.active) return;

            instance.ColorControl.baseColor = !sabotageActive ? Color.gray : color;
            float percentage = _repairUsed ? 1f : 0f;
            
            foreach (MapRoom room in instance.infectedOverlay.rooms)
            {
                if (room.special == null) continue;
                
                room.special.material.SetFloat("_Desat", !sabotageActive ? 1f : 0f);
                room.special.enabled = true;
                room.special.gameObject.SetActive(true);
                room.special.material.SetFloat("_Percent", !PlayerControl.LocalPlayer.Data.IsDead ? percentage : 1f);
            }
        }
        
        // Activates the buttons for the emergencies
        public bool SetRepairButtonsActive()
        {
            return false;
        }

        private bool CanRepair()
        {
            return !_repairUsed && sabotageActive && !player.Data.IsDead;
        }

        public bool RepairReactor()
        {
            if (!CanRepair()) return false;
            
            _repairUsed = true;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
            return false;
        }

        public bool RepairLight()
        {
            if (!CanRepair()) return false;
            
            _repairUsed = true;
            SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
            WriteImmediately(RPC.FixLights);
            return false;
        }

        public bool RepairComms()
        {
            if (!CanRepair()) return false;
            
            _repairUsed = true;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
            return false;
        }

        public bool RepairOxy()
        {
            
            if (!CanRepair()) return false;
            
            _repairUsed = true;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
            return false;
        }

        public bool RepairSeismic()
        {
            if (!CanRepair()) return false;
            
            _repairUsed = true;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);
            return false;
        }
    }
}