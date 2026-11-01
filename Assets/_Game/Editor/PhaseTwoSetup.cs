using System;
using System.Linq;
using DoubleDown.Blackjack;
using DoubleDown.Presentation;
using DoubleDown.Scoring;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DoubleDown.Editor
{
    public static class PhaseTwoSetup
    {
        private const string ScenePath = "Assets/_Game/Scenes/BlackjackTable.unity";
        private const string RulesPath = "Assets/_Game/Settings/BlackjackRules.asset";
        private const string ScoringPath = "Assets/_Game/Settings/ScoringBalance.asset";
        private const string FontPath = "Assets/Thaleah_PixelFont/ThaleahFat.fontsettings";

        [MenuItem("Double Down/Phase 2/Build Blackjack Table")]
        public static void Build()
        {
            var rules = AssetDatabase.LoadAssetAtPath<BlackjackRulesConfig>(RulesPath);
            if (rules == null)
            {
                rules = ScriptableObject.CreateInstance<BlackjackRulesConfig>();
                AssetDatabase.CreateAsset(rules, RulesPath);
            }
            var scoring = AssetDatabase.LoadAssetAtPath<ScoringBalanceConfig>(ScoringPath);
            if (scoring == null)
            {
                scoring = ScriptableObject.CreateInstance<ScoringBalanceConfig>();
                AssetDatabase.CreateAsset(scoring, ScoringPath);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(RulesPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(ScoringPath, ImportAssetOptions.ForceUpdate);
            rules = Load<BlackjackRulesConfig>(RulesPath);
            scoring = Load<ScoringBalanceConfig>(ScoringPath);
            var font = Load<Font>(FontPath);
            var faces = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/Cards/Kenney" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".png") && !path.EndsWith("card_back.png"))
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
                .ToArray();
            if (faces.Length != 52 || faces.Any(sprite => sprite == null))
                throw new InvalidOperationException("The blackjack table needs 52 Kenney card sprites.");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            rules = Load<BlackjackRulesConfig>(RulesPath);
            scoring = Load<ScoringBalanceConfig>(ScoringPath);
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 2.8125f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.012f, 0.029f, 0.06f, 1f);
            cameraObject.AddComponent<AudioListener>();
            PhaseOneSetup.ConfigurePixelPerfect(cameraObject);

            var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;

            var table = GameObject.CreatePrimitive(PrimitiveType.Quad);
            table.name = "Casino Vortex (Shader)";
            table.transform.position = new Vector3(0f, 0f, 1f);
            table.transform.localScale = new Vector3(10f, 5.625f, 1f);
            UnityEngine.Object.DestroyImmediate(table.GetComponent<Collider>());
            var backdrop = table.GetComponent<MeshRenderer>();
            backdrop.sharedMaterial = Load<Material>("Assets/_Game/Settings/CasinoVortex.mat");
            backdrop.sortingOrder = -200;

            var controller = new GameObject("Blackjack Table Controller").AddComponent<BlackjackTableController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("rulesAsset").objectReferenceValue = rules;
            serialized.FindProperty("scoringAsset").objectReferenceValue = scoring;
            serialized.FindProperty("cardBack").objectReferenceValue =
                Load<Sprite>("Assets/Art/Cards/Kenney/card_back.png");
            serialized.FindProperty("cardInkMaterial").objectReferenceValue =
                Load<Material>("Assets/_Game/Settings/CardInk.mat");
            serialized.FindProperty("fontAsset").objectReferenceValue = font;
            serialized.FindProperty("dealSound").objectReferenceValue =
                Load<AudioClip>("Assets/Audio/SFX/Kenney/SFX_CardDeal.ogg");
            serialized.FindProperty("selectSound").objectReferenceValue =
                Load<AudioClip>("Assets/Audio/SFX/Kenney/SFX_CardSelect.ogg");
            serialized.FindProperty("flipSound").objectReferenceValue =
                Load<AudioClip>("Assets/Audio/SFX/Kenney/SFX_CardFlip.ogg");
            var cards = serialized.FindProperty("cardFaces");
            cards.arraySize = faces.Length;
            for (var i = 0; i < faces.Length; i++)
                cards.GetArrayElementAtIndex(i).objectReferenceValue = faces[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
            if (serialized.FindProperty("rulesAsset").objectReferenceValue == null)
                throw new InvalidOperationException("Blackjack rules were not assigned to the table.");
            if (serialized.FindProperty("scoringAsset").objectReferenceValue == null)
                throw new InvalidOperationException("Scoring balance was not assigned to the table.");

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save Blackjack Table scene.");
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene("Assets/_Game/Scenes/FeelSandbox.unity", true),
                new EditorBuildSettingsScene("Assets/_Game/Scenes/Bootstrap.unity", false)
            };
            AssetDatabase.SaveAssets();
            Debug.Log("DOUBLE DOWN Phase 2: Blackjack Table built.");
        }

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new InvalidOperationException("Missing asset: " + path);
            return asset;
        }
    }
}
