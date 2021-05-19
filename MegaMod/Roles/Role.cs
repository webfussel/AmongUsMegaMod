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
                        playerVote.NameText.m_Color = color;
            }
            else
                player.nameText.m_Color = color;
        }

        public virtual void SetIntro(IntroCutscene._CoBegin_d__11 instance)
        {
            instance.__4__this.Title.m_text = name;
            instance.__4__this.Title.m_renderer?.material?.SetColor("_OutlineColor", borderColor);
            instance.__4__this.Title.transform.localScale = titleScale;
            instance._c_5__2 = color;
            instance.__4__this.ImpostorText.m_text = startText;
            instance.__4__this.BackgroundBar.material.color = color;
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