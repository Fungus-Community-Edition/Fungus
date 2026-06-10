using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace AtMycelia.Amanita.DialogueSys
{
	/// <summary>
	/// Presents multiple choice buttons to the players.
	/// 
	/// Notes:
	/// - This class is UI-first and hyphlow-free. If a `buttonPrefab` is assigned, MenuDialog
	///   will instantiate buttons from that prefab at runtime and manage them dynamically.
	/// - For backwards compatibility, if no `buttonPrefab` is assigned the MenuDialog will
	///   continue to use any buttons present as children (cachedButtons).
	/// - Clients may add options with a callback Action. The hyphlow bridge (Mycorrhiza)
	///   should provide helpers that attach Blocks to callbacks.
	/// </summary>
	public class MenuDialog : MonoBehaviour
	{
		[Tooltip("Button prefab used when creating menu options at runtime. If null, any " +
			"buttons present in the scene as children will be used.")]
		[SerializeField] protected Button _buttonPrefab;

		[Tooltip("Automatically select the first interactable button when the menu is shown.")]
		[FormerlySerializedAs("autoSelectFirstButton")]
		[SerializeField] protected bool _autoSelectFirstButton = false;

		/// <summary>
		/// Set or change the button prefab at runtime. Existing dynamic buttons are preserved.
		/// </summary>
		public virtual Button ButtonPrefab
		{
			get { return _buttonPrefab; }
			set { _buttonPrefab = value; }
		}

		protected virtual void Awake()
		{
			GetRequiredComponents();
			AvoidAutoDisablingButtonsInEditor();

			void GetRequiredComponents()
			{
				// Cache any buttons that already exist as children (backwards compat).
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
		}

		protected Slider cachedSlider;
		public virtual Slider CachedSlider { get { return cachedSlider; } }

		// Backwards-compatible cached buttons (existing prefab that contains a fixed set of buttons).
		protected Button[] cachedButtons;
		public virtual Button[] CachedButtons { get { return cachedButtons; } }

		/// <summary>
		/// Clear all displayed options in the Menu Dialog.
		/// </summary>
		public virtual void Clear()
		{
			StopAllCoroutines();

			// If something was shown, notify that we are ending
			if (nextOptionIndex != 0)
			{
				MenuSignals.MenuStartedEnding(this);
			}

			nextOptionIndex = 0;

			StopListeningForClicks();
			ReorderAndHideOptions();
			HideSlider();
		}

		private void StopListeningForClicks()
		{
			// Remove listeners from dynamic buttons
			for (int i = 0; i < dynamicButtons.Count; i++)
			{
				var buttonEl = dynamicButtons[i];
				if (buttonEl != null)
				{
					buttonEl.onClick.RemoveAllListeners();
					Destroy(buttonEl.gameObject);
				}
			}

			dynamicButtons.Clear();

			// Also strip listeners from any cached (legacy) buttons
			if (cachedButtons != null)
			{
				for (int i = 0; i < cachedButtons.Length; i++)
				{
					var button = cachedButtons[i];
					if (button != null)
					{
						button.onClick.RemoveAllListeners();
					}
				}
			}
		}

		// Dynamically created buttons when using _buttonPrefab
		protected readonly List<Button> dynamicButtons = new List<Button>();

		private void ReorderAndHideOptions()
		{
			// Hide dynamic buttons — they have been destroyed in StopListeningForClicks.
			// Hide and reset legacy cached buttons
			if (cachedButtons != null)
			{
				for (int i = 0; i < cachedButtons.Length; i++)
				{
					var button = cachedButtons[i];
					if (button != null)
					{
						button.transform.SetSiblingIndex(i);
						button.gameObject.SetActive(false);
					}
				}
			}
		}

		private void HideSlider()
		{
			Slider timeoutSlider = CachedSlider;
			if (timeoutSlider != null)
			{
				timeoutSlider.gameObject.SetActive(false);
			}
		}

		

		private int nextOptionIndex;

		protected virtual void OnEnable()
		{
			// The canvas may fail to update if the menu dialog is enabled in the first game frame.
			// To fix this we just need to force a canvas update when the object is enabled.
			Canvas.ForceUpdateCanvases();
		}

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

		/// <summary>
		/// Adds the option to the list of displayed options. Calls a callback when selected.
		/// Will cause the Menu dialog to become visible if it is not already visible.
		/// This is the canonical method for clients (hyphlow bridge will adapt Blocks -> Actions).
		/// </summary>
		public virtual bool AddOption(string text, bool interactable, bool hideOption, Action callback)
		{
			if (!gameObject.activeSelf)
			{
				gameObject.SetActive(true);
			}

			Action localCallback = callback;
			UnityEngine.Events.UnityAction action = delegate
			{
				StopAllCoroutines();
				// Stop timeout
				Clear();
				// Use a coroutine to call the callback on the next frame
				StartCoroutine(CallAction(localCallback));
			};

			return AddOptionInternal(text, interactable, hideOption, action);
		}

		/// <summary>
		/// Internal add option implementation.
		/// Instantiates from _buttonPrefab when available; otherwise uses legacy cached buttons.
		/// Returns true on success.
		/// </summary>
		private bool AddOptionInternal(string text, bool interactable, bool hideOption, 
			UnityEngine.Events.UnityAction action)
		{
			Button buttonToUse = null;
			int optionIndex = nextOptionIndex;

			// Try dynamic prefab mode first
			if (_buttonPrefab != null)
			{
				try
				{
					Button newBtn = Instantiate(_buttonPrefab, this.transform);
					newBtn.gameObject.SetActive(true);
					dynamicButtons.Add(newBtn);
					buttonToUse = newBtn;
					// Ensure sibling order for consistent keyboard navigation
					newBtn.transform.SetSiblingIndex(optionIndex);
				}
				catch (Exception ex)
				{
					Debug.LogError("MenuDialog: Failed to instantiate button prefab: " + ex.Message);
					return false;
				}
			}
			else
			{
				// Legacy behaviour: use cached buttons array
				if (cachedButtons == null || optionIndex >= cachedButtons.Length)
				{
					Debug.LogWarning("Unable to add menu item, not enough buttons: " + text);
					return false;
				}
				buttonToUse = cachedButtons[optionIndex];
				if (buttonToUse == null)
				{
					Debug.LogWarning("Unable to add menu item, cached button is null: " + text);
					return false;
				}
				buttonToUse.gameObject.SetActive(true);
			}

			// move forward for next call
			nextOptionIndex++;

			// don't need to set anything on it
			if (hideOption)
			{
				// keep internal state consistent but don't display
				buttonToUse.gameObject.SetActive(false);
				return true;
			}

			Button btn = buttonToUse;
			btn.interactable = interactable;

			// Optionally auto-select the first interactable button
			if (interactable && _autoSelectFirstButton)
			{
				if (!cachedButtons?.Select(x => x.gameObject).Contains(EventSystem.current.currentSelectedGameObject) ?? true)
				{
					EventSystem.current.SetSelectedGameObject(btn.gameObject);
				}
			}

			TextAdapter textAdapter = new TextAdapter();
			textAdapter.InitFromGameObject(btn.gameObject, true);
			if (textAdapter.HasTextObject())
			{
				text = TextVariationHandler.SelectVariations(text);
				textAdapter.Text = text;
			}

			// Wrap action so callers get the button and index
			UnityEngine.Events.UnityAction wrappedAction = delegate
			{
				action?.Invoke();
				try
				{
					OptionClicked?.Invoke(btn, optionIndex);
				}
				catch (Exception) { }
			};

			btn.onClick.AddListener(wrappedAction);

			// Ensure GameObject active (in case cached button)
			btn.gameObject.SetActive(true);

			return true;
		}

		/// <summary>
		/// Event fired when any menu option is clicked.
		/// Parameters: (Button clickedButton, int optionIndex)
		/// </summary>
		public event Action<Button, int> OptionClicked = delegate { };

		/// <summary>
		/// Show a timer during which the player can select an option. Calls a callback when the timer expires.
		/// </summary>
		public virtual IEnumerator ShowTimer(float duration, Action callback)
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

			callback?.Invoke();
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

				// count dynamic buttons
				for (int i = 0; i < dynamicButtons.Count; i++)
				{
					var b = dynamicButtons[i];
					if (b != null && b.gameObject.activeSelf) count++;
				}

				// count legacy cached visible buttons
				if (cachedButtons != null)
				{
					for (int i = 0; i < cachedButtons.Length; i++)
					{
						var button = cachedButtons[i];
						if (button != null && button.gameObject.activeSelf) count++;
					}
				}

				return count;
			}
		}

		/// <summary>
		/// Shuffle the parent order of the cached buttons and dynamic buttons,
		/// allows for randomising button order. Buttons are auto reordered when cleared.
		/// </summary>
		public void Shuffle(System.Random r)
		{
			// shuffle dynamic buttons
			for (int i = 0; i < dynamicButtons.Count; i++)
			{
				if (dynamicButtons[i] != null)
				{
					dynamicButtons[i].transform.SetSiblingIndex(r.Next(dynamicButtons.Count));
				}
			}

			// shuffle cached buttons
			if (cachedButtons != null)
			{
				for (int i = 0; i < cachedButtons.Length; i++)
				{
					if (cachedButtons[i] != null)
					{
						cachedButtons[i].transform.SetSiblingIndex(r.Next(cachedButtons.Length));
					}
				}
			}
		}

		#region Helpers

		protected IEnumerator CallAction(Action callback)
		{
			yield return new WaitForEndOfFrame();
			callback?.Invoke();
		}

		#endregion
	}    
}
