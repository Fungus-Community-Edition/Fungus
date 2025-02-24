using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CGT.UI
{
    public class UIPointerEvents : MonoBehaviour, IPointerClickHandler, IPointerDownHandler,
        IPointerUpHandler, IPointerEnterHandler,
        IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            PointerClick(eventData);
        }

        public event UnityAction<PointerEventData> PointerClick = delegate { };

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            PointerDown(eventData);
        }

        public event UnityAction<PointerEventData> PointerDown = delegate { };


        public virtual void OnPointerUp(PointerEventData eventData)
        {
            PointerUp(eventData);
        }

        public event UnityAction<PointerEventData> PointerUp = delegate { };

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnter(eventData);
        }

        public event UnityAction<PointerEventData> PointerEnter = delegate { };

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            PointerExit(eventData);
        }

        public event UnityAction<PointerEventData> PointerExit = delegate { };

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            BeginDrag(eventData);
        }

        public event UnityAction<PointerEventData> BeginDrag = delegate { };

        public virtual void OnDrag(PointerEventData eventData)
        {
            Drag(eventData);
        }

        public event UnityAction<PointerEventData> Drag = delegate { };

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            EndDrag(eventData);
        }

        public event UnityAction<PointerEventData> EndDrag = delegate { };

        public virtual void OnDrop(PointerEventData eventData)
        {
            Drop(eventData);
        }

        public event UnityAction<PointerEventData> Drop = delegate { };

    }
}