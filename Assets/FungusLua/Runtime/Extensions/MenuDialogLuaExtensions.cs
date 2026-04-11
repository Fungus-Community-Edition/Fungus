using System;
using System.Collections;
using MoonSharp.Interpreter;
using AtMycelia.Amanita.DialogueSys;

namespace AtMycelia.Amanita.Lua
{
    public static class MenuDialogLuaExtensions
    {
        public static bool AddOption(this MenuDialog menuDialog, string text, bool interactable, 
            LuaEnvironment luaEnv, Closure callBack)
        {
            if (menuDialog == null)
            {
                return false;
            }

            Action callback = null;
            if (luaEnv != null && callBack != null)
            {
                callback = () => luaEnv.RunLuaFunction(callBack, true);
            }

            return menuDialog.AddOption(text, interactable, false, callback);
        }

        public static IEnumerator ShowTimer(this MenuDialog menuDialog, float duration, LuaEnvironment luaEnv, Closure callBack)
        {
            if (menuDialog == null)
            {
                yield break;
            }

            Action callback = null;
            if (luaEnv != null && callBack != null)
            {
                callback = () => luaEnv.RunLuaFunction(callBack, true);
            }

            yield return menuDialog.ShowTimer(duration, callback);
        }
    }
}