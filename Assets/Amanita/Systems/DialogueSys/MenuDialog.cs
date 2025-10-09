using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;
using MoonSharp.Interpreter;
using Amanita.Lua;
using System;
using Amanita.VScripting;

namespace Amanita.DialogueSys
{
	/// <summary>
	/// Presents multiple choice buttons to the players.
	/// </summary>
	public class MenuDialog : MonoBehaviour
	{
		[Tooltip("Automatically select the first interactable button when the menu is shown.")]
		[SerializeField] protected bool autoSelectFirstButton = false;

		protected virtual void Awake()
		{
			GetRequiredComponents();
			AvoidAutoDisablingButtonsInEditor();
			EnsureEventSystemExists();

			void GetRequiredComponents()
			{
				Button[] optionButtons = GetComponentsInChildren<Button>();
				cachedButtons = optionButtons;

				Slider timeoutSlider = GetComponentInChildren<Slider>();
				cachedSlider = timeoutSlider;
			}
			void AvoidAutoDisablingButtonsInEditor()
			{
				if (Application.isPlaying)
				{
					Clear();
				}
			}
			void EnsureEventSystemExists()
			{
			// There must be an Event System in the scene for Say and Menu input to work.
				EventSystem eventSystem = GameObject.FindFirstObjectByType<EventSystem>();
				if (eventSystem == null)
				{
					Debug.LogWarning("No EventSystem found in the scene. Auto-spawning one from prefab.");
					// Auto spawn an Event System from the prefab
					GameObject prefab = Resources.Load<GameObject>(AmanitaConstants.EventSystemPrefabName);
					if (prefab != null)
					{
						GameObject go = Instantiate(prefab);
						go.name = "EventSystem";
					}
				}
			}
		}

		protected Button[] cachedButtons;
		public virtual Button[] CachedButtons { get { return cachedButtons; } }
		
		protected Slider cachedSlider;
		
		/// <summary>
		/// Clear all displayed options in the Menu Dialog.
		/// </summary>
		public virtual void Clear()
		{
			StopAllCoroutines();

			// If something was shown, notify that we are ending
			if (nextOptionIndex != 0)
				MenuSignals.DoMenuEnd(this);

			nextOptionIndex = 0;

			StopListeningForClicks();
			ReorderAndHideOptions();
			HideSlider();

			void StopListeningForClicks()
			{
				for (int i = 0; i < CachedButtons.Length; i++)
				{
					var button = CachedButtons[i];
					button.onClick.RemoveAllListeners();
				}
			}
			void ReorderAndHideOptions()
			{
				for (int i = 0; i < CachedButtons.Length; i++)
				{
					var button = CachedButtons[i];
					if (button != null)
					{
						button.transform.SetSiblingIndex(i);
						button.gameObject.SetActive(false);
					}
				}
			}
			void HideSlider()
			{
				Slider timeoutSlider = CachedSlider;
				if (timeoutSlider != null)
				{
					timeoutSlider.gameObject.SetActive(false);
				}
			}
			
		}

		private int nextOptionIndex;

		protected virtual void OnEnable()
		{
			// The canvas may fail to update if the menu dialog is enabled in the first game frame.
			// To fix this we just need to force a canvas update when the object is enabled.
			Canvas.ForceUpdateCanvases();
		}

		#region Public members

		/// <summary>
		/// A cached slider object used for the timer in the menu dialog.
		/// </summary>
		/// <value>The cached slider.</value>
		public virtual Slider CachedSlider { get { return cachedSlider; } }

		/// <summary>
		/// Sets the active state of the Menu Dialog gameobject.
		/// </summary>
		public virtual void SetActive(bool state)
		{
			gameObject.SetActive(state);
		}

		/// <summary>
		/// Returns a menu dialog by searching for one in the scene or creating one if none exists.
		/// </summary>
		public static MenuDialog GetMenuDialog()
		{
			if (ActiveMenuDialog == null)
			{
				// Use first Menu Dialog found in the scene (if any)
				var menuDialogFound = FindFirstObjectByType<MenuDialog>();
				if (menuDialogFound != null)
				{
					ActiveMenuDialog = menuDialogFound;
				}

				if (ActiveMenuDialog == null)
				{
					// Auto spawn a menu dialog object from the prefab
					GameObject prefab = Resources.Load<GameObject>("Prefabs/MenuDialog");
					if (prefab != null)
					{
						GameObject go = Instantiate(prefab) as GameObject;
						go.SetActive(false);
						go.name = "MenuDialog";
						ActiveMenuDialog = go.GetComponent<MenuDialog>();
					}
				}
			}

			return ActiveMenuDialog;
		}

		/// <summary>
		/// Currently active Menu Dialog used to display Menu options
		/// </summary>
		public static MenuDialog ActiveMenuDialog { get; set; }

		protected virtual IEnumerator WaitForTimeout(float timeoutDuration, Block targetBlock)
		{
			float elapsedTime = 0;

			Slider timeoutSlider = CachedSlider;

			while (elapsedTime < timeoutDuration)
			{
				if (timeoutSlider != null)
				{
					float t = 1f - elapsedTime / timeoutDuration;
					timeoutSlider.value = t;
				}

				elapsedTime += Time.deltaTime;

				yield return null;
			}

			Clear();
			gameObject.SetActive(false);

			HideSayDialog();

			if (targetBlock != null)
			{
				targetBlock.StartExecution();
			}
		}

		/// <summary>
		/// Hides any currently displayed Say Dialog.
		/// </summary>
		public virtual void HideSayDialog()
		{
			var sayDialog = SayDialog.GetSayDialog();
			if (sayDialog != null)
			{
				sayDialog.FadeWhenDone = true;
			}
		}

		protected IEnumerator CallBlock(Block block)
		{
			yield return new WaitForEndOfFrame();
			block.StartExecution();
		}

		protected IEnumerator CallLuaClosure(LuaEnvironment luaEnv, Closure callback)
		{
			yield return new WaitForEndOfFrame();
			if (callback != null)
			{
				luaEnv.RunLuaFunction(callback, true);
			}
		}

		/// <summary>
		/// Adds the option to the list of displayed options. Calls a Block when selected.
		/// Will cause the Menu dialog to become visible if it is not already visible.
		/// </summary>
		/// <returns><c>true</c>, if the option was added successfully.</returns>
		/// <param name="text">The option text to display on the button.</param>
		/// <param name="interactable">If false, the option is displayed but is not selectable.</param>
		/// <param name="hideOption">If true, the option is not displayed but the menu knows that option can or did exist</param>
		/// <param name="targetBlock">Block to execute when the option is selected.</param>
		public virtual bool AddOption(string text, bool interactable, bool hideOption, Block targetBlock)
		{
			var block = targetBlock;
			UnityEngine.Events.UnityAction action = delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				StopAllCoroutines();
				// Stop timeout
				Clear();
				HideSayDialog();
				if (block != null)
				{
					var flowchart = block.GetFlowchart();
					gameObject.SetActive(false);
					// Use a coroutine to call the block on the next frame
					// Have to use the Flowchart gameobject as the MenuDialog is now inactive
					flowchart.StartCoroutine(CallBlock(block));
				}
			};

			return AddOption(text, interactable, hideOption, action);
		}

		/// <summary>
		/// Adds the option to the list of displayed options, calls a Lua function when selected.
		/// Will cause the Menu dialog to become visible if it is not already visible.
		/// </summary>
		/// <returns><c>true</c>, if the option was added successfully.</returns>
		public virtual bool AddOption(string text, bool interactable, LuaEnvironment luaEnv, Closure callBack)
		{
			if (!gameObject.activeSelf)
			{
				gameObject.SetActive(true);
			}

			// Copy to local variables 
			LuaEnvironment env = luaEnv;
			Closure call = callBack;
			UnityEngine.Events.UnityAction action = delegate
			{
				StopAllCoroutines();
				// Stop timeout
				Clear();
				HideSayDialog();
				// Use a coroutine to call the callback on the next frame
				StartCoroutine(CallLuaClosure(env, call));
			};

			return AddOption(text, interactable, false, action);
		}

		/// <summary>
		/// Adds the option to the list of displayed options. Calls a Block when selected.
		/// Will cause the Menu dialog to become visible if it is not already visible.
		/// </summary>
		/// <returns><c>true</c>, if the option was added successfully.</returns>
		/// <param name="text">The option text to display on the button.</param>
		/// <param name="interactable">If false, the option is displayed but is not selectable.</param>
		/// <param name="hideOption">If true, the option is not displayed but the menu knows that option can or did exist</param>
		/// <param name="action">Action attached to the button on the menu item</param>
		private bool AddOption(string text, bool interactable, bool hideOption, UnityEngine.Events.UnityAction action)
		{
			if (nextOptionIndex >= CachedButtons.Length)
			{
				Debug.LogWarning("Unable to add menu item, not enough buttons: " + text);
				return false;
			}
			//if first option notify that a menu has started
			if(nextOptionIndex == 0)
				MenuSignals.DoMenuStart(this);

			var button = cachedButtons[nextOptionIndex];
			
			//move forward for next call
			nextOptionIndex++;

			//don't need to set anything on it
			if (hideOption)
				return true;

			button.gameObject.SetActive(true);
			button.interactable = interactable;
			if (interactable && autoSelectFirstButton && !cachedButtons.Select(x => x.gameObject).Contains(EventSystem.current.currentSelectedGameObject))
			{
				EventSystem.current.SetSelectedGameObject(button.gameObject);
			}

			TextAdapter textAdapter = new TextAdapter();
			textAdapter.InitFromGameObject(button.gameObject, true);
			if (textAdapter.HasTextObject())
			{
				text = TextVariationHandler.SelectVariations(text);

				textAdapter.Text = text;
			}

			button.onClick.AddListener(action);
			
			return true;
		}

		/// <summary>
		/// Show a timer during which the player can select an option. Calls a Block when the timer expires.
		/// </summary>
		/// <param name="duration">The duration during which the player can select an option.</param>
		/// <param name="targetBlock">Block to execute if the player does not select an option in time.</param>
		public virtual void ShowTimer(float duration, Block targetBlock)
		{
			if (cachedSlider != null)
			{
				cachedSlider.gameObject.SetActive(true);
				gameObject.SetActive(true);
				StopAllCoroutines();
				StartCoroutine(WaitForTimeout(duration, targetBlock));
			}
			else
			{
				Debug.LogWarning("Unable to show timer, no slider set");
			}
		}

		/// <summary>
		/// Show a timer during which the player can select an option. Calls a Lua function when the timer expires.
		/// </summary>
		public virtual IEnumerator ShowTimer(float duration, LuaEnvironment luaEnv, Closure callBack)
		{
			if (CachedSlider == null ||
				duration <= 0f)
			{
				yield break;
			}

			CachedSlider.gameObject.SetActive(true);
			StopAllCoroutines();

			float elapsedTime = 0;
			Slider timeoutSlider = CachedSlider;

			while (elapsedTime < duration)
			{
				if (timeoutSlider != null)
				{
					float t = 1f - elapsedTime / duration;
					timeoutSlider.value = t;
				}

				elapsedTime += Time.deltaTime;

				yield return null;
			}

			Clear();
			gameObject.SetActive(false);
			HideSayDialog();

			if (callBack != null)
			{
				luaEnv.RunLuaFunction(callBack, true);
			}
		}

		/// <summary>
		/// Returns true if the Menu Dialog is currently displayed.
		/// </summary>
		public virtual bool IsActive()
		{
			return gameObject.activeInHierarchy;
		}

		/// <summary>
		/// Returns the number of currently displayed options.
		/// </summary>
		public virtual int DisplayedOptionsCount
		{
			get {
				int count = 0;
				for (int i = 0; i < cachedButtons.Length; i++)
				{
					var button = cachedButtons[i];
					if (button.gameObject.activeSelf)
					{
						count++;
					}
				}
				return count;
			}
		}

		/// <summary>
		/// Shuffle the parent order of the cached buttons, allows for randomising button order, buttons are auto reordered when cleared
		/// </summary>
		public void Shuffle(System.Random r)
		{
			for (int i = 0; i < CachedButtons.Length; i++)
			{
				CachedButtons[i].transform.SetSiblingIndex(r.Next(CachedButtons.Length));
			}
		}

		#endregion
	}    
}
