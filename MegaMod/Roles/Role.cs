using Reactor.Extensions;
using UnityEngine;

namespace MegaMod.Roles
{
    public abstract class Role {

        public PlayerControl player;
        public string name {get; protected set;}
        public Color color {get; protected set;}
        protected string colorAsHex {get; set;}
        protected Color borderColor { get; set; } = new Color(0, 0, 0, 1);
        protected string startText {get; set;}
        
        protected Vector3 titleScale { get; set; } = new Vector3(1, 1, 1);

        public abstract void ClearSettings();
        protected abstract void SetConfigSettings();
        public abstract void CheckDead(HudManager instance);
        public string EjectMessage(string playerName) => $"{playerName} was the {name}";

        protected Role(PlayerControl player)
        {
            this.player = player;
            SetConfigSettings();
            SetRoleDescription();
        }

        public void SetNameColor()
        {

            if (MeetingHud.Instance != null)
            {
                foreach (PlayerVoteArea playerVote in MeetingHud.Instance.playerStates)
                    if (player.PlayerId == playerVote.TargetPlayerId)
                        playerVote.NameText.Color = color;
            }
            else
                player.nameText.Color = color;
        }

        public virtual void SetIntro(IntroCutscene.CoBegin__d instance)
        {
            instance.__this.Title.Text = name;
            instance.__this.Title.render?.material?.SetColor("_OutlineColor", borderColor);
            instance.__this.Title.transform.localScale = titleScale;
            instance.c = color;
            instance.__this.ImpostorText.Text = startText;
            instance.__this.BackgroundBar.material.color = color;
        }

        public void SetRoleDescription()
        {
            ImportantTextTask roleDescription = new GameObject("roleDescription").AddComponent<ImportantTextTask>();
            roleDescription.transform.SetParent(player.transform, false);
            roleDescription.Text = $"[{colorAsHex}]You are the {name}![]";
            player.myTasks.Insert(0, roleDescription);
        }
    }
}