using System;
using System.Collections.Generic;
using Hazel;
using UnityEngine;
using static MegaMod.MegaModManager;

namespace MegaMod.Roles
{
    public class Pathfinder : Role
    {
        public static readonly byte RoleID = 108;

        public Pathfinder(PlayerControl player) : base(player)
        {
            name = "Pathfinder";
            color = new Color(0f, 0.2f, 0f);
            colorAsHex = "003300FF";
            startText = "Noone can hide from you";
        }

        /*
        * Sets the Role if spawn chance is reached.
        * Can only set Role if crew still has space for Role.
        * Removes crew free space on successful assignment.
        */
        public static void SetRole(List<PlayerControl> crew)
        {
            float spawnChance = MainConfig.OptPathfinderSpawnChance.GetValue();
            if (spawnChance < 1) return;
            bool spawnChanceAchieved = Rng.Next(1, 101) <= spawnChance;
            if ((crew.Count <= 0 || !spawnChanceAchieved)) return;

            int random = Rng.Next(0, crew.Count);
            Pathfinder pathfinder = new Pathfinder(crew[random]);
            AddSpecialRole(pathfinder);
            crew.RemoveAt(random);

            MessageWriter writer = GetWriter(RPC.SetRole);
            writer.Write(RoleID);
            writer.Write(pathfinder.player.PlayerId);
            CloseWriter(writer);
        }

        public override void ClearSettings()
        {
            player = null;
        }

        protected override void SetConfigSettings()
        {
            FootPrint.Initialize
            (
                MainConfig.OptPathfinderFootprintLifespan.GetValue(),
                MainConfig.OptPathfinderFootprintInterval.GetValue(),
                new Color(0.2f, 0.2f, 0.2f, 1f),
                MainConfig.OptPathfinderAnonymousFootprints.GetValue(),
                footprintSprite
            );
        }

        public override void CheckDead(HudManager instance)
        {

        }

        public class FootPrint
        {
            private static float lifespan;
            private static float interval;
            private static Color anonymousColor;
            private static bool anonymous;
            private static Sprite sprite;

            public static Dictionary<PlayerControl, List<FootPrint>> allSorted;

            private Color color;
            public Vector3 Position { get; private set; }
            private readonly float footPrintDuration;
            private readonly int footPrintUnixTime;
            private GameObject footPrint;
            private SpriteRenderer spriteRenderer;
            private readonly PlayerControl player;

            public static void Initialize(float _lifespan, float _interval, Color _anonymousColor, bool _anonymous, Sprite _sprite)
            {
                lifespan = _lifespan * 1000;
                interval = _interval;
                anonymousColor = _anonymousColor;
                anonymous = _anonymous;
                sprite = _sprite;
                allSorted = new Dictionary<PlayerControl, List<FootPrint>>();
            }

            public FootPrint(PlayerControl _player)
            {
                player = _player;
                color = anonymous ? anonymousColor : (Color)Palette.PlayerColors[player.Data.ColorId];
                footPrintUnixTime = (int) DateTimeOffset.Now.ToUnixTimeSeconds();

                footPrint = new GameObject();
                footPrint.transform.position = footPrint.transform.localPosition = Position = player.transform.position;
                spriteRenderer = footPrint.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = color;

                footPrint.SetActive(true);
                AddToDictionary(this);
            }

            public void Update()
            {
                int currentUnixTime = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
                float alpha = Map(currentUnixTime, footPrintUnixTime, footPrintUnixTime + lifespan, 0, 1);
                ConsoleTools.Info($"Current time: {currentUnixTime}, footprint birth: {footPrintUnixTime}, footprint death: {footPrintUnixTime + lifespan}");


                spriteRenderer.color = AdjustAlpha(alpha);

                if (footPrintUnixTime + (int)footPrintDuration < currentUnixTime)
                {
                    RemoveFromDictionary(this);
                    UnityEngine.Object.Destroy(footPrint);
                }
            }

            private Color AdjustAlpha(float alpha) => new Color(color.r, color.g, color.b, alpha);

            private static void AddToDictionary(FootPrint fp)
            {
                if (allSorted.ContainsKey(fp.player))
                    allSorted[fp.player].Add(fp);
                allSorted.Add(fp.player, new List<FootPrint>() { fp });
            }

            private static void RemoveFromDictionary(FootPrint fp) => allSorted[fp.player].Remove(fp);

            private static float Map(float value, float from_min, float from_max, float to_min, float to_max)
            {
                if (value <= from_min)
                    return to_min;
                else if (value >= from_max)
                    return to_max;
                return (to_max - to_min) * ((value - from_min) / (from_max - from_min)) + to_min;
            }
        }
    }
}
