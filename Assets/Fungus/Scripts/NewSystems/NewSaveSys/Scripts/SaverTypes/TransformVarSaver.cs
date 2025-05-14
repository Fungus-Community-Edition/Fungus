using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

namespace CGTUnity.Fungus.SaveSystem
{
    /// <summary>
    /// Saves the state of all Transform variables belonging to specified flowcharts.
    /// </summary>
    public class TransformVarSaver : DataSaver<TransformVarData>, 
        ISaveCreator<TransformVarData, Transform>,
        IGroupSaver<TransformVarData>
    {
        [SerializeField] protected Flowchart[] flowcharts = null;

        public virtual TransformVarData CreateSave(Transform transform)
        {
            return new TransformVarData(transform);
        }

        public virtual IList<TransformVarData> CreateSaves()
        {
            var saveGroup = TransformVariablesSavedFromFlowcharts();
            return saveGroup;
        }

        protected virtual IList<TransformVarData> TransformVariablesSavedFromFlowcharts()
        {
            var saveGroup = new List<TransformVarData>();

            for (int i = 0; i < flowcharts.Length; i++)
            {
                var flowchart = flowcharts[i];
                var transformVars = flowchart.GetVariables<TransformVariable>();
                SaveTransformVarsToGroup(transformVars, saveGroup);
            }

            return saveGroup;
        }

        protected void SaveTransformVarsToGroup(IList<TransformVariable> transformVars, IList<TransformVarData> group)
        {
            for (int i = 0; i < transformVars.Count; i++)
            {
                var tVar = transformVars[i];
                var transformValue = tVar.Value;
                var newSave = TransformVarData.CreateFrom(transformValue);
                group.Add(newSave);
            }
        }

        public override IList<SaveDataItem> CreateItems()
        {
            var saves = CreateSaves();
            var items = new SaveDataItem[saves.Count];

            for (int i = 0; i < saves.Count; i++)
            {
                var currentSave = saves[i];
                items[i] = currentSave.ToSaveDataItem();
            }

            return items;
        }


    }
}