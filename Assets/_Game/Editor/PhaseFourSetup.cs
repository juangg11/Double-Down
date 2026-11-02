using UnityEditor;
using UnityEngine;

namespace DoubleDown.Editor
{
    public static class PhaseFourSetup
    {
        [MenuItem("Double Down/Phase 4/Build Presentation")]
        public static void Build()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/TablePresentation" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spritePixelsPerUnit = 64;
                importer.SaveAndReimport();
            }
            const string pathMaterial = "Assets/Resources/TablePresentation/PixelGlass.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(pathMaterial) == null)
                AssetDatabase.CreateAsset(new Material(Shader.Find("DoubleDown/PixelGlass")), pathMaterial);
            AssetDatabase.SaveAssets();
            PhaseTwoSetup.Build();
            Debug.Log("DOUBLE DOWN Phase 4: presentation built.");
        }
    }
}
