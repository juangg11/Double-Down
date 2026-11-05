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
        private Canvas canvas;
        private TableControls controls;
        private Material backdrop;
        private void Awake()
        {
            Application.targetFrameRate=60;
            if(EventSystem.current==null)new GameObject("UI Event System",typeof(EventSystem),typeof(InputSystemUIInputModule));
            EventSystem.current.sendNavigationEvents=false;
            controls=gameObject.AddComponent<TableControls>();var audio=gameObject.AddComponent<AudioSource>();audio.playOnAwake=false;
            canvas=PortfolioUi.Canvas("Main Menu HUD");var ui=new PortfolioUi(fontAsset,audio);
            ui.Panel(canvas.transform,"Shade",0,0,640,360,new Color(0.015f,0.025f,0.04f,0.36f)).GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            ui.Text(canvas.transform,"Title","DOUBLE DOWN",40,56,560,78,64);
            ui.Button(canvas.transform,"New Run","NUEVA PARTIDA",44,164,236,44,StartRun,true);
            ui.Button(canvas.transform,"How To Play","COMO JUGAR",44,226,112,30,()=>Open(help));
            ui.Button(canvas.transform,"Settings","AJUSTES",168,226,112,30,()=>GetComponent<TablePresentation>().Toggle());
            ui.Button(canvas.transform,"Credits","CREDITOS",44,270,112,30,()=>Open(credits));
#if UNITY_WEBGL && !UNITY_EDITOR
            ui.Button(canvas.transform,"Language","ENGLISH",168,270,112,30,()=>{GameLanguage.Toggle();RefreshLanguage();});
#else
            ui.Button(canvas.transform,"Quit","SALIR",168,270,112,30,Application.Quit);
            ui.Button(canvas.transform,"Language","ENGLISH",168,310,112,26,()=>{GameLanguage.Toggle();RefreshLanguage();});
#endif
            ui.Text(canvas.transform,"Version","v0.1.1",44,332,110,20);
            for(var i=0;i<decoration.Length;i++)
            {
                var card=new GameObject("Menu Card "+i,typeof(SpriteRenderer));card.transform.SetParent(transform);
                card.transform.position=new Vector3(1.45f+i*0.65f,-0.83f+i*0.10f+(i==1?0.22f:0f),-0.2f);card.transform.localScale=Vector3.one*1.75f;card.transform.rotation=Quaternion.Euler(0,0,12-i*12);
                card.GetComponent<SpriteRenderer>().sprite=decoration[i];card.GetComponent<SpriteRenderer>().sortingOrder=20+i;
            }
            help=Modal(ui,canvas.transform,"How To Play Panel","COMO JUGAR",
                "1. ALCANZA LA META ANTES DE AGOTAR LAS MANOS.\n2. PIDE CARTAS O PLANTATE: NO SUPERES 21.\n3. GANAR O EMPATAR PUNTUA BASE x MULT.\n4. COMPRA RELIQUIAS, VALES Y CONSUMIBLES.\n5. ABRE MAZO PARA MEJORAR CARTAS ENTRE MANOS.\n\nCADA TERCERA RONDA TRAE UNA REGLA JEFE.\nREORDENA RELIQUIAS: SUS EFECTOS SIGUEN ESE ORDEN.\n\nESC: AJUSTES Y CONTROLES. NO SE GUARDAN RUNS\nAL CERRAR EN ESTA VERSION. LOS AJUSTES SI.");
            credits=Modal(ui,canvas.transform,"Credits Panel","CREDITOS",
                "DOUBLE DOWN / JUANGG11\n\nCARTAS, SIMBOLOS Y SFX: KENNEY / CC0.\nUI: CRUSENHO AGUS HENNIHUNO / CC BY 4.0.\nCRUSENHO.ITCH.IO / TINTES Y ESCALADO ADAPTADOS.\nTECLAS: GERALD'S KEYS / CC0.\nFUENTE: THALEAH / TINY WORLDS.\n\nICONOS DE OBJETOS PROVISIONALES.\nLICENCIAS Y ATRIBUCIONES EN EL PAQUETE.");
            var renderer=GameObject.Find("Casino Vortex (Shader)").GetComponent<MeshRenderer>();backdrop=renderer.material;
            gameObject.AddComponent<TablePresentation>().Initialize(canvas,fontAsset,backdrop,false);
            RefreshLanguage();
        }
        private GameObject Modal(PortfolioUi ui,Transform parent,string name,string title,string text)
        {
            var root=ui.Panel(parent,name,0,0,640,360,new Color(0,0,0,0.85f));
            var body=ui.Panel(root.transform,"Body",34,24,572,312,CasinoUiSkin.Ink,true);
            ui.Text(body.transform,"Title",title,16,8,540,38,32);ui.Text(body.transform,"Content",text,16,54,540,210);
            ui.Button(body.transform,"Back","VOLVER",16,274,540,26,()=>root.SetActive(false));body.AddComponent<PanelMotion>();root.SetActive(false);return root;
        }
        private void Open(GameObject modal){modal.SetActive(true);modal.transform.SetAsLastSibling();}
        private void RefreshLanguage()
        {
            Set("New Run","NUEVA PARTIDA","NEW RUN");
            Set("How To Play","COMO JUGAR","HOW TO PLAY");
            Set("Settings","AJUSTES","SETTINGS");
            Set("Credits","CREDITOS","CREDITS");
            Set("Quit","SALIR","QUIT");
            Set("Language","ENGLISH","ESPAÑOL");
            SetModal(help,"COMO JUGAR","HOW TO PLAY",
                "1. ALCANZA LA META ANTES DE AGOTAR LAS MANOS.\n2. PIDE CARTAS O PLANTATE: NO SUPERES 21.\n3. GANAR O EMPATAR PUNTUA BASE x MULT.\n4. COMPRA RELIQUIAS, VALES Y CONSUMIBLES.\n5. ABRE MAZO PARA MEJORAR CARTAS ENTRE MANOS.\n\nELIGE UNA RELIQUIA AL EMPEZAR.\nCADA TERCERA RONDA TRAE UNA REGLA JEFE.\n\nESC: AJUSTES Y CONTROLES. NO SE GUARDAN RUNS\nAL CERRAR EN ESTA VERSION. LOS AJUSTES SI.",
                "1. REACH THE TARGET BEFORE YOU RUN OUT OF HANDS.\n2. HIT OR STAND: STAY AT OR BELOW 21.\n3. WINS AND PUSHES SCORE BASE x MULT.\n4. BUY RELICS, VOUCHERS AND CONSUMABLES.\n5. OPEN DECK TO UPGRADE CARDS BETWEEN HANDS.\n\nCHOOSE A STARTING RELIC.\nEVERY THIRD ROUND HAS A BOSS RULE.\n\nESC: SETTINGS AND CONTROLS. RUNS ARE NOT SAVED\nON EXIT IN THIS VERSION. SETTINGS ARE.");
            SetModal(credits,"CREDITOS","CREDITS",
                "DOUBLE DOWN / JUANGG11\n\nCARTAS, SIMBOLOS Y SFX: KENNEY / CC0.\nUI: CRUSENHO AGUS HENNIHUNO / CC BY 4.0.\nCRUSENHO.ITCH.IO / TINTES Y ESCALADO ADAPTADOS.\nTECLAS: GERALD'S KEYS / CC0.\nFUENTE: THALEAH / TINY WORLDS.\n\nICONOS DE OBJETOS PROVISIONALES.\nLICENCIAS Y ATRIBUCIONES EN EL PAQUETE.",
                "DOUBLE DOWN / JUANGG11\n\nCARDS, SYMBOLS AND SFX: KENNEY / CC0.\nUI: CRUSENHO AGUS HENNIHUNO / CC BY 4.0.\nCRUSENHO.ITCH.IO / ADAPTED TINTS AND SCALE.\nKEYS: GERALD'S KEYS / CC0.\nFONT: THALEAH / TINY WORLDS.\n\nITEM ICONS ARE TEMPORARY.\nLICENSES AND CREDITS ARE IN THE PACKAGE.");
            GetComponent<TablePresentation>().RefreshLanguage();
        }
        private void Set(string name,string es,string en)
        {
            var label=canvas.transform.Find(name+"/Label");
            if(label!=null)label.GetComponent<UnityEngine.UI.Text>().text=GameLanguage.Pick(es,en);
        }
        private static void SetModal(GameObject root,string titleEs,string titleEn,string bodyEs,string bodyEn)
        {
            var body=root.transform.Find("Body");
            body.Find("Title").GetComponent<UnityEngine.UI.Text>().text=GameLanguage.Pick(titleEs,titleEn);
            body.Find("Content").GetComponent<UnityEngine.UI.Text>().text=GameLanguage.Pick(bodyEs,bodyEn);
            body.Find("Back/Label").GetComponent<UnityEngine.UI.Text>().text=GameLanguage.Pick("VOLVER","BACK");
        }
        public void StartRun() => SceneManager.LoadScene("BlackjackTable");
        private void Update()
        {
            if(controls.EscapePressed && (help.activeSelf || credits.activeSelf)){help.SetActive(false);credits.SetActive(false);controls.ConsumeFrame();}
        }
        private void OnDestroy(){if(backdrop!=null)Destroy(backdrop);}
    }
}
