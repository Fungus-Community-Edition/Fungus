using UnityEngine;


namespace Amanita.VScripting
{
	/// <summary>
	/// Character variable type.
	/// </summary>
	[VariableInfo("Narrative", "Character", typeof(Character))]
	[AddComponentMenu("")]
	[System.Serializable]
	public class CharacterVariable : VariableBase<Amanita.Character>
	{ }

	/// <summary>
	/// Container for a Character variable reference or constant value.
	/// </summary>
	[System.Serializable]
	[VariableData(typeof(Character), typeof(CharacterVariable))]
	public class CharacterData : VariableData<Character>
	{
		[SerializeField, SerializeReference]
		[VariableProperty("<Value>", typeof(CharacterVariable))]
		public IVariable<Character> characterRef;


		public static implicit operator Amanita.Character(CharacterData CharacterData)
		{
			return CharacterData.Value;
		}

		public CharacterData() : base(default) { }
		public CharacterData(Character startVal = null) : base(startVal) { }

		public override void Refresh()
		{
			varRef ??= characterRef;
		}
	}
}