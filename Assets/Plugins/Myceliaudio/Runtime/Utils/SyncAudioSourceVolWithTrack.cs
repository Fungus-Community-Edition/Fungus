using UnityEngine;

namespace AtMycelia.Myceliaudio
{
	public class SyncAudioSourceVolWithTrack : MonoBehaviour
	{
		[SerializeField] protected TrackGroup _trackGroup = TrackGroup.Null;
		[SerializeField] protected int _track;
		[SerializeField] protected AudioSource _audioSource;

		protected virtual void Awake()
		{
			if (_trackGroup == TrackGroup.Null)
			{
				Debug.LogError("TrackGroup is not set on " + this.name);
				return;
			}

			if (_audioSource == null)
			{
				_audioSource = GetComponent<AudioSource>();
			}

			UpdateAudioSourceVolume();
		}

		protected virtual void UpdateAudioSourceVolume()
		{
			float vol = AudioSystem.S.GetTrackVol(_trackGroup, _track);
			vol /= AudioMath.VolumeConversion;
			_audioSource.volume = vol;
		}

		protected virtual void OnEnable()
		{
			ListenForEvents();
		}

		protected virtual void ListenForEvents()
		{
			AudioEvents.TrackSetVolChanged += OnTrackSetVolChanged;
		}

		private void OnTrackSetVolChanged(TrackGroup groupInvolved, float newBaseVol)
		{
			// We don't need to worry about whether the group involved is the same one we're
			// going with, since the former might be the anchor for the latter. Plus, all we're
			// really doing is fetching and applying a float value, so...
			UpdateAudioSourceVolume();
		}

		protected virtual void OnDisable()
		{
			UNlistenForEvents();
		}

		protected virtual void UNlistenForEvents()
		{
			AudioEvents.TrackSetVolChanged -= OnTrackSetVolChanged;
		}
	}
}