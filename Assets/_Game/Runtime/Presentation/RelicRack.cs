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
        public bool IsOpen => modal != null && modal.activeSelf;
        private readonly Text[] labels = new Text[4];
        private readonly Image[] icons = new Image[4];
        private readonly Image[] faces = new Image[4];
        private readonly Queue<int> activations = new Queue<int>();
        private RelicDefinition[] catalog;
        private RelicDefinition candidate;
        private Font font;
        private GameObject modal;
        private Text details;
        private Text slotText;
        private Func<bool> canEdit;
        private Button equip;
        private int selected;
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
            Text(canvas.transform,"Relic Heading","RELIQUIAS [TAB]",14,118,125,16);
            for (var i=0;i<4;i++)
            {
                var slot=i;
                var button=Button(canvas.transform,"Relic Slot "+i,"",14,136+i*22,128,20,()=>Open(slot));
                labels[i]=Text(button.transform,"Name","",24,1,102,18);
                faces[i]=button.GetComponent<Image>();
                icons[i]=Panel(button.transform,"Icon",2,2,16,16,Color.white).GetComponent<Image>();
                icons[i].raycastTarget=false;
            }
            modal=Panel(canvas.transform,"Relic Collection",0,0,640,360,new Color(0,0,0,0.8f));
            var body=Panel(modal.transform,"Body",34,42,572,278,new Color(0.018f,0.04f,0.065f));
            slotText=Text(body.transform,"Slot","",12,10,540,20);
            for(var i=0;i<catalog.Length;i++)
            {
                var relic=catalog[i];
                var choice=Button(body.transform,"Relic "+relic.id,relic.displayName,
                    12+(i%3)*184,36+(i/3)*27,176,23,()=>{candidate=relic; Refresh();});
            }
            details=Text(body.transform,"Description","",12,176,548,42);
            Button(body.transform,"Up","SUBIR",12,232,88,30,()=>Move(-1));
            Button(body.transform,"Down","BAJAR",108,232,88,30,()=>Move(1));
            equip=Button(body.transform,"Equip","EQUIPAR",226,232,124,30,()=>
            {
                if(canEdit() && Loadout.Equip(selected,candidate)) Refresh();
            });
            Button(body.transform,"Close","VOLVER [TAB]",380,232,176,30,Toggle);
            candidate=Loadout[0];
            modal.SetActive(false);
            Refresh();
        }
        private void Open(int slot) { selected=slot; candidate=Loadout[slot]; modal.SetActive(true); Refresh(); }
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
            if(Keyboard.current!=null && Keyboard.current.tabKey.wasPressedThisFrame &&
                !(GetComponent<TablePresentation>()?.IsOpen ?? false)) Toggle();
            timer-=Time.unscaledDeltaTime;
            if(timer<=0)
            {
                if(glowing>=0) { labels[glowing].color=new Color(1f,0.81f,0.48f); faces[glowing].color=new Color(0.06f,0.16f,0.19f); }
                glowing=activations.Count>0?activations.Dequeue():-1;
                if(glowing>=0) { labels[glowing].color=Color.white; faces[glowing].color=new Color(0.3f,0.43f,0.24f); timer=0.22f; }
            }
        }
        private void Refresh()
        {
            for(var i=0;i<4;i++) { labels[i].text=Loadout[i].displayName; icons[i].sprite=Loadout[i].icon; }
            slotText.text=$"HUECO {selected+1}/4  |  ORDEN DE ARRIBA A ABAJO";
            details.text=candidate.displayName+": "+candidate.description+
                (canEdit()?"":"\nTERMINA LA MANO PARA CAMBIAR RELIQUIAS.");
            var duplicate=false;
            for(var i=0;i<4;i++) if(i!=selected && Loadout[i].id==candidate.id) duplicate=true;
            equip.interactable=canEdit()&&!duplicate;
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
            var text=go.GetComponent<Text>();text.font=font;text.fontSize=16;text.text=value;
            text.color=new Color(1f,0.81f,0.48f);text.raycastTarget=false;return text;
        }
        private Button Button(Transform parent,string name,string value,float x,float y,float w,float h,UnityEngine.Events.UnityAction action)
        {
            var go=Panel(parent,name,x,y,w,h,new Color(0.06f,0.16f,0.19f));
            var button=go.AddComponent<Button>();button.targetGraphic=go.GetComponent<Image>();button.onClick.AddListener(action);
            var sound=Resources.Load<AudioClip>("TablePresentation/Audio/click3");
            button.onClick.AddListener(()=>{var audio=GetComponent<AudioSource>();if(audio!=null && sound!=null) audio.PlayOneShot(sound,0.35f);});
            var label=Text(go.transform,"Label",value,2,0,w-4,h);label.alignment=TextAnchor.MiddleCenter;
            return button;
        }
    }
}
