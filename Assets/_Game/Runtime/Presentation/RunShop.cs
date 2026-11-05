using System;
using System.Linq;
using System.Collections.Generic;
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
        public event Action RoundRewarded;
        private RunInventory inventory;
        private ConsumableDefinition[] consumables;
        private VoucherDefinition[] vouchers;
        private readonly ConsumableDefinition[] consumableOffers = new ConsumableDefinition[3];
        private readonly VoucherDefinition[] voucherOffers = new VoucherDefinition[3];
        private System.Random contentRandom;
        private int tab, seed;
        private EconomyBalanceConfig balance;
        private GameObject inspection;
        private Text inspectionName, inspectionText;
        public bool Inspecting => inspection != null && inspection.activeSelf;
        private readonly Button[] tabs = new Button[3];
        public int ConsumablePrice => Math.Max(1, balance.consumablePrice - inventory.Bonus(VoucherEffect.ConsumableDiscount));
        public int VoucherPrice => balance.voucherPrice;
        public IReadOnlyList<ConsumableDefinition> ConsumableOffers => consumableOffers;
        public IReadOnlyList<VoucherDefinition> VoucherOffers => voucherOffers;
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
        private int openedFrame;

        public void Initialize(Canvas canvas,Font pixelFont,int seed,RunInventory inventory)
        {
            this.inventory=inventory;this.seed=seed;contentRandom=new System.Random(seed ^ 0x57A3);
            consumables=Resources.LoadAll<ConsumableDefinition>("Consumables").OrderBy(c=>c.id,StringComparer.Ordinal).ToArray();
            vouchers=Resources.LoadAll<VoucherDefinition>("Vouchers").OrderBy(c=>c.id,StringComparer.Ordinal).ToArray();
            font=pixelFont; catalog=Resources.LoadAll<RelicDefinition>("Relics");
            balance = Resources.Load<EconomyBalanceConfig>("EconomyBalance");
            if (balance == null) throw new InvalidOperationException("EconomyBalance asset is missing.");
            Economy=new RunEconomy(catalog.Select(r=>r.id),seed,balance.ToRules());
            Economy.AdditionalOffersAvailable=consumables.Length>0;
            root=Panel(canvas.transform,"Run Shop",172,92,456,260,new Color(0.018f,0.04f,0.06f));
            CasinoUiSkin.Surface(root.GetComponent<Image>(), "Frame", CasinoUiSkin.Ink);
            heading=Text(root.transform,"Heading","",12,8,432,22);
            for(var i=0;i<3;i++)
            {
                var t=i;tabs[i]=Button(root.transform,"Tab "+i,GameLanguage.English?new[]{"RELICS","CONSUMABLES","VOUCHERS"}[i]:new[]{"RELIQUIAS","CONSUMIBLES","VALES"}[i],12+i*146,32,140,22,()=>{tab=t;Refresh();});
            }
            for(var i=0;i<3;i++)
            {
                var index=i;
                var card=Panel(root.transform,"Offer "+i,12+i*146,62,140,136,new Color(0.04f,0.12f,0.15f));
                CasinoUiSkin.Surface(card.GetComponent<Image>(), "Frame", CasinoUiSkin.Felt);
                icons[i]=Panel(card.transform,"Icon",60,4,20,20,Color.white).GetComponent<Image>();
                names[i]=Text(card.transform,"Name","",6,26,128,30);names[i].alignment=TextAnchor.MiddleCenter;
                descriptions[i]=Text(card.transform,"Effect","",6,56,128,50);
                descriptions[i].fontSize=12;
                buy[i]=Button(card.transform,"Buy","",6,108,128,24,()=>BuyOffer(index));
            }
            status=Text(root.transform,"Status","",12,202,432,20);
            reroll=Button(root.transform,"Reroll","",12,224,128,28,()=>{if(Economy.Reroll())RollContent();Refresh();});
            Button(root.transform,"Continue",GameLanguage.Pick("SIGUIENTE RONDA","NEXT ROUND"),158,224,286,28,LeaveShop);
            inspection=Panel(canvas.transform,"Shop Inspection",0,0,640,360,new Color(0,0,0,0.85f));
            var info=Panel(inspection.transform,"Body",120,78,400,204,CasinoUiSkin.Ink);
            CasinoUiSkin.Surface(info.GetComponent<Image>(),"Frame",CasinoUiSkin.Ink);
            inspectionName=Text(info.transform,"Title","",16,16,368,32);
            inspectionText=Text(info.transform,"Description","",16,54,368,100);
            Button(info.transform,"Close","VOLVER",16,164,368,28,()=>inspection.SetActive(false));
            info.AddComponent<PanelMotion>();inspection.SetActive(false);
            root.AddComponent<PanelMotion>();
            root.SetActive(false);
        }
        public void Open(int round,Action onLeave,int remainingHands=0)
        {
            openedFrame=Time.frameCount;
            SyncBonuses();
            var previous=Economy.RewardedRound;
            Economy.Open(round,remainingHands);
            if(Economy.RewardedRound>previous) { RollContent();RoundRewarded?.Invoke(); }
            leave=onLeave;root.SetActive(true);root.transform.SetAsLastSibling();Refresh();
        }
        private void LeaveShop()
        { if (!IsOpen) return; DepartedRound=Economy.RewardedRound;root.SetActive(false);inspection.SetActive(false);leave?.Invoke(); }
        private void Inspect(int index)
        {
            var relic=Array.Find(catalog,r=>r.id==Economy.Offers[index]);var c=consumableOffers[index];var v=voucherOffers[index];
            if(tab==0 && relic==null || tab==1 && c==null || tab==2 && v==null)return;
            inspectionName.text=tab==0?relic.displayName:tab==1?c.displayName:v.displayName;
            inspectionText.text=tab==0?relic.description+"\nRAREZA: "+new[]{"COMUN","RARA","EPICA","LEGENDARIA"}[(int)relic.rarity]:tab==1?c.description:v.description;
            inspection.SetActive(true);inspection.transform.SetAsLastSibling();
        }
        private void Update()
        {
            if(Inspecting) { if(GetComponent<TableControls>().EscapePressed){inspection.SetActive(false);GetComponent<TableControls>().ConsumeFrame();} return; }
            if (!IsOpen || Time.frameCount==openedFrame || GetComponent<DeckWorkshop>()?.IsOpen == true || GetComponent<RelicRack>().IsOpen || GetComponent<TablePresentation>().IsOpen) return;
            if (GetComponent<TableControls>().Pressed(TableAction.Confirm)) LeaveShop();
        }
        public void ResetRun() { Economy.Reset();contentRandom=new System.Random(seed ^ 0x57A3); Array.Clear(consumableOffers,0,3);Array.Clear(voucherOffers,0,3);DepartedRound=0;tab=0;root.SetActive(false); }
        private void SyncBonuses()
        {
            Economy.RelicDiscount=inventory.Bonus(VoucherEffect.RelicDiscount);
            Economy.RerollDiscount=inventory.Bonus(VoucherEffect.RerollDiscount);
            Economy.BonusReward=inventory.Bonus(VoucherEffect.RoundGold);
            Economy.InterestCap=inventory.Bonus(VoucherEffect.Interest);
        }
        private void RollContent()
        {
            var pool=consumables.ToList();var eligible=vouchers.Where(inventory.CanBuy).ToList();
            for(var i=0;i<3;i++)
            {
                if(pool.Count==0)consumableOffers[i]=null;
                else {var n=contentRandom.Next(pool.Count);consumableOffers[i]=pool[n];pool.RemoveAt(n);}
                if(eligible.Count==0)voucherOffers[i]=null;
                else {var n=contentRandom.Next(eligible.Count);voucherOffers[i]=eligible[n];eligible.RemoveAt(n);}
            }
        }
        private void BuyOffer(int index)
        {
            if(!IsOpen)return;
            if(tab==0)
            {
                var rack=GetComponent<RelicRack>();
                var relic=Array.Find(catalog,r=>r.id==Economy.Offers[index]);
                if(rack!=null && rack.HasEmptySlot && Economy.Buy(index)) rack.EquipFirstFree(relic);
            }
            else if(tab==1 && inventory.Buy(consumableOffers[index],Economy,ConsumablePrice))consumableOffers[index]=null;
            else if(tab==2 && inventory.Buy(voucherOffers[index],Economy,VoucherPrice)) {voucherOffers[index]=null;SyncBonuses();}
            Refresh();
        }
        private void Refresh()
        {
            heading.text=$"{GameLanguage.Pick("TIENDA","SHOP")}  /  {GameLanguage.Pick("ORO","GOLD")} {Economy.Gold}  /  {GameLanguage.Pick("PREMIO","REWARD")} +{Economy.LastReward}";
            for(var i=0;i<3;i++)
            {
                tabs[i].interactable=i!=tab;
                var relic=Array.Find(catalog,r=>r.id==Economy.Offers[i]);
                var c=consumableOffers[i];var v=voucherOffers[i];
                var available=tab==0?relic!=null:tab==1?c!=null:v!=null;
                names[i].text=!available?GameLanguage.Pick("AGOTADO","SOLD OUT"):tab==0?relic.displayName:tab==1?c.displayName:v.displayName;
                descriptions[i].text=!available?"":tab==0?GameLanguage.RelicEffect(relic):tab==1?GameLanguage.ConsumableEffect(c):GameLanguage.VoucherEffect(v);
                icons[i].enabled=available;
                if(available)icons[i].sprite=tab==0?relic.icon:tab==1?c.icon:v.icon;
                var price=tab==0?Economy.RelicPrice:tab==1?ConsumablePrice:VoucherPrice;
                buy[i].interactable=available && Economy.Gold>=price && (tab!=0 || GetComponent<RelicRack>().HasEmptySlot) && (tab!=1 || inventory.Consumables.Count<inventory.ConsumableCapacity) && (tab!=2 || inventory.CanBuy(v));
                buy[i].GetComponentInChildren<Text>().text=!available?GameLanguage.Pick("AGOTADO","SOLD OUT"):$"{GameLanguage.Pick("COMPRAR","BUY")} {price}";
            }
            reroll.interactable=Economy.CanReroll;
            reroll.GetComponentInChildren<Text>().text=$"{GameLanguage.Pick("RENOVAR","REROLL")} {Economy.RerollPrice}";
            status.text=tab==1?$"{GameLanguage.Pick("BOLSA","BAG")} {inventory.Consumables.Count}/{inventory.ConsumableCapacity} - {GameLanguage.Pick("USA EN MAZO","USE IN DECK")}":tab==2?GameLanguage.Pick("PERMANENTES DURANTE ESTA RUN","PERMANENT FOR THIS RUN"):GetComponent<RelicRack>().HasEmptySlot?GameLanguage.Pick("AL COMPRAR, SE EQUIPA AUTOMATICAMENTE","PURCHASES AUTO-EQUIP"):GameLanguage.Pick("SIN HUECOS: REORDENA TUS RELIQUIAS","NO FREE SLOTS: REORDER RELICS");
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
