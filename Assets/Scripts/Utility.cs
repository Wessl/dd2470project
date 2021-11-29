using System.Linq;

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
    }
}
