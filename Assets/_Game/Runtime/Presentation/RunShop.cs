using System;
using System.Linq;
using DoubleDown.Scoring;
using UnityEngine;
using UnityEngine.UI;

namespace DoubleDown.Presentation
{
    public sealed class RunShop : MonoBehaviour
    {
        public RunEconomy Economy { get; private set; }
        public bool IsOpen => root != null && root.activeSelf;
        public int DepartedRound { get; private set; }
        public event Action Changed;
        private GameObject root;
        private Font font;
        private RelicDefinition[] catalog;
        private readonly Text[] names=new Text[3];
        private readonly Text[] descriptions=new Text[3];
        private readonly Image[] icons=new Image[3];
        private readonly Button[] buy=new Button[3];
        private Text heading;
        private Text status;
        private Button reroll;
        private Action leave;

        public void Initialize(Canvas canvas,Font pixelFont,int seed)
        {
            font=pixelFont; catalog=Resources.LoadAll<RelicDefinition>("Relics");
            var balance = Resources.Load<EconomyBalanceConfig>("EconomyBalance");
            if (balance == null) throw new InvalidOperationException("EconomyBalance asset is missing.");
            Economy=new RunEconomy(catalog.Select(r=>r.id),seed,balance.ToRules());
            root=Panel(canvas.transform,"Run Shop",172,92,456,260,new Color(0.018f,0.04f,0.06f));
            CasinoUiSkin.Surface(root.GetComponent<Image>(), "Frame", CasinoUiSkin.Ink);
            heading=Text(root.transform,"Heading","",12,8,432,22);
            for(var i=0;i<3;i++)
            {
                var index=i;
                var card=Panel(root.transform,"Offer "+i,12+i*146,36,140,150,new Color(0.04f,0.12f,0.15f));
                CasinoUiSkin.Surface(card.GetComponent<Image>(), "Frame", CasinoUiSkin.Felt);
                icons[i]=Panel(card.transform,"Icon",58,6,24,24,Color.white).GetComponent<Image>();
                names[i]=Text(card.transform,"Name","",6,34,128,32);names[i].alignment=TextAnchor.MiddleCenter;
                descriptions[i]=Text(card.transform,"Effect","",6,68,128,48);
                buy[i]=Button(card.transform,"Buy","",6,118,128,28,()=>{Economy.Buy(index);Refresh();});
            }
            status=Text(root.transform,"Status","TAB: EQUIPAR TUS RELIQUIAS",12,192,432,24);
            reroll=Button(root.transform,"Reroll","",12,224,128,28,()=>{Economy.Reroll();Refresh();});
            Button(root.transform,"Continue","SIGUIENTE RONDA",158,224,286,28,()=>
            { DepartedRound=Economy.RewardedRound;root.SetActive(false);leave?.Invoke(); });
            root.SetActive(false);
        }
        public void Open(int round,Action onLeave,int remainingHands=0)
        {
            Economy.Open(round,remainingHands);leave=onLeave;root.SetActive(true);root.transform.SetAsLastSibling();Refresh();
        }
        public void ResetRun() { Economy.Reset();DepartedRound=0;root.SetActive(false); }
        private void Refresh()
        {
            heading.text=$"TIENDA  /  ORO {Economy.Gold}  /  PREMIO +{Economy.LastReward}";
            for(var i=0;i<3;i++)
            {
                var id=Economy.Offers[i];var relic=Array.Find(catalog,r=>r.id==id);
                names[i].text=relic!=null?relic.displayName:"AGOTADO";
                descriptions[i].text=relic!=null?relic.description:"";
                icons[i].enabled=relic!=null;if(relic!=null) icons[i].sprite=relic.icon;
                buy[i].interactable=relic!=null && Economy.Gold>=Economy.RelicPrice;
                buy[i].GetComponentInChildren<Text>().text = relic == null ? "AGOTADO" : $"COMPRAR {Economy.RelicPrice}";
            }
            reroll.interactable=Economy.CanReroll;
            reroll.GetComponentInChildren<Text>().text=$"RENOVAR {Economy.RerollPrice}";
            status.text="TAB: EQUIPAR  /  RENOVAR SUBE EL PRECIO";
            Changed?.Invoke();
        }
        private static GameObject Panel(Transform parent,string name,float x,float y,float w,float h,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
            Set(go.GetComponent<RectTransform>(),x,y,w,h);go.GetComponent<Image>().color=color;return go;
        }
        private Text Text(Transform parent,string name,string content,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
            Set(go.GetComponent<RectTransform>(),x,y,w,h);var text=go.GetComponent<Text>();
            text.font=font;text.material=Graphic.defaultGraphicMaterial;text.fontSize=16;text.text=content;text.color=new Color(1f,0.81f,0.48f);text.raycastTarget=false;return text;
        }
        private Button Button(Transform parent,string name,string label,float x,float y,float w,float h,UnityEngine.Events.UnityAction action)
        {
            var shadow=Panel(parent,name+" Shadow",x+2,y+3,w,h,new Color(0.006f,0.015f,0.025f)).GetComponent<Image>();shadow.raycastTarget=false;
            var go=Panel(parent,name,x,y,w,h,new Color(0.12f,0.29f,0.3f));go.AddComponent<CanvasGroup>();
            var button=go.AddComponent<Button>();button.targetGraphic=go.GetComponent<Image>();button.transition=Selectable.Transition.None;
            CasinoUiSkin.Surface(go.GetComponent<Image>(), "Button", CasinoUiSkin.Felt);
            CasinoUiSkin.Surface(shadow, "Button", CasinoUiSkin.Ink);
            Text(go.transform,"Label",label,2,0,w-4,h).alignment=TextAnchor.MiddleCenter;
            button.onClick.AddListener(action);
            go.AddComponent<ArcadeButtonFeel>().Initialize(go.GetComponent<Image>(),shadow,GetComponent<AudioSource>(),Resources.Load<AudioClip>("TablePresentation/Audio/click3"));
            return button;
        }
        private static void Set(RectTransform rect,float x,float y,float w,float h)
        { rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(w,h); }
    }
}
