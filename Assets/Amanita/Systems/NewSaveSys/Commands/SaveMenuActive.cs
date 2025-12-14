using Amanita.VScripting;
using UnityEngine;

namespace Amanita.SaveSys.UI.VScripting
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

        public override void OnEnter()
        {
            base.OnEnter();

            switch (state)
            {
                case SaveMenuState.Open:
                    Menu.Open();
                    break;
                case SaveMenuState.Close:
                    Menu.Close();
                    break;
                case SaveMenuState.Toggle:
                    Menu.Toggle();
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

        private static SaveMenuManager Menu => AmanitaManager.SaveMenu;

        public override string GetSummary()
        {
            string result = "Save Menu: ";
            if (Menu != null)
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