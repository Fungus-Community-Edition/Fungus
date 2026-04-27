using UnityEngine;
using AtMycelia.Hyphlow;
using AtMycelia.SaveSys.Ui;

namespace AtMycelia.SaveSys.VScripting
{
    [CommandInfo("Save Sys/DebugOnly",
        "SaveCanvasSortOrder",
        "Sets the sort order of the save canvas for testing purposes.")]
    public class SetSaveCanvasSortOrder : Command
    {
        [SerializeField] private IntegerData _sortOrder = new IntegerData(0);
        public override void OnEnter()
        {
            if (!Application.isEditor)
            {
                Continue();
                return;
            }
            base.OnEnter();
            SaveMenuManager menuManager = FindFirstObjectByType<SaveMenuManager>();
            if (menuManager == null)
            {
                string errorMessage = "SetSaveCanvasSortOrder: No SaveMenuManager found in the scene.";
                Debug.LogError(errorMessage);
                Continue();
                return;
            }

            Canvas canvas = menuManager.GetComponent<Canvas>();
            canvas.sortingOrder = _sortOrder;
            Continue();
        }

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_sortOrder);
        }

        public override string GetSummary()
        {
            string result = "Set Save Canvas Sort Order to " + _sortOrder.Value;
            return result;
        }
    }
}