using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public static class Utility
    { 
        /*
        * Find the average value of a jagged array.
        */
        public static float JaggedArrAvg(float[,] arr)
        {
            return arr.Cast<float>().Sum() / arr.Length;
        }

        public static float Saturate(float v)
        {
            return Mathf.Max(0, Mathf.Min(1, v));
        }
    }
}
