using Amanita.Lua;
using Amanita.VScripting.EventHandlers;
using Amanita.VScripting.UI;
using Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using AmanitaEventHandler = Amanita.VScripting.EventHandlers.EventHandler;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Amanita.VScripting
{
    /// <summary>
    /// Visual scripting controller for the Flowchart programming language.
    /// Flowchart objects may be edited visually using the Flowchart editor window.
    /// </summary>
    [ExecuteInEditMode]
    public class Flowchart : MonoBehaviour, ISubstitutionHandler, 
        IReorderableVariableSource, IReorderableMuscariableSource,
        IForceResetUidHandler, ISerializationCallbackReceiver, ITearDownResponder
    {
        /// <summary>
        /// Force reset the unique identifier for this Flowchart. Use with caution!
        /// </summary>
        public virtual void ForceResetUid()
        {
            this.UniqueId = Guid.NewGuid().ToString();
        }

        public virtual IVariable GetVariable(byte itemID)
        {
            varLookupById.TryGetValue(itemID, out IVariable result);
            return result;
        }
        
        public const string SubstituteVariableRegexString = "{\\$.*?}";

        // For more performant lookups, we cache a dictionary of vars by their id.
        protected IDictionary<byte, IVariable> varLookupById = new Dictionary<byte, IVariable>();

        [HideInInspector]
        [SerializeField] protected int version = 0; // Default to 0 to always trigger an update for older versions of Amanita.
        
        [HideInInspector]
        [FormerlySerializedAs("variables")]
        [SerializeField] protected List<Variable> legacyVariables = new List<Variable>();

        [HideInInspector]
        [SerializeReference] protected List<Muscariable> muscariables = new List<Muscariable>();

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

        [Tooltip("Lua Environment to be used by default for all Execute Lua commands in this Flowchart")]
        [FormerlySerializedAs("luaEnvironment")]
        [SerializeField] protected LuaEnvironment _luaEnvironment;

        [Tooltip("The ExecuteLua command adds a global Lua variable with this name bound to the flowchart prior to executing.")]
        [FormerlySerializedAs("_luaBindingName")]
        [SerializeField] protected string luaBindingName = "flowchart";

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
        
        public IReadOnlyCollection<Block> Blocks => _blocks;
        public IReadOnlyCollection<Command> Commands => (IReadOnlyCollection<Command>)_commands;

        protected virtual void Awake()
        {
            if (!this.IsInTheScene)
            {
                // Don't do anything if this isn't even in the scene yet
                return;
            }

            legacyVariables ??= new List<Variable>();
            if (legacyVariables.Count == 0)
            {
                var found = GetComponents<Variable>();
                legacyVariables.AddRange(found);
            }

            var blocksFound = GetComponents<Block>();
            _blocks.AddRange(blocksFound);
            _commands = GetComponents<Command>().ToList();

#if UNITY_EDITOR
            UIModel.Owner = this.gameObject;
#endif

            if (Application.IsPlaying(this))
            {
                GetAndInitVars();
            }
        }

        [SerializeField] [HideInInspector] private HashSet<Block> _blocks = new HashSet<Block>();
        [SerializeField] [HideInInspector] private IList<Command> _commands = new List<Command>();

        protected virtual void Start()
        {
            if (Application.IsPlaying(this))
            {
                StartCoroutine(HandleGameStartedBlocks());
            }
        }

        protected virtual IEnumerator HandleGameStartedBlocks()
        {
            IList<GameStarted> gsEventHandler = GetComponentsInChildren<GameStarted>();

            if (gsEventHandler.Count == 0)
            {
                yield break;
            }

            while (AmanitaManager.S == null || !AmanitaManager.S.IsFullyInitted)
            {
                yield return null;
            }

            foreach (var elem in gsEventHandler)
            {
                elem.Trigger();
            }
            
        }

        /// <summary>
        /// Specifically for legacy variables.
        /// </summary>
        /// <param name="index"></param>
        public virtual void RemoveVariableAtIndex(int index)
        {
            if (index >= 0 && index < legacyVariables.Count)
            {
                IVariable toRemove = legacyVariables[index];
                legacyVariables.RemoveAt(index);
                MonoBehaviour component = toRemove as MonoBehaviour;
                Destroy(component);
                VariableRemoved(toRemove);
            }
        }

        public virtual void RemoveMuscariableAtIndex(int index)
        {
            if (index >= 0 && index < muscariables.Count)
            {
                IVariable toRemove = muscariables[index];
                muscariables.RemoveAt(index);
                VariableRemoved(toRemove);
            }
        }

        public virtual void RemoveVariable(IVariable toRemove)
        {
            // Different variables have Equals() implementations that don't always
            // return true based on ref, so we have to be extra clear about
            // how we care about references here.
            int index;
            if (legacyVariables.ContainsReference(toRemove))
            {
                index = legacyVariables.IndexOfReference(toRemove);
                RemoveVariableAtIndex(index);
            }

            if (muscariables.ContainsReference(toRemove))
            {
                index = muscariables.IndexOfReference(toRemove);
                RemoveMuscariableAtIndex(index);
            }
        }

        /// <summary>
        /// Removes all variables from this Flowchart.
        /// </summary>
        public virtual void ClearVariables()
        {
            // We'll remove them one by one so the right events fire
            while (legacyVariables.Count > 0)
            {
                RemoveVariableAtIndex(0);
            }

            while (muscariables.Count > 0)
            {
                RemoveMuscariableAtIndex(0);
            }
        }

        protected virtual void GetAndInitVars()
        {
            // Muscariables get automatically serialized as part of the list

            IList<IVariable> allVars = legacyVariables.Cast<IVariable>()
                .Concat(muscariables.Cast<IVariable>())
                .ToList();

            for (int i = 0; i < legacyVariables.Count; i++)
            {
                var currentVar = legacyVariables[i];
                currentVar.Init();
            }

            ReplaceLegacyWithMuscaris();
            void ReplaceLegacyWithMuscaris()
            {
                IList<Muscariable> newMuscaris = (from elem in legacyVariables
                                                        select elem.ToMuscariable()).ToList();

                while (legacyVariables.Count > 0)
                {
                    RemoveVariableAtIndex(0);
                }

                foreach (var elem in newMuscaris)
                {
                    elem.ParentFlowchart = this;
                    AddVariable(elem);
                }

            }

            for (int i = 0; i < muscariables.Count; i++)
            {
                var currentVar = muscariables[i];
                currentVar.Init();
            }
        }

        protected void OnActiveSceneChanged(Scene prevScene, Scene currentScene)
        {
            // Reset the flag for checking for an event system as there may not be one in the newly loaded scene.
            
        }

        protected virtual void OnEnable()
        {
            if (!this.IsInTheScene)
            {
                // Don't do anything if this isn't even in the scene yet
                return;
            }

            AmanitaManager.EnsureExists();
            var cachedFlowcharts = AmanitaManager.S.FlowchartsInScene;
            if (!cachedFlowcharts.Contains(this))
            {
                SceneManager.activeSceneChanged -= OnActiveSceneChanged; // Just in case.
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
            }

            Refresh();

            StringSubstituter.RegisterHandler(this);   
            FlowchartSignals.FlowchartEnabled(this);
        }

        private bool IsInTheScene => gameObject.scene.IsValid() && !string.IsNullOrEmpty(gameObject.scene.name);

        public virtual void Refresh()
        {
            AssertUniqueID();
            AssertOwnership();
            
            CheckItemIds();
            CleanupComponents();
            RefreshVarLookups();
            UpdateVersion();
#if UNITY_EDITOR
            RefreshEditorCaches();
            UpdateHideFlags();
#endif
        }

#if UNITY_EDITOR
        private void RefreshEditorCaches()
        {
            if (Application.IsPlaying(this))
            {
                return;
            }

            if (_blocks == null)
            {
                _blocks = new HashSet<Block>();
            }
            else
            {
                _blocks.Clear();
            }

            Block[] blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                _blocks.Add(blocks[i]);
            }

            _commands = GetComponents<Command>().ToList();
        }
#endif

        protected virtual void RefreshVarLookups()
        {
            varLookupById.Clear();
            foreach (var variable in Variables)
            {
                varLookupById[variable.ItemId] = variable;
            }
        }

        protected virtual void AssertOwnership()
        {
            foreach (Muscariable elem in Variables.Where((elem) => elem is Muscariable))
            {
                elem.Owner = this;
                elem.ParentFlowchart = this;
            }

            // Legacy variables automatically get their owner-registration done;
            // it's always the Flowchart they're attached to.
        }

        protected virtual void OnDisable()
        {
            StopAllBlocks();
            StopAllCoroutines();
            if (!AlwaysKeepGuid)
            {
                GuidRegistry fcReg = AmanitaManager.GetOrAddGuidRegistryFor<Flowchart>();
                fcReg.RemoveGuid(this.UniqueId);
            }
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
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
            if (version == AmanitaConstants.CurrentVersion)
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
                if (toUpdate != null)
                {
                    toUpdate.UpdateToVersion(version, AmanitaConstants.CurrentVersion);
                }
            }

            version = AmanitaConstants.CurrentVersion;
        }

        protected virtual void CheckItemIds()
        {
            // Make sure item ids are unique and monotonically increasing.
            // This should always be the case, but some legacy Flowcharts may have issues.
            List<ushort> usedIds = new List<ushort>();
            CheckForBlocks();
            void CheckForBlocks()
            {
                
                var blocks = GetComponents<Block>();
                for (ushort i = 0; i < blocks.Length; i++)
                {
                    var block = blocks[i];
                    if (block.ItemId == 0 || usedIds.Contains(block.ItemId))
                    {
                        block.ItemId = NextItemId();
                    }
                    usedIds.Add(block.ItemId);
                }
            }
            
            CheckForCommands();
            void CheckForCommands()
            {
                var commands = GetComponents<Command>();
                for (ushort i = 0; i < commands.Length; i++)
                {
                    var command = commands[i];
                    if (command.ItemId == 0 || usedIds.Contains(command.ItemId))
                    {
                        command.ItemId = NextItemId();
                    }
                    usedIds.Add(command.ItemId);
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

            // Due to how variables were directly added through the Variables list,
            // and how we don't want to change all other parts of the code base so they use
            // AddVariable... we're going with this workaround.
            EnsureVarsHaveValidIDs();
            void EnsureVarsHaveValidIDs()
            {
                IList<byte> usedIds = new List<byte>();
                for (int i = 0; i < Variables.Count; i++)
                {
                    var currentVar = Variables[i];
                    if (usedIds.Contains(currentVar.ItemId) || currentVar.ItemId == 0)
                    {
                        currentVar.ItemId = NextValidVarID();
                    }
                    usedIds.Add(currentVar.ItemId);
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
            legacyVariables.RemoveAll(item => item == null);

            var allVariables = GetComponents<Variable>();
            for (int i = 0; i < allVariables.Length; i++)
            {
                var variable = allVariables[i];
                if (!legacyVariables.Contains(variable))
                {
                    DestroyImmediate(variable);
                }
            }
            
            var blocks = GetComponents<Block>();
            var commands = GetComponents<Command>();
            for (int i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                bool found = false;
                for (int j = 0; j < blocks.Length; j++)
                {
                    var block = blocks[j];
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
            
            var eventHandlers = GetComponents<AmanitaEventHandler>();
            for (int i = 0; i < eventHandlers.Length; i++)
            {
                var eventHandler = eventHandlers[i];
                bool found = false;
                for (int j = 0; j < blocks.Length; j++)
                {
                    var block = blocks[j];
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

        public void UpdateSelectedCache()
        {
            SelectedBlocks.Clear();
            var res = gameObject.GetComponents<Block>();
            SelectedBlocks = res.Where(x => x.IsSelected).ToList();
        }

        public void ReverseUpdateSelectedCache()
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
            FlowchartSignals.BlockSelectionCleared(this, blocksToSignal);
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

        /// <summary>
        /// Set the block objects to be hidden or visible depending on the hideComponents property.
        /// </summary>
        public virtual void UpdateHideFlags()
        {
            if (hideComponents)
            {
                var blocks = _blocks;
                foreach (var block in blocks)
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
                var eventHandlers = GetComponents<AmanitaEventHandler>();
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

        public virtual IReadOnlyList<IVariable> Variables
        {
            get
            {
                IReadOnlyList<IVariable> copyOfList = legacyVariables.Cast<IVariable>()
                    .Concat(muscariables.Cast<IVariable>())
                    .ToList();

                return copyOfList;
            }
        }

        public virtual int VariableCount { get { return muscariables.Count; } }

        /// <summary>
        /// Description text displayed in the Flowchart editor window
        /// </summary>
        public virtual string Description { get { return description; } }

        /// <summary>
        /// Unique identifier for identifying this flowchart in localized string keys.
        /// </summary>
        public virtual string LocalizationId { get { return localizationId; } }

        /// <summary>
        /// Lua Environment to be used by default for all Execute Lua commands in this Flowchart.
        /// </summary>
        public virtual LuaEnvironment LuaEnv { get { return _luaEnvironment; } }

        /// <summary>
        /// The ExecuteLua command adds a global Lua variable with this name bound to the flowchart prior to executing.
        /// </summary>
        public virtual string LuaBindingName { get { return luaBindingName; } }

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
            // As for why we make Blocks and Commands get IDs from the same pool while vars get their own...
            // we want to give users the option to move commands between blocks without worrying about ID conflicts,
            // but variables added to a Flowchart are supposed to forever be with that same Flowchart.
            ushort maxId = 0;
            var blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                maxId = Math.Max(maxId, block.ItemId);
            }

            var commands = GetComponents<Command>();
            for (int i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                maxId = Math.Max(maxId, command.ItemId);
            }
            return (ushort)(maxId + 1);
        }

        #region Block-Handling

        /// <summary>
        /// Create a new block node which you can then add commands to.
        /// </summary>
        public virtual Block CreateBlock(Vector2 position, string blockName = null)
        {
            blockName ??= AmanitaConstants.DefaultBlockName;
            Block created = CreateBlockComponent(gameObject);
#if UNITY_EDITOR
            created._NodeRect = new Rect(position, defaultBlockSize);
#endif
            created.BlockName = GetUniqueBlockKey(blockName, created);
            created.ItemId = NextItemId();
            _blocks.Add(created);
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
            foreach (var block in _blocks)
            {
                if (block.BlockName == blockName)
                {
                    return block;
                }
            }

            return null;
        }

        public virtual Block FindBlockByItemId(int itemId)
        {
            Block result = null;

            foreach (var blockEl in _blocks)
            {
                if (blockEl.ItemId == itemId)
                {
                    result = blockEl;
                    break;
                }
            }

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
                Debug.LogError("Block " + blockName  + " does not exist");
                return;
            }

            if (!ExecuteBlock(block))
            {
                Debug.LogWarning("Block " + blockName  + " failed to execute");
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
                Debug.LogError("Block " + blockName  + " does not exist");
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
                baseKey = AmanitaConstants.DefaultBlockName;
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
            var ordered = new List<Variable>(legacyVariables.Count);
            var seen = new HashSet<Variable>();

            for (int i = 0; i < newOrder.Count; i++)
            {
                if (newOrder[i] is Variable legacy && legacyVariables.ContainsReference(legacy) && seen.Add(legacy))
                    ordered.Add(legacy);
            }

            // Append the rest (not explicitly positioned)
            for (int i = 0; i < legacyVariables.Count; i++)
            {
                var elem = legacyVariables[i];
                if (!seen.Contains(elem))
                {
                    ordered.Add(elem);
                }
            }
            if (ordered.Count == legacyVariables.Count)
            {
                legacyVariables = ordered;
            }
        }

        Muscariable IMuscariableSource.GetVariable(string name, StringComparison comparisonType = StringComparison.Ordinal)
        {
            Muscariable result = null;
            for (int i = 0; i < muscariables.Count; i++)
            {
                var currentVar = muscariables[i];
                if (currentVar.Key.Equals(name, comparisonType))
                {
                    result = currentVar;
                    break;
                }
            }
            return result;
        }

        public Muscariable AddNewVariableOfContentType(Type contentType, string key)
        {
            Muscariable muscaVar = VariableFactory.CreateByContentType(contentType, null);
            IntegrateMuscariable(muscaVar);
            return muscaVar;
        }

        IVariable IVariableSource.AddVariable(IVariable toAdd)
        {
            return AddVariable(toAdd.ToMuscariable());
        }

        /// <summary>
        /// Adds an already-existing Muscariable to the Flowchart, getting it integrated as something
        /// owned by said Flowchart. If the variable is already registered,
        /// it will not be added again.
        /// </summary>
        public Muscariable AddVariable(Muscariable toAdd)
        {
            Muscariable result = null;
            if (!muscariables.ContainsReference(toAdd))
            {
                IntegrateMuscariable(toAdd);
                result = toAdd;
            }
            return result;
        }

        Muscariable IVariableSource<Muscariable>.GetVar(int itemId)
        {
            return muscariables.Where((elem) => elem.ItemId == itemId).FirstOrDefault();
        }

        public virtual void SetVariable<TBase, TVarType>(string key, TBase value)
        where TVarType : VariableBase<TBase>
        {
            var variable = GetVariable<TVarType>(key);

            if (variable != null)
            {
                variable.Value = value;
            }
            else
            {
                LetUserKnowVarDoesntExist(key);
            }
        }


        /// <summary>
        /// Adds and registers a new var to the flowchart. If the passed key is null or empty,
        /// a unique key will be generated. If TVarType is a legacy Variable type, it will be converted
        /// into its Muscariable equivalent and the legacy variable will be destroyed.
        /// </summary>
        public virtual TVarType AddNewVariable<TValHeld, TVarType>(string key,
            TValHeld value = default,
            VariableScope scope = VariableScope.Private)
            where TVarType : class, IVariable<TValHeld>
        {
            TVarType newVar = null;
            bool wantMbType = typeof(MonoBehaviour).IsAssignableFrom(typeof(TVarType));
            if (wantMbType)
            {
                newVar = gameObject.AddComponent(typeof(TVarType)) as TVarType;
            }
            else
            {
                newVar = VariableFactory.Create(typeof(TValHeld)) as TVarType;
            }

            newVar.Key = UniqueKeyGenerator.GetUniqueKeyFor(key, (IList<IVariable>)Variables);
            newVar.Value = value;
            newVar.Scope = scope;
            newVar.ItemId = NextValidVarID();

            IVariable toRegister = newVar;
            bool createdLegacyVar = newVar is not Muscariable;
            if (createdLegacyVar)
            {
                // We want to minimize use of the legacy variables, so we convert to Muscariable on the fly
                // and then get rid of the legacy var.
                Debug.Log($"AddNewVariable: Added legacy variable of type {typeof(TVarType).Name}. Converting it to its" +
                    $" Muscariable equivalent. Returning null.");
                toRegister = newVar.ToMuscariable();
                AddVariable(toRegister);

                if (Application.IsPlaying(this))
                {
                    Destroy(newVar as MonoBehaviour);
                }
                else
                {
                    DestroyImmediate(newVar as MonoBehaviour);
                }
            }

            AddVariable(toRegister);
            VariableAdded(toRegister);

            if (createdLegacyVar)
                return null;
            else
                return newVar;
        }

        /// <summary>
        /// Adds an already-existing variable to the flowchart. If the variable is already registered,
        /// nothing happens. The variable's key and ID will be made unique if necessary.
        /// If the variable is a legacy Variable, a Muscariable version of it will
        /// be registered instead.
        /// </summary>
        public virtual void AddVariable(IVariable toAdd)
        {
            bool alreadyRegistered = legacyVariables.ContainsReference(toAdd) || muscariables.ContainsReference(toAdd);
            if (alreadyRegistered)
            {
                return;
            }

            toAdd = toAdd.ToMuscariable();
            Muscariable muscari = toAdd as Muscariable;
            AddVariable(muscari);
        }

        /// <summary>
        /// Returns the variable with the specified key, or null if the key is not found.
        /// You will need to cast the returned variable to the correct sub-type.
        /// You can then access the variable's value using the Value property. e.g.
        /// BooleanVariable boolVar = flowchart.GetVariable("MyBool") as BooleanVariable;
        /// boolVar.Value = false;
        /// </summary>
        public IVariable GetVariable(string key, StringComparison strCompare = StringComparison.Ordinal)
        {
            IVariable result = muscariables.Where(v => v.Key == key).FirstOrDefault();
            result ??= legacyVariables.Where(v => v.Key == key).FirstOrDefault();

            return result;
        }

        /// <summary>
        /// Alias for the GetVariable(string key) method.
        /// </summary>
        public IVariable GetVariableByName(string name)
        {
            return GetVariable(name);
        }

        public virtual IVariable GetVariableByIndex(int index)
        {
            IVariable result = null;
            if (muscariables.Count > index && index >= 0)
            {
                result = muscariables[index];
            }
            return result;
        }

        public virtual IVariable GetVariableById(int id)
        {
            return GetVariableById((byte)id);
        }

        public virtual IVariable GetVariableById(byte id)
        {
            IVariable result = (from varEl in muscariables
                                where varEl.ItemId == id
                                select varEl).FirstOrDefault();
            if (result == null)
            {
                Debug.LogWarning($"Variable with item ID {id} not found.");
            }

            return result;
        }

        /// <summary>
        /// Returns the variable with the specified key, or null if the key is not found.
        /// You can then access the variable's value using the Value property. e.g.
        /// BooleanVariable boolVar = flowchart.GetVariable<BooleanVariable>("MyBool");
        /// boolVar.Value = false;
        /// </summary>
        public T GetVariable<T>(string key) where T : class, IVariable
        {
            for (int i = 0; i < muscariables.Count; i++)
            {
                var variable = muscariables[i];
                if (variable != null && variable.Key == key)
                {
                    return variable as T;
                }
            }

            Debug.LogWarning("Variable " + key + " not found.");
            return null;
        }

        /// <summary>
        /// Returns a list of variables matching the specified type.
        /// </summary>
        public virtual IList<T> GetMultiVariables<T>() where T: class, IVariable
        {
            var varsFound = new List<T>();
            
            for (int i = 0; i < muscariables.Count; i++)
            {
                var currentVar = muscariables[i];
                if (currentVar is T)
                    varsFound.Add(currentVar as T);
            }

            return varsFound;
        }

        /// <summary>
        /// Checks if a given variable exists in the flowchart.
        /// </summary>
        public virtual bool HasVariable(string key)
        {
            for (int i = 0; i < muscariables.Count; i++)
            {
                var elem = muscariables[i];
                if (elem != null && elem.Key == key)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool HasVariable(IVariable varInst)
        {
            return legacyVariables.Contains(varInst) || muscariables.Contains(varInst);
        }

        /// <summary>
        /// Returns the list of variable names in the Flowchart.
        /// </summary>
        public virtual string[] GetVariableNames()
        {
            var vList = new string[legacyVariables.Count];

            for (int i = 0; i < legacyVariables.Count; i++)
            {
                var elem = legacyVariables[i];
                if (elem != null)
                {
                    vList[i] = elem.Key;
                }
            }
            return vList;
        }

        /// <summary>
        /// Gets a list of all variables with public scope in this Flowchart.
        /// </summary>
        public virtual IList<IVariable> GetPublicVariables()
        {
            IList<IVariable> publicVariables = new List<IVariable>();
            for (int i = 0; i < muscariables.Count; i++)
            {
                var elem = muscariables[i];
                if (elem != null && elem.Scope == VariableScope.Public)
                {
                    publicVariables.Add(elem);
                }
            }

            return publicVariables;
        }

        /// <summary>
        /// Creates and returns a new Muscariable of the specified type, with this
        /// as the parent Flowchart.
        /// </summary>
        public virtual TVarType AddNewMuscariable<TValueType, TVarType>(string key = "", TValueType initValue = default,
            VariableScope scope = VariableScope.Private) where TVarType : Muscariable<TValueType>, new()
        {
            TVarType result = new TVarType();
            result.Value = initValue;
            result.Scope = scope;
            result.Key = key;
            IntegrateMuscariable(result);
            return result;
        }

        /// <summary>
        /// Sets up the Muscariable to belong to this Flowchart before adding it.
        /// </summary>
        public virtual void IntegrateMuscariable(Muscariable toAdd)
        {
            bool hasValidId = toAdd.ItemId != Muscariable.InvalidID;
            bool shouldAssignNewId = !hasValidId || muscariables.Any(registered => registered.ItemId == toAdd.ItemId && hasValidId);
            if (shouldAssignNewId)
            {
                toAdd.ItemId = NextValidVarID();
            }

            toAdd.ParentFlowchart = this;
            toAdd.Owner = this;
            toAdd.Key = UniqueKeyGenerator.GetUniqueKeyFor(toAdd.Key, (IList<IVariable>)Variables, null);
            toAdd.Init();
            muscariables.Add(toAdd);
            varLookupById[toAdd.ItemId] = toAdd;
            VariableAdded(toAdd);
        }

        /// <summary>
        /// Unregisters the Muscariable from this Flowchart, setting it to have no parent FC.
        /// </summary>
        /// <param name="toRemove"></param>
        public virtual void RemoveVariable(Muscariable toRemove)
        {
            if (muscariables.Contains(toRemove))
            {
                toRemove.ParentFlowchart = null;
                muscariables.Remove(toRemove);
                VariableRemoved(toRemove);
            }

        }

        public virtual IList<TVarType> GetMuscariablesOfType<TVarType>() where TVarType : Muscariable
        {
            IList<TVarType> result = (from elem in muscariables
                                      where elem.GetType().IsAssignableFrom(typeof(TVarType))
                                      select elem).Cast<TVarType>().ToList();
            return result;
        }

        public virtual TVarType GetMuscariableWithKey<TVarType>(string key) where TVarType : Muscariable
        {
            TVarType result = (from elem in muscariables
                               where elem.Key == key
                               where elem is TVarType
                               select elem).FirstOrDefault() as TVarType;
            return result;

        }

        public virtual int MuscariableCount { get { return muscariables.Count; } }

        public virtual void RefreshVars()
        {
            muscariables = (from elem in muscariables
                            where elem != null
                            select elem).ToList();
            legacyVariables = (from elem in legacyVariables
                               where elem != null
                               select elem).ToList();
        }

        public event Action<IVariable> VariableAdded = delegate { };
        public event Action<IVariable> VariableRemoved = delegate { };

        public virtual void InsertVariable(int index, Variable whatToInsert)
        {
            legacyVariables.Insert(index, whatToInsert);
            VariableAdded(whatToInsert);
        }

        #endregion


        /// <summary>
        /// Reset the commands and variables in the Flowchart.
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
                for (int i = 0; i < legacyVariables.Count; i++)
                {
                    var variable = legacyVariables[i];
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
            if (stringSubstituter == null)
            {
                stringSubstituter = new StringSubstituter();
            }

            // Use the string builder from StringSubstituter for efficiency.
            StringBuilder sb = stringSubstituter._StringBuilder;
            sb.Length = 0;
            sb.Append(input);

            // Instantiate the regular expression object.
            Regex r = new Regex(SubstituteVariableRegexString);

            bool changed = false;

            // Match the regular expression pattern against a text string.
            var results = r.Matches(input);
            for (int i = 0; i < results.Count; i++)
            {
                Match match = results[i];
                string key = match.Value.Substring(2, match.Value.Length - 3);
                // Look for any matching private variables in this Flowchart first
                for (int j = 0; j < legacyVariables.Count; j++)
                {
                    var variable = legacyVariables[j];
                    if (variable == null)
                        continue;
                    if (variable.Scope == VariableScope.Private && variable.Key == key)
                    {
                        string value = variable.ToString();
                        sb.Replace(match.Value, value);
                        changed = true;
                    }
                }
            }

            // Now do all other substitutions in the scene
            changed |= stringSubstituter.SubstituteStrings(sb);

            if (changed)
            {
                return sb.ToString();
            }
            else
            {
                return input;
            }
        }

        public virtual void DetermineSubstituteVariables(string str, IList<IVariable> vars)
        {
            Regex r = new Regex(SubstituteVariableRegexString);

            // Match the regular expression pattern against a text string.
            var results = r.Matches(str);
            for (int i = 0; i < results.Count; i++)
            {
                var match = results[i];
                var v = GetVariable(match.Value.Substring(2, match.Value.Length - 3));
                if (v != null)
                {
                    vars.Add(v);
                }
            }
        }
#endregion

        #region IStringSubstituter implementation

        /// <summary>
        /// Implementation of StringSubstituter.ISubstitutionHandler which matches any public variable in the Flowchart.
        /// To perform full variable substitution with all substitution handlers in the scene, you should
        /// use the SubstituteVariables() method instead.
        /// </summary>
        [MoonSharp.Interpreter.MoonSharpHidden]
        public virtual bool SubstituteStrings(StringBuilder input)
        {
            // Instantiate the regular expression object.
            Regex r = new Regex(SubstituteVariableRegexString);

            bool modified = false;

            // Match the regular expression pattern against a text string.
            var results = r.Matches(input.ToString());
            for (int i = 0; i < results.Count; i++)
            {
                Match match = results[i];
                string key = match.Value.Substring(2, match.Value.Length - 3);
                // Look for any matching public variables in this Flowchart
                for (int j = 0; j < legacyVariables.Count; j++)
                {
                    var variable = legacyVariables[j];
                    if (variable == null)
                    {
                        continue;
                    }
                    if (variable.Scope == VariableScope.Public && variable.Key == key)
                    {
                        string value = variable.ToString();
                        input.Replace(match.Value, value);
                        modified = true;
                    }
                }
            }

            return modified;
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

        IReadOnlyList<Muscariable> IVariableSource<Muscariable>.Variables
        {
            get
            {
                return muscariables.ToList();
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

                AmanitaManager.EnsureExists();

                legacyVariables.RemoveAll((elem) => elem == null);
                muscariables.RemoveAll((elem) => elem == null);

                uiModel ??= new FlowchartUIModel();
                if (uiModel.Owner == null)
                {
                    uiModel.Owner = this.gameObject;
                }

                Refresh();

                EnsureBlocksHaveAValidSize();
                void EnsureBlocksHaveAValidSize()
                {
                    IList<Block> blocks = GetComponents<Block>();
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

        protected virtual void LetUserKnowVarDoesntExist(string varName)
        {
            string warningMessage = $"Variable named {varName} in Flowchart {this.name} is just like Santa Claus: it doesn't exist.";
            Debug.LogWarning(warningMessage);
        }


#if UNITY_EDITOR

        public static void ResetStaticsForTest()
        {
            eventSystemPresent = false;
        }


        public virtual void OnTearDown()
        {
            GuidRegistry fcReg = AmanitaManager.GetOrAddGuidRegistryFor<Flowchart>();
            fcReg.RemoveGuid(this.UniqueId);
        }

#endif

        public bool Contains(IVariable var)
        {
            return legacyVariables.Contains(var) || muscariables.Contains(var);
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
        }

#if UNITY_EDITOR
        public T AddCommand<T>(Block toAddTo) where T : Command
        {
            Undo.RecordObject(this, "Add Command");
            var added = this.gameObject.AddComponent<T>();
            EditorUtility.SetDirty(this);
            _commands.Add(added);
            toAddTo.CommandList.Add(added);
            return added;
        }

#endif

    }
}
