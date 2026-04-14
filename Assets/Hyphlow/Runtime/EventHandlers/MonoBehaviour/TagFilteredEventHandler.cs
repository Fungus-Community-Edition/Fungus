using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Base class for all of our physics event handlers
    /// </summary>
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
	public abstract class TagFilteredEventHandler : EventHandler
    {
        protected StringData[] _tagFilter = new StringData[0];

        protected void ProcessTagFilter(string tagOnOther)
        {
            if (DoesPassFilter(tagOnOther))
            {
                ExecuteBlock();
            }
        }

        protected bool DoesPassFilter(string tagOnOther)
        {
            if (_tagFilter.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < _tagFilter.Length; i++)
            {
                if (_tagFilter[i].Value == tagOnOther)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnAfterDeserializeBackwardsCompat()
        {
            base.OnAfterDeserializeBackwardsCompat();
            if (tagFilter != null && tagFilter.Length > 0)
            {
                _tagFilter = new StringData[tagFilter.Length];
                for (int i = 0; i < tagFilter.Length; i++)
                {
                    _tagFilter[i] = new StringData(tagFilter[i]);
                }
            }
        }

        [Tooltip("Only fire the event if one of the tags match. Empty means any will fire.")]
        [SerializeField]
        [HideInInspector]
        protected string[] tagFilter;
    }
}