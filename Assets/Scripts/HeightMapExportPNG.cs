using System.CodeDom.Compiler;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts
{
    public class HeightmapExportPNG : EditorWindow
    {
        static TerrainData terraindata;
 
 
        [MenuItem("Window/Terrain to image")]
        static void Init()
        {
            terraindata = null;
            Terrain terrain = null;
 
            if (Selection.activeGameObject)
                terrain = Selection.activeGameObject.GetComponent<Terrain>();
 
            if (!terrain)
            {
                terrain = Terrain.activeTerrain;
            }
            if (terrain)
            {
                terraindata = terrain.terrainData;
            }
            if (terraindata == null)
            {
                EditorUtility.DisplayDialog("No terrain selected", "Please select a terrain.", "Cancel");
                return;
            }
 
            //// get the terrain heights into an array and apply them to a texture2D
            Texture2D duplicateHeightMap = new Texture2D(terraindata.heightmapResolution, terraindata.heightmapResolution, TextureFormat.ARGB32, false);
            float[,] rawHeights = terraindata.GetHeights(0, 0, terraindata.heightmapResolution, terraindata.heightmapResolution);
 
            // run through the array row by row
            for (int y = 0; y < duplicateHeightMap.height; y++)
            {
                for (int x = 0; x < duplicateHeightMap.width; x++)
                {
                    // for each pixel set RGB to the same so it's gray
                    var color = new Vector4(rawHeights[x,y], rawHeights[x,y], rawHeights[x,y], 1);
                    duplicateHeightMap.SetPixel(x, y, color);
                }
            }
            // Apply all SetPixel calls
            duplicateHeightMap.Apply();
 
            string path = EditorUtility.SaveFilePanel(
                "Save texture as",
                "",
                "heightMap",
                "png, jpg");
 
            var extension = Path.GetExtension(path);
            byte[] pngData = null;// duplicateHeightMap.EncodeToPNG();
 
            switch(extension)
            {
                case ".jpg":
                    pngData = duplicateHeightMap.EncodeToJPG();
                    break;
 
                case ".png":
                    pngData = duplicateHeightMap.EncodeToPNG();
                    break;
            }
 
            if (pngData != null)
            {
                File.WriteAllBytes(path, pngData);
                EditorUtility.DisplayDialog("Heightmap Duplicated", "Saved as" + extension + " in " + path, "Pog");
            }else
            {
                EditorUtility.DisplayDialog("Failed to duplicate height map", "WHAD ARE U DOIN??? Y U NO SAVE", "やばい");
            }
 
            AssetDatabase.Refresh();
        }
        
    }

    public static class StaticHeightMapExportPNG
    {
        public static void Generate(TerrainData terraindata)
        {
            if (terraindata == null)
            {
                EditorUtility.DisplayDialog("No terrain selected", "Please select a terrain.", "Cancel");
                return;
            }
 
            //// get the terrain heights into an array and apply them to a texture2D
            Texture2D duplicateHeightMap = new Texture2D(terraindata.heightmapResolution, terraindata.heightmapResolution, TextureFormat.ARGB32, false);
            float[,] rawHeights = terraindata.GetHeights(0, 0, terraindata.heightmapResolution, terraindata.heightmapResolution);
 
            // run through the array row by row
            for (int y = 0; y < duplicateHeightMap.height; y++)
            {
                for (int x = 0; x < duplicateHeightMap.width; x++)
                {
                    // for each pixel set RGB to the same so it's gray
                    var color = new Vector4(rawHeights[x,y], rawHeights[x,y], rawHeights[x,y], 1);
                    duplicateHeightMap.SetPixel(x, y, color);
                }
            }
            // Apply all SetPixel calls
            duplicateHeightMap.Apply(true, false);
 
            string path = EditorUtility.SaveFilePanel(
                "Save texture as",
                "",
                "heightMap",
                "png, jpg");
 
            var extension = Path.GetExtension(path);
            byte[] pngData = null;// duplicateHeightMap.EncodeToPNG();
 
            switch(extension)
            {
                case ".jpg":
                    pngData = duplicateHeightMap.EncodeToJPG();
                    break;
 
                case ".png":
                    pngData = duplicateHeightMap.EncodeToPNG();
                    break;
            }
 
            if (pngData != null)
            {
                File.WriteAllBytes(path, pngData);
                EditorUtility.DisplayDialog("Heightmap Duplicated", "Saved as" + extension + " in " + path, "Pog");
            }else
            {
                EditorUtility.DisplayDialog("Failed to duplicate height map", "WHAD ARE U DOIN??? Y U NO SAVE", "やばい");
            }
 
            AssetDatabase.Refresh();
        }
    }
}
