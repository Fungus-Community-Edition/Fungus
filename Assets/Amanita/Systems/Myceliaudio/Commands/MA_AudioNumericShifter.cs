using UnityEngine;
using Amanita.VScripting;

namespace Amanita.Myceliaudio.VScripting
{
    /// <summary>
    /// For changing some numeric value in an AudioSource. Volume, pitch, etc
    /// </summary>
    public abstract class MA_AudioNumericShifter : MyceliaudioCommand
    {
        [SerializeField] protected GetOrSet operation = GetOrSet.Set;
        [SerializeField] protected IntegerData track = new IntegerData(0);
        [SerializeField] protected FloatData targetVol = new FloatData();
        [SerializeField] protected TrackSelection trackSelection = TrackSelection.Group;

        public override void OnEnter()
        {
            base.OnEnter();

            switch (operation)
            {
                case GetOrSet.Set:
                    HandleShifting();
                    break;
                case GetOrSet.Get:
                    HandleGetting();
                    break;
                default:
                    string errorMessage = $"Cannot set or get track volume when the operation is {operation}";
                    Debug.LogError(errorMessage);
                    break;
            }

            Continue();
        }

        protected abstract void HandleShifting();
        protected abstract void HandleGetting();

    }
}