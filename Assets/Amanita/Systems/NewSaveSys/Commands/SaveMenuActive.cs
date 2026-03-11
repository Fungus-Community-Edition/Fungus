using AtMycelia.Amanita.VScripting;
using UnityEngine;

namespace AtMycelia.SaveSys.UI.VScripting
{
	[CommandInfo("Save Sys/UI", 
		"Save Menu", 
		"Sets the save menu's activation state.")]
	public class SaveMenuActive : Command
	{
		[SerializeField] private SaveMenuState state = SaveMenuState.Open;
		public enum SaveMenuState
		{
			Null,
			Open,
			Close,
			Toggle
		}

		protected virtual void Awake()
		{
			saveMenu = FindFirstObjectByType<SaveMenuManager>();
		}

		public override void OnEnter()
		{
			base.OnEnter();

			switch (state)
			{
				case SaveMenuState.Open:
					saveMenu.Open(null);
					break;
				case SaveMenuState.Close:
					saveMenu.Close(null);
					break;
				case SaveMenuState.Toggle:
					saveMenu.Toggle();
					break;
				case SaveMenuState.Null:
					string errorMessage = "SaveMenuActive Command: SaveMenuState is set to Null, so no action will be taken.";
					Debug.LogError(errorMessage);
					break;
				default:
					break;
			}

			Continue();
		}

		private static SaveMenuManager saveMenu;

		public override string GetSummary()
		{
			string result = "Save Menu: ";
			if (saveMenu != null)
			{
				switch (state)
				{
					case SaveMenuState.Open:
						result += "Open";
						break;
					case SaveMenuState.Close:
						result += "Close";
						break;
					case SaveMenuState.Toggle:
						result += "Toggle";
						break;
					case SaveMenuState.Null:
						result += "No Action (Null State)";
						break;
					default:
						result += "Unknown State";
						break;
				}
			}
			else
			{
				result = "Error: No SaveMenuManager found in AmanitaManager.";
			}
			return result;
		}
	}
}