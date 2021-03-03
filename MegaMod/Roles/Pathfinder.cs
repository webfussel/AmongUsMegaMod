using System;
using System.Collections.Generic;
using Hazel;
using UnityEngine;
using static MegaMod.MegaModManager;
using static HelperMethods.HelperMethods;

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
            private static Color anonymousColor;
            private static bool anonymous;
            private static Sprite sprite;

            public static Dictionary<byte, List<FootPrint>> allSorted;

            private Color color;
            public Vector3 Position { get; private set; }
            private GameObject footPrint;
            private SpriteRenderer spriteRenderer;
            private readonly PlayerControl player;
            private float age;

            public static void Initialize(float _lifespan, Color _anonymousColor, bool _anonymous, Sprite _sprite)
            {
                lifespan = _lifespan;
                anonymousColor = _anonymousColor;
                anonymous = _anonymous;
                sprite = _sprite;
                allSorted = new Dictionary<byte, List<FootPrint>>();
            }

            public FootPrint(PlayerControl _player)
            {
                player = _player;
                color = anonymous ? anonymousColor : (Color) Palette.PlayerColors[player.Data.ColorId];
                age = 0;

                footPrint = new GameObject();
                footPrint.transform.position = Position = player.transform.position;
                spriteRenderer = footPrint.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = color;

                footPrint.SetActive(true);
                AddToDictionary(this, player.PlayerId);
            }

            public void Update(float ageToAdd)
            {
                age += ageToAdd;

                float alpha = Map(age, 0, lifespan, 0, 1);
                spriteRenderer.color = AdjustAlpha(color, 1 - alpha);

                if (age >= lifespan)
                {
                    RemoveFromDictionary(this);
                    UnityEngine.Object.Destroy(footPrint);
                }
            }

            private static void AddToDictionary(FootPrint fp, byte playerId)
            {
                if (allSorted.ContainsKey(playerId))
                    allSorted[playerId].Add(fp);
                else
                    allSorted.Add(playerId, new List<FootPrint>() { fp });
            }

            private static void RemoveFromDictionary(FootPrint fp) => allSorted[fp.player.PlayerId].Remove(fp);
        }
    }
}
