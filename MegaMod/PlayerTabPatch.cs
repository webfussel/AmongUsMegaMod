﻿using HarmonyLib;
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
            float yMin = -0.05f;
            float add = 0.45f;

            float x = xMin;
            float y = yMin;
            for (int i = 0; i < __instance.ColorChips.Count; ++i)
            {
                if (i % columns == 0)
                {
                    x = xMin;
                    y -= add;
                }
                else
                {
                    x += add;
                }
                
                ColorChip chip = __instance.ColorChips[i];
                chip.transform.localPosition = new Vector3(x, y, -1f);
                chip.transform.localScale = chip.transform.localScale * 0.65f;
            }
        }
    }
}