using AtMycelia.Hyphlow;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Amanita.DialogueSys.VScripting
{
    /// <summary>
    /// Do multiple say and portrait commands in a single block of text. Format is: [character] [portrait] [stage position] [hide] [<<< | >>>] [clear | noclear] [wait | nowait] [fade | nofade] [: Story text].
    /// </summary>
    [CommandInfo("Narrative", 
                 "Conversation",
                 "Do multiple say and portrait commands in a single block of text. Format is: [character] [portrait] [stage position] [hide] [<<< | >>>] [clear | noclear] [wait | nowait] [fade | nofade] [: Story text]")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [MovedFrom("AtMycelia.Amanita.DialogueSys.Commands")]
    public class Conversation : Command
    {
        [HyphlowTextArea(5, 10)]
        [SerializeField] protected StringDataMulti conversationText;

        protected ConversationManager conversationManager = new ConversationManager();

        [SerializeField] protected BooleanData clearPrevious = new BooleanData(true);
        [SerializeField] protected BooleanData waitForInput = new BooleanData(true);
        [Tooltip("a wait for seconds added to each item of the conversation.")]
        [SerializeField] protected FloatData waitForSeconds = new FloatData(0);
        [SerializeField] protected BooleanData fadeWhenDone = new BooleanData(true);

        protected virtual void Start()
        {
            conversationManager.PopulateCharacterCache();
        }

        protected virtual IEnumerator DoConversation()
        {
            var flowchart = GetFlowchart(); 
            string subbedText = flowchart.SubstituteVariables(conversationText.Value);

            conversationManager.ClearPrev = clearPrevious;
            conversationManager.WaitForInput = waitForInput;
            conversationManager.FadeDone = fadeWhenDone;
            conversationManager.WaitForSeconds = waitForSeconds;

            yield return StartCoroutine(conversationManager.DoConversation(subbedText));

            Continue();
        }

        #region Public members

        public override void OnEnter()
        {
            StartCoroutine(DoConversation());
        }

        public override string GetSummary()
        {
            return conversationText.Value;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Narrative;
        }

        public override bool HasReference(IVariable variable)
        {
            return ReferenceEquals(clearPrevious.VarRef, variable) || 
                ReferenceEquals(waitForInput.VarRef, variable) || 
                ReferenceEquals(waitForSeconds.VarRef, variable) || 
                ReferenceEquals(fadeWhenDone.VarRef, variable) ||
                base.HasReference(variable);
        }

        #endregion


        #region Editor caches
#if UNITY_EDITOR
        protected override void RefreshVariableCache()
        {
            base.RefreshVariableCache();

            
        }

#endif
        #endregion Editor caches

    }
}
