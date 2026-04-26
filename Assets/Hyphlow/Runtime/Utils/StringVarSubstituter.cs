using AtMycelia.Hyphlow.Sys;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    /// <summary>
    /// For substituting the values of Hyphlow IVariables into strings. This is good for
    /// when a Command (like SetText) needs a string representation of a variable's value.
    /// </summary>
    public class StringVarSubstituter
    {
        public string SubstituteVariables(string input, IVariableSource variableSource)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (variableSource == null)
            {
                Debug.LogError("StringVarSubstituter.SubstituteVariables called with a null IVariableSource.");
                return input;
            }

            Regex regex = new Regex(SubstituteVariableRegexString);
            MatchCollection matches = regex.Matches(input);
            if (matches.Count == 0)
            {
                return input;
            }

            StringBuilder sb = new StringBuilder(input);
            HashSet<string> missingKeys = null;
            bool changed = false;

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                string key = match.Value.Substring(2, match.Value.Length - 3);
                if (TryGetVariableValue(key, variableSource, out string value))
                {
                    sb.Replace(match.Value, value);
                    changed = true;
                    continue;
                }

                missingKeys ??= new HashSet<string>(StringComparer.Ordinal);
                if (missingKeys.Add(key))
                {
                    Debug.LogError($"StringVarSubstituter: Variable key '{key}' was not found in any Flowchart or global variable sets.");
                }
            }

            return changed ? sb.ToString() : input;
        }

        public const string SubstituteVariableRegexString = "{\\$.*?}";

        private bool TryGetVariableValue(string key, IVariableSource variableSource, out string value)
        {
            IVariable variable;
            if (TryGetVariableFromSource(variableSource, key, out variable) ||
                TryGetVariableFromOtherFlowcharts(variableSource, key, out variable) ||
                TryGetVariableFromGlobalSources(key, out variable))
            {
                value = variable.BoxedValue.ToString();
                return true;
            }

            value = null;
            return false;
        }

        private bool TryGetVariableFromSource(IVariableSource source, string key, out IVariable variable)
        {
            variable = source.GetVariable(key, StringComparison.Ordinal);
            return variable != null;
        }

        private bool TryGetVariableFromOtherFlowcharts(IVariableSource source, string key, out IVariable variable)
        {
            IReadOnlyList<Flowchart> flowcharts = FlowchartRegistry.GetSceneFlowcharts();
            for (int i = 0; i < flowcharts.Count; i++)
            {
                Flowchart flowchart = flowcharts[i];
                if (flowchart == null || ReferenceEquals(flowchart, source))
                {
                    continue;
                }

                IVariable candidate = flowchart.GetVariable(key, StringComparison.Ordinal);
                if (candidate == null || candidate.Scope != VariableScope.Public)
                {
                    continue;
                }

                variable = candidate;
                return true;
            }

            variable = null;
            return false;
        }

        private bool TryGetVariableFromGlobalSources(string key, out IVariable variable)
        {
            VariableRegistryConfig registryConfig = HyphlowRuntimeSysAssets.S.VariableRegistryConfig;
            if (registryConfig == null)
            {
                variable = null;
                return false;
            }

            IReadOnlyList<VariableSourceAsset> sources = registryConfig.GlobalSources;
            for (int i = 0; i < sources.Count; i++)
            {
                VariableSourceAsset source = sources[i];
                if (source == null)
                {
                    continue;
                }

                IVariable candidate = source.GetVariableByName(key, StringComparison.Ordinal);
                if (candidate != null)
                {
                    variable = candidate;
                    return true;
                }
            }

            variable = null;
            return false;
        }
    }
}