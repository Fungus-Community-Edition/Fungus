using UnityEngine;

namespace AtMycelia.Amanita.Lua
{
    /// <summary>
    /// Lua-specific settings that can be added to a Flowchart.
    /// </summary>
    public class FlowchartLuaSettings : MonoBehaviour
    {
        [Tooltip("Lua Environment to be used by default for all Execute Lua commands in this Flowchart")]
        [SerializeField] protected LuaEnvironment luaEnvironment;

        [Tooltip("The ExecuteLua command adds a global Lua variable with this name bound to the flowchart prior to executing.")]
        [SerializeField] protected string luaBindingName = "flowchart";

        public virtual LuaEnvironment LuaEnvironment
        {
            get { return luaEnvironment; }
            set { luaEnvironment = value; }
        }

        public virtual string LuaBindingName
        {
            get { return luaBindingName; }
            set { luaBindingName = value; }
        }
    }
}