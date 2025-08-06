using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Amanita.VScripting;
using UnityObject = UnityEngine.Object;

namespace Amanita.EditorUtils
{
    public static class UitkFieldGenerator
    {
        public static VisualElement GenerateObjectField(IVariable varToRepresent)
        {
            VisualElement result = null;
            var varType = varToRepresent.ContentType;
            if (varToRepresent.Value is AudioClip)
            {
                result = GenAudioClipField(varToRepresent);
            }

            if (result == null)
            {
                Debug.LogWarning($"Could not generate object field for variable of type {varType.Name}");
            }

            return result;
        }

        private static VisualElement GenAudioClipField(IVariable audioVar)
        {
            if (audioVar.Value is not AudioClip)
            {
                Debug.LogWarning($"For some reason, we are not recognizing the var's value as an audio clip");
            }
            var varAsObj = audioVar as UnityObject;
            var varType = audioVar.ContentType;
            AudioClip clip = (AudioClip)audioVar.Value;
            var objField = new ObjectField
            {
                objectType = typeof(AudioClip),
                value = clip,
            };
            objField.RegisterValueChangedCallback(evt =>
            {
                SerializedObject so = new SerializedObject(varAsObj);
                SerializedProperty valProp = so.FindProperty("baseVal");
                Undo.RecordObject(varAsObj, "Change AudioClip Variable Value");
                valProp.objectReferenceValue = evt.newValue as AudioClip;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(varAsObj);
            });
            return objField;

        }
    }
}