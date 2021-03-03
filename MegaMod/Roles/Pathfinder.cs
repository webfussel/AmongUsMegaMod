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
                MainConfig.OptPathfinderAnonymousFootprints.GetValue(),
                footprintSprite
            );
        }

        public override void CheckDead(HudManager instance)
        {

        }

        public void FixedUpdate(float interval)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data.IsDead || player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    continue;

                List<FootPrint> thisPlayersFootprints;

                if (FootPrint.allSorted.ContainsKey(player.PlayerId) && FootPrint.allSorted[player.PlayerId].Count != 0)
                {
                    thisPlayersFootprints = FootPrint.allSorted[player.PlayerId];
                    for (int i = thisPlayersFootprints.Count - 1; i >= 0; i--)
                        thisPlayersFootprints[i].Update(interval);
                }
                else
                {
                    CheckIfNewFootprint(player);
                    continue;
                }

                CheckIfNewFootprint(player);
            }
        }

        private void CheckIfNewFootprint(PlayerControl player)
        {
            if (Vector2.SqrMagnitude(FootPrint.lastFootprintPositions[player.PlayerId] - player.transform.position) > 0.1f && !player.inVent)
                new FootPrint(player);
        }

        public class FootPrint
        {
            private static float lifespan;
            private static Color anonymousColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            private static bool anonymous;
            private static Sprite sprite;

            public static Dictionary<byte, List<FootPrint>> allSorted;
            public static Dictionary<byte, Vector3> lastFootprintPositions;

            private Color color;
            public Vector3 Position { get; private set; }
            private readonly GameObject footPrint;
            private readonly SpriteRenderer spriteRenderer;
            private readonly PlayerControl player;
            private float age;

            public static void Initialize(float _lifespan, bool _anonymous, Sprite _sprite)
            {
                lifespan = _lifespan;
                anonymous = _anonymous;
                sprite = _sprite;

                allSorted = new Dictionary<byte, List<FootPrint>>(PlayerControl.AllPlayerControls.Count);
                lastFootprintPositions = new Dictionary<byte, Vector3>(PlayerControl.AllPlayerControls.Count);

                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    lastFootprintPositions.Add(player.PlayerId, player.transform.position);
            }

            public FootPrint(PlayerControl _player)
            {
                player = _player;
                color = anonymous ? anonymousColor : (Color) Palette.PlayerColors[player.Data.ColorId];
                age = 0;

                footPrint = new GameObject();
                footPrint.transform.position = Position = player.transform.position + Vector3.forward;
                spriteRenderer = footPrint.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = color;

                byte playerId = player.PlayerId;
                if (lastFootprintPositions.ContainsKey(playerId))
                    lastFootprintPositions[playerId] = Position;
                else
                    lastFootprintPositions.Add(playerId, Position);

                if (allSorted.ContainsKey(playerId))
                    allSorted[playerId].Add(this);
                else
                    allSorted.Add(playerId, new List<FootPrint>() { this });
            }

            public void Update(float ageToAdd)
            {
                age += ageToAdd;

                float alpha = Map(age, 0, lifespan, 0, 1);
                spriteRenderer.color = AdjustAlpha(color, 1 - alpha);

                if (age >= lifespan)
                {
                    allSorted[player.PlayerId].Remove(this);
                    Object.Destroy(footPrint);
                }
            }
        }
    }
}
