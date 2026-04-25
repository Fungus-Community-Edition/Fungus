using AtMycelia.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using PointerEventData = UnityEngine.EventSystems.PointerEventData;

namespace AtMycelia.Myceliaudio
{
    [System.Obsolete("Better to use PlayAudioOnPointerEvent as that follows the new architecture better")]
    public class PlaySoundOnPointerEvent : MonoBehaviour
    {
        [SerializeField] protected UIPointerEventType _toRespondTo;
        [SerializeField] protected UIPointerEvents _events;
        [SerializeField] protected TrackGroup _trackGroup;
        [SerializeField] protected int _track;
        [SerializeField] protected AudioClip _soundToPlay;

        protected virtual void Awake()
        {
            if (_events == null)
            {
                _events = GetComponent<UIPointerEvents>();
            }

            bool shouldCreateOwnEvents = _events == null;
            if (shouldCreateOwnEvents)
            {
                _events = this.gameObject.AddComponent<UIPointerEvents>();
            }

            _selectable = _events.GetComponent<Selectable>();

            _audioArgs.TrackGroup = _trackGroup;
            _audioArgs.Track = _track;
            _audioArgs.MainClip = _soundToPlay;
        }

        protected Selectable _selectable;
        protected PlayAudioArgs _audioArgs = new PlayAudioArgs();

        protected List<UnityAction<PointerEventData>> toListenFor = new List<UnityAction<PointerEventData>>();

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
            AudioSystem.S.Play(_audioArgs);
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

    }
}