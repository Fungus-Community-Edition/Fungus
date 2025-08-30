using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace Amanita.VScripting.EditorUtils
{
    public class VariableListView : IVariableListView
    {
        public VariableListView(ScrollView list, UitkLabel count, ILayoutRefresher refresher)
        {
            _listDisplay = list;
            _countDisplay = count;
            _refresher = refresher;
        }

        protected ScrollView _listDisplay;
        protected UitkLabel _countDisplay;
        protected ILayoutRefresher _refresher;

        public virtual void AddRow(VariableRow toAdd)
        {
            if (_listDisplay == null || _rows.Contains(toAdd) || toAdd.RootElement?.parent != null)
            {
                return;
            }

            _rows.Add(toAdd);
            _listDisplay.Add(toAdd.RootElement);
            _rowLookup.Add(toAdd.VarToRepresent, toAdd);
            UpdateCount();
        }

        protected IList<VariableRow> _rows = new List<VariableRow>();
        public virtual IReadOnlyList<VariableRow> Rows => _rows as IReadOnlyList<VariableRow>;
        
        protected IDictionary<IVariable, VariableRow> _rowLookup = new Dictionary<IVariable, VariableRow>();

        public virtual void RemoveRow(VariableRow toRemove)
        {
            if (!_listDisplay.Contains(toRemove.RootElement))
            {
                return;
            }

            _listDisplay.Remove(toRemove.RootElement);
            _rows.Remove(toRemove);
            _rowLookup.Remove(toRemove.VarToRepresent);
            UpdateCount();
        }

        /// <summary>
        /// Removes the row that has the passed variable assigned to it
        /// </summary>
        /// <param name="variable"></param>
        public virtual void RemoveRow(IVariable variable)
        {
            if (_rowLookup.ContainsKey(variable))
            {
                _rowLookup.Remove(variable);
            }
        }

        public virtual VariableRow RowAtIndex(int index)
        {
            VariableRow result = null;
            if (_rows.Count > index)
            {
                result = _rows[index];
            }
            return result;
        }

        public virtual void Clear()
        {
            foreach (var elem in _rows)
            {
                elem.RootElement?.RemoveFromHierarchy();
            }

            _rows.Clear();
            _listDisplay.Clear();
            _rowLookup.Clear();
            Refresh();
        }

        public virtual bool Contains(VariableRow row)
        {
            return _rows.Contains(row);
        }

        public void UpdateCount()
        {
            if (_countDisplay != null)
                _countDisplay.text = $"Count: {_listDisplay.childCount}";
        }

        public void ScheduleLayoutFix()
        {
            if (_refresher != null && _listDisplay != null)
                _refresher.Refresh(_listDisplay);
        }

        public virtual int RowCount
        {
            get
            {
                if (_listDisplay == null) return 0;
                return _listDisplay.childCount;
            }
        }

        public virtual void Refresh()
        {
            UpdateCount();
            ScheduleLayoutFix();
        }

        public virtual void Dispose()
        {
            Clear();
            _listDisplay.RemoveFromHierarchy();
            _countDisplay.RemoveFromHierarchy();
            _listDisplay = null;
            _countDisplay = null;
            _refresher = null;
        }

    }

    public interface IVariableListView : IDisposable
    {
        void AddRow(VariableRow row);
        void RemoveRow(VariableRow row);
        void RemoveRow(IVariable variable);
        int RowCount { get; }
        bool Contains(VariableRow row);
        VariableRow RowAtIndex(int index);
        void Refresh();
        IReadOnlyList<VariableRow> Rows { get; }
    }
}