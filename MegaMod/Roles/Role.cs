using System;
using UnityEngine;

public class Role {

    public PlayerControl player;
    public string name {get; set;}
    public Color color {get; set;}
    public string startText {get; set;}

    public static string ejectMessage(string playerName) {
        return $"{playerName} was the {name}";
    }

}