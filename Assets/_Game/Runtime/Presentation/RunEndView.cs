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
            ui.Text(body.transform,"Title","FIN DE LA RUN",20,16,380,42,32);
            summary=ui.Text(body.transform,"Summary","",20,74,380,100);
            ui.Button(body.transform,"Retry","OTRA PARTIDA",20,190,180,34,()=>{root.SetActive(false);retry();},true);
            ui.Button(body.transform,"Menu","MENU PRINCIPAL",220,190,180,34,GoToMenu);
            body.AddComponent<PanelMotion>();root.SetActive(false);
            leave=ui.Panel(canvas.transform,"Leave Run",0,0,640,360,new Color(0,0,0,0.85f));
            var confirm=ui.Panel(leave.transform,"Body",110,92,420,176,CasinoUiSkin.Ink,true);
            ui.Text(confirm.transform,"Title","VOLVER AL MENU",20,14,380,38,32);
            ui.Text(confirm.transform,"Warning","LA RUN ACTUAL TERMINARA.\nLOS AJUSTES SE CONSERVAN.",20,64,380,44);
            ui.Button(confirm.transform,"Cancel","SEGUIR JUGANDO",20,124,180,30,()=>leave.SetActive(false));
            ui.Button(confirm.transform,"Confirm","VOLVER AL MENU",220,124,180,30,GoToMenu);confirm.AddComponent<PanelMotion>();leave.SetActive(false);
        }
        public void Show(ScoreRun score)
        {
            summary.text=$"MESA {(score.Round+2)/3}  /  RONDA {score.Round}\nPUNTOS {score.Score}  /  META {score.Goal}\nMEJOR MANO {score.HighestHand}\nTOTAL DE LA RUN {score.RunTotal}";
            root.SetActive(true);root.transform.SetAsLastSibling();
        }
        public void ConfirmLeave(){leave.SetActive(true);leave.transform.SetAsLastSibling();}
        public static void GoToMenu() => SceneManager.LoadScene("MainMenu");
        private void Update()
        { if(leave.activeSelf && GetComponent<TableControls>().EscapePressed){leave.SetActive(false);GetComponent<TableControls>().ConsumeFrame();} }
    }
}
