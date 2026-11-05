using System;
using DoubleDown.Blackjack;
using DoubleDown.Scoring;
using UnityEngine;
using UnityEngine.UI;
namespace DoubleDown.Presentation
{
    public sealed class DeckWorkshop : MonoBehaviour
    {
        public bool IsOpen => root != null && root.activeSelf;
        public event Action Changed;
        private GameObject root;
        private Font font;
        private RunInventory inventory;
        private Func<bool> editable;
        private Func<PlayingCard, Sprite> sprite;
        private readonly Button[] cards = new Button[18];
        private readonly Image[] faces = new Image[18];
        private readonly Button[] items = new Button[4];
        private Text details, status, paging, heading;
        private Button apply, discard;
        private int page, selectedId = 1, selectedItem = -1;
        public void Initialize(Canvas canvas, Font font, RunInventory inventory, Func<bool> editable, Func<PlayingCard,Sprite> sprite)
        {
            this.font=font;this.inventory=inventory;this.editable=editable;this.sprite=sprite;
            Button(canvas.transform,"Open Deck",GameLanguage.Pick("MAZO","DECK"),460,1,76,18,Toggle);
            root=Panel(canvas.transform,"Deck Workshop",0,0,640,360,new Color(0,0,0,0.85f));
            var body=Panel(root.transform,"Body",18,14,604,332,CasinoUiSkin.Ink);
            CasinoUiSkin.Surface(body.GetComponent<Image>(),"Frame",CasinoUiSkin.Ink);
            heading=Text(body.transform,"Heading",GameLanguage.Pick("MAZO","DECK"),12,8,470,28);
            Button(body.transform,"Close",GameLanguage.Pick("VOLVER","BACK"),508,8,80,26,Toggle);
            for(var i=0;i<18;i++)
            {
                var index=i;
                cards[i]=Button(body.transform,"Card "+i,"",12+(i%6)*58,44+(i/6)*68,52,64,()=>{var n=page*18+index;if(n<inventory.Deck.Cards.Count)selectedId=inventory.Deck.Cards[n].Card.InstanceId;Refresh();});
                var image=Panel(cards[i].transform,"Face",7,3,38,56,Color.white).GetComponent<Image>();
                image.preserveAspect=true;image.raycastTarget=false;faces[i]=image;
            }
            details=Text(body.transform,"Details","",372,44,216,100);
            Text(body.transform,"Items Caption",GameLanguage.Pick("CONSUMIBLES","CONSUMABLES"),372,146,216,18);
            for(var i=0;i<4;i++)
            { var n=i; items[i]=Button(body.transform,"Consumable "+i,"",372,166+i*24,216,22,()=>{selectedItem=n;Refresh();}); }
            apply=Button(body.transform,"Apply",GameLanguage.Pick("APLICAR","APPLY"),372,266,140,26,Use);
            discard=Button(body.transform,"Discard",GameLanguage.Pick("TIRAR","DROP"),524,266,64,26,()=>{if(inventory.Discard(selectedItem,editable())){selectedItem=-1;Refresh();Changed?.Invoke();}});
            Button(body.transform,"Previous","<",12,266,54,26,()=>{page=Math.Max(0,page-1);Refresh();});
            paging=Text(body.transform,"Page","",78,266,200,26);paging.alignment=TextAnchor.MiddleCenter;
            Button(body.transform,"Next",">",302,266,52,26,()=>{page=Math.Min((inventory.Deck.Cards.Count-1)/18,page+1);Refresh();});
            status=Text(body.transform,"Status","",12,296,576,34);
            body.AddComponent<PanelMotion>();root.SetActive(false);
        }
        public void Toggle() { root.SetActive(!root.activeSelf); if(IsOpen){root.transform.SetAsLastSibling();Refresh();} }
        private void Update() { if(IsOpen && GetComponent<TableControls>().EscapePressed) { Toggle();GetComponent<TableControls>().ConsumeFrame(); } }
        private void Use()
        {
            if(!inventory.Use(selectedItem,selectedId,editable()))return;
            selectedItem=-1;
            if(inventory.Deck.Find(selectedId)==null)selectedId=inventory.Deck.Cards[0].Card.InstanceId;
            page=Math.Min(page,(inventory.Deck.Cards.Count-1)/18);Refresh();Changed?.Invoke();
        }
        private void Refresh()
        {
            page=Math.Min(page,(inventory.Deck.Cards.Count-1)/18);
            if(inventory.Deck.Find(selectedId)==null)selectedId=inventory.Deck.Cards[0].Card.InstanceId;
            if(selectedItem>=inventory.Consumables.Count)selectedItem=-1;
            heading.text=$"{GameLanguage.Pick("MAZO","DECK")} {inventory.Deck.Cards.Count}  /  {GameLanguage.Pick("CONSUMIBLES","CONSUMABLES")} {inventory.Consumables.Count}/{inventory.ConsumableCapacity}";
            paging.text=$"{GameLanguage.Pick("PAGINA","PAGE")} {page+1}/{(inventory.Deck.Cards.Count+17)/18}";
            for(var i=0;i<18;i++)
            {
                var n=page*18+i;cards[i].gameObject.SetActive(n<inventory.Deck.Cards.Count);
                if(n>=inventory.Deck.Cards.Count)continue;
                var card=inventory.Deck.Cards[n];faces[i].sprite=sprite(card.Card);
                cards[i].GetComponent<ArcadeButtonFeel>().SetTint(card.Card.InstanceId==selectedId?CasinoUiSkin.Red:CasinoUiSkin.Felt);
            }
            var selected=inventory.Deck.Find(selectedId);
            details.text=selected==null?GameLanguage.Pick("ELIGE UNA CARTA","CHOOSE A CARD"):$"{GameLanguage.Pick("CARTA","CARD")} #{selectedId}\n"+(GameLanguage.English?new[]{"CLUBS","DIAMONDS","HEARTS","SPADES"}:new[]{"TREBOLES","DIAMANTES","CORAZONES","PICAS"})[(int)selected.Card.Suit]+" "+((int)selected.Card.Rank<=10?((int)selected.Card.Rank).ToString():selected.Card.Rank==CardRank.Jack?"J":selected.Card.Rank==CardRank.Queen?"Q":"K");
            if(selected!=null)for(var l=0;l<3;l++)details.text+="\n"+(selected[(CardLayer)l]?.displayName??(GameLanguage.English?new[]{"NO EDITION","NO ENHANCEMENT","NO SEAL"}:new[]{"SIN EDICION","SIN MEJORA","SIN SELLO"})[l]);
            for(var i=0;i<4;i++)
            {
                items[i].gameObject.SetActive(i<inventory.ConsumableCapacity);
                items[i].GetComponentInChildren<Text>().text=i<inventory.Consumables.Count?(i==selectedItem?"> ":"")+inventory.Consumables[i].displayName:GameLanguage.Pick("VACIO","EMPTY");
                items[i].interactable=i<inventory.Consumables.Count;
            }
            var item=selectedItem>=0 && selectedItem<inventory.Consumables.Count?inventory.Consumables[selectedItem]:null;
            apply.interactable=editable() && item!=null && inventory.Deck.CanUse(item,selectedId);
            discard.interactable=editable() && item!=null;
            status.text=!editable()?GameLanguage.Pick("TERMINA LA MANO PARA EDITAR EL MAZO","FINISH THE HAND TO EDIT YOUR DECK"):item!=null?GameLanguage.ConsumableEffect(item):GameLanguage.Pick("ELIGE CARTA Y CONSUMIBLE. SE USA SOLO AL CONFIRMAR.","CHOOSE A CARD AND CONSUMABLE. APPLY TO USE.");
            status.fontSize=16;
        }
        private static GameObject Panel(Transform parent,string name,float x,float y,float w,float h,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);go.GetComponent<Image>().color=color;return go;
        }
        private Text Text(Transform parent,string name,string value,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);
            var t=go.GetComponent<Text>();t.font=font;t.fontSize=16;t.text=value;t.color=CasinoUiSkin.Paper;t.raycastTarget=false;return t;
        }
        private Button Button(Transform parent,string name,string value,float x,float y,float w,float h,UnityEngine.Events.UnityAction click)
        {
            var go=Panel(parent,name,x,y,w,h,CasinoUiSkin.Felt);CasinoUiSkin.Surface(go.GetComponent<Image>(),"Button",CasinoUiSkin.Felt);
            var b=go.AddComponent<Button>();b.targetGraphic=go.GetComponent<Image>();b.transition=Selectable.Transition.None;b.onClick.AddListener(click);
            Text(go.transform,"Label",value,2,0,w-4,h).alignment=TextAnchor.MiddleCenter;
            go.AddComponent<CanvasGroup>();
            go.AddComponent<ArcadeButtonFeel>().Initialize(go.GetComponent<Image>(),null,GetComponent<AudioSource>(),Resources.Load<AudioClip>("TablePresentation/Audio/click3"));
            return b;
        }
    }
}
