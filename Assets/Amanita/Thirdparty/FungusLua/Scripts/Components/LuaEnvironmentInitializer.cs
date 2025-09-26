using UnityEngine;

namespace Amanita.Lua
{
    /// <summary>
    /// Helper class used to extend the initialization behavior of LuaEnvironment.
    /// </summary>
    public abstract class LuaEnvironmentInitializer : MonoBehaviour
    {
        #region Public members

        /// <summary>
        /// Called when the LuaEnvironment is initializing.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Applies transformations to the input script prior to execution.
        /// </summary>
        public abstract string PreprocessScript(string input);

        #endregion
    }
}