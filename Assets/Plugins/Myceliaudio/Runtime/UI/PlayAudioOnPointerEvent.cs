using AtMycelia.Myceliaudio.Utils;
using AtMycelia.UI;
using UnityEngine;
using PointerEventData = UnityEngine.EventSystems.PointerEventData;

namespace AtMycelia.Myceliaudio
{
    public class PlayAudioOnPointerEvent : MonoBehaviour
    {
        [SerializeField] protected UIPointerEventType _toRespondTo;
        [SerializeField] protected GameObject _respondsToPointerEvents;
        [SerializeField] protected QuickPlayAudio _audioPlayer;

        protected virtual void Awake()
        {
            _events = _respondsToPointerEvents.GetOrAddComponent<UIPointerEvents>();
        }

        protected UIPointerEvents _events;

        protected virtual void OnEnable()
        {
            ListenForEvents();
        }

        protected virtual void ListenForEvents()
        {
            if ((_toRespondTo & UIPointerEventType.Up) == UIPointerEventType.Up)
            {
                _events.PointerUp += OnPointerEventTriggered;
            }

            if ((_toRespondTo & UIPointerEventType.Down) == UIPointerEventType.Down)
            {
                _events.PointerDown += OnPointerEventTriggered;
            }

            if ((_toRespondTo & UIPointerEventType.Click) == UIPointerEventType.Click)
            {
                _events.PointerClick += OnPointerEventTriggered;
            }

            //////

            if ((_toRespondTo & UIPointerEventType.Enter) == UIPointerEventType.Enter)
            {
                _events.PointerEnter += OnPointerEventTriggered;
            }

            if ((_toRespondTo & UIPointerEventType.Exit) == UIPointerEventType.Exit)
            {
                _events.PointerExit += OnPointerEventTriggered;
            }

            //////

            if ((_toRespondTo & UIPointerEventType.BeginDrag) == UIPointerEventType.BeginDrag)
            {
                _events.BeginDrag += OnPointerEventTriggered;
            }

            if ((_toRespondTo & UIPointerEventType.Drag) == UIPointerEventType.Drag)
            {
                _events.Drag += OnPointerEventTriggered;
            }

            if ((_toRespondTo & UIPointerEventType.EndDrag) == UIPointerEventType.EndDrag)
            {
                _events.EndDrag += OnPointerEventTriggered;
            }

            if ((_toRespondTo & UIPointerEventType.Drop) == UIPointerEventType.Drop)
            {
                _events.Drop += OnPointerEventTriggered;
            }
        }

        protected virtual void OnPointerEventTriggered(PointerEventData eventData)
        {
            _audioPlayer.Play();
        }

        protected virtual void OnDisable()
        {
            UNlistenForEvents();
        }

        protected virtual void UNlistenForEvents()
        {
            _events.PointerUp -= OnPointerEventTriggered;
            _events.PointerDown -= OnPointerEventTriggered;
            _events.PointerClick -= OnPointerEventTriggered;

            _events.PointerEnter -= OnPointerEventTriggered;
            _events.PointerExit -= OnPointerEventTriggered;

            _events.BeginDrag -= OnPointerEventTriggered;
            _events.Drag -= OnPointerEventTriggered;
            _events.EndDrag -= OnPointerEventTriggered;
            _events.Drop -= OnPointerEventTriggered;
        }

        protected virtual void OnValidate()
        {
            if (_respondsToPointerEvents == null)
            {
                _respondsToPointerEvents = this.gameObject;
            }

            if (_audioPlayer == null)
            {
                _audioPlayer = this.gameObject.GetOrAddComponent<QuickPlayAudio>();
            }
        }
    }
}