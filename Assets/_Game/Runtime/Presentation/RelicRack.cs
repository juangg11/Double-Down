using System;
using System.Collections.Generic;
using DoubleDown.Scoring;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DoubleDown.Presentation
{
    public sealed class RelicRack : MonoBehaviour
    {
        public RelicLoadout Loadout { get; } = new RelicLoadout();
        public Func<string,bool> Owns { get; set; } = _ => true;
        public bool IsOpen => modal != null && modal.activeSelf;
        private readonly Text[] labels = new Text[6];
        private readonly Image[] icons = new Image[6];
        private readonly Queue<int> activations = new Queue<int>();
        private RelicDefinition[] catalog;
        private RelicDefinition candidate;
        private Font font;
        private GameObject modal;
        private Text details;
        private Text slotText;
        private Func<bool> canEdit;
        private Button equip;
        private int selected, page;
        private readonly List<GameObject> choices = new List<GameObject>();
        private readonly Button[] slotButtons = new Button[6];
        public void SetCapacity(int capacity) { Loadout.SetCapacity(capacity); selected = Math.Min(selected, capacity - 1); Refresh(); }
        private int glowing = -1;
        private float timer;
        public void Initialize(Canvas canvas, Font pixelFont, Func<bool> editable)
        {
            font = pixelFont; canEdit = editable;
            catalog = Resources.LoadAll<RelicDefinition>("Relics");
            Array.Sort(catalog, (a,b) => string.CompareOrdinal(a.id,b.id));
            if (catalog.Length < 15) throw new InvalidOperationException("Build the Phase 5 relic catalog first.");
            var initial = new[] {0,1,8,14};
            for (var i=0;i<4;i++) Loadout.Equip(i,catalog[initial[i]]);
            Text(canvas.transform,"Relic Heading","RELIQUIAS",180,2,436,16);
            for (var i=0;i<6;i++)
            {
                var slot=i;
                var button=Button(canvas.transform,"Relic Slot "+i,"",180+i*112,20,100,64,()=>Open(slot));
                slotButtons[i] = button;
                labels[i]=Text(button.transform,"Name","",4,30,92,32);
                labels[i].alignment=TextAnchor.MiddleCenter;
                icons[i]=Panel(button.transform,"Icon",38,5,24,24,CasinoUiSkin.Paper).GetComponent<Image>();
                icons[i].raycastTarget=false;
                var drag=button.gameObject.AddComponent<RelicSlotDrag>();drag.Slot=i;drag.Icon=icons[i].rectTransform;drag.CanEdit=canEdit;
                drag.Move=(from,to)=>{if(canEdit() && Loadout.Move(from,to)){selected=to;Refresh();}};
            }
            modal=Panel(canvas.transform,"Relic Collection",0,0,640,360,new Color(0,0,0,0.8f));
            var body=Panel(modal.transform,"Body",34,42,572,278,CasinoUiSkin.Ink);
            CasinoUiSkin.Surface(body.GetComponent<Image>(), "Frame", CasinoUiSkin.Felt);
            slotText=Text(body.transform,"Slot","",12,10,430,20);
            Button(body.transform,"Previous Page","<",454,8,44,24,()=>{page=Math.Max(0,page-1);Refresh();});
            Button(body.transform,"Next Page",">",510,8,44,24,()=>{page=Math.Min((catalog.Length-1)/15,page+1);Refresh();});
            for(var i=0;i<catalog.Length;i++)
            {
                var relic=catalog[i];
                var choice=Button(body.transform,"Relic "+relic.id,relic.displayName,
                    12+(i%3)*184,36+((i%15)/3)*27,176,23,()=>{candidate=relic; Refresh();});
                choices.Add(choice.gameObject);
            }
            details=Text(body.transform,"Description","",12,176,548,42);
            Button(body.transform,"Up","IZQUIERDA",12,232,88,30,()=>Move(-1));
            Button(body.transform,"Down","DERECHA",108,232,88,30,()=>Move(1));
            equip=Button(body.transform,"Equip","EQUIPAR",226,232,124,30,()=>
            {
                if(canEdit() && Owns(candidate.id) && Loadout.Equip(selected,candidate)) Refresh();
            });
            Button(body.transform,"Close","VOLVER",380,232,176,30,Toggle);
            body.AddComponent<PanelMotion>();
            candidate=Loadout[0];
            modal.SetActive(false);
            Refresh();
        }
        private void Open(int slot) { selected=slot; candidate=Loadout[slot] ?? catalog[0]; page=Array.IndexOf(catalog,candidate)/15; modal.SetActive(true); modal.transform.SetAsLastSibling(); Refresh(); }
        public void ResetLoadout()
        {
            Loadout.SetCapacity(4); selected=0; page=0;
            for(var i=0;i<4;i++) Loadout.Equip(i,null);
            var initial=new[]{0,1,8,14};for(var i=0;i<4;i++) Loadout.Equip(i,catalog[initial[i]]);
            candidate=Loadout[selected];Refresh();
        }
        public void Toggle() { if(IsOpen) modal.SetActive(false); else Open(selected); }
        private void Move(int direction)
        {
            var next=selected+direction;
            if(canEdit() && Loadout.Move(selected,next)) { selected=next; Refresh(); }
        }
        public void Activate(IEnumerable<int> slots)
        {
            activations.Clear();
            foreach(var slot in slots) activations.Enqueue(slot);
            timer=0;
        }
        private void Update()
        {
            if(GetComponent<TableControls>().Pressed(TableAction.Relics) &&
                !(GetComponent<TablePresentation>()?.IsOpen ?? false) && !(GetComponent<DeckWorkshop>()?.IsOpen ?? false) && !(GetComponent<RunShop>()?.Inspecting ?? false)) Toggle();
            timer-=Time.unscaledDeltaTime;
            if(glowing>=0 && !TablePresentation.ReducedMotion)
                icons[glowing].rectTransform.anchoredPosition=new Vector2(icons[glowing].rectTransform.anchoredPosition.x,-5+Mathf.Round(Mathf.Sin(Mathf.Clamp01(timer/0.22f)*Mathf.PI)*2f));
            if(timer<=0)
            {
                if(glowing>=0) icons[glowing].rectTransform.anchoredPosition=new Vector2(icons[glowing].rectTransform.anchoredPosition.x,-5);
                glowing=activations.Count>0?activations.Dequeue():-1;
                if(glowing>=0) timer=0.22f;
            }
        }
        private void Refresh()
        {
            if (candidate == null) candidate = catalog[0];
            for (var i=0;i<choices.Count;i++) choices[i].SetActive(i/15 == page);
            var width = 436f / Loadout.SlotCount;
            for(var i=0;i<6;i++)
            {
                slotButtons[i].gameObject.SetActive(i < Loadout.SlotCount);
                if (i >= Loadout.SlotCount) continue;
                var rect=slotButtons[i].GetComponent<RectTransform>(); rect.anchoredPosition=new Vector2(Mathf.Round(180+i*width),-20);rect.sizeDelta=new Vector2(Mathf.Floor(width)-8,64);
                labels[i].rectTransform.sizeDelta=new Vector2(rect.sizeDelta.x-8,32);
                labels[i].text=Loadout[i]?.displayName ?? "VACIO"; icons[i].sprite=Loadout[i]?.icon; icons[i].enabled=Loadout[i]!=null;
                icons[i].rectTransform.anchoredPosition=new Vector2(Mathf.Round((rect.sizeDelta.x-24)/2),-5);
            }
            slotText.text=$"HUECO {selected+1}/{Loadout.SlotCount}  |  PAG {page+1}/{(catalog.Length+14)/15}";
            details.text=candidate.displayName+": "+candidate.description+
                (!Owns(candidate.id)?"\nNO ADQUIRIDA: DISPONIBLE EN LA TIENDA.":canEdit()?"":"\nTERMINA LA MANO PARA CAMBIAR RELIQUIAS.");
            var duplicate=false;
            for(var i=0;i<Loadout.SlotCount;i++) if(i!=selected && Loadout[i]!=null && Loadout[i].id==candidate.id) duplicate=true;
            equip.interactable=canEdit()&&Owns(candidate.id)&&!duplicate;
        }
        private static GameObject Panel(Transform parent,string name,float x,float y,float w,float h,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);
            rect.anchoredPosition=new Vector2(x,-y); rect.sizeDelta=new Vector2(w,h); go.GetComponent<Image>().color=color;
            return go;
        }
        private Text Text(Transform parent,string name,string value,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);
            rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(w,h);
            var text=go.GetComponent<Text>();text.font=font;text.material=Graphic.defaultGraphicMaterial;text.fontSize=16;text.text=value;
            text.color=CasinoUiSkin.Paper;text.raycastTarget=false;return text;
        }
        private Button Button(Transform parent,string name,string value,float x,float y,float w,float h,UnityEngine.Events.UnityAction action)
        {
            var go=Panel(parent,name,x,y,w,h,new Color(0.06f,0.16f,0.19f));
            CasinoUiSkin.Surface(go.GetComponent<Image>(), "Button", CasinoUiSkin.Felt);
            go.AddComponent<CanvasGroup>();
            var button=go.AddComponent<Button>();button.targetGraphic=go.GetComponent<Image>();button.onClick.AddListener(action);
            button.transition=Selectable.Transition.None;
            var sound=Resources.Load<AudioClip>("TablePresentation/Audio/click3");
            go.AddComponent<ArcadeButtonFeel>().Initialize(go.GetComponent<Image>(),null,GetComponent<AudioSource>(),sound);
            var label=Text(go.transform,"Label",value,2,0,w-4,h);label.alignment=TextAnchor.MiddleCenter;
            return button;
        }
    }
}
