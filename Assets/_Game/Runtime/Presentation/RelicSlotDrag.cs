using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace DoubleDown.Presentation
{
    public sealed class RelicSlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public int Slot;
        public Func<bool> CanEdit;
        public Action<int,int> Move;
        public RectTransform Icon;
        private Vector2 origin;
        private bool dragging;
        public void OnBeginDrag(PointerEventData e) { dragging=CanEdit();if(dragging)origin=Icon.anchoredPosition; }
        public void OnDrag(PointerEventData e)
        {
            if(!dragging)return;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)Icon.parent,e.position,e.pressEventCamera,out var point))
                Icon.localPosition=new Vector3(Mathf.Round(point.x),Mathf.Round(point.y),0);
        }
        public void OnEndDrag(PointerEventData e)
        {
            if(!dragging)return;dragging=false;Icon.anchoredPosition=origin;
            var target=e.pointerCurrentRaycast.gameObject?.GetComponentInParent<RelicSlotDrag>();
            if(target!=null && CanEdit())Move(Slot,target.Slot);
        }
    }
}
