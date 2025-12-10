using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Collections.Generic;

namespace Amanita
{
    /// <summary>
    /// Helper class for hiding the many, many ways we might want to show text to the user.
    /// </summary>
    public class TextAdapter : IWriterTextDestination
    {
        protected Text textUI;
        protected InputField inputField;
        protected TextMesh textMesh;
        protected TMPro.TMP_Text tmpro;
        protected Component textComponent;
        protected PropertyInfo textProperty;
        protected IWriterTextDestination writerTextDestination;

        public void InitFromGameObject(GameObject go, bool includeChildren = false)
        {
            if (go == null)
            {
                return;
            }

            if (!includeChildren)
            {
                textUI = go.GetComponent<Text>();
                inputField = go.GetComponent<InputField>();
                textMesh = go.GetComponent<TextMesh>();
                tmpro = go.GetComponent<TMPro.TMP_Text>();
                writerTextDestination = go.GetComponent<IWriterTextDestination>();
            }
            else
            {
                textUI = go.GetComponentInChildren<Text>();
                inputField = go.GetComponentInChildren<InputField>();
                textMesh = go.GetComponentInChildren<TextMesh>();
                tmpro = go.GetComponentInChildren<TMPro.TMP_Text>();
                writerTextDestination = go.GetComponentInChildren<IWriterTextDestination>();
            }
            
            if (textUI == null && inputField == null && textMesh == null && writerTextDestination == null)
            {
                textComponent = FindFirstComponentWithTextProperty(go, includeChildren);
            }
        }

        private Component FindFirstComponentWithTextProperty(GameObject baseGo, bool includeChildren)
        {
            Component result = null;
            IList<Component> allComponents;
            if (!includeChildren)
            {
                allComponents = baseGo.GetComponents<Component>();
            }
            else
            {
                allComponents = baseGo.GetComponentsInChildren<Component>();
            }

            for (int i = 0; i < allComponents.Count; i++)
            {
                var elem = allComponents[i];
                textProperty = elem.GetType().GetProperty("text");
                if (textProperty != null)
                {
                    result = elem;
                    break;
                }
            }
            return result;
        }

        public void ForceRichText()
        {
            if (textUI != null)
            {
                textUI.supportRichText = true;
            }

            // Input Field does not support rich text

            if (textMesh != null)
            {
                textMesh.richText = true;
            }

            if (tmpro != null)
            {
                tmpro.richText = true;
            }

            if (writerTextDestination != null)
            {
                writerTextDestination.ForceRichText();
            }
        }

        public void SetTextColor(Color textColor)
        {
            if (textUI != null)
            {
                textUI.color = textColor;
            }
            else if (inputField != null)
            {
                if (inputField.textComponent != null)
                {
                    inputField.textComponent.color = textColor;
                }
            }
            else if (textMesh != null)
            {
                textMesh.color = textColor;
            }

            else if (tmpro != null)
            {
                tmpro.color = textColor;
            }

            else if (writerTextDestination != null)
            {
                writerTextDestination.SetTextColor(textColor);
            }
        }

        public void SetTextAlpha(float textAlpha)
        {
            if (textUI != null)
            {
                Color tempColor = textUI.color;
                tempColor.a = textAlpha;
                textUI.color = tempColor;
            }
            else if (inputField != null)
            {
                if (inputField.textComponent != null)
                {
                    Color tempColor = inputField.textComponent.color;
                    tempColor.a = textAlpha;
                    inputField.textComponent.color = tempColor;
                }
            }
            else if (textMesh != null)
            {
                Color tempColor = textMesh.color;
                tempColor.a = textAlpha;
                textMesh.color = tempColor;
            }

            else if (tmpro != null)
            {
                tmpro.alpha = textAlpha;
            }

            else if (writerTextDestination != null)
            {
                writerTextDestination.SetTextAlpha(textAlpha);
            }
        }

        public bool HasTextObject()
        {
            return (textUI != null || inputField != null || textMesh != null 
                || textComponent != null || tmpro != null || writerTextDestination != null);
        }

        public bool SupportsRichText()
        {
            if (textUI != null)
            {
                return textUI.supportRichText;
            }
            if (inputField != null)
            {
                return false;
            }
            if (textMesh != null)
            {
                return textMesh.richText;
            }

            if (tmpro != null)
            {
                return true;
            }

            if (writerTextDestination != null)
            {
                return writerTextDestination.SupportsRichText();
            }
            return false;
        }

        public bool SupportsHiddenCharacters()
        {
            if (tmpro != null)
            {
                return true;
            }

            return false;
        }

        public int RevealedCharacters
        {
            get
            {

                if (tmpro != null)
                {
                    return tmpro.maxVisibleCharacters;
                }

                return 0;
            }
            set
            {

                if (tmpro != null)
                {
                    tmpro.maxVisibleCharacters = value;
                }

            }
        }

        public char LastRevealedCharacter
        {
            get
            {

                if (tmpro != null && tmpro.textInfo != null && tmpro.textInfo.characterInfo != null)
                {
                    if (tmpro.maxVisibleCharacters < tmpro.textInfo.characterInfo.Length && tmpro.maxVisibleCharacters > 0)
                    {
                        return tmpro.textInfo.characterInfo[tmpro.maxVisibleCharacters - 1].character;
                    }
                }

                return (char)0;
            }
        }

        public int CharactersToReveal
        {
            get
            {

                if (tmpro != null)
                {
                    return tmpro.textInfo.characterCount;
                }

                return 0;
            }
        }

        public virtual string Text
        {
            get
            {
                if (textUI != null)
                {
                    return textUI.text;
                }
                else if (inputField != null)
                {
                    return inputField.text;
                }
                else if (writerTextDestination != null)
                {
                    return Text;
                }
                else if (textMesh != null)
                {
                    return textMesh.text;
                }

                else if (tmpro != null)
                {
                    return tmpro.text;
                }

                else if (textProperty != null)
                {
                    return textProperty.GetValue(textComponent, null) as string;
                }

                return "";
            }

            set
            {
                if (textUI != null)
                {
                    textUI.text = value;
                }
                else if (inputField != null)
                {
                    inputField.text = value;
                }
                else if (writerTextDestination != null)
                {
                    Text = value;
                }
                else if (textMesh != null)
                {
                    textMesh.text = value;
                }

                else if (tmpro != null)
                {
                    tmpro.text = value;
                    tmpro.ForceMeshUpdate();
                }

                else if (textProperty != null)
                {
                    textProperty.SetValue(textComponent, value, null);
                }
            }
        }
    }
}