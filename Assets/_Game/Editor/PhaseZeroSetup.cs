using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace DoubleDown.Editor
{
    public static class PhaseZeroSetup
    {
        private const string ScenePath = "Assets/_Game/Scenes/Bootstrap.unity";

        [MenuItem("Double Down/Phase 0/Configure Bootstrap Scene")]
        public static void Configure()
        {
            if (GraphicsSettings.defaultRenderPipeline == null)
            {
                throw new InvalidOperationException("URP pipeline asset is not assigned in Graphics Settings.");
            }

            var pixelPerfectType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("UnityEngine.Rendering.Universal.PixelPerfectCamera", false))
                .FirstOrDefault(type => type != null);
            if (pixelPerfectType == null)
            {
                throw new InvalidOperationException("URP Pixel Perfect Camera is not available.");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 2.8125f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.078f, 0.09f, 1f);
            cameraObject.AddComponent<AudioListener>();

            var pixelPerfect = cameraObject.AddComponent(pixelPerfectType);
            SetProperty(pixelPerfect, "assetsPPU", 64);
            SetProperty(pixelPerfect, "refResolutionX", 640);
            SetProperty(pixelPerfect, "refResolutionY", 360);
            SetEnumProperty(pixelPerfect, "gridSnapping", "UpscaleRenderTexture");
            SetEnumProperty(pixelPerfect, "cropFrame", "Windowbox");
            var pixelSettings = new SerializedObject(pixelPerfect);
            pixelSettings.FindProperty("m_FilterMode").enumValueIndex = 1;
            pixelSettings.ApplyModifiedPropertiesWithoutUndo();

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException("Could not save bootstrap scene.");
            }

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("DOUBLE DOWN Phase 0: URP and pixel-perfect bootstrap scene configured.");
        }

        private static void SetProperty(Component target, string name, object value)
        {
            var property = target.GetType().GetProperty(name);
            if (property == null || !property.CanWrite)
            {
                throw new MissingMemberException(target.GetType().FullName, name);
            }

            property.SetValue(target, value);
        }

        private static void SetEnumProperty(Component target, string name, string value)
        {
            var property = target.GetType().GetProperty(name);
            if (property == null || !property.CanWrite)
            {
                throw new MissingMemberException(target.GetType().FullName, name);
            }

            property.SetValue(target, Enum.Parse(property.PropertyType, value));
        }
    }
}
