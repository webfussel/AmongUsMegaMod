using UnityEngine;

namespace MegaMod
{
    public static class StringNamesPatch
    {
        public enum StringNames
        {
            ColorAqua = 2300,
            ColorNavy = 2301,
            ColorMint = 2302,
            ColorSnot = 2303,
            ColorMagenta = 2304,
            ColorPig = 2305,
            ColorWine = 2306,
            ColorRose = 2307,
            ColorLavender = 2308,
            ColorMaroon = 2309,
            ColorSand = 2310,
            ColorGrey = 2311,
            ColorIce = 2312,
            VitalsAqua = 2313,
            VitalsNavy = 2314,
            VitalsMint = 2315,
            VitalsSnot = 2316,
            VitalsMagenta = 2317,
            VitalsPig = 2318,
            VitalsWine = 2319,
            VitalsRose = 2320,
            VitalsLavender = 2321,
            VitalsMaroon = 2322,
            VitalsSand = 2323,
            VitalsGrey = 2324,
            VitalsIce = 2325
        }
    }
    public static class PalettePatch
    {
        public static readonly StringNames[] ColorNames =
        {
            (StringNames) 2300,
            (StringNames) 2301,
            (StringNames) 2302,
            (StringNames) 2303,
            (StringNames) 2304,
            (StringNames) 2305,
            (StringNames) 2306,
            (StringNames) 2307,
            (StringNames) 2308,
            (StringNames) 2309,
            (StringNames) 2310,
            (StringNames) 2311,
            (StringNames) 2312
        };

        public static readonly StringNames[] ShortColorNames =
        {
            (StringNames) 2313,
            (StringNames) 2314,
            (StringNames) 2315,
            (StringNames) 2316,
            (StringNames) 2317,
            (StringNames) 2318,
            (StringNames) 2319,
            (StringNames) 2320,
            (StringNames) 2321,
            (StringNames) 2322,
            (StringNames) 2323,
            (StringNames) 2324,
            (StringNames) 2325
        };

        public static readonly Color32[] PlayerColors =
        {
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

        public static readonly Color32[] ShadowColors =
        {
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