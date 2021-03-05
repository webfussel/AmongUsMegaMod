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
            startText = "No one can hide from you";
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
                footsteps
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

                if (FootPrint.AllSorted.ContainsKey(player.PlayerId) && FootPrint.AllSorted[player.PlayerId].Count != 0)
                {
                    thisPlayersFootprints = FootPrint.AllSorted[player.PlayerId];
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

        private void CheckIfNewFootprint(PlayerControl _player)
        {
            if (Vector2.SqrMagnitude(FootPrint.LastFootprintPositions[_player.PlayerId] - _player.transform.position) > 0.025f && !_player.inVent)
                new FootPrint(_player);
        }

        public class FootPrint
        {
            private static float _lifespan;
            private static readonly Color AnonymousColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            private static bool _anonymous;
            private static Sprite _sprite;

            public static Dictionary<byte, List<FootPrint>> AllSorted;
            public static Dictionary<byte, Vector3> LastFootprintPositions;

            private readonly Color _color;
            private Vector3 Position { get; }
            private readonly GameObject _footPrint;
            private readonly SpriteRenderer _spriteRenderer;
            private readonly PlayerControl _player;
            private float _age;

            public static void Initialize(float lifespan, bool anonymous, Sprite sprite)
            {
                _lifespan = lifespan;
                _anonymous = anonymous;
                _sprite = sprite;

                AllSorted = new Dictionary<byte, List<FootPrint>>(PlayerControl.AllPlayerControls.Count);
                LastFootprintPositions = new Dictionary<byte, Vector3>(PlayerControl.AllPlayerControls.Count);

                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    LastFootprintPositions.Add(player.PlayerId, player.transform.position);
            }

            public FootPrint(PlayerControl player)
            {
                _player = player;
                _color = _anonymous ? AnonymousColor : (Color) Palette.PlayerColors[_player.Data.ColorId];
                _age = 0;

                _footPrint = new GameObject();
                _footPrint.transform.position = Position = _player.transform.position + Vector3.forward;
                _spriteRenderer = _footPrint.AddComponent<SpriteRenderer>();
                _spriteRenderer.sprite = _sprite;
                _spriteRenderer.color = _color;

                byte playerId = _player.PlayerId;
                if (LastFootprintPositions.ContainsKey(playerId))
                    LastFootprintPositions[playerId] = Position;
                else
                    LastFootprintPositions.Add(playerId, Position);

                if (AllSorted.ContainsKey(playerId))
                    AllSorted[playerId].Add(this);
                else
                    AllSorted.Add(playerId, new List<FootPrint> { this });
            }

            public void Update(float ageToAdd)
            {
                _age += ageToAdd;

                float alpha = Map(_age, 0, _lifespan, 0, 1);
                _spriteRenderer.color = AdjustAlpha(_color, 1 - alpha);

                if (_age >= _lifespan)
                {
                    AllSorted[_player.PlayerId].Remove(this);
                    Object.Destroy(_footPrint);
                }
            }
        }
    }
}
