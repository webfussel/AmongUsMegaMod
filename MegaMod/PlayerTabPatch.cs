using System;
using HarmonyLib;
using UnityEngine;

namespace MegaMod
{
    [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
    public class PlayerTabPatch
    {
        static void Postfix(PlayerTab __instance)
        {
            int columns = 5;

            float xMin = 1.45f;

            float scale = 0.65f;
            float add = 0.45f;

            float x = xMin;
            float y = -0.05f;
            for (int chipIndex = 0; chipIndex < __instance.ColorChips.Count; ++chipIndex)
            {
                if (chipIndex % columns == 0)
                {
                    x = xMin;
                    y -= add;
                }
                else
                {
                    x += add;
                }
                
                ColorChip chip = (ColorChip) __instance.ColorChips[(Index) chipIndex];
                chip.transform.localPosition = new Vector3(x, y, -1f);
                chip.transform.localScale *= scale;
            }
        }
    }
}