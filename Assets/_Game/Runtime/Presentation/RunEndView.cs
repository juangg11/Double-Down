using System;
using DoubleDown.Scoring;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace DoubleDown.Presentation
{
    public sealed class RunEndView : MonoBehaviour
    {
        public bool IsOpen => root!=null && root.activeSelf;
        public bool Confirming => leave != null && leave.activeSelf;
        private GameObject root, leave;
        private Text summary;
        public void Initialize(Canvas canvas,Font font,Action retry)
        {
            var ui=new PortfolioUi(font,GetComponent<AudioSource>());
            root=ui.Panel(canvas.transform,"Run End",0,0,640,360,new Color(0,0,0,0.85f));
            var body=ui.Panel(root.transform,"Body",110,58,420,244,CasinoUiSkin.Ink,true);
            ui.Text(body.transform,"Title",GameLanguage.Pick("FIN DE LA RUN","RUN OVER"),20,16,380,42,32);
            summary=ui.Text(body.transform,"Summary","",20,74,380,100);
            ui.Button(body.transform,"Retry",GameLanguage.Pick("OTRA PARTIDA","TRY AGAIN"),20,190,180,34,()=>{root.SetActive(false);retry();},true);
            ui.Button(body.transform,"Menu",GameLanguage.Pick("MENU PRINCIPAL","MAIN MENU"),220,190,180,34,GoToMenu);
            body.AddComponent<PanelMotion>();root.SetActive(false);
            leave=ui.Panel(canvas.transform,"Leave Run",0,0,640,360,new Color(0,0,0,0.85f));
            var confirm=ui.Panel(leave.transform,"Body",110,92,420,176,CasinoUiSkin.Ink,true);
            ui.Text(confirm.transform,"Title",GameLanguage.Pick("VOLVER AL MENU","RETURN TO MENU"),20,14,380,38,32);
            ui.Text(confirm.transform,"Warning",GameLanguage.Pick("LA RUN ACTUAL TERMINARA.\nLOS AJUSTES SE CONSERVAN.","YOUR CURRENT RUN WILL END.\nSETTINGS WILL BE SAVED."),20,64,380,44);
            ui.Button(confirm.transform,"Cancel",GameLanguage.Pick("SEGUIR JUGANDO","KEEP PLAYING"),20,124,180,30,()=>leave.SetActive(false));
            ui.Button(confirm.transform,"Confirm",GameLanguage.Pick("VOLVER AL MENU","RETURN TO MENU"),220,124,180,30,GoToMenu);confirm.AddComponent<PanelMotion>();leave.SetActive(false);
        }
        public void Show(ScoreRun score)
        {
            summary.text=$"{GameLanguage.Pick("MESA","TABLE")} {(score.Round+2)/3}  /  {GameLanguage.Pick("RONDA","ROUND")} {score.Round}\n{GameLanguage.Pick("PUNTOS","SCORE")} {score.Score}  /  {GameLanguage.Pick("META","TARGET")} {score.Goal}\n{GameLanguage.Pick("MEJOR MANO","BEST HAND")} {score.HighestHand}\n{GameLanguage.Pick("TOTAL DE LA RUN","RUN TOTAL")} {score.RunTotal}";
            root.SetActive(true);root.transform.SetAsLastSibling();
        }
        public void ConfirmLeave(){leave.SetActive(true);leave.transform.SetAsLastSibling();}
        public static void GoToMenu() => SceneManager.LoadScene("MainMenu");
        private void Update()
        { if(leave.activeSelf && GetComponent<TableControls>().EscapePressed){leave.SetActive(false);GetComponent<TableControls>().ConsumeFrame();} }
    }
}
