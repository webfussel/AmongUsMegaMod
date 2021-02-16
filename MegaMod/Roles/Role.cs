using UnityEngine;

public abstract class Role {

    public PlayerControl player;
    public string name {get; set;}
    public Color color {get; protected set;}
    public string startText {get; set;}

    public abstract void ClearSettings();
    public abstract void SetConfigSettings();
    public string EjectMessage(string playerName) {
        return $"{playerName} was the {name}";
    }
    public void setIntro(IntroCutscene.CoBegin__d __instance)
    {
        if (PlayerControl.LocalPlayer == player)
        {
            __instance.__this.Title.Text = name;
            __instance.__this.Title.Color = color;
            __instance.__this.ImpostorText.Text = startText;
            __instance.__this.BackgroundBar.material.color = color;
        }
    }
}