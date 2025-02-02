using System;
using System.Collections.Generic;
using System.Drawing;
using Hazel;
using UnityEngine;
using static MegaMod.MegaModManager;
using Color = UnityEngine.Color; // TODO: wtf?

namespace MegaMod.Roles
{
    public class Tracker : Role
    {
        private bool markTrapUsed;
        private readonly Sprite _specialButton;
        private static readonly Color _color = new Color(0.77f, 1f, 0.34f);

        public bool sabotageActive;
        public static readonly byte RoleID = 105;
        public SystemTypes? markedSystem;

        public Tracker(PlayerControl player) : base(player)
        {
            name = "Tracker";
            color = _color;
            colorAsHex = "C4FF56FF";
            startText = "Track down the [FF0000FF]Impostors[]";
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

        public void AdjustChat(HudManager instance, bool typingEnabled)
        {
            instance.Chat.gameObject.SetActive(true);
            instance.Chat.TypingArea.gameObject.SetActive(typingEnabled);           
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
            float percentage = markTrapUsed ? 1f : 0f;
            
            foreach (MapRoom room in instance.infectedOverlay.rooms)
            {
                if (room.special == null) continue;

                room.special.material.color = room.room == markedSystem ? _color : Color.white;
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

        public bool MarkSystem(SystemTypes system)
        {
            if (markTrapUsed || sabotageActive || player.Data.IsDead) return false;

            markTrapUsed = true;
            markedSystem = system;
            ConsoleTools.Info($"Marked {system}");

            MessageWriter writer = GetWriter(RPC.SetTrackerMark);
            writer.Write((int) markedSystem);
            CloseWriter(writer);

            return false;
        }

        public void OnSabotageHappened(SystemTypes system)
        {
            if (system != markedSystem) return;

            markTrapUsed = false;
            markedSystem = null;
            
            TrapSuccessful(SystemTypes.Admin);
            MessageWriter writer = GetWriter(RPC.TrapSuccessful);
            writer.Write((byte) currentRoomId);
            CloseWriter(writer);
        }

        public void TrapSuccessful(SystemTypes room)
        {
            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance && !player.Data.IsDead)
            {
                string roomName = DestroyableSingleton<TranslationController>.Instance.GetString(room);
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, $"The last known position of the saboteur was in {roomName}!");
            }
            
            markTrapUsed = false;
            markedSystem = null;
        }
    }
}