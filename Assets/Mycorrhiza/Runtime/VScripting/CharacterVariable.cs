using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using AtMycelia.Hyphlow;
using AtMycelia.Amanita;

namespace AtMycelia.Mycorrhiza
{
	/// <summary>
	/// Character variable type.
	/// </summary>
	[VariableInfo("Narrative", "Character", typeof(Character), false)]
	[AddComponentMenu("")]
	[System.Serializable]
	[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
	public class CharacterVariable : VariableBase<Amanita.Character>
	{ }

	/// <summary>
	/// Container for a Character variable reference or constant value.
	/// </summary>
	[System.Serializable]
	[VariableData(typeof(Character), typeof(CharacterVariable))]
	public class CharacterData : VariableData<Character>
	{
		[SerializeField]
		[VariableProperty("<Value>", typeof(CharacterVariable))]
		public CharacterVariable characterRef;
		

		public static implicit operator Character(CharacterData CharacterData)
		{
			return CharacterData.Value;
		}

		public CharacterData() : base(default) { }
		public CharacterData(Character startVal = null) : base(startVal) { }

		protected override Variable LegacyVarRef
		{
			get => characterRef;
			set => characterRef = value as CharacterVariable;
		}
	}
}
