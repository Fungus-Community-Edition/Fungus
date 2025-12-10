using UnityEngine;

namespace Amanita.VScripting.Legacy
{
	/// <summary>
	/// Plays a once-off sound effect. Multiple sound effects can be played at the same time.
	/// </summary>
	[CommandInfo("Audio",
				 "Play Random Sound",
				 "Plays a once-off sound effect from a list of available sound effects. Multiple sound effects can be played at the same time.")]
	[AddComponentMenu("")]
	public class PlayRandomSound : LegacyAudioCommand
	{
		[Tooltip("Sound effect clip to play")]
		[SerializeField]
		protected AudioClip[] soundClip;

		[Range(0, 1)]
		[Tooltip("Volume level of the sound effect")]
		[SerializeField]
		protected float volume = 1;


		#region Public members

		public override void OnEnter()
		{
			int rand = Random.Range(0, soundClip.Length);
			if (soundClip == null)
			{
				Continue();
				return;
			}

			AudioClip clipToPlay = soundClip[rand];
			MusicManager.PlaySound(clipToPlay, volume);

			if (waitUntilFinished)
			{
				Invoke(nameof(Continue), clipToPlay.length);
			}
			else
			{
				Continue();
			}
		}

		public override string GetSummary()
		{
			if (soundClip == null)
			{
				return "Error: No sound clip selected";
			}
			
			string sounds = "[";
			foreach (AudioClip ac in soundClip) {
				if(ac!=null)
					sounds+=ac.name+" ,";
			}
			sounds = sounds.TrimEnd(' ', ',');
			sounds += "]";
			return "Random sounds "+sounds;
		}

		public override Color GetButtonColor()
		{
			return CommandColors.Audio;
		}

		#endregion
	}
}
