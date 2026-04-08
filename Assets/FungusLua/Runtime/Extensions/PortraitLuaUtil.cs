    using MoonSharp.Interpreter;

namespace AtMycelia.Amanita.DialogueSys
{
    public static class PortraitLuaUtil
    {
        /// <summary>
        /// Convert a Moonsharp table to portrait options.
        /// If the table returns a null for any of the parameters, it should keep the defaults.
        /// </summary>
        public static PortraitOptions ConvertTableToPortraitOptions(Table table, Stage stage)
        {
            PortraitOptions options = new PortraitOptions(true);

            // If the table supplies a nil, keep the default
            options.character = table.Get("character").ToObject<Character>() 
                ?? options.character;

            options.replacedCharacter = table.Get("replacedCharacter").ToObject<Character>()
                ?? options.replacedCharacter;

            if (!table.Get("portrait").IsNil())
            {
                options.portrait = options.character.GetPortrait(table.Get("portrait").CastToString());
            }

            if (!table.Get("display").IsNil())
            {
                options.display = table.Get("display").ToObject<DisplayType>();
            }

            if (!table.Get("offset").IsNil())
            {
                options.offset = table.Get("offset").ToObject<PositionOffset>();
            }

            if (!table.Get("fromPosition").IsNil())
            {
                options.fromPosition = stage.GetPosition(table.Get("fromPosition").CastToString());
            }

            if (!table.Get("toPosition").IsNil())
            {
                options.toPosition = stage.GetPosition(table.Get("toPosition").CastToString());
            }

            if (!table.Get("facing").IsNil())
            {
                var facingDirection = FacingDirection.None;
                DynValue v = table.Get("facing");
                if (v.Type == DataType.String)
                {
                    if (string.Compare(v.String, "left", true) == 0)
                    {
                        facingDirection = FacingDirection.Left;
                    }
                    else if (string.Compare(v.String, "right", true) == 0)
                    {
                        facingDirection = FacingDirection.Right;
                    }
                }
                else
                {
                    facingDirection = table.Get("facing").ToObject<FacingDirection>();
                }

                options.facing = facingDirection;
            }

            if (!table.Get("useDefaultSettings").IsNil())
            {
                options.useDefaultSettings = table.Get("useDefaultSettings").CastToBool();
            }

            if (!table.Get("fadeDuration").IsNil())
            {
                options.fadeDuration = table.Get("fadeDuration").ToObject<float>();
            }

            if (!table.Get("moveDuration").IsNil())
            {
                options.moveDuration = table.Get("moveDuration").ToObject<float>();
            }

            if (!table.Get("move").IsNil())
            {
                options.move = table.Get("move").CastToBool();
            }
            else if (options.fromPosition != options.toPosition)
            {
                options.move = true;
            }

            if (!table.Get("shiftIntoPlace").IsNil())
            {
                options.shiftIntoPlace = table.Get("shiftIntoPlace").CastToBool();
            }

            if (!table.Get("waitUntilFinished").IsNil())
            {
                options.waitUntilFinished = table.Get("waitUntilFinished").CastToBool();
            }

            return options;
        }
    }
}