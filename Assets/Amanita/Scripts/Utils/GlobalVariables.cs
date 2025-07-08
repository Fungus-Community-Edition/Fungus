// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using System.Collections.Generic;
using System;

namespace Amanita
{
	/// <summary>
	/// Storage for a collection of Amanita variables that can then be accessed globally.
	/// </summary>
	public class GlobalVariables : MonoBehaviour
	{
		private Flowchart holder;

		Dictionary<string, Variable> variables = new Dictionary<string, Variable>();

		public virtual void Init()
		{
			holder = GetComponent<Flowchart>();

			if (holder == null)
			{
				holder = this.gameObject.AddComponent<Flowchart>();
			}
		}

		public Variable GetVariable(string variableKey)
		{
			Variable v = null;
			variables.TryGetValue(variableKey, out v);
			return v;
		}

		public VariableBase<TVarValue> GetOrAddVariable<TVarValue>(string variableKey, TVarValue defaultvalue, Type type)
		{
			Variable varInstance = null;
			VariableBase<TVarValue> varAsType = null;
			var varFound = variables.TryGetValue(variableKey, out varInstance);

			if (varFound && varInstance != null)
			{
				varAsType = varInstance as VariableBase<TVarValue>;

				if (varAsType != null)
				{
					return varAsType;
				}
				else
				{
					Debug.LogError("An Amanita variable of name " + variableKey + " already exists, but of a different type");
				}
			}
			else
			{
				//create the variable
				varAsType = holder.gameObject.AddComponent(type) as VariableBase<TVarValue>;
				varAsType.Value = defaultvalue;
				varAsType.Key = variableKey;
				varAsType.Scope = VariableScope.Public;
				variables[variableKey] = varAsType;
				holder.Variables.Add(varAsType);
			}

			return varAsType;
		}

		public virtual TVarType GetOrAddVariable<TValHeld, TVarType>(string key, TValHeld value)
			where TVarType : VariableBase<TValHeld>
		{
			TVarType newVar = holder.AddVariable<TValHeld, TVarType>(key, value, VariableScope.Global);
			variables[key] = newVar;
			return newVar;
		}
	}
}