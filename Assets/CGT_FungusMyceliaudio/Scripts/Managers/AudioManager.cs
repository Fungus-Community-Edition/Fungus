using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CGT.FungusExt.Myceliaudio.Internal
{
    public class AudioManager
    {
        protected IDictionary<int, FungusAudioTrack> tracks = new Dictionary<int, FungusAudioTrack>();

        public AudioManager(GameObject gameObject)
        {
            forTweens = gameObject;
            SetUpInitialTracks();
        }

        protected GameObject forTweens;

        protected virtual void SetUpInitialTracks()
        {
            for (int i = 0; i < initTrackCount; i++)
            {
                tracks[i] = new FungusAudioTrack(forTweens);
            }
        }

        protected int initTrackCount = 2;

        public virtual void Play(AudioArgs args)
        {
            tracks[args.Track].Play(args);
        }

        public virtual void SetVolume(AudioArgs args)
        {
            tracks[args.Track].SetVolume(args);
        }

        public virtual void FadeVolume(AudioArgs args)
        {
            tracks[args.Track].FadeVolume(args);
        }

        public virtual void SetPitch(AudioArgs args)
        {
            tracks[args.Track].SetPitch(args);
        }

        public virtual void Stop(AudioArgs args)
        {
            tracks[args.Track].Stop(args);
        }

        public virtual float GetVolume(AudioArgs args)
        {
            return tracks[args.Track].CurrentVolume;
        }

        public virtual float GetPitch(AudioArgs args)
        {
            return tracks[args.Track].CurrentPitch;
        }
    
        public virtual string Name { get; set; }
    }
}