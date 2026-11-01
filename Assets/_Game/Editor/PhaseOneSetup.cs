using System;
using System.Linq;
using DoubleDown.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DoubleDown.Editor
{
    public static class PhaseOneSetup
    {
        private const string ScenePath = "Assets/_Game/Scenes/FeelSandbox.unity";
        private const string ProfilePath = "Assets/_Game/Settings/FeelProfile.asset";
        private const string TableMaterialPath = "Assets/_Game/Settings/CasinoVortex.mat";
        private const string CardInkMaterialPath = "Assets/_Game/Settings/CardInk.mat";
        private const string OutlineMaterialPath = "Assets/_Game/Settings/SelectionOutline.mat";

        [MenuItem("Double Down/Phase 1/Rebuild Both Scenes")]
        public static void RebuildAll()
        {
            PhaseZeroSetup.Configure();
            Build();
        }

        [MenuItem("Double Down/Phase 1/Build Feel Sandbox")]
        public static void Build()
        {
            if (GraphicsSettings.defaultRenderPipeline == null)
                throw new InvalidOperationException("URP is not assigned in Graphics Settings.");

            var profile = AssetDatabase.LoadAssetAtPath<FeelProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<FeelProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(ProfilePath, ImportAssetOptions.ForceUpdate);

            var faces = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/Cards/Kenney" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".png") && !path.EndsWith("card_back.png"))
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
                .ToArray();
            if (faces.Length != 52 || faces.Any(sprite => sprite == null))
                throw new InvalidOperationException("Expected 52 imported Kenney card sprites.");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            profile = LoadRequired<FeelProfile>(ProfilePath);
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 2.8125f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.075f, 0.075f, 1f);
            cameraObject.AddComponent<AudioListener>();
            ConfigurePixelPerfect(cameraObject);

            var lightObject = new GameObject("Global Light 2D");
            var globalLight = lightObject.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.intensity = 1f;

            var tableShader = LoadRequired<Shader>("Assets/Art/Shaders/CasinoVortex.shader");
            var tableMaterial = AssetDatabase.LoadAssetAtPath<Material>(TableMaterialPath);
            if (tableMaterial == null)
            {
                tableMaterial = new Material(tableShader) { name = "Casino Vortex" };
                AssetDatabase.CreateAsset(tableMaterial, TableMaterialPath);
            }
            else if (tableMaterial.shader != tableShader)
            {
                tableMaterial.shader = tableShader;
                EditorUtility.SetDirty(tableMaterial);
            }
            if (tableMaterial.name != "CasinoVortex")
            {
                tableMaterial.name = "CasinoVortex";
                EditorUtility.SetDirty(tableMaterial);
            }

            var cardInkShader = LoadRequired<Shader>("Assets/Art/Shaders/CardInk.shader");
            var cardInkMaterial = AssetDatabase.LoadAssetAtPath<Material>(CardInkMaterialPath);
            if (cardInkMaterial == null)
            {
                cardInkMaterial = new Material(cardInkShader) { name = "Card Ink" };
                AssetDatabase.CreateAsset(cardInkMaterial, CardInkMaterialPath);
            }
            else if (cardInkMaterial.shader != cardInkShader)
            {
                cardInkMaterial.shader = cardInkShader;
                EditorUtility.SetDirty(cardInkMaterial);
            }

            var outlineShader = LoadRequired<Shader>("Assets/Art/Shaders/SelectionOutline.shader");
            var outlineMaterial = AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterialPath);
            if (outlineMaterial == null)
            {
                outlineMaterial = new Material(outlineShader) { name = "Selection Outline" };
                AssetDatabase.CreateAsset(outlineMaterial, OutlineMaterialPath);
            }
            else if (outlineMaterial.shader != outlineShader)
            {
                outlineMaterial.shader = outlineShader;
                EditorUtility.SetDirty(outlineMaterial);
            }

            var tableObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            tableObject.name = "Casino Vortex (Shader)";
            tableObject.transform.position = new Vector3(0f, 0f, 1f);
            tableObject.transform.localScale = new Vector3(10f, 5.625f, 1f);
            UnityEngine.Object.DestroyImmediate(tableObject.GetComponent<Collider>());
            var tableRenderer = tableObject.GetComponent<MeshRenderer>();
            tableRenderer.sharedMaterial = tableMaterial;
            tableRenderer.sortingOrder = -200;

            var sandboxObject = new GameObject("Feel Sandbox Controller");
            var sandbox = sandboxObject.AddComponent<FeelSandboxController>();
            var serialized = new SerializedObject(sandbox);
            serialized.FindProperty("profileAsset").objectReferenceValue = profile;
            serialized.FindProperty("inputActions").objectReferenceValue =
                LoadRequired<UnityEngine.Object>("Assets/_Game/Settings/DoubleDownInput.inputactions");
            serialized.FindProperty("cardBack").objectReferenceValue =
                LoadRequired<Sprite>("Assets/Art/Cards/Kenney/card_back.png");
            serialized.FindProperty("cardShadow").objectReferenceValue =
                LoadRequired<Sprite>("Assets/_Placeholders/PH_CardShadow.png");
            serialized.FindProperty("sparkSprite").objectReferenceValue =
                LoadRequired<Sprite>("Assets/_Placeholders/PH_HoverSpark.png");
            serialized.FindProperty("cardInkMaterial").objectReferenceValue = cardInkMaterial;
            serialized.FindProperty("selectionOutlineMaterial").objectReferenceValue = outlineMaterial;

            var faceProperty = serialized.FindProperty("cardFaces");
            faceProperty.arraySize = faces.Length;
            for (var i = 0; i < faces.Length; i++)
                faceProperty.GetArrayElementAtIndex(i).objectReferenceValue = faces[i];

            var names = new[]
            {
                "Hover", "Pickup", "Drop", "Swap", "Flip", "Deal", "Select"
            };
            var soundProperty = serialized.FindProperty("soundClips");
            soundProperty.arraySize = names.Length;
            for (var i = 0; i < names.Length; i++)
                soundProperty.GetArrayElementAtIndex(i).objectReferenceValue =
                    LoadRequired<AudioClip>("Assets/Audio/SFX/Kenney/SFX_Card" + names[i] + ".ogg");

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(sandbox);
            if (serialized.FindProperty("profileAsset").objectReferenceValue == null)
                throw new InvalidOperationException("Feel Profile was not assigned to the sandbox controller.");
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save Feel Sandbox scene.");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene("Assets/_Game/Scenes/Bootstrap.unity", false)
            };
            AssetDatabase.SaveAssets();
            Debug.Log("DOUBLE DOWN Phase 1: Feel Sandbox scene built with 52 cards and 7 SFX.");
        }

        private static T LoadRequired<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new InvalidOperationException("Missing or invalid asset: " + path);
            return asset;
        }

        internal static void ConfigurePixelPerfect(GameObject cameraObject)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("UnityEngine.Rendering.Universal.PixelPerfectCamera", false))
                .FirstOrDefault(found => found != null);
            if (type == null) throw new InvalidOperationException("URP Pixel Perfect Camera is unavailable.");

            var component = cameraObject.AddComponent(type);
            Set(component, "assetsPPU", 64);
            Set(component, "refResolutionX", 640);
            Set(component, "refResolutionY", 360);
            SetEnum(component, "gridSnapping", "UpscaleRenderTexture");
            SetEnum(component, "cropFrame", "Windowbox");
            var settings = new SerializedObject(component);
            settings.FindProperty("m_FilterMode").enumValueIndex = 1;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(Component component, string propertyName, object value)
        {
            var property = component.GetType().GetProperty(propertyName);
            if (property == null || !property.CanWrite)
                throw new MissingMemberException(component.GetType().FullName, propertyName);
            property.SetValue(component, value);
        }

        private static void SetEnum(Component component, string propertyName, string value)
        {
            var property = component.GetType().GetProperty(propertyName);
            if (property == null || !property.CanWrite)
                throw new MissingMemberException(component.GetType().FullName, propertyName);
            property.SetValue(component, Enum.Parse(property.PropertyType, value));
        }
    }
}
