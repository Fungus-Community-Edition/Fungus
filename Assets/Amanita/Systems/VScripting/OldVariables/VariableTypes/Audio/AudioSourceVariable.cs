


using System;
using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// AudioSource variable type.
    /// </summary>
    [VariableInfo("Other", "AudioSource")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class AudioSourceVariable : VariableBase<AudioSource>
    {
    }

    /// <summary>
    /// Container for an AudioSource variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public class AudioSourceData : VariableData<AudioSource, IVariable<AudioSource>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(AudioSourceVariable))]
        public AudioSourceVariable audioSourceRef;
        
        [SerializeField]
        public AudioSource audioSourceVal;

        public static implicit operator AudioSource(AudioSourceData audioSourceData)
        {
            return audioSourceData.Value;
        }

        public AudioSourceData(AudioSource startVal = null) : base(startVal) { }

        public override IVariable VarRef
        {
            get { return audioSourceRef; }
            set
            {
                if (value == null) { audioSourceRef = null; return; }

                if (VarRef.ContentType.Equals(this.ContentType))
                {
                    audioSourceRef = value as AudioSourceVariable;
                }
                else
                {
                    string errorMessage = $"This can only accept a variable type that holds content of type {ContentType.Name}.";
                    throw new System.InvalidCastException(errorMessage);
                }

            }
        }

    }
}