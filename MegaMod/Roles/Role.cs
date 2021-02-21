using UnityEngine;

namespace MegaMod.Roles
{
    public abstract class Role {

        public PlayerControl player;
        public string name {get; protected set;}
        protected Color color {get; set;}
        protected string startText {get; set;}

        public abstract void ClearSettings();
        protected abstract void SetConfigSettings();
        public abstract void CheckDead(HudManager instance);
        public string EjectMessage(string playerName) => $"{playerName} was the {name}";

        protected Role(PlayerControl player)
        {
            this.player = player;
            SetConfigSettings();
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
            instance.c = color;
            instance.__this.ImpostorText.Text = startText;
            instance.__this.BackgroundBar.material.color = color;
        }
    }
}