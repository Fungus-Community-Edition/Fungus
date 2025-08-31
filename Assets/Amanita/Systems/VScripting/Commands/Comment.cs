using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Use comments to record design notes and reminders about your game.
    /// </summary>
    [CommandInfo("", 
                 "Comment", 
                 "Use comments to record design notes and reminders about your game.")]
    [AddComponentMenu("")]
    public class Comment : Command
    {   
        [Tooltip("Name of Commenter")]
        [SerializeField] protected string commenterName = "";

        [Tooltip("Text to display for this comment")]
        [TextArea(2,4)]
        [SerializeField] protected string commentText = "";

        #region Public members

        public override void OnEnter()
        {
            Continue();
        }

        public override string GetSummary()
        {
            if (commenterName != "")
            {
                return commenterName + ": " + commentText;
            }
            return commentText;
        }

        public override Color GetButtonColor()
        {
            return new Color32(220, 220, 220, 255);
        }

        #endregion
    }
}
