using UnityEditor.Search;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Reflection;

namespace Contracts
{
    public static class SerializedPropertyExtensions
    {
        /// <summary>
        /// Create a field element from this serialized property.
        /// </summary>
        public static VisualElement CreateFieldElement(this SerializedProperty serializedProperty)
        {
            // Create the appropriate input field based on the property type.
            var label = serializedProperty.displayName;
            VisualElement input = serializedProperty.propertyType switch
            {
                SerializedPropertyType.Boolean => new Toggle(label),
                SerializedPropertyType.Integer => new IntegerField(label),
                SerializedPropertyType.Float => new FloatField(label),
                SerializedPropertyType.String => new TextField(label),
                SerializedPropertyType.Enum => new EnumField(label, Enum.GetValues(serializedProperty.serializedObject.targetObject.GetType().GetField(serializedProperty.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FieldType).GetValue(serializedProperty.enumValueIndex) as Enum),
                SerializedPropertyType.Vector2 => new Vector2Field(label),
                SerializedPropertyType.Vector3 => new Vector3Field(label),
                SerializedPropertyType.Vector4 => new Vector4Field(label),
                SerializedPropertyType.Color => new ColorField(label),
                SerializedPropertyType.ObjectReference => new UnityEditor.Search.ObjectField(label)
                {
                    objectType = serializedProperty.serializedObject.targetObject.GetType(),
                    searchContext = SearchService.CreateContext("Assets"),
                },
                // TODO: handle the rest of this enum properly.
                _ => new Label($"(Unsupported Input) {label}"),
            };
            input.name = serializedProperty.propertyPath;

            // Bind the input to the serialized property.
            if (input is BindableElement bindableElement)
            {
                bindableElement.bindingPath = serializedProperty.propertyPath;
            }

            return input;
        }
    }
}
