using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public interface IRowVisualElementBuilder
    {
        RowVisualElements Build(VisualTreeAsset template);
    }

    public static class RowVisualElementBuilderRegistry
    {
        private static IRowVisualElementBuilder _current = new DefaultRowVisualElementBuilder();

        public static IRowVisualElementBuilder Current
        {
            get => _current;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _current = value;
            }
        }
    }

    public sealed class RowVisualElements
    {
        public static RowVisualElements Empty { get; } =
            new RowVisualElements(null, null, null, null, null, null);

        public RowVisualElements(
            VisualElement root,
            TextField keyField,
            VisualElement valueFieldHolder,
            IBindable valueField,
            EnumField scopeField,
            Button removeButton)
        {
            Root = root;
            KeyField = keyField;
            ValueFieldHolder = valueFieldHolder;
            ValueField = valueField;
            ScopeField = scopeField;
            RemoveButton = removeButton;
        }

        public VisualElement Root { get; }
        public TextField KeyField { get; }
        public VisualElement ValueFieldHolder { get; }
        public IBindable ValueField { get; }
        public EnumField ScopeField { get; }
        public Button RemoveButton { get; }
    }

    public sealed class DefaultRowVisualElementBuilder : IRowVisualElementBuilder
    {
        public RowVisualElements Build(VisualTreeAsset template)
        {
            if (template == null)
            {
                Debug.LogWarning("DefaultRowVisualElementBuilder was given a null template.");
                return RowVisualElements.Empty;
            }

            VisualElement rowRoot = template.CloneTree();

            TextField keyField = rowRoot.Q<TextField>("KeyInput");
            if (keyField == null)
            {
                Debug.LogWarning("Variable row template is missing a TextField named 'KeyInput'.");
            }
            else
            {
                keyField.isDelayed = true;
                keyField.multiline = false;
            }

            VisualElement valueFieldHolder = rowRoot.Q<VisualElement>("ValueFieldHolder");
            VisualElement valueFieldElement = rowRoot.Q("ValueField");
            IBindable valueField = valueFieldElement as IBindable;
            if (valueFieldElement != null && valueField == null)
            {
                Debug.LogWarning("'ValueField' element does not implement IBindable.");
            }

            EnumField scopeField = rowRoot.Q<EnumField>("Scope");
            if (scopeField == null)
            {
                Debug.LogWarning("Variable row template is missing an EnumField named 'Scope'.");
            }

            Button removeButton = rowRoot.Q<Button>("RemoveButton");
            if (removeButton == null)
            {
                Debug.LogWarning("Variable row template is missing a Button named 'RemoveButton'.");
            }

            return new RowVisualElements(
                rowRoot,
                keyField,
                valueFieldHolder,
                valueField,
                scopeField,
                removeButton);
        }
    }
}