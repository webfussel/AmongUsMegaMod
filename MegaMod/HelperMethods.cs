using System.Collections.Generic;
using UnityEngine;

namespace HelperMethods
{
    public static class HelperMethods
    {
        public static float Map(float value, float from_min, float from_max, float to_min, float to_max)
        {
            if (value <= from_min)
                return to_min;
            else if (value >= from_max)
                return to_max;
            return (to_max - to_min) * ((value - from_min) / (from_max - from_min)) + to_min;
        }

        public static Color AdjustAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
    }
}
