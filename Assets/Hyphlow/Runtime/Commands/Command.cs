using AtMycelia.Amanita;
using AtMycelia.Hyphlow.Sys;
using AtMycelia.AmaniTween;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Base class for Commands. Commands can be added to Blocks to create an execution sequence.
    /// </summary>
    [ExecuteInEditMode]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public abstract class Command : MonoBehaviour, IVariableReference, IRefreshable, IOnPreCutHandler,
        ISerializationCallbackReceiver, IBackwardsCompatibilityApplier
    {
        [FormerlySerializedAs("commandId")]
        [HideInInspector]
        [SerializeField] protected ushort itemId = 0;

        [HideInInspector]
        [SerializeField] protected int indentLevel;

        protected string errorMessage = "";

        /// <summary>
        /// This is for Commands that have too much polymorphic state for Unity's serializedProperty system to 
        /// copy over normally. Base implementation returns false. If overridden to return true, the editor will 
        /// use something else (maybe json) to make sure that the pasted copies of this Command type have the 
        /// state they should. This allows for correct copying of complex polymorphic data, at the cost of 
        /// maybe some performance and losing reference copying (i.e. if two fields reference the same object, 
        /// after pasting, they will reference two different but identical objects).
        /// </summary>
        public virtual bool NonStandardPaste => false;

        /// <summary>
        /// Whether or not instances of this Command should have their execution states saved and loaded
        /// by a save system.
        /// </summary>
        public virtual bool ReexecutableOnLoad => true;

        protected virtual void OnEnable()
        {
            RefreshForVarDataStability();
            ApplyBackwardsCompatibility();
        }

        /// <summary>
        /// For refreshing the Command's state so that things like var datas are stable
        /// and won't lose data or references when copying/pasting or doing other editor operations.
        /// </summary>
        public virtual void Refresh()
        {
            RefreshForVarDataStability();
        }

        public virtual void OnPreCut()
        {
            Refresh();
        }

        private void RefreshForVarDataStability()
        {
            bool thisIsInAScene = this.gameObject.scene.IsValid();
            if (!thisIsInAScene)
            {
                return;
            }
            EnsureVariableDataInstances();
            RefreshVariableDataCache();
            AssertOwnership();
            RefreshVariableDatas();
        }

        private void EnsureVariableDataInstances()
        {
            if (Application.isPlaying)
            {
                return;
            }
#if UNITY_EDITOR
            // We only want to do this in the editor, since at runtime, we expect the
            // VariableDatas to already be populated and don't want to risk overwriting any data.
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] fields = GetType().GetFields(flags);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                System.Type fieldType = field.FieldType;
                if (!typeof(IVariableData).IsAssignableFrom(fieldType))
                {
                    continue;
                }

                if (fieldType.IsAbstract)
                {
                    continue;
                }

                if (field.GetValue(this) != null)
                {
                    continue;
                }

                object created = Activator.CreateInstance(fieldType);
                if (created != null)
                {
                    field.SetValue(this, created);
                }
            }
#endif
        }

        /// <summary>
        /// Helps keep VariableDatas stable during the editor and runtime.
        /// </summary>
        protected virtual void RefreshVariableDataCache()
        {
            // We expect child classes to add their VariableDatas to this list
            _variableDataCache ??= new List<IVariableData>(); // In case it was null during a unit test or something
            _variableDataCache.Clear();
        }

        protected IList<IVariableData> _variableDataCache = new List<IVariableData>();

        protected virtual void RefreshVariableDatas()
        {
            for (int i = 0; i < _variableDataCache.Count; i++)
            {
                var refreshable = _variableDataCache[i] as IRefreshable;
                refreshable?.Refresh();
            }
#if UNITY_EDITOR
            if (_variableDataCache.Count > 0)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
        protected virtual void AssertOwnership()
        {
            Flowchart fChart = GetFlowchart();
            for (int i = 0; i < _variableDataCache.Count; i++)
            {
                var currentVarData = _variableDataCache[i];

                // We only want to assert ownership if there is no owner already set.
                // We want to allow the variable datas to have other owners so
                // that we can have them refer said other owners' vars if needed.
                if (currentVarData.VarRef != null)
                {
                    currentVarData.VarOwner ??= currentVarData.VarRef.Owner;
                    // ^For when the var assigned belongs to another Flowchart or something.
                }
                currentVarData.VarOwner ??= fChart;
            }
        }

        #region Editor caches
#if UNITY_EDITOR
        //
        protected IList<IVariable> referencedVariables = new List<IVariable>();

        //used by var list adapter to highlight variables 
        public bool IsVariableReferenced(IVariable variable)
        {
            return referencedVariables.Contains(variable) || HasReference(variable);
        }

        /// <summary>
        /// Called by OnValidate
        /// 
        /// Child classes to specialise to add variable references to referencedVariables, either directly or
        /// via the use of Flowchart.DetermineSubstituteVariables
        /// </summary>
        protected virtual void RefreshVariableCache()
        {
            // Not sure why, but sometimes, this gets set to null
            referencedVariables?.Clear();
        }

#endif
        #endregion Editor caches


        /// <summary>
        /// Unique identifier for this command.
        /// Unique for this Flowchart.
        /// </summary>
        public virtual ushort ItemId { get { return itemId; } set { itemId = value; } }

        /// <summary>
        /// Error message to display in the command inspector.
        /// </summary>
        public virtual string ErrorMessage { get { return errorMessage; } }

        /// <summary>
        /// Indent depth of the current commands.
        /// Commands are indented inside If, While, etc. sections.
        /// </summary>
        public virtual int IndentLevel { get { return indentLevel; } set { indentLevel = value; } }

        /// <summary>
        /// Index of the command in the parent block's command list.
        /// </summary>
        public virtual byte CommandIndex { get; set; }

        /// <summary>
        /// Set to true by the parent block while the command is executing.
        /// </summary>
        public virtual bool IsExecuting { get; set; }

        /// <summary>
        /// Timer used to control appearance of executing icon in inspector.
        /// </summary>
        public virtual float ExecutingIconTimer { get; set; }

        /// <summary>
        /// Reference to the Block object that this command belongs to.
        /// This reference is only populated at runtime and in the editor when the 
        /// block is selected.
        /// </summary>
        public virtual Block ParentBlock { get; set; }

        /// <summary>
        /// Returns the Flowchart that this command belongs to.
        /// </summary>
        public virtual Flowchart GetFlowchart()
        {
            var flowchart = GetComponent<Flowchart>();
            if (flowchart == null &&
                transform.parent != null)
            {
                flowchart = transform.parent.GetComponent<Flowchart>();
            }
            return flowchart;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        public virtual void Execute()
        {
            OnEnter();
        }

        /// <summary>
        /// End execution of this command and continue execution at the next command.
        /// </summary>
        public virtual void Continue()
        {
            // This is a noop if the Block has already been stopped
            if (IsExecuting)
            {
                Continue(CommandIndex + 1);
            }
        }

        public Action<Command> StartedContinue = delegate { };

        /// <summary>
        /// End execution of this command and continue execution at a specific command index.
        /// </summary>
        /// <param name="nextCommandIndex">Next command index.</param>
        public virtual void Continue(int nextCommandIndex)
        {
            OnExit();
            if (ParentBlock != null)
            {
                ParentBlock.JumpToCommandIndex = nextCommandIndex;
            }
            StartedContinue(this);
        }

        /// <summary>
        /// Stops the parent Block executing.
        /// </summary>
        public virtual void StopParentBlock()
        {
            OnExit();
            if (ParentBlock != null)
            {
                ParentBlock.Stop();
            }
        }

        /// <summary>
        /// Called when the parent block has been requested to stop executing, and
        /// this command is the currently executing command.
        /// Use this callback to terminate any asynchronous operations and 
        /// cleanup state so that the command is ready to execute again later on.
        /// </summary>
        public virtual void OnStopExecuting()
        {}

        /// <summary>
        /// Called when the new command is added to a block in the editor.
        /// </summary>
        public virtual void OnCommandAdded(Block parentBlock)
        {}

        /// <summary>
        /// Called when the command is deleted from a block in the editor.
        /// </summary>
        public virtual void OnCommandRemoved(Block parentBlock)
        {}

        /// <summary>
        /// Called when this command starts execution.
        /// </summary>
        public virtual void OnEnter()
        {
            Entered(this);
        }

        public Action<Command> Entered = delegate { };

        /// <summary>
        /// Called when this command ends execution.
        /// </summary>
        public virtual void OnExit()
        {
            Exited(this);
        }

        public Action<Command> Exited = delegate { };

        /// <summary>
        /// Called when this command is reset. This happens when the Reset command is used.
        /// </summary>
        public virtual void OnReset()
        {}

        /// <summary>
        /// Populates a list with the Blocks that this command references.
        /// </summary>
        public virtual void GetConnectedBlocks(ref List<Block> connectedBlocks)
        {}

        /// <summary>
        /// Returns true if this command references the variable.
        /// Used to highlight variables in the variable list when a command is selected.
        /// </summary>
        public virtual bool HasReference(Variable variable)
        {
            return false;
        }

        public virtual string GetLocationIdentifier()
        {
            if (ParentBlock == null)
            {
                return "";
            }
            string fcName = ParentBlock.GetFlowchart().name;
            string thisTypeName = this.GetType().Name;
            string indexStr = CommandIndex.ToString();

            return fcName + ":" + ParentBlock.BlockName + "." + thisTypeName + "#" + indexStr; 
        }

        /// <summary>
        /// Called by unity when script is loaded or its data changed by editor. Yes, this includes
        /// stuff like serializedObject.ApplyModifiedProperties(), which is why this func can get called
        /// before an editor func is done executing.
        /// </summary>
        protected virtual void OnValidate()
        {
            RefreshForVarDataStability();
            RefreshVariableCache();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                DelayedOnValidate();
            };
#endif
        }

        /// <summary>
        /// Returns the summary text to display in the command inspector.
        /// </summary>
        public virtual string GetSummary()
        {
            return "";
        }

        /// <summary>
        /// Returns the searchable content for searches on the flowchart window.
        /// </summary>
        public virtual string GetSearchableContent()
        {
            return GetSummary();
        }

        /// <summary>
        /// Returns the help text to display for this command.
        /// </summary>
        public virtual string GetHelpText()
        {
            return "";
        }

        /// <summary>
        /// Return true if this command opens a block of commands. Used for indenting commands.
        /// </summary>
        public virtual bool OpenBlock()
        {
            return false;
        }

        /// <summary>
        /// Return true if this command closes a block of commands. Used for indenting commands.
        /// </summary>
        public virtual bool CloseBlock()
        {
            return false;
        }

        /// <summary>
        /// Return the color for the command background in inspector.
        /// </summary>
        /// <returns>The button color.</returns>
        public virtual Color GetButtonColor()
        {
            return Color.white;
        }

        /// <summary>
        /// Returns true if the specified property should be displayed in the inspector. 
        /// This is useful for hiding certain properties based on the value of another property.
        /// </summary>
        public virtual bool IsPropertyVisible(string propertyName)
        {
            return true;
        }

        /// <summary>
        /// Returns true if the specified property should be displayed as a reorderable list in the inspector.
        /// This only applies for array properties and has no effect for non-array properties.
        /// </summary>
        public virtual bool IsReorderableArray(string propertyName)
        {
            return false;
        }

        public bool HasReference(IVariable variable)
        {
            return false;
        }

        protected virtual IEnumerator WaitForTask(Task task, bool callContinueAfterwards = true)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            yield return null; // Just one more frame to ensure any follow-up actions are ready.
            if (callContinueAfterwards)
            {
                Continue();
            }
        }

        public virtual void OnBeforeSerialize()
        {
        }

        public virtual void OnAfterDeserialize()
        {
        }

        /// <summary>
        /// Legacy var ids were allowed to be 0 (which is now considered an invalid value),
        /// so we'll need to check for that and fix it if we see it. This is only needed for 
        /// Commands that reference variables using legacy var ids, and should be called in 
        /// OnAfterDeserializeBackwardsCompatibility. Command subclasses should
        /// override this as needed.
        /// </summary>
        protected virtual void EnsureLegacyVarIdsAreValid()
        {

        }

        public virtual void ApplyBackwardsCompatibility()
        {
            EnsureLegacyVarIdsAreValid();
        }

        /// <summary>
        /// Override this for OnValidate code that might need to do stuff like access GameObjects
        /// </summary>
        protected virtual void DelayedOnValidate()
        {
            
        }

        protected virtual DefaultTweenAdapter DefaultTweener => HyphlowRuntimeSysAssets.S.TweenAdapter;


    }
}