using System;
using System.IO;
using System.Linq;
using DoubleDown.Presentation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DoubleDown.Editor
{
    public static class PortfolioBuild
    {
        private const string Menu="Assets/_Game/Scenes/MainMenu.unity";
        private const string Table="Assets/_Game/Scenes/BlackjackTable.unity";
        [MenuItem("Double Down/Portfolio/Prepare Main Menu")]
        public static void Prepare()
        {
            var scene=EditorSceneManager.OpenScene(Table);
            var controller=UnityEngine.Object.FindFirstObjectByType<BlackjackTableController>();
            var data=new SerializedObject(controller);var font=data.FindProperty("fontAsset").objectReferenceValue;
            UnityEngine.Object.DestroyImmediate(controller.gameObject);
            var menu=new GameObject("Portfolio Menu").AddComponent<PortfolioMenu>();var settings=new SerializedObject(menu);
            settings.FindProperty("fontAsset").objectReferenceValue=font;
            var art=settings.FindProperty("decoration");art.arraySize=3;
            var names=new[]{"card_spades_K","card_hearts_Q","card_clubs_A"};
            for(var i=0;i<3;i++) art.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Cards/Kenney/"+names[i]+".png");
            settings.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.SaveScene(scene,Menu);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Menu,true),new EditorBuildSettingsScene(Table,true),new EditorBuildSettingsScene("Assets/_Game/Scenes/FeelSandbox.unity",true)};
            PlayerSettings.productName="DOUBLE DOWN";PlayerSettings.companyName="juangg11";PlayerSettings.bundleVersion="0.1.0";
            PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;
            PlayerSettings.runInBackground=false;QualitySettings.vSyncCount=1;AssetDatabase.SaveAssets();
        }
        [MenuItem("Double Down/Portfolio/Build Windows x64")]
        public static void Build()
        {
            Prepare();
            var target=Environment.GetEnvironmentVariable("DOUBLEDOWN_BUILD_DIR");if(string.IsNullOrWhiteSpace(target))target="Builds/DoubleDown-0.1.0-Windows-x64";
            Directory.CreateDirectory(target);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Menu,Table},locationPathName=Path.Combine(target,"DoubleDown.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            Directory.CreateDirectory("docs");File.WriteAllText("docs/PORTFOLIO_BUILD_RESULT.txt",$"Result: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\nUnity: {Application.unityVersion}\n");
            if(report.summary.result!=BuildResult.Succeeded || report.summary.totalErrors>0)throw new InvalidOperationException("Portfolio build failed");
        }
    }
}
