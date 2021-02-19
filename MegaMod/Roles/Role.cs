using UnityEngine;

public abstract class Role {

    public PlayerControl player;
    public string name {get; set;}
    public Color color {get; protected set;}
    public string startText {get; set;}

    public abstract void ClearSettings();
    public abstract void SetConfigSettings();
    public abstract void CheckDead(HudManager instance);
    public string EjectMessage(string playerName) => $"{playerName} was the {name}";

    public Role(PlayerControl player)
    {
        this.player = player;
        SetConfigSettings();
    }

    public void SetNameColor()
    {
        if (PlayerControl.LocalPlayer == player) player.nameText.Color = color;
    }
    public virtual void SetIntro(IntroCutscene.CoBegin__d instance)
    {
        instance.__this.Title.Text = name;
        instance.__this.Title.Color = color;
        instance.__this.ImpostorText.Text = startText;
        instance.__this.BackgroundBar.material.color = color;
    }
}