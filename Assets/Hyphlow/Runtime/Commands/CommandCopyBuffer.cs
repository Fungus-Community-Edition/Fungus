using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Temporary buffer object used when copying and pasting commands.
    /// </summary>
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class CommandCopyBuffer : Block 
    {
        protected static CommandCopyBuffer instance;

        protected virtual void Start()
        {
            if (Application.isPlaying)
            {
                Destroy(this.gameObject);
            }
        }

        #region Public members

        /// <summary>
        /// Returns the CommandCopyBuffer singleton instance.
        /// Will create a CommandCopyBuffer game object if none currently exists.
        /// </summary>
        public static CommandCopyBuffer GetInstance()
        {
            if (instance == null)
            {
                // Static variables are not serialized (e.g. when playing in the editor)
                // We need to reaquire the static reference to the game object in this case
                GameObject go = GameObject.Find("_CommandCopyBuffer");
                if (go == null)
                {
                    go = new GameObject("_CommandCopyBuffer");
                    go.hideFlags = HideFlags.HideAndDontSave;
                }

                instance = go.GetComponent<CommandCopyBuffer>();
                if (instance == null)
                {
                    instance = go.AddComponent<CommandCopyBuffer>();
                }
            }

            return instance;
        }

        public virtual bool HasCommands()
        {
            return GetCommands().Length > 0;
        }

        public virtual Command[] GetCommands()
        {
            return GetComponents<Command>();
        }

        public virtual void Clear()
        {
            var commands = GetCommands();
            for (int i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                DestroyImmediate(command);
            }
        }

        #endregion
    }
}