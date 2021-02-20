using MegaMod;
using UnityEngine;

public abstract class Role {

    public PlayerControl player;
    public string name {get; set;}
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

    public void SetNameColor() => player.nameText.Color = color;
    
    public virtual void SetIntro(IntroCutscene.CoBegin__d instance)
    {
        instance.__this.Title.Text = name;
        instance.c = color;
        instance.__this.ImpostorText.Text = startText;
        instance.__this.BackgroundBar.material.color = color;
    }
}