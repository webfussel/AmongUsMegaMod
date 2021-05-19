using UnityEngine;

namespace MegaMod
{
    public static class PalettePatch
    {
      public static readonly StringNames[] ColorNames =
      {
        "Aqua",
        "Navy",
        "Mint",
        "Snot",
        "Magenta",
        "Pig",
        "Wine",
        "Rose",
        "Lavender",
        "Maroon",
        "Sand",
        "Grey",
        "Ice"
      };
      
      public static readonly StringNames[] ShortColorNames = {
        "AQUA",
        "NAVY",
        "MINT",
        "SNOT",
        "MGTA",
        "PIG",
        "WINE",
        "ROSE",
        "LVDR",
        "MARN",
        "SAND",
        "GREY",
        "ICE"
      };
      public static readonly Color32[] PlayerColors = {
        new Color32(0, 150, 136, byte.MaxValue),
        new Color32(32, 47, 82, byte.MaxValue),
        new Color32(220, 235, 196, byte.MaxValue),
        new Color32(134, 120, 37, byte.MaxValue),
        new Color32(194, 0, 255, byte.MaxValue),
        new Color32(247, 197, 159, byte.MaxValue),
        new Color32(85, 35, 58, byte.MaxValue),
        new Color32(242, 215, 232, byte.MaxValue),
        new Color32(223, 214, 240, byte.MaxValue),
        new Color32(97, 29, 0, byte.MaxValue),
        new Color32(167, 134, 99, byte.MaxValue),
        new Color32(149, 149, 149, byte.MaxValue),
        new Color32(166, 217, 220, byte.MaxValue)
      };
      public static readonly Color32[] ShadowColors = {
        new Color32(0, 112, 101, byte.MaxValue),
        new Color32(29, 34, 54, byte.MaxValue),
        new Color32(155, 188, 119, byte.MaxValue),
        new Color32(85, 83, 37, byte.MaxValue),
        new Color32(127, 0, 167, byte.MaxValue),
        new Color32(239, 159, 142, byte.MaxValue),
        new Color32(80, 5, 39, byte.MaxValue),
        new Color32(198, 135, 168, byte.MaxValue),
        new Color32(160, 132, 192, byte.MaxValue),
        new Color32(61, 11, 4, byte.MaxValue),
        new Color32(111, 94, 76, byte.MaxValue),
        new Color32(103, 103, 103, byte.MaxValue),
        new Color32(71, 121, 156, byte.MaxValue)
      };
    }
}