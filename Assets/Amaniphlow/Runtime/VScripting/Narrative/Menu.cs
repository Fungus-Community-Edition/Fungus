using AtMycelia.Hyphlow;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Amanita.DialogueSys.VScripting
{
    /// <summary>
    /// Displays a button in a multiple choice menu.
    /// </summary>
    [CommandInfo("Narrative", 
                 "Menu", 
                 "Displays a button in a multiple choice menu")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.DialogueSys.Commands")]
    public class Menu : Command, ILocalizable, IBlockCaller
    {
        [Tooltip("Text to display on the menu button")]
        [SerializeField] protected StringData _text = new StringData("Option Text");

        [Tooltip("Notes about the option text for other authors, localization, etc.")]
        [HyphlowTextArea()]
        [SerializeField] protected StringData _description = new StringData("");

        [Tooltip("Block to execute when this option is selected")]
        [SerializeField] protected BlockReference _targetBlock = new BlockReference();

        [Tooltip("Hide this option if the target block has been executed previously")]
        [SerializeField] protected BooleanData _hideIfVisited = new BooleanData(false);

        [Tooltip("If false, the menu option will be displayed but will not be selectable")]
        [FormerlySerializedAs("interactable")]
        [SerializeField] protected BooleanData _interactable = new BooleanData(true);

        [Tooltip("A custom Menu Dialog to use to display this menu. All subsequent " +
            "Menu commands will use this dialog.")]
        [FormerlySerializedAs("setMenuDialog")]
        [SerializeField] protected MenuDialog _setMenuDialog;

        [Tooltip("If true, this option will be passed to the Menu Dialogue but marked as hidden, " +
            "this can be used to hide options while maintaining a Menu Shuffle.")]
        [FormerlySerializedAs("hideThisOption")]
        [SerializeField] protected BooleanData _hideThisOption = new BooleanData(false);

        #region Public members

        public MenuDialog SetMenuDialog
        {
            get { return _setMenuDialog; }
            set { _setMenuDialog = value; }
        }

        public override void OnEnter()
        {
            if (_setMenuDialog != null)
            {
                // Override the active menu dialog
                MenuDialog.ActiveMenuDialog = _setMenuDialog;
            }

            var targBlock = _targetBlock.Block;
            bool hideOption = (_hideIfVisited && targBlock != null && 
                targBlock.ExecutionCount > 0) || _hideThisOption.Value;

            var menuDialog = MenuDialog.GetMenuDialog();
            if (menuDialog != null)
            {
                menuDialog.SetActive(true);

                var flowchart = ParentBlock.ParentFlowchart;
                var subber = StringVarSubstitutionService.Shared;

                string displayText = subber.SubstituteVariables(_text, flowchart);

                menuDialog.AddOption(displayText, _interactable, hideOption, 
                    () => flowchart.ExecuteBlock(_targetBlock.Block));
            }
            
            Continue();
        }

        public override void GetConnectedBlocks(ref IList<IBlock> connectedBlocks)
        {
            if (_targetBlock != null && _targetBlock.Block != null)
            {
                connectedBlocks.Add(_targetBlock.Block);
            }       
        }

        public override string GetSummary()
        {
            if (_targetBlock.Block == null)
            {
                return "Error: No target block selected";
            }

            if (string.IsNullOrEmpty(_text.Value))
            {
                return "Error: No button text selected";
            }

            string result = $"{_text.Value} : {_targetBlock.Block.BlockName}";
            return result;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Narrative;
        }

        public override bool HasReference(IVariable variable)
        {
            bool result = ReferenceEquals(_interactable.VarRef, variable) || 
                ReferenceEquals(_hideThisOption.VarRef, variable) ||
                base.HasReference(variable);
            return result;
        }

        public bool MayCallBlock(IBlock block)
        {
            bool result = ReferenceEquals(block, _targetBlock.Block);
            return result;
        }

        #endregion

        #region ILocalizable implementation

        public virtual string GetStandardText()
        {
            return _text;
        }

        public virtual void SetStandardText(string standardText)
        {
            _text.Value = standardText;
        }
        
        public virtual string GetDescription()
        {
            return _description;
        }
        
        public virtual string GetStringId()
        {
            // String id for Menu commands is MENU.<Localization Id>.<Command id>
            string result = $"MENU.{_itemId}";
            return result;
        }

        #endregion

        #region Editor caches
#if UNITY_EDITOR
        protected override void RefreshVariableCache()
        {
            base.RefreshVariableCache();

            if (ParentBlock == null)
            {
                // This might be getting called before this Command's ownership
                // was cemented after assembly reload
                return;
            }
            var fc = ParentBlock.ParentFlowchart;
            if (fc == null)
            {
                return;
            }
            var subber = StringVarSubstitutionService.Shared;
            subber.DetermineSubstitutionVariables(_text, fc, _referencedVariables);
        }

#endif

        protected override void DelayedOnValidate()
        {
            base.DelayedOnValidate();
            if (!string.IsNullOrEmpty(_descriptionOld))
            {
                _description.Value = _descriptionOld;
                _descriptionOld = "";
            }

            if (!string.IsNullOrEmpty(_textOld))
            {
                _text.Value = _textOld;
                _textOld = "";
            }

            if (_targetBlockOld != null)
            {
                _targetBlock.BlockOwner = _targetBlockOld.Owner as IBlockSource;
                _targetBlock.Block = _targetBlockOld;
                _targetBlockOld = null;
            }

            if (!_hideIfVisitedOld)
            {
                _hideIfVisited.Value = _hideIfVisitedOld;
                _hideIfVisitedOld = false;
            }
        }

        [FormerlySerializedAs("description")]
        [SerializeField] protected string _descriptionOld = "";

        [FormerlySerializedAs("text")]
        [SerializeField] protected string _textOld = "Option Text";

        [FormerlySerializedAs("targetBlock")]
        [SerializeField] protected Block _targetBlockOld;

        [FormerlySerializedAs("hideIfVisited")]
        [SerializeField] protected bool _hideIfVisitedOld;

        #endregion Editor caches
    }
}
