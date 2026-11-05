using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace DoubleDown.Presentation
{
    public sealed class PortfolioUi
    {
        private readonly Font font; private readonly AudioSource audio;
        public PortfolioUi(Font font,AudioSource audio){this.font=font;this.audio=audio;}
        public static Canvas Canvas(string name)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=Camera.main;canvas.overrideSorting=true;canvas.sortingOrder=2000;
            var r=go.GetComponent<RectTransform>();r.sizeDelta=new Vector2(640,360);r.pivot=new Vector2(0,1);r.position=new Vector3(-5,2.8125f,-1);r.localScale=Vector3.one/64f;return canvas;
        }
        public static RectTransform Rect(GameObject go,Transform parent,float x,float y,float w,float h)
        {
            go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        public GameObject Panel(Transform parent,string name,float x,float y,float w,float h,Color color,bool frame=false)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));Rect(go,parent,x,y,w,h);var image=go.GetComponent<Image>();image.color=color;if(frame)CasinoUiSkin.Surface(image,"Frame",color);return go;
        }
        public Text Text(Transform parent,string name,string value,float x,float y,float w,float h,int size=16)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));Rect(go,parent,x,y,w,h);var t=go.GetComponent<Text>();t.font=font;t.fontSize=size;t.text=value;t.color=CasinoUiSkin.Paper;t.raycastTarget=false;return t;
        }
        public Button Button(Transform parent,string name,string value,float x,float y,float w,float h,UnityAction action,bool primary=false)
        {
            var shadow=Panel(parent,name+" Shadow",x+2,y+3,w,h,CasinoUiSkin.Ink).GetComponent<Image>();shadow.raycastTarget=false;CasinoUiSkin.Surface(shadow,"Button",CasinoUiSkin.Ink);
            var go=Panel(parent,name,x,y,w,h,primary?CasinoUiSkin.Red:CasinoUiSkin.Felt);CasinoUiSkin.Surface(go.GetComponent<Image>(),"Button",primary?CasinoUiSkin.Red:CasinoUiSkin.Felt);
            var b=go.AddComponent<Button>();b.targetGraphic=go.GetComponent<Image>();b.transition=Selectable.Transition.None;b.onClick.AddListener(action);go.AddComponent<CanvasGroup>();
            Text(go.transform,"Label",value,4,0,w-8,h).alignment=TextAnchor.MiddleCenter;
            go.AddComponent<ArcadeButtonFeel>().Initialize(go.GetComponent<Image>(),shadow,audio,Resources.Load<AudioClip>("TablePresentation/Audio/click3"));return b;
        }
    }
}
