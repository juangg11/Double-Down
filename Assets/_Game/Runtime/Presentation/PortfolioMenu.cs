using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
namespace DoubleDown.Presentation
{
    public sealed class PortfolioMenu : MonoBehaviour
    {
        [SerializeField] private Font fontAsset;
        [SerializeField] private Sprite[] decoration;
        public bool ModalOpen => (help!=null && help.activeSelf) || (credits!=null && credits.activeSelf);
        private GameObject help, credits;
        private TableControls controls;
        private Material backdrop;
        private void Awake()
        {
            Application.targetFrameRate=60;
            if(EventSystem.current==null)new GameObject("UI Event System",typeof(EventSystem),typeof(InputSystemUIInputModule));
            EventSystem.current.sendNavigationEvents=false;
            controls=gameObject.AddComponent<TableControls>();var audio=gameObject.AddComponent<AudioSource>();audio.playOnAwake=false;
            var canvas=PortfolioUi.Canvas("Main Menu HUD");var ui=new PortfolioUi(fontAsset,audio);
            ui.Panel(canvas.transform,"Shade",0,0,640,360,new Color(0.015f,0.025f,0.04f,0.36f)).GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            ui.Text(canvas.transform,"Eyebrow","UNA MANO MAS. UNA APUESTA MAYOR.",44,30,552,24);
            ui.Text(canvas.transform,"Title","DOUBLE DOWN",40,60,560,78,64);
            ui.Text(canvas.transform,"Tagline","BLACKJACK / RELIQUIAS / RIESGO",44,132,552,24);
            ui.Button(canvas.transform,"New Run","NUEVA PARTIDA",44,182,236,42,StartRun,true);
            ui.Button(canvas.transform,"How To Play","COMO JUGAR",44,240,112,28,()=>Open(help));
            ui.Button(canvas.transform,"Settings","AJUSTES",168,240,112,28,()=>GetComponent<TablePresentation>().Toggle());
            ui.Button(canvas.transform,"Credits","CREDITOS",44,280,112,28,()=>Open(credits));
            ui.Button(canvas.transform,"Quit","SALIR",168,280,112,28,Application.Quit);
            ui.Text(canvas.transform,"Version","PORTFOLIO EDITION  /  v0.1.0",44,328,550,20);
            for(var i=0;i<decoration.Length;i++)
            {
                var card=new GameObject("Menu Card "+i,typeof(SpriteRenderer));card.transform.SetParent(transform);
                card.transform.position=new Vector3(1.45f+i*0.65f,-0.83f+i*0.10f,-0.2f);card.transform.localScale=Vector3.one*1.75f;card.transform.rotation=Quaternion.Euler(0,0,12-i*12);
                card.GetComponent<SpriteRenderer>().sprite=decoration[i];card.GetComponent<SpriteRenderer>().sortingOrder=20+i;
            }
            help=Modal(ui,canvas.transform,"How To Play Panel","COMO JUGAR",
                "1. ALCANZA LA META ANTES DE AGOTAR LAS MANOS.\n2. PIDE CARTAS O PLANTATE: NO SUPERES 21.\n3. GANAR O EMPATAR PUNTUA BASE x MULT.\n4. COMPRA RELIQUIAS, VALES Y CONSUMIBLES.\n5. ABRE MAZO PARA MEJORAR CARTAS ENTRE MANOS.\n\nCADA TERCERA RONDA TRAE UNA REGLA JEFE.\nREORDENA RELIQUIAS: SUS EFECTOS SIGUEN ESE ORDEN.\n\nESC: AJUSTES Y CONTROLES. NO SE GUARDAN RUNS\nAL CERRAR EN ESTA VERSION. LOS AJUSTES SI.");
            credits=Modal(ui,canvas.transform,"Credits Panel","CREDITOS",
                "DOUBLE DOWN / JUANGG11\n\nCARTAS, SIMBOLOS Y SFX: KENNEY / CC0.\nUI: CRUSENHO AGUS HENNIHUNO / CC BY 4.0.\nCRUSENHO.ITCH.IO / TINTES Y ESCALADO ADAPTADOS.\nTECLAS: GERALD'S KEYS / CC0.\nFUENTE: THALEAH / TINY WORLDS.\n\nICONOS DE OBJETOS PROVISIONALES.\nLICENCIAS Y ATRIBUCIONES EN EL PAQUETE.");
            var renderer=GameObject.Find("Casino Vortex (Shader)").GetComponent<MeshRenderer>();backdrop=renderer.material;
            gameObject.AddComponent<TablePresentation>().Initialize(canvas,fontAsset,backdrop,false);
        }
        private GameObject Modal(PortfolioUi ui,Transform parent,string name,string title,string text)
        {
            var root=ui.Panel(parent,name,0,0,640,360,new Color(0,0,0,0.85f));
            var body=ui.Panel(root.transform,"Body",34,24,572,312,CasinoUiSkin.Ink,true);
            ui.Text(body.transform,"Title",title,16,8,540,38,32);ui.Text(body.transform,"Content",text,16,54,540,210);
            ui.Button(body.transform,"Back","VOLVER",16,274,540,26,()=>root.SetActive(false));body.AddComponent<PanelMotion>();root.SetActive(false);return root;
        }
        private void Open(GameObject modal){modal.SetActive(true);modal.transform.SetAsLastSibling();}
        public void StartRun() => SceneManager.LoadScene("BlackjackTable");
        private void Update()
        {
            if(controls.EscapePressed && (help.activeSelf || credits.activeSelf)){help.SetActive(false);credits.SetActive(false);controls.ConsumeFrame();}
        }
        private void OnDestroy(){if(backdrop!=null)Destroy(backdrop);}
    }
}
