using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;
using BaseFungus = Fungus;

namespace CGTUnity.Fungus.SaveSystem
{
    public static class FlowchartExtensions 
    {
        /// <summary>
        /// Sets the value of a variable in the flowchart to the value that was passed.
        /// </summary>
        public static void SetVariable<TBase, TVarType>(this Flowchart flowchart, string key, TBase value)
        where TVarType: BaseFungus.VariableBase<TBase>
        {
            var variable = flowchart.GetVariable<TVarType>(key);

            if (variable != null)
                variable.Value = value; 
            else
                LetUserKnowVarDoesntExist(flowchart, key);
        }

        static void LetUserKnowVarDoesntExist(Flowchart flowchart, string varName)
        {
            string messageFormat = "Variable named {0} in Flowchart named {1} not found.";
            string warningMessage = string.Format(messageFormat, flowchart.name, varName);
            Debug.LogWarning(warningMessage);
        }

        /// <summary>
        /// Returns a collection of all variables of the specified type, that the flowchart has.
        /// </summary>
        public static IList<TVarType> GetVariables<TVarType>(this Flowchart flowchart) where TVarType: BaseFungus.Variable
        {
            var varsFound = new List<TVarType>();
            var allVars = flowchart.Variables;

            for (int i = 0; i < allVars.Count; i++)
            {
                var currentVar = allVars[i];
                if (currentVar is TVarType)
                    varsFound.Add(currentVar as TVarType);
            }

            return varsFound;
        }
    }
}