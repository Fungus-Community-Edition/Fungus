using MoonSharp.Interpreter;
using UnityEngine;

namespace AtMycelia.Amanita.DialogueSys
{
    public static class PortraitControllerLuaExtensions
    {
        private static Stage ResolveStage(PortraitController controller)
        {
            Stage stageComponent = controller.GetComponentInParent<Stage>();
            if (stageComponent == null)
            {
                Debug.LogError("PortraitController Lua extensions require a Stage in the parent hierarchy.");
            }

            return stageComponent;
        }

        /// <summary>
        /// From lua, you can pass an options table with named arguments
        /// example:
        ///     stage.show{character=jill, portrait="happy", fromPosition="right", toPosition="left"}
        /// Any option available in the PortraitOptions is available from Lua
        /// </summary>
        public static void Show(this PortraitController controller, Table optionsTable)
        {
            Stage stageComponent = ResolveStage(controller);
            if (stageComponent == null)
            {
                return;
            }

            var pOptions = PortraitLuaUtil.ConvertTableToPortraitOptions(optionsTable, stageComponent);
            controller.Show(pOptions);
        }

        /// <summary>
        /// From lua, you can pass an options table with named arguments
        /// example:
        ///     stage.hide{character=jill, toPosition="left"}
        /// Any option available in the PortraitOptions is available from Lua
        /// </summary>
        public static void Hide(this PortraitController controller, Table optionsTable)
        {
            Stage stageComponent = ResolveStage(controller);
            if (stageComponent == null)
            {
                return;
            }

            var pOptions = PortraitLuaUtil.ConvertTableToPortraitOptions(optionsTable, stageComponent);
            controller.Hide(pOptions);
        }
    }
}