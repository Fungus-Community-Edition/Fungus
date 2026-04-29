using System.Collections.Generic;
using System.Reflection;
using AtMycelia.Amanita.VScripting;
using AtMycelia.AmaniTween;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public static class FadeSpriteToFadeColorReplacer
    {
        private const string MenuPath = "Hyphlow/Flowchart/Replace Fade Sprite With Fade Color";
        private const string ContextMenuPath = "CONTEXT/Flowchart/Replace Fade Sprite With Fade Color";

        [MenuItem(MenuPath)]
        public static void ReplaceFadeSpriteWithFadeColor()
        {
            Flowchart flowchart = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<Flowchart>()
                : null;

            if (flowchart == null)
            {
                Debug.LogWarning("Select a Flowchart GameObject to replace Fade Sprite commands.");
                return;
            }

            ReplaceOnFlowchart(flowchart);
        }

        [MenuItem(ContextMenuPath)]
        private static void ReplaceFadeSpriteWithFadeColorFromContext(MenuCommand menuCommand)
        {
            if (menuCommand == null || menuCommand.context is not Flowchart flowchart)
            {
                Debug.LogWarning("Right-click a Flowchart component to replace Fade Sprite commands.");
                return;
            }

            ReplaceOnFlowchart(flowchart);
        }

        private static void ReplaceOnFlowchart(Flowchart flowchart)
        {
            VariableManagerComponent varManager = flowchart.GetComponent<VariableManagerComponent>();
            Block[] blocks = flowchart.GetComponents<Block>();

            Undo.SetCurrentGroupName("Replace Fade Sprite Commands");
            int undoGroup = Undo.GetCurrentGroup();

            Undo.RecordObject(flowchart, "Replace Fade Sprite Commands");
            if (varManager != null)
            {
                Undo.RecordObject(varManager, "Replace Fade Sprite Commands");
            }

            int replacedCount = 0;

            for (int b = 0; b < blocks.Length; b++)
            {
                Block block = blocks[b];
                if (block == null)
                {
                    continue;
                }

                Undo.RecordObject(block, "Replace Fade Sprite Commands");

                List<Command> commands = block.CommandList;
                for (int i = 0; i < commands.Count; i++)
                {
                    FadeSprite fadeSprite = commands[i] as FadeSprite;
                    if (fadeSprite == null)
                    {
                        continue;
                    }

                    FadeColor fadeColor = Undo.AddComponent<FadeColor>(flowchart.gameObject);

                    InitializeFadeColor(flowchart, fadeSprite, fadeColor);

                    fadeColor.ItemId = fadeSprite.ItemId;
                    fadeColor.IndentLevel = fadeSprite.IndentLevel;
                    fadeColor.ParentBlock = block;

                    commands[i] = fadeColor;

                    ReplaceInFlowchartCache(flowchart, fadeSprite, fadeColor);

                    fadeSprite.OnCommandRemoved(block);
                    fadeColor.OnCommandAdded(block);

                    Undo.DestroyObjectImmediate(fadeSprite);

                    replacedCount++;
                }
            }

            if (replacedCount > 0)
            {
                EditorUtility.SetDirty(flowchart);
                if (varManager != null)
                {
                    EditorUtility.SetDirty(varManager);
                }
            }

            flowchart.Refresh();
            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log($"Replaced {replacedCount} Fade Sprite command(s) on {flowchart.name}.");
        }

        private static void InitializeFadeColor(Flowchart flowchart, FadeSprite fadeSprite, FadeColor fadeColor)
        {
            SpriteRenderer spriteRenderer = GetFieldValue<SpriteRenderer>(fadeSprite, "_spriteRenderer");
            FloatData duration = GetFieldValue<FloatData>(fadeSprite, "_duration");
            ColorData targetColor = GetFieldValue<ColorData>(fadeSprite, "_targetColor");
            BooleanData waitUntilFinished = GetFieldValue<BooleanData>(fadeSprite, "_waitUntilFinished");
            ScriptableObject fadeTweener = GetFieldValue<ScriptableObject>(fadeSprite, "_fadeTweener");

            VariableReference targetRef = GetOrCreateTargetReference(flowchart, spriteRenderer);
            SetFieldValue(fadeColor, "_target", targetRef);

            FloatData fadeColorDuration = GetFieldValue<FloatData>(fadeColor, "_duration");
            if (fadeColorDuration == null)
            {
                fadeColorDuration = new FloatData();
                SetFieldValue(fadeColor, "_duration", fadeColorDuration);
            }

            if (duration != null)
            {
                fadeColorDuration.SetContentsTo(duration);
                fadeColorDuration.Value = duration.Value;
            }

            ColorData fadeColorTargetColor = GetFieldValue<ColorData>(fadeColor, "_targetColor");
            if (targetColor != null && fadeColorTargetColor != null)
            {
                fadeColorTargetColor.SetContentsTo(targetColor);
            }

            FloatData targetAlpha = GetFieldValue<FloatData>(fadeColor, "_targetAlpha");
            if (targetAlpha != null)
            {
                targetAlpha.VarRef = null;

                float alphaValue = targetColor != null ? targetColor.Value.a : 1f;
                targetAlpha.Value = Mathf.Clamp01(alphaValue) * 100f;
            }

            BooleanData fadeOnlyAlpha = GetFieldValue<BooleanData>(fadeColor, "_fadeOnlyAlpha");
            if (fadeOnlyAlpha != null)
            {
                fadeOnlyAlpha.VarRef = null;
                fadeOnlyAlpha.Value = false;
            }

            BooleanData affectChildren = GetFieldValue<BooleanData>(fadeColor, "_affectChildren");
            if (affectChildren != null)
            {
                affectChildren.VarRef = null;
                affectChildren.Value = false;
            }

            BooleanData fadeColorWait = GetFieldValue<BooleanData>(fadeColor, "_waitUntilFinished");
            if (waitUntilFinished != null && fadeColorWait != null)
            {
                fadeColorWait.SetContentsTo(waitUntilFinished);
            }

            SetFieldValue(fadeColor, "_tweenerSO", fadeTweener);

            fadeColor.Refresh();
        }

        private static VariableReference GetOrCreateTargetReference(Flowchart flowchart, SpriteRenderer spriteRenderer)
        {
            VariableReference result = new VariableReference();

            if (flowchart == null || spriteRenderer == null)
            {
                return result;
            }

            IVariable existing = FindExistingTargetVariable(flowchart, spriteRenderer);
            if (existing == null)
            {
                IReadOnlyList<IVariable> vars = flowchart.Variables;
                List<IVariable> varList = new List<IVariable>(vars);

                string baseKey = string.IsNullOrEmpty(spriteRenderer.name)
                    ? "FadeSpriteTarget"
                    : $"FadeSpriteTarget_{spriteRenderer.name}";

                string key = UniqueKeyGenerator.GetUniqueKeyFor(baseKey, varList, null);
                existing = new GameObjectMuscariable()
                {
                    Key = key,
                    BoxedValue = spriteRenderer.gameObject,
                    Scope = VariableScope.Private
                };
                flowchart.AddVariable(existing);
            }

            result.Variable = existing;
            return result;
        }

        private static IVariable FindExistingTargetVariable(Flowchart flowchart, SpriteRenderer spriteRenderer)
        {
            IReadOnlyList<IVariable> vars = flowchart.Variables;
            for (int i = 0; i < vars.Count; i++)
            {
                IVariable variable = vars[i];
                if (variable == null)
                {
                    continue;
                }

                object value = variable.BoxedValue;
                if (ReferenceEquals(value, spriteRenderer) ||
                    ReferenceEquals(value, spriteRenderer.gameObject))
                {
                    return variable;
                }
            }

            return null;
        }

        private static void ReplaceInFlowchartCache(Flowchart flowchart, Command oldCommand, Command newCommand)
        {
            FieldInfo field = typeof(Flowchart).GetField("_commands",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (field == null)
            {
                return;
            }

            List<Command> cache = field.GetValue(flowchart) as List<Command>;
            if (cache == null)
            {
                return;
            }

            int index = cache.IndexOf(oldCommand);
            if (index >= 0)
            {
                cache[index] = newCommand;
            }
            else
            {
                cache.Add(newCommand);
            }
        }

        private static T GetFieldValue<T>(object target, string fieldName) where T : class
        {
            FieldInfo field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            return field != null ? field.GetValue(target) as T : null;
        }

        private static void SetFieldValue(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}