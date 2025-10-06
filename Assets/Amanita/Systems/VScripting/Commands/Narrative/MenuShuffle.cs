using UnityEngine;
using Amanita.VScripting;

namespace Amanita.DialogueSys.VScripting
{
	/// <summary>
	/// Shuffle the order of the items in a Fungus Menu
	/// </summary>
	[CommandInfo("Narrative", 
				 "Menu Shuffle", 
		"Shuffle the order of the items in a Fungus Menu")]
	[AddComponentMenu("")]
	[ExecuteInEditMode]
	public class MenuShuffle : Command
	{
		public enum Mode
		{
			Every,
			Once
		}
		[SerializeField]
		[Tooltip("Determines if the order is shuffled everytime this command is it (Every) or if it is consistent when returned to but random (Once)")]
		protected Mode shuffleMode = Mode.Once;

		private int seed = -1;

		public override void OnEnter()
		{
			var menuDialog = MenuDialog.GetMenuDialog();

			//if we shuffle every time or we haven't shuffled yet
			if(shuffleMode == Mode.Every || seed == -1)
			{
				seed = Random.Range(0,1000000);
			}

			if (menuDialog != null)
			{
				menuDialog.Shuffle(new System.Random(seed));
			}

			Continue();
		}

		public override string GetSummary()
		{
			return shuffleMode.ToString();
		}

		public override Color GetButtonColor()
		{
			return CommandColors.Narrative;
		}
	}
}
