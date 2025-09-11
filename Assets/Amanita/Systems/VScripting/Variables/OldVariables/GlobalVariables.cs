using UnityEngine;
using System.Collections.Generic;
using System;
using Amanita.VScripting;
using System.Linq;

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
			EnsureWeHaveFlowchart();
		}

		protected virtual void EnsureWeHaveFlowchart()
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
				holder.AddVariable(varAsType);
			}

			return varAsType;
		}

		public virtual TVarType GetOrAddVariable<TValHeld, TVarType>(string key, TValHeld value)
			where TVarType : VariableBase<TValHeld>
		{
			TVarType newVar = holder.AddNewVariable<TValHeld, TVarType>(key, value, VariableScope.Global);
			variables[key] = newVar;
			return newVar;
		}

		public Muscariable GetMuscariable(string key)
		{
			Muscariable theVar = null;
			muscariables.TryGetValue(key, out theVar);
			return theVar;

		}

		Dictionary<string, Muscariable> muscariables = new Dictionary<string, Muscariable>();

		/// <summary>
		/// Returns a copy of the list of variables registered here, be they legacy or muscari.
		/// </summary>
		public virtual IReadOnlyList<IVariable> Variables
		{
			get
			{
				EnsureWeHaveFlowchart();
				return holder.Variables;
			}
		}

		public virtual void OnDestroy()
		{

		}
	}
}