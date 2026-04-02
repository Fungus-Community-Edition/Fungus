using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia
{
    /// <summary>
    /// Reusable tag filtering logic that can be composed into event handlers
    /// </summary>
    [Serializable]
    public class TagFilter : ITagFilterable<string>
    {
        [Tooltip("Only fire the event if one of the tags match. Empty means any will fire.")]
        [SerializeField]
        protected List<string> _tagFilter = new List<string>();

        public IList<string> Filters
        { 
            get => _tagFilter;
            set
            {
                _tagFilter.Clear();

                if (value == null || value.Count == 0)
                {
                    return;
                }

                _tagFilter.AddRange(value);
            }
        }

        IList<object> ITagFilterable.Filters
        {
            get => _tagFilter.ConvertAll(x => (object)x);
            set
            {
                if (value == null)
                {
                    _tagFilter.Clear();
                    return;
                }

                if (value is not IList<string> stringList)
                {
                    Debug.LogWarning($"TagFilter was passed a list of type {value.GetType()} when it expected a " +
                        $"list of strings. Not changing contents.");
                    return;
                }

                _tagFilter.Clear();
                _tagFilter.AddRange(stringList);
            }
        }

        public bool PassesFilter(string tagToCheck, StringComparison strCompare = StringComparison.Ordinal)
        {
            if (_tagFilter.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < _tagFilter.Count; i++)
            {
                var currentTag = _tagFilter[i];
                if (currentTag.Equals(tagToCheck, strCompare))
                {
                    return true;
                }
            }

            return false;
        }

        public bool PassesFilter(object tag, StringComparison strCompare = StringComparison.Ordinal)
        {
            if (tag is string strTag)
            {
                return PassesFilter(strTag, strCompare);
            }
            else
            {
                Debug.LogWarning($"TagFilter was passed a tag of type {tag.GetType()} when it expected a " +
                    $"string. Returning false.");
                return false;
            }
        }
    }
}