using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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

		[SerializeField] protected Transform _buttonHolder;

		[Tooltip("Automatically select the first interactable button when the menu is shown.")]
		[FormerlySerializedAs("autoSelectFirstButton")]
		[SerializeField] protected bool _autoSelectFirstButton = false;
		[SerializeField] protected bool _logMessages = true;

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
				_cachedButtons = optionButtons;

				Slider timeoutSlider = GetComponentInChildren<Slider>();
				_cachedSlider = timeoutSlider;
			}

			void AvoidAutoDisablingButtonsInEditor()
			{
				if (Application.isPlaying)
				{
					Clear();
				}
			}
		}

		protected Slider _cachedSlider;
		public virtual Slider CachedSlider { get { return _cachedSlider; } }

		// Backwards-compatible cached buttons (existing prefab that contains a fixed set of buttons).
		protected Button[] _cachedButtons;
		public virtual IReadOnlyList<Button> CachedButtons { get { return _cachedButtons; } }

		/// <summary>
		/// Clear all displayed options in the Menu Dialog.
		/// </summary>
		public virtual void Clear()
		{
			StopAllCoroutines();

			// If something was shown, notify that we are ending
			if (_nextOptionIndex != 0)
			{
				MenuSignals.MenuStartedEnding(this);
			}

			_nextOptionIndex = 0;

			StopListeningForClicks();
			ReorderAndHideOptions();
			HideSlider();
			_visibleButtons.Clear();
		}

		private void StopListeningForClicks()
		{
			// Remove listeners from dynamic buttons
			for (int i = 0; i < _dynamicButtons.Count; i++)
			{
				var buttonEl = _dynamicButtons[i];
				if (buttonEl != null)
				{
					buttonEl.onClick.RemoveAllListeners();
					Destroy(buttonEl.gameObject);
				}
			}

			_dynamicButtons.Clear();

			// Also strip listeners from any cached (legacy) buttons
			if (_cachedButtons != null)
			{
				for (int i = 0; i < _cachedButtons.Length; i++)
				{
					var button = _cachedButtons[i];
					if (button != null)
					{
						button.onClick.RemoveAllListeners();
					}
				}
			}
		}

		// Dynamically created buttons when using _buttonPrefab
		protected readonly List<Button> _dynamicButtons = new List<Button>();

		private void ReorderAndHideOptions()
		{
			// Hide dynamic buttons — they have been destroyed in StopListeningForClicks.
			// Hide and reset legacy cached buttons
			if (_cachedButtons != null)
			{
				for (int i = 0; i < _cachedButtons.Length; i++)
				{
					var button = _cachedButtons[i];
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

		private int _nextOptionIndex;

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
					SpawnAndUseDefault();
					static void SpawnAndUseDefault()
					{
						GameObject prefab = Resources.Load<GameObject>(_pathToPrefab);
						if (prefab != null)
						{
							GameObject go = Instantiate(prefab);
							go.SetActive(false);
							go.name = prefab.name;
							ActiveMenuDialog = go.GetComponent<MenuDialog>();
							var amanitaRoot = AmanitaRuntimeBootstrapper.Root;
							go.transform.SetParent(amanitaRoot.transform, false);
						}
					}
				}
			}

			return ActiveMenuDialog;
		}

		protected static readonly string _pathToPrefab = "Runtime/Prefabs/MenuDialog";

		/// <summary>
		/// Currently active Menu Dialog used to display Menu options
		/// </summary>
		public static MenuDialog ActiveMenuDialog { get; set; }

		/// <summary>
		/// Adds the option to the list of displayed options. Calls a callback when selected.
		/// Will cause the Menu dialog to become visible if it is not already visible.
		/// This is the canonical method for clients (hyphlow bridge will adapt Blocks -> Actions).
		/// </summary>
		public virtual bool AddOption(string text, bool interactable,
			bool hideOption, Action callback)
		{
			Action localCallback = callback;
			void UpdatedCallback()
			{
				StopAllCoroutines();
				// ^Stops timeout

				Clear();
				// ^Since we want the menu cleared as soon as the option is chosen.

				// Use a coroutine to call the callback on the next frame
				StartCoroutine(CallActionAfterOneFrame(localCallback));
			}

			return AddOptionInternal(text, interactable, hideOption, UpdatedCallback);
		}

		/// <summary>
		/// Internal add option implementation.
		/// Instantiates from _buttonPrefab when available; otherwise uses legacy cached buttons.
		/// Returns true on success.
		/// </summary>
		private bool AddOptionInternal(string text, bool interactable, bool hideOption, 
			UnityEngine.Events.UnityAction action)
		{
			Button buttonToUse = GetFirstINactiveButton();
			int optionIndex = _nextOptionIndex;
			string logMessage;

			if (buttonToUse != null)
			{
				if (_logMessages)
				{
					logMessage = "MenuDialog: Reusing existing button for option: " + text;
					Debug.Log(logMessage);
				}

				// Ensure sibling order for consistent keyboard navigation
				buttonToUse.transform.SetSiblingIndex(optionIndex);
			}

			if (buttonToUse == null && _buttonPrefab != null)
			{
				try
				{
					Button newBtn = Instantiate(_buttonPrefab, _buttonHolder);
					newBtn.gameObject.SetActive(true);
					_dynamicButtons.Add(newBtn);
					buttonToUse = newBtn;
					// Ensure sibling order for consistent keyboard navigation
					_buttonHolder.SetSiblingIndex(optionIndex);
				}
				catch (Exception ex)
				{
					if (_logMessages)
					{
						logMessage = "MenuDialog: Failed to instantiate button prefab " +
							"for option: " + text;
						Debug.LogWarning(logMessage);
					}
					return false;
				}
			}
			else
			{
				// Legacy behaviour: use cached buttons array
				if (_cachedButtons == null || optionIndex >= _cachedButtons.Length)
				{
					if (_logMessages)
					{
						logMessage = "MenuDialog: Unable to add menu item, not " +
							"enough buttons: " + text;
						Debug.LogWarning(logMessage);
					}
					return false;
				}
				buttonToUse = _cachedButtons[optionIndex];
				if (buttonToUse == null)
				{
					if (_logMessages)
					{
						logMessage = "MenuDialog: Unable to add menu item, cached " +
							"button is null: " + text;
						Debug.LogWarning(logMessage);
					}
					return false;
				}
				buttonToUse.gameObject.SetActive(true);
				
			}

			_visibleButtons.Add(buttonToUse);

			// move forward for next call
			_nextOptionIndex++;

			// don't need to set anything on it
			if (hideOption)
			{
				// keep internal state consistent but don't display
				buttonToUse.gameObject.SetActive(false);
				return true;
			}

			Button btn = buttonToUse;
			btn.interactable = interactable;
			_textAdapter.InitFromGameObject(btn.gameObject, true);
			if (_textAdapter.HasTextObject())
			{
				text = TextVariationHandler.SelectVariations(text);
				_textAdapter.Text = text;
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

		public virtual IReadOnlyList<Button> VisibleButtons { get { return _visibleButtons; } }
		private readonly List<Button> _visibleButtons = new List<Button>();
		private readonly TextAdapter _textAdapter = new TextAdapter();

		protected virtual Button GetFirstINactiveButton()
		{
			Button result = null;
			// Check dynamic buttons first
			for (int i = 0; i < _dynamicButtons.Count; i++)
			{
				var buttonEl = _dynamicButtons[i];
				if (buttonEl != null && !buttonEl.gameObject.activeSelf)
				{
					result = buttonEl;
					break;
				}
			}

			if (result == null && _cachedButtons != null)
			{
				for (int i = 0; i < _cachedButtons.Length; i++)
				{
					var button = _cachedButtons[i];
					if (button != null && !button.gameObject.activeSelf)
					{
						result = button;
						break;
					}
				}
			}

			return result;
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
				for (int i = 0; i < _dynamicButtons.Count; i++)
				{
					var b = _dynamicButtons[i];
					if (b != null && b.gameObject.activeSelf) count++;
				}

				// count legacy cached visible buttons
				if (_cachedButtons != null)
				{
					for (int i = 0; i < _cachedButtons.Length; i++)
					{
						var button = _cachedButtons[i];
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
			for (int i = 0; i < _dynamicButtons.Count; i++)
			{
				if (_dynamicButtons[i] != null)
				{
					_dynamicButtons[i].transform.SetSiblingIndex(r.Next(_dynamicButtons.Count));
				}
			}

			// shuffle cached buttons
			if (_cachedButtons != null)
			{
				for (int i = 0; i < _cachedButtons.Length; i++)
				{
					if (_cachedButtons[i] != null)
					{
						_cachedButtons[i].transform.SetSiblingIndex(r.Next(_cachedButtons.Length));
					}
				}
			}
		}

		#region Helpers

		protected IEnumerator CallActionAfterOneFrame(Action callback)
		{
			yield return new WaitForEndOfFrame();
			callback?.Invoke();
		}

		#endregion

		protected virtual void OnValidate()
		{
			#region Ensure Root Scale is Valid
			// This is to compensate for a serialization issue in Unity 6.0
			// where the local scale (in the yaml) can be (0, 0, 0), 
			// which makes Unity 2022.3 freeze upon trying to open or 
			// instantiate the prefab. 
			var root = transform; // We assume we are on the root transform here

			if (root != null)
			{
				Vector3 localScale = root.localScale;
				bool invalidScale = Mathf.Approximately(localScale.x, 0f) ||
					Mathf.Approximately(localScale.y, 0f) ||
					Mathf.Approximately(localScale.z, 0f);

				if (invalidScale)
				{
					root.localScale = Vector3.one;
				}
			}
			#endregion
		}

	}    
}
