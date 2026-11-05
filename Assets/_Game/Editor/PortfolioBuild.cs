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
            PlayerSettings.productName="DOUBLE DOWN";PlayerSettings.companyName="juangg11";PlayerSettings.bundleVersion="0.1.1";
            PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;
            PlayerSettings.runInBackground=false;QualitySettings.vSyncCount=1;AssetDatabase.SaveAssets();
        }
        [MenuItem("Double Down/Portfolio/Build Windows x64")]
        public static void Build()
        {
            Prepare();
            var target=Environment.GetEnvironmentVariable("DOUBLEDOWN_BUILD_DIR");if(string.IsNullOrWhiteSpace(target))target="Builds/DoubleDown-0.1.1-Windows-x64";
            Directory.CreateDirectory(target);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Menu,Table},locationPathName=Path.Combine(target,"DoubleDown.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            Directory.CreateDirectory("docs");File.WriteAllText("docs/PORTFOLIO_BUILD_RESULT.txt",$"Result: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\nUnity: {Application.unityVersion}\n");
            if(report.summary.result!=BuildResult.Succeeded || report.summary.totalErrors>0)throw new InvalidOperationException("Portfolio build failed");
        }
        [MenuItem("Double Down/Portfolio/Build WebGL for itch.io")]
        public static void BuildWeb()
        {
            Prepare();
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback=false;
            var target=Environment.GetEnvironmentVariable("DOUBLEDOWN_WEB_BUILD_DIR");
            if(string.IsNullOrWhiteSpace(target))target="Builds/DoubleDown-0.1.1-WebGL";
            Directory.CreateDirectory(target);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes=new[]{Menu,Table},locationPathName=target,target=BuildTarget.WebGL,options=BuildOptions.None
            });
              if(report.summary.result==BuildResult.Succeeded)
              {
                  var index=Path.Combine(target,"index.html");
                  var html=File.ReadAllText(index)
                      .Replace("Unity Web Player | DOUBLE DOWN","DOUBLE DOWN")
                      .Replace("width=960 height=600","width=960 height=540")
                      .Replace("canvas.style.height = \"600px\";","canvas.style.height = \"540px\";")
                      .Replace("</head>","<style>html,body{width:100%;height:100%;overflow:hidden;background:#111a26}#unity-container.unity-desktop{position:relative;left:auto;top:auto;transform:none;width:min(100vw,177.7778vh);height:min(100vh,56.25vw);margin:auto}#unity-canvas{width:100%!important;height:100%!important;image-rendering:pixelated}#unity-footer,#unity-logo{display:none!important}#unity-loading-bar{z-index:2}</style></head>");
                  File.WriteAllText(index,html);
              }
            Directory.CreateDirectory("docs");
            File.WriteAllText("docs/WEB_BUILD_RESULT.txt",$"Result: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\nUnity: {Application.unityVersion}\n");
            if(report.summary.result!=BuildResult.Succeeded || report.summary.totalErrors>0)
                throw new InvalidOperationException("WebGL build failed");
        }
    }
}
