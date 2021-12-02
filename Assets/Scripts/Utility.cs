using System.Linq;
using UnityEditor;
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
        
        public static void SetTextureImporterFormat( Texture2D texture, bool isReadable)
        {
            if ( null == texture ) return;

            string assetPath = AssetDatabase.GetAssetPath( texture );
            var tImporter = AssetImporter.GetAtPath( assetPath ) as TextureImporter;
            if ( tImporter != null )
            {
                tImporter.textureType = TextureImporterType.Default;

                tImporter.isReadable = isReadable;

                AssetDatabase.ImportAsset( assetPath );
                AssetDatabase.Refresh();
            }
        }
    }
}
