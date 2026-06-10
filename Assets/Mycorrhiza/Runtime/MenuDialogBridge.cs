using System.Collections;
using UnityEngine;
using AtMycelia.Hyphlow;
using AtMycelia.Amanita.DialogueSys;

namespace AtMycelia.Amanita.VScripting.UI
{
	/// <summary>
	/// Hyphlow bridge helpers for MenuDialog.
	/// This class lives in the Mycorrhiza bridge assembly and adapts Hyphlow IBlock semantics
	/// to MenuDialog's Action-based callbacks.
	/// </summary>
	public static class MenuDialogBridge
	{
		/// <summary>
		/// Adds a menu option that executes the provided Hyphlow targetBlock when clicked.
		/// Uses the MenuDialog.AddOption(Action) API under the hood.
		/// </summary>
		public static void AddOptionForBlock(MenuDialog menu, string text, bool interactable,
			bool hideOption, IBlock targetBlock)
		{
			if (menu == null || targetBlock == null)
			{
				return;
			}

			// Create an Action that starts the block execution on the Flowchart on the
			// next frame. This mirrors previous behavior where MenuDialog used a
			// coroutine to call the block on the next frame.
			menu.AddOption(text, interactable, hideOption, () =>
			{
				var fc = targetBlock.ParentFlowchart;
				if (fc == null)
				{
					return;
				}
				// Start coroutine on the flowchart to run ExecuteBlock in the next frame.
				fc.StartCoroutine(CallBlockNextFrame(fc, targetBlock));
			});
		}

		private static IEnumerator CallBlockNextFrame(Flowchart flowchart, IBlock block)
		{
			yield return new WaitForEndOfFrame();
			if (flowchart != null && block != null)
			{
				flowchart.ExecuteBlock(block);
			}
		}
	}
}