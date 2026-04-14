using AtMycelia.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AtMycelia.Hyphlow.UI;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Visual scripting controller for the Flowchart programming language.
    /// Flowchart objects may be edited visually using the Flowchart editor window.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(VariableManagerComponent))]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Flowchart : MonoBehaviour, ISubstitutionHandler, IReorderableMuscariableSource,
        IForceResetUidHandler, ISerializationCallbackReceiver, ITearDownResponder, IRefreshable,
        IBackwardsCompatibilityApplier
    {
        [SerializeField, HideInInspector] private VariableManagerComponent _varManager;

        [FormerlySerializedAs("variableManager")]
        [SerializeField, HideInInspector] private VariableManager legacyVariableManager = new VariableManager();

        [HideInInspector]
        [SerializeField] protected int version = 0; // Default to 0 to always trigger an update for older versions of Amanita.

        [HideInInspector]
        [FormerlySerializedAs("variables")]
        [FormerlySerializedAs("legacyVariables")]
        [SerializeField] protected List<Variable> _legacyVariables = new List<Variable>();

        [HideInInspector]
        [FormerlySerializedAs("muscariables")]
        [SerializeReference] protected List<Muscariable> _oldMuscariables = new List<Muscariable>();

        /// <summary>
        /// Force reset the unique identifier for this Flowchart. Use with caution!
        /// </summary>
        public virtual void ForceResetUid()
        {
            this.UniqueId = Guid.NewGuid().ToString();
        }


#if UNITY_EDITOR

        // Locking this under #if UNITY_EDITOR to avoid unnecessary serialization in builds

        [TextArea(3, 5)]
        [Tooltip("Description text displayed in the Flowchart editor window")]
        [FormerlySerializedAs("description")]
        [SerializeField] protected string description = "";

        /// <summary>
        /// What the editor utils should use to decide how to render this FC's data in the 
        /// FlowchartWindow and BlockInspector.
        /// </summary>
        public virtual FlowchartUIModel UIModel
        {
            get { return uiModel; }
        }

        [HideInInspector]
        [SerializeField]
        protected FlowchartUIModel uiModel = new FlowchartUIModel();

        [Range(0f, 5f)]
        [Tooltip("Adds a pause after each execution step to make it easier to visualise program flow. Editor only, has no effect in platform builds.")]
        [SerializeField] protected float stepPause = 0f;

        [Tooltip("Use command color when displaying the command list in the Fungus Editor window")]
        [SerializeField] protected bool colorCommands = true;

        [Tooltip("Hides the Flowchart block and command components in the inspector. Deselect to inspect the block and command components that make up the Flowchart.")]
        [SerializeField] protected bool hideComponents = true;

        [Tooltip("Saves the selected block and commands when saving the scene. Helps avoid version control conflicts if you've only changed the active selection.")]
        [SerializeField] protected bool saveSelection = true;

        [Tooltip("Display line numbers in the command list in the Block inspector.")]
        [SerializeField] protected bool showLineNumbers = false;

        [Tooltip("List of commands to hide in the Add Command menu. Use this to restrict the set of commands available when editing a Flowchart.")]
        [SerializeField] protected List<string> hideCommands = new List<string>();
#endif

        [Tooltip("Unique identifier for this flowchart in localized string keys. If no id is specified then the name of the Flowchart object will be used.")]
        [FormerlySerializedAs("localizationId")]
        [SerializeField] protected string localizationId = "";

        #region Save Sys Involvement
        [Tooltip("Whether or not the save system should save (and when appropriate, load) this Flowchart's variables.")]
        [SerializeField] protected bool includeInSaves = true;

        [Tooltip("Whether or not the execution state of this FC's Blocks should be considered for saving.")]
        [SerializeField] protected bool saveBlocks = true;

        [Tooltip("Whether or not this FC's vars should be saved or loaded.")]
        [SerializeField] protected bool saveVariables = true;

        [Tooltip("Affects the order this FC will get loaded relative to others. Lower number, earlier loading.")]
        [SerializeField] protected int loadPriority = 0;
        #endregion

        [SerializeField] private bool alwaysKeepGuid = true;

        public virtual bool IncludeInSaves
        {
            get { return includeInSaves; }
            set { includeInSaves = value; }
        }

        #region SaveSys Involvement
        public virtual bool SaveBlocks
        {
            get { return saveBlocks; }
            set { saveBlocks = value; }
        }

        public virtual bool SaveVariables
        {
            get { return saveVariables; }
            set { saveVariables = value; }
        }

        public virtual int LoadPriority
        {
            get { return loadPriority; }
            set { loadPriority = value; }
        }
        #endregion

        protected static bool eventSystemPresent;

        protected StringSubstituter stringSubstituter;

        public IReadOnlyCollection<Block> Blocks
        {
            get
            {
                // Refresh if cache is empty or contains null entries.
                if (_blockListCache == null || _blockListCache.Count == 0 ||
                    _blockListCache.Any(block => block == null))
                {
                    RefreshBlockAndCommandCache();
                }

                return _blockListCache;
            }
        }
        public IReadOnlyCollection<Command> Commands => _commands;

        protected virtual void Awake()
        {
            if (!this.IsInTheScene)
            {
                // Don't do anything if this isn't even in the scene yet
                return;
            }

            if (_varManager == null)
            {
                _varManager = GetComponent<VariableManagerComponent>();
            }

            RegisterLegacyVars();
            void RegisterLegacyVars()
            {
                _legacyVariables ??= new List<Variable>();
                if (_legacyVariables.Count == 0)
                {
                    var found = GetComponents<Variable>();
                    _legacyVariables.AddRange(found);
                }
            }

            AssertOwnership();
            RefreshBlockAndCommandCache();
            _varManager.Refresh();
#if UNITY_EDITOR
            UIModel.Owner = this.gameObject;
            EditorUtility.SetDirty(this);
#endif

        }

        private void RefreshBlockAndCommandCache()
        {
            _blockListCache ??= new List<Block>();
            _blocks ??= new Dictionary<uint, Block>();
            _commands ??= new List<Command>();
            // ^Despite the initializers in this class, weird things can happen with Unity

            _blockListCache.Clear();
            _blocks.Clear();
            _commands.Clear();

            var blocksFound = GetComponents<Block>();
            for (int i = 0; i < blocksFound.Length; i++)
            {
                var currentBlock = blocksFound[i];
                if (currentBlock == null)
                {
                    continue;
                }

                if (currentBlock.ItemId == 0 || _blocks.ContainsKey(currentBlock.ItemId))
                {
                    currentBlock.ItemId = NextItemId();
                }

                _blockListCache.Add(currentBlock);
                _blocks[currentBlock.ItemId] = currentBlock;
            }

            var commandsFound = GetComponents<Command>();
            _commands.AddRange(commandsFound);
        }

        [SerializeField][HideInInspector] private List<Block> _blockListCache = new List<Block>();
        private IDictionary<uint, Block> _blocks = new Dictionary<uint, Block>();
        [SerializeField][HideInInspector] private List<Command> _commands = new List<Command>();

        protected virtual void Start()
        {
            if (Application.IsPlaying(this))
            {
                StartCoroutine(HandleGameStartedBlocks());
            }
        }

        protected virtual IEnumerator HandleGameStartedBlocks()
        {
            IList<GameStarted> gsEventHandler = GetComponents<GameStarted>();

            if (gsEventHandler.Count == 0)
            {
                yield break;
            }

            foreach (var elem in gsEventHandler)//
            {
                elem.Trigger();
            }

        }


        protected virtual void OnEnable()
        {
            if (!this.IsInTheScene)
            {
                // Don't do anything if this isn't even in the scene yet
                return;
            }

            Refresh();
            ToggleSubs(true);

            StringSubstituter.RegisterHandler(this);
            FlowchartSignals.FlowchartEnabled(this);
        }

        private void ToggleSubs(bool on)
        {
            if (_varManager == null)
            {
                return;
            }
            if (on)
            {
                _varManager.VariableAdded += OnVarAdded;
                _varManager.VariableRemoved += OnVarRemoved;
            }
            else
            {
                _varManager.VariableAdded -= OnVarAdded;
                _varManager.VariableRemoved -= OnVarRemoved;
            }
        }

        private void OnVarAdded(IVariable added)
        {
            VariableAdded(added);
            FlowchartSignals.VariableAdded(this, added);
        }

        public event Action<IVariable> VariableAdded = delegate { };

        private void OnVarRemoved(IVariable removed)
        {
            VariableRemoved(removed);
            FlowchartSignals.VariableRemoved(this, removed);
        }

        public event Action<IVariable> VariableRemoved = delegate { };

        public int VariableCount
        {
            get
            {
                if (_varManager == null)
                {
                    _varManager = gameObject.GetOrAddComponent<VariableManagerComponent>();
                }


                return _varManager.Variables.Count;
            }
        }

        private bool IsInTheScene
        {
            get
            {
                if (!gameObject.scene.IsValid() || !gameObject.scene.isLoaded)
                {
                    return false;
                }

#if UNITY_EDITOR
                //return PrefabStageUtility.GetPrefabStage(gameObject) == null;
                return true;
#else
        return true;
#endif
            }
        }

        public virtual void Refresh()
        {
            AssertUniqueID();
            AssertOwnership();//
#if UNITY_EDITOR
            RefreshEditorCaches();
            UpdateHideFlags();
#endif
            CheckItemIds();
            CleanupComponents();
            UpdateVersion();
        }

#if UNITY_EDITOR
        private void RefreshEditorCaches()
        {
            if (Application.IsPlaying(this))
            {
                return;
            }

            RefreshBlockAndCommandCache();
        }
#endif

#if UNITY_EDITOR

        public virtual void GetVariableManagerMigrationData(out VariableManager legacyManager,
            out IList<Muscariable> oldMuscariables,
            out IList<Variable> legacyVariables)
        {
            legacyManager = legacyVariableManager;
            oldMuscariables = _oldMuscariables;
            legacyVariables = _legacyVariables;
        }

        public virtual void ClearVariableManagerMigrationData()
        {
            _oldMuscariables.Clear();
            _legacyVariables.Clear();
            legacyVariableManager.Clear();
            EditorUtility.SetDirty(this);
        }

        public virtual void RefreshVariableManagerForEditorReload()
        {
            if (!IsInTheScene || Application.isPlaying)
            {
                return;
            }

            AssertOwnership();
        }

#endif


        public IVariableSource VariableManager
        {
            get
            {
                if (_varManager != null)
                {
                    return _varManager;
                }
                else
                {
                    return legacyVariableManager;
                }
            }
        }

        protected virtual void AssertOwnership()
        {
            EnsureVariableManagerComponent();
            _varManager.Owner = this;
            // Legacy variables automatically get their owner-registration done;
            // it's always the Flowchart they're attached to.
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
            StopAllBlocks();
            StopAllCoroutines();
            StringSubstituter.UnregisterHandler(this);
            FlowchartSignals.FlowchartDisabled(this);
        }

        protected virtual void OnDestroy()
        {
            VariableAdded = delegate { };
            VariableRemoved = delegate { };
            FlowchartSignals.FlowchartDestroyed(this);
        }

        protected virtual void UpdateVersion()
        {
            if (version == HyphlowConstants.CurrentVersion)
            {
                // No need to update
                return;
            }

            // Tell all components that implement IUpdateable to update to the new version
            // This is important for when we rework Variables and Blocks to be more lightweight;
            // might want to make the old var and Block types IUpdatables
            var components = GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                IUpdateable toUpdate = component as IUpdateable;
                toUpdate?.UpdateToVersion(version, HyphlowConstants.CurrentVersion);
            }

            version = HyphlowConstants.CurrentVersion;
        }

        protected virtual void CheckItemIds()
        {
            // Make sure item ids are unique and monotonically increasing.
            // This should always be the case, but some legacy Flowcharts may have issues.
            List<ushort> usedIds = new List<ushort>();
            RefreshBlockAndCommandCache();
            CheckForBlocks();
            void CheckForBlocks()
            {
                foreach (var blockEl in _blocks.Values)
                {
                    if (blockEl == null) continue;

                    if (blockEl.ItemId == 0 || usedIds.Contains(blockEl.ItemId))
                    {
                        blockEl.ItemId = NextItemId();
                    }
                    usedIds.Add(blockEl.ItemId);
                }
            }

            CheckForCommands();
            void CheckForCommands()
            {
                var commands = GetComponents<Command>();
                foreach (Command commandEl in _commands)
                {
                    if (commandEl == null)
                    {
                        Debug.LogWarning($"Found null Command while ensuring unique IDs.");
                        continue;
                    }

                    if (commandEl.ItemId == 0 || usedIds.Contains(commandEl.ItemId))
                    {
                        commandEl.ItemId = NextItemId();
                    }
                    usedIds.Add(commandEl.ItemId);
                }
            }

            UpdateNextValidVarID();
            void UpdateNextValidVarID()
            {
                var varWithHighestID = Variables.OrderByDescending(x => x.ItemId).FirstOrDefault();
                if (varWithHighestID == null)
                {
                    return;
                }
                byte highestIDFound = varWithHighestID.ItemId;
                if (nextValidVarID < highestIDFound)
                {
                    nextValidVarID = (byte)(highestIDFound + 1);
                }
            }

        }

        protected virtual byte NextValidVarID()
        {
            byte toReturn = nextValidVarID;
            nextValidVarID++;
            return toReturn;
        }

        [HideInInspector]
        [SerializeField] protected byte nextValidVarID = 1;

        protected virtual void CleanupComponents()
        {
            // Delete any unreferenced components which shouldn't exist any more
            // Unreferenced components don't have any effect on the flowchart behavior, but
            // they waste memory so should be cleared out periodically.

            // Remove any null entries in the variables list
            // It shouldn't happen but it seemed to occur for a user on the forum 
            _legacyVariables.RemoveAll(item => item == null);

            // Aviod destroying the legacy vars. Let them exist, even if we have muscaris
            // acting in their place.

            #region Destroy Commands that aren't in any blocks
            for (int i = 0; i < _commands.Count; i++)
            {
                var command = _commands[i];
                bool found = false;
                foreach (Block block in _blocks.Values)
                {
                    if (block.CommandList.Contains(command))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    DestroyImmediate(command);
                }
            }
            #endregion

            #region Destroy EventHandlers that aren't on any blocks
            var eventHandlers = GetComponents<EventHandler>();
            for (int i = 0; i < eventHandlers.Length; i++)
            {
                var eventHandler = eventHandlers[i];
                bool found = false;
                foreach (Block block in _blocks.Values)
                {
                    if (block._EventHandler == eventHandler)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    DestroyImmediate(eventHandler);
                }
            }
            #endregion
        }

        protected virtual Block CreateBlockComponent(GameObject parent)
        {
            Block block = parent.AddComponent<Block>();
            return block;
        }

        #region Public members

#if UNITY_EDITOR
        #region Flowchart UI State and Methods

        public bool SelectedCommandsStale
        {
            get => UIModel.SelectedCommandsStale;
            set => UIModel.SelectedCommandsStale = value;
        }

        /// <summary>
        /// Scroll position of Flowchart editor window.
        /// </summary>
        public virtual Vector2 ScrollPos
        {
            get => uiModel.ScrollPos;
            set => uiModel.ScrollPos = value;
        }

        public virtual float Zoom
        {
            get => uiModel.Zoom;
            set => uiModel.Zoom = value;
        }

        /// <summary>
        /// Scrollable area for Flowchart editor window.
        /// </summary>
        public virtual Rect ScrollViewRect
        {
            get => uiModel.ScrollViewRect;
            set => uiModel.ScrollViewRect = value;
        }

        /// <summary>
        /// Current actively selected block in the Flowchart editor.
        /// </summary>
        public virtual Block SelectedBlock
        {
            get => uiModel.SelectedBlock;
            set => uiModel.SelectedBlock = value;
        }

        public virtual IList<Block> SelectedBlocks
        {
            get => uiModel.SelectedBlocks;
            set => uiModel.SelectedBlocks = value;
        }

        /// <summary>
        /// Currently selected command in the Flowchart editor.
        /// </summary>
        public virtual IList<Command> SelectedCommands
        {
            get => uiModel.SelectedCommands; // Returns a copy
            set => uiModel.SelectedCommands = value;
        }

        public virtual int SelectedCommandCount
        {
            get { return uiModel.CommandCount; }
        }

        public virtual int SelectedBlockCount
        {
            get { return uiModel.BlockCount; }
        }

        public virtual void UpdateSelectedCache()
        {
            SelectedBlocks.Clear();
            var res = gameObject.GetComponents<Block>();
            SelectedBlocks = res.Where(x => x.IsSelected).ToList();
        }

        public virtual void ReverseUpdateSelectedCache()
        {
            for (int i = 0; i < SelectedBlockCount; i++)
            {
                if (SelectedBlocks[i] != null)
                {
                    SelectedBlocks[i].IsSelected = true;
                }
            }
        }

        /// <summary>
        /// Clears the list of selected blocks.
        /// </summary>
        public virtual void ClearSelectedBlocks()
        {
            IList<Block> blocksToSignal = SelectedBlocks;
            UIModel.ClearSelectedBlocks();
        }

        public virtual void AddRangeToSelection(IList<Block> toSelect)
        {
            UIModel.AddRangeToSelection(toSelect);
        }

        /// <summary>
        /// Adds a block to the list of selected blocks.
        /// </summary>
        public virtual void AddToSelection(Block block) => UIModel.AddToSelection(block);

        public virtual void DeselectBlockNoCheck(Block toDeselect) => UIModel.Deselect(toDeselect);

        public virtual void DeselectAll()
        {
            UIModel.ClearSelectedBlocks();
            UIModel.ClearSelectedCommands();
        }

        /// <summary>
        /// Set the block objects to be hidden or visible depending on the hideComponents property.
        /// </summary>
        public virtual void UpdateHideFlags()
        {
            if (hideComponents)
            {
                var blocks = _blocks;
                foreach (var block in blocks.Values)
                {
                    block.hideFlags = HideFlags.HideInInspector;
                    if (block.gameObject != gameObject)
                    {
                        block.hideFlags = HideFlags.HideInHierarchy;
                    }
                }

                var commands = _commands;
                foreach (var command in commands)
                {
                    command.hideFlags = HideFlags.HideInInspector;
                }
                var eventHandlers = GetComponents<EventHandler>();
                for (int i = 0; i < eventHandlers.Length; i++)
                {
                    var eventHandler = eventHandlers[i];
                    eventHandler.hideFlags = HideFlags.HideInInspector;
                }
            }
            else
            {
                var monoBehaviours = GetComponents<MonoBehaviour>();
                for (int i = 0; i < monoBehaviours.Length; i++)
                {
                    var monoBehaviour = monoBehaviours[i];
                    if (monoBehaviour == null)
                    {
                        continue;
                    }
                    monoBehaviour.hideFlags = HideFlags.None;
                    monoBehaviour.gameObject.hideFlags = HideFlags.None;
                }
            }
        }

        /// <summary>
        /// Override this in a Flowchart subclass to filter which commands are shown in the Add Command list.
        /// </summary>
        public virtual bool IsCommandSupported(CommandInfoAttribute commandInfo)
        {
            for (int i = 0; i < hideCommands.Count; i++)
            {
                // Match on category or command name (case insensitive)
                var key = hideCommands[i];
                if (String.Compare(commandInfo.Category, key, StringComparison.OrdinalIgnoreCase) == 0 || String.Compare(commandInfo.CommandName, key, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Clears the list of selected commands.
        /// </summary>
        public virtual void ClearSelectedCommands()
        {
            UIModel.ClearSelectedCommands();
#if UNITY_EDITOR
            SelectedCommandsStale = true;
#endif
        }

        /// <summary>
        /// Adds a command to the list of selected commands.
        /// </summary>
        public virtual void AddSelectedCommand(Command command)
        {
            if (!uiModel.Contains(command))
            {
                // The SelectedCommands getter returns a defensive decoy. Thus, rather than something
                // like SelectedCommands.Add, we call the ui model's method specifically for registering
                // Commands.
                UIModel.AddToSelection(command);
#if UNITY_EDITOR
                SelectedCommandsStale = true;
#endif
                SelectedCommandAdded(command);
            }
        }

        /// <summary>
        /// For when added through AddSelectedCommand (as opposed to just setting 
        /// the SelectedCommands property or such)
        /// </summary>
        public event Action<Command> SelectedCommandAdded = delegate { };

        #endregion
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Slow down execution in the editor to make it easier to visualise program flow.
        /// </summary>
        public virtual float StepPause { get { return stepPause; } }

        /// <summary>
        /// Use command color when displaying the command list in the inspector.
        /// </summary>
        public virtual bool ColorCommands { get { return colorCommands; } }

        /// <summary>
        /// Saves the selected block and commands when saving the scene. Helps avoid version control conflicts if you've only changed the active selection.
        /// </summary>
        public virtual bool SaveSelection { get { return saveSelection; } }

        /// <summary>
        /// Display line numbers in the command list in the Block inspector.
        /// </summary>
        public virtual bool ShowLineNumbers { get { return showLineNumbers; } }

#endif

        /// <summary>
        /// Description text displayed in the Flowchart editor window
        /// </summary>
        public virtual string Description { get { return description; } }

        /// <summary>
        /// Unique identifier for identifying this flowchart in localized string keys.
        /// </summary>
        public virtual string LocalizationId { get { return localizationId; } }

        /// <summary>
        /// Position in the center of all blocks in the flowchart.
        /// </summary>
        public virtual Vector2 CenterPosition { set; get; }

        /// <summary>
        /// Variable to track flowchart's version so components can update to new versions.
        /// </summary>
        public int Version { set { version = value; } }

        /// <summary>
        /// Returns true if the Flowchart gameobject is active.
        /// </summary>
        public bool IsActive()
        {
            return gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Returns the next id to assign to a new Block or Command.
        /// Item ids increase monotically so they are guaranteed to
        /// be unique within a Flowchart.
        /// </summary>
        public ushort NextItemId()
        {
            // As for why we make Blocks and Commands get IDs from the same pool while vars get 
            // their own... we want to give users the option to move Commands between Blocks
            // without worrying about ID conflicts, but variables added to a Flowchart are
            // supposed to forever be with that same Flowchart.
            ushort maxId = 0;
            _blockListCache.RemoveAll(item => item == null);
            for (int i = 0; i < _blockListCache.Count; i++)
            {
                var block = _blockListCache[i];
                maxId = Math.Max(maxId, block.ItemId);
            }

            _commands.RemoveAll(item => item == null);
            for (int i = 0; i < _commands.Count; i++)
            {
                var command = _commands[i];
                maxId = Math.Max(maxId, command.ItemId);
            }

            maxId++;
            return maxId;
        }

        #region Block-Handling

        /// <summary>
        /// Create a new block node which you can then add commands to.
        /// </summary>
        public virtual Block CreateBlock(Vector2 position, string blockName = null)
        {
            blockName ??= HyphlowConstants.DefaultBlockName;
            Block created = CreateBlockComponent(gameObject);
#if UNITY_EDITOR
            created._NodeRect = new Rect(position, defaultBlockSize);
#endif
            created.BlockName = GetUniqueBlockKey(blockName, created);
            created.ItemId = NextItemId();
            _blocks.Add(created.ItemId, created);
            _blockListCache.Add(created);
            BlockSignals.BlockCreated(created);
            return created;
        }

        protected static Vector2 defaultBlockSize = new Vector2(300, 100);

        public virtual IList<Block> CreateMultiBlocks(IList<Vector2> positions)
        {
            IList<Block> blocksCreated = new Block[positions.Count];
            for (int i = 0; i < positions.Count; i++)
            {
                Vector2 currentPos = positions[i];
                Block newBlock = CreateBlock(currentPos);
                blocksCreated[i] = newBlock;
            }
            return blocksCreated;
        }

        /// <summary>
        /// Returns the named Block in the flowchart, or null if not found.
        /// </summary>
        public virtual Block FindBlock(string blockName)
        {
            foreach (var blockEl in _blocks.Values)
            {
                if (blockEl.BlockName == blockName)
                {
                    return blockEl;
                }
            }

            return null;
        }

        public virtual Block FindBlockByItemId(uint itemId)
        {
            _blocks.TryGetValue(itemId, out Block result);
            return result;
        }

        /// <summary>
        /// Checks availability of the block in the Flowchart.
        /// You can use this method in a UI event. e.g. to test availability block, before handle it.
        public virtual bool HasBlock(string blockName)
        {
            var block = FindBlock(blockName);
            return block != null;
        }

        /// <summary>
        /// Executes the block if it is available in the Flowchart.
        /// You can use this method in a UI event. e.g. to try executing block without confidence in its existence.
        public virtual bool ExecuteIfHasBlock(string blockName)
        {
            if (HasBlock(blockName))
            {
                ExecuteBlock(blockName);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Execute a child block in the Flowchart.
        /// You can use this method in a UI event. e.g. to handle a button click.
        public virtual void ExecuteBlock(string blockName)
        {
            var block = FindBlock(blockName);

            if (block == null)
            {
                Debug.LogError("Block " + blockName + " does not exist");
                return;
            }

            if (!ExecuteBlock(block))
            {
                Debug.LogWarning("Block " + blockName + " failed to execute");
            }
        }

        /// <summary>
        /// Stops an executing Block in the Flowchart.
        /// </summary>
        public virtual void StopBlock(string blockName)
        {
            var block = FindBlock(blockName);

            if (block == null)
            {
                Debug.LogError("Block " + blockName + " does not exist");
                return;
            }

            if (block.IsExecuting())
            {
                block.Stop();
            }
        }

        /// <summary>
        /// Execute a child block in the flowchart.
        /// The block must be in an idle state to be executed.
        /// This version provides extra options to control how the block is executed.
        /// Returns true if the Block started execution.            
        /// </summary>
        public virtual bool ExecuteBlock(Block block, int commandIndex = 0, Action onComplete = null)
        {
            if (block == null)
            {
                Debug.LogError("Block must not be null");
                return false;
            }

            if (block.gameObject != gameObject)
            {
                Debug.LogError("Block must belong to the same gameObject as this Flowchart");
                return false;
            }

            // Can't restart a running block, have to wait until it's idle again
            if (block.IsExecuting())
            {
                Debug.LogWarning(block.BlockName + " cannot be called/executed, it is already running.");
                return false;
            }

            // Start executing the Block as a new coroutine
            StartCoroutine(block.Execute(commandIndex, onComplete));

            return true;
        }

        /// <summary>
        /// Stop all executing Blocks in this Flowchart.
        /// </summary>
        public virtual void StopAllBlocks()
        {
            var blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                if (block.IsExecuting())
                {
                    block.Stop();
                }
            }
        }

        /// <summary>
        /// Returns a new Block key that is guaranteed not to clash with any existing Block in the Flowchart.
        /// </summary>
        public virtual string GetUniqueBlockKey(string originalKey, Block ignoreBlock = null)
        {
            int suffix = 0;
            string baseKey = originalKey.Trim();

            // No empty keys allowed
            if (baseKey.Length == 0)
            {
                baseKey = HyphlowConstants.DefaultBlockName;
            }

            var blocks = GetComponents<Block>();

            string key = baseKey;
            while (true)
            {
                bool collision = false;
                for (int i = 0; i < blocks.Length; i++)
                {
                    var block = blocks[i];
                    if (block == ignoreBlock || block.BlockName == null)
                    {
                        continue;
                    }
                    if (block.BlockName.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        collision = true;
                        suffix++;
                        key = baseKey + suffix;
                    }
                }

                if (!collision)
                {
                    return key;
                }
            }
        }

        #endregion

        /// <summary>
        /// Returns a new Label key that is guaranteed not to clash with any existing Label in the Block.
        /// </summary>
        public virtual string GetUniqueLabelKey(string originalKey, Label ignoreLabel)
        {
            int suffix = 0;
            string baseKey = originalKey.Trim();

            // No empty keys allowed
            if (baseKey.Length == 0)
            {
                baseKey = "New Label";
            }

            var block = ignoreLabel.ParentBlock;

            string key = baseKey;
            while (true)
            {
                bool collision = false;
                var commandList = block.CommandList;
                for (int i = 0; i < commandList.Count; i++)
                {
                    var command = commandList[i];
                    Label label = command as Label;
                    if (label == null || label == ignoreLabel)
                    {
                        continue;
                    }
                    if (label.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        collision = true;
                        suffix++;
                        key = baseKey + suffix;
                    }
                }

                if (!collision)
                {
                    return key;
                }
            }
        }

        #region Variable-Handling

        /// <summary>
        /// Reorders the legacy Variable list to match the sequence supplied (only
        /// for those Variables already registered). Muscariables are not affected.
        /// Variables not present in newOrder retain their relative order at the end.
        /// Does not raise add/remove events (pure reordering).
        /// </summary>
        public virtual void ReorderVariables(IList<IVariable> newOrder)
        {
            if (newOrder == null || newOrder.Count == 0) return;

            // Extract legacy variables that appear in newOrder, in that order
            var ordered = new List<Variable>(_legacyVariables.Count);
            var seen = new HashSet<Variable>();

            for (int i = 0; i < newOrder.Count; i++)
            {
                if (newOrder[i] is Variable legacy && _legacyVariables.ContainsReference(legacy) && seen.Add(legacy))
                    ordered.Add(legacy);
            }

            // Append the rest (not explicitly positioned)
            for (int i = 0; i < _legacyVariables.Count; i++)
            {
                var elem = _legacyVariables[i];
                if (!seen.Contains(elem))
                {
                    ordered.Add(elem);
                }
            }
            if (ordered.Count == _legacyVariables.Count)
            {
                _legacyVariables = ordered;
            }
        }


        /// <summary>
        /// Adds an already-existing Muscariable to the Flowchart, getting it integrated as something
        /// owned by said Flowchart. If the variable is already registered,
        /// it will not be added again.
        /// </summary>



        /// <summary>
        /// Adds and registers a new var to the flowchart. If the passed key is null or empty,
        /// a unique key will be generated. If TVarType is a legacy Variable type, it will be converted
        /// into its Muscariable equivalent and the legacy variable will be destroyed.
        /// </summary>

        /// <summary>
        /// Adds an already-existing variable to the flowchart. If the variable is already registered,
        /// nothing happens. The variable's key and ID will be made unique if necessary.
        /// If the variable is a legacy Variable, a Muscariable version of it will
        /// be registered instead.
        /// </summary>

        /// <summary>
        /// Returns the variable with the specified key, or null if the key is not found.
        /// You will need to cast the returned variable to the correct sub-type.
        /// You can then access the variable's value using the Value property. e.g.
        /// BooleanVariable boolVar = flowchart.GetVariable("MyBool") as BooleanVariable;
        /// boolVar.Value = false;
        /// </summary>



        /// <summary>
        /// Returns the variable with the specified key, or null if the key is not found.
        /// You can then access the variable's value using the Value property. e.g.
        /// BooleanVariable boolVar = flowchart.GetVariable<BooleanVariable>("MyBool");
        /// boolVar.Value = false;
        /// </summary>


        /// <summary>
        /// Returns a list of variables matching the specified type.
        /// </summary>




        /// <summary>
        /// Creates and returns a new Muscariable of the specified type, with this
        /// as the parent Flowchart.
        /// </summary>


        /// <summary>
        /// Sets up the Muscariable to belong to this Flowchart before adding it.
        /// </summary>
        public virtual void IntegrateMuscariable(Muscariable toAdd)
        {
            FlowchartSignals.VariableAdded(this, toAdd);
        }

        /// <summary>
        /// Unregisters the Muscariable from this Flowchart, setting it to have no parent FC.
        /// </summary>
        /// <param name="toRemove"></param>




        #endregion

        /// <summary>
        /// Reset the Commands and Variables in the Flowchart.
        /// </summary>
        public virtual void ResetFlowchart(bool resetCommands, bool resetVariables)
        {
            if (resetCommands)
            {
                var commands = GetComponents<Command>();
                for (int i = 0; i < commands.Length; i++)
                {
                    var command = commands[i];
                    command.OnReset();
                }
            }

            if (resetVariables)
            {
                for (int i = 0; i < _legacyVariables.Count; i++)
                {
                    var variable = _legacyVariables[i];
                    variable.OnReset();
                }
            }
        }

        /// <summary>
        /// Returns true if there are any executing blocks in this Flowchart.
        /// </summary>
        public virtual bool HasExecutingBlocks()
        {
            var blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                if (block.IsExecuting())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a list of all executing blocks in this Flowchart.
        /// </summary>
        public virtual List<Block> GetExecutingBlocks()
        {
            var executingBlocks = new List<Block>();
            var blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                if (block.IsExecuting())
                {
                    executingBlocks.Add(block);
                }
            }

            return executingBlocks;
        }

        /// <summary>
        /// Substitute variables in the input text with the format {$VarName}
        /// This will first match with private variables in this Flowchart, and then
        /// with public variables in all Flowcharts in the scene (and any component
        /// in the scene that implements StringSubstituter.ISubstitutionHandler).
        /// </summary>
        public virtual string SubstituteVariables(string input)
        {
            string prevResult = input;
            string result = _stringVarSubstituter.SubstituteVariables(input, this);
            while (prevResult != result) // Nested tags and all.
            {
                prevResult = result;
                result = _stringVarSubstituter.SubstituteVariables(result, this);
            }
            return result;
        }

        private StringVarSubstituter _stringVarSubstituter = new StringVarSubstituter();
        public const string SubstituteVariableRegexString = StringVarSubstituter.SubstituteVariableRegexString;

        public virtual void DetermineSubstituteVariables(string str, IList<IVariable> vars)
        {
            Regex r = new Regex(SubstituteVariableRegexString);

            // Match the regular expression pattern against a text string.
            var results = r.Matches(str);
            for (int i = 0; i < results.Count; i++)
            {
                var match = results[i];
                string varName = match.Value.Substring(2, match.Value.Length - 3);
                var v = GetVariable(varName);
                if (v != null)
                {
                    vars.Add(v);
                }
            }
        }
        #endregion

        #region IStringSubstituter implementation

        /// <summary>
        /// Implementation of StringSubstituter.ISubstitutionHandler.
        /// </summary>
        public virtual bool SubstituteStrings(StringBuilder input)
        {
            if (input == null)
            {
                return false;
            }

            string original = input.ToString();
            string replaced = _stringVarSubstituter.SubstituteVariables(original, this);
            bool anyChangesApplied = !string.Equals(original, replaced, StringComparison.Ordinal);
            if (!anyChangesApplied)
            {
                return false;
            }

            input.Length = 0;
            input.Append(replaced);
            return true;
        }

        #endregion

        [HideInInspector]
        [SerializeField] private string uniqueId = string.Empty;
        /// <summary>
        /// Unique identifier not specific to localization. Don't assign to this unless you know what you're doing.
        /// </summary>
        public string UniqueId
        {
            get => uniqueId;
            set
            {
                if (!string.IsNullOrEmpty(uniqueId))
                {
                    Debug.LogWarning($"Assigning a new unique ID to {this.name}, a Flowchart that already has one. " +
                        $"Old ID: {uniqueId}, New ID: {value}. If this was intentional, make sure you " +
                        $"know what you're doing.");
                }

                string prevId = uniqueId;
                uniqueId = value;
            }
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!this.IsInTheScene || Application.isPlaying)
            {
                // Don't do anything if this isn't even in the scene yet
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (this == null) // Object may have been destroyed
                {
                    return;
                }

                EnsureVariableManagerComponent();
                
                _legacyVariables.RemoveAll((elem) => elem == null);
                _oldMuscariables.RemoveAll((elem) => elem == null);

                uiModel ??= new FlowchartUIModel();
                if (uiModel.Owner == null)
                {
                    uiModel.Owner = this.gameObject;
                }

                Refresh();
                EnsureBlocksHaveAValidSize();
                void EnsureBlocksHaveAValidSize()
                {
                    IList<Block> blocks = GetComponents<Block>();//
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        var currentBlock = blocks[i];
                        Rect nodeRect = currentBlock._NodeRect;
                        if (nodeRect.size.Equals(Vector2.zero))
                        {
                            string logMessage = $"Fixing the size of Block {currentBlock.BlockName}. There may be an underlying problem.";
                            Debug.LogWarning(logMessage);
                            Rect fixedRect = new Rect(nodeRect.position, defaultBlockSize);
                            currentBlock._NodeRect = fixedRect;
                        }
                    }
                }

            };

        }
#endif

        protected virtual void AssertUniqueID()
        {
            if (string.IsNullOrEmpty(uniqueId))
            {
                UniqueId = Guid.NewGuid().ToString();
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public virtual bool AlwaysKeepGuid
        {
            get
            {
                return alwaysKeepGuid;
            }
            set
            {
                alwaysKeepGuid = value;
            }
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public virtual IReadOnlyList<IVariable> Variables
        {
            get
            {
#if UNITY_EDITOR
                if (this == null) // Possible in unit tests
                {
                    return Array.Empty<IVariable>();
                }
#endif
                EnsureVariableManagerComponent();
                _varManager.Owner = this;
                return VariableManager.Variables;
            }
        }

        IReadOnlyList<Muscariable> IVariableSource<Muscariable>.Variables => ((IVariableSource<Muscariable>)_varManager).Variables;

        private void EnsureVariableManagerComponent()
        {
            if (_varManager != null)
            {
                return;
            }

            _varManager = GetComponent<VariableManagerComponent>();
            if (_varManager == null)
            {
                _varManager = gameObject.AddComponent<VariableManagerComponent>();
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        protected virtual void LetUserKnowVarDoesntExist(string varName)
        {
            string warningMessage = $"Variable named {varName} in Flowchart {this.name} is just " +
                $"like Santa Claus: it doesn't exist.";
            Debug.LogWarning(warningMessage);
        }


#if UNITY_EDITOR

        public static void ResetStaticsForTest()
        {
            eventSystemPresent = false;
        }


        public virtual void OnTearDown()
        {
        }

#endif

        public bool Contains(IVariable var)
        {
            return _legacyVariables.Contains(var) || _oldMuscariables.Contains(var);
        }

        public virtual void OnBeforeSerialize()
        {
        }

        public virtual void OnAfterSerialize()
        {
        }


#if UNITY_EDITOR
        public T AddCommand<T>(Block toAddTo) where T : Command
        {
            return AddCommand(typeof(T), toAddTo) as T;
        }

        public Command AddCommand(Type commandType, Block toAddTo)
        {
            if (!typeof(Command).IsAssignableFrom(commandType))
            {
                Debug.LogError($"AddCommand: {commandType} does not inherit from Command.");
                return null;
            }

            // Record the Flowchart because we're about to modify its internal _commands list
            Undo.RecordObject(this, $"Add {commandType.Name} Command");

            // Record the GameObject because we're adding a component to it
            Undo.RecordObject(this.gameObject, $"Add {commandType.Name} Command Component");

            // Create the component with Undo support
            var added = Undo.AddComponent(this.gameObject, commandType) as Command;

            if (added == null)
            {
                Debug.LogError($"AddCommand: Failed to add component of type {commandType}.");
                return null;
            }

            added.ItemId = NextItemId();

            // Update Flowchart's internal list
            _commands.Add(added);

            // Update the Block's list
            toAddTo.CommandList.Add(added);

            added.OnCommandAdded(toAddTo);

            // Mark Flowchart dirty so Unity saves the change
            EditorUtility.SetDirty(this);

            return added;
        }

        /// <summary>
        /// For editor operations only. Removes the blocks from the list of blocks in the flowchart,
        /// without destroying them. This is used for operations like deleting multiple blocks, where we
        /// want to remove the blocks from the flowchart's list of blocks before destroying them,
        /// to avoid null references in the flowchart's list of blocks.
        /// </summary>
        public virtual void RemoveMultiBlocks(IList<Block> toUnregister)
        {
            for (int i = 0; i < toUnregister.Count; i++)
            {
                RemoveBlock(toUnregister[i]);
            }
        }

        /// <summary>
        /// For editor operations only. Removes the block from the list of blocks in the flowchart, 
        /// without destroying it. This is used for operations like deleting a block, where we 
        /// want to remove the block from the flowchart's list of blocks before destroying it, 
        /// to avoid null references in the flowchart's list of blocks.
        /// </summary>
        public virtual void RemoveBlock(Block toUnregister)
        {
            _blocks.Remove(toUnregister.ItemId);
            _blockListCache.Remove(toUnregister);
        }

        public virtual void ApplyBackwardsCompatibility()
        {
        }

        public virtual void OnAfterDeserialize()
        {

        }
#endif

        public virtual IVariable AddVariable(IVariable toAdd)
        {
            return VariableManager.AddVariable(toAdd);
        }

        public virtual void RemoveVariable(IVariable toRemove)
        {
            VariableManager.RemoveVariable(toRemove);
        }

        public IVariable GetVariable(byte itemID)
        {
            return VariableManager.GetVariable(itemID);
        }

        public virtual void ClearVariables()
        {
            _varManager.Clear();
        }

        public Muscariable AddVariable(Muscariable toAdd)
        {
            return _varManager.AddVariable(toAdd);
        }

        public virtual void RemoveVariable(Muscariable toRemove)
        {
            _varManager.RemoveVariable(toRemove);
        }

        T IVariableSource.GetVariableOfType<T>()
        {
            return VariableManager.GetVariableOfType<T>();
        }

        public IVariable GetVariable(string name, StringComparison strCompare = StringComparison.Ordinal)
        {
            return VariableManager.GetVariable(name, strCompare);
        }

        T IVariableSource.GetVariableOfType<T>(string name, StringComparison strCompare)
        {
            return VariableManager.GetVariableOfType<T>(name, strCompare);
        }

        public IVariable GetVariableOfType(Type type, string name, StringComparison strCompare = StringComparison.Ordinal)
        {
            return VariableManager.GetVariableOfType(type, name, strCompare);
        }

        public virtual TVarType AddNewMuscariable<TContentType, TVarType>(string key,
            TContentType defaultValue = default,
            VariableScope scope = VariableScope.Private)
            where TVarType : Muscariable<TContentType>, new()
        {
            var result = _varManager.AddNewVariableOfContentType(typeof(TContentType), key) as TVarType;
            if (result != null)
            {
                result.Scope = scope;
                result.Init(defaultValue);
            }
            return result;
        }

        public virtual IVariable<TContentType> AddNewVariable<TContentType>(string key,
            TContentType defaultValue = default,
            VariableScope scope = VariableScope.Private)
        {
            var result = _varManager.AddNewVariableOfContentType(typeof(TContentType), key) as IVariable<TContentType>;
            if (result != null)
            {
                result.Value = defaultValue;
                result.Scope = scope;
            }
            return result;
        }

        public Muscariable AddNewVariableOfContentType<TContentType>(string k, TContentType defaultVal,
            VariableScope scope = VariableScope.Private)
        {
            return _varManager.AddNewVariableOfContentType(k, defaultVal, scope);
        }

        public Muscariable AddNewVariableOfContentType(Type contentType, string key)
        {
            return _varManager.AddNewVariableOfContentType(contentType, key);
        }
    }
    
}
