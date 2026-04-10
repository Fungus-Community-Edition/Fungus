using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class VarDataTagFilter : ITagFilterable<StringData>, ITagFilterable<string>
    {
        [Tooltip("Only fire the event if one of the tags match. Empty means any will fire.")]
        [SerializeField]
        protected List<StringData> _tagFilter = new List<StringData>();

        public VarDataTagFilter()
        {
        }

        public VarDataTagFilter(IEnumerable<StringData> tags)
        {
            if (tags != null)
            {
                _tagFilter.AddRange(tags);
            }
        }

        public VarDataTagFilter(IEnumerable<string> tags)
        {
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    _tagFilter.Add(new StringData(tag));
                }
            }
        }

        public IList<StringData> Filters
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
                if (value is not IList<StringData> stringDataList)
                {
                    Debug.LogWarning($"VarDataTagFilter was passed a list of type {value.GetType()} when it expected a " +
                        $"list of StringData. Not changing contents.");
                    return;
                }
                _tagFilter.Clear();
                _tagFilter.AddRange(stringDataList);
            }
        }

        IList<string> ITagFilterable<string>.Filters
        {
            get => _tagFilter.ConvertAll(x => x.Value);
            set
            {
                if (value == null)
                {
                    _tagFilter.Clear();
                    return;
                }

                _tagFilter.Clear();

                foreach (var str in value)
                {
                    _tagFilter.Add(new StringData(str));
                }
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
                var currentTag = _tagFilter[i].Value;
                if (currentTag != null && currentTag.Equals(tagToCheck, strCompare))
                {
                    return true;
                }
            }

            return false;
        }

        public bool PassesFilter(StringData tagToCheck, StringComparison strCompare = StringComparison.Ordinal)
        {
            if (_tagFilter.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < _tagFilter.Count; i++)
            {
                var currentTag = _tagFilter[i].Value;
                if (currentTag != null && currentTag.Equals(tagToCheck.Value, strCompare))
                {
                    return true;
                }
            }

            return false;
        }

        public bool PassesFilter(object tag, StringComparison strCompare = StringComparison.Ordinal)
        {
            StringData strData = tag as StringData;
            string tagStr = tag as string;
            if (strData == null && tagStr == null)
            {
                Debug.LogError($"Tag passed to {nameof(PassesFilter)} is not a string or StringData. " +
                    $"Returning false.");
                return false;
            }

            if (strData != null)
            {
                return PassesFilter(strData, strCompare);
            }
            else if (tagStr != null)
            {
                return PassesFilter(tagStr, strCompare);
            }

            return false;
        }
    }
}