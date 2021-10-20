using UnityEditor;
using UnityEngine;
using TeddyToolKit.Core.Attribute;

namespace TeddyToolKit.Editor.Core.Attribute
{
    /// <summary>
    /// this makes it possible to list all the tags in the inspector window for a variable defined with Tag attribute
    /// </summary>
    [CustomPropertyDrawer(typeof(TagAttribute))]
    public class TagDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Start drawing this specific instance of the tag property
            EditorGUI.BeginProperty(position, label, property);
            // Indicates the block of code is part of the property
            {
                //Determine if the property was set to nothing by default
                bool isNotSet = string.IsNullOrEmpty(property.stringValue);

                // Draw the string as a tag instead of a normal string box
                property.stringValue = EditorGUI.TagField(position, label,
                    isNotSet
                        ? (property.serializedObject.targetObject as Component)?.gameObject.tag
                        : property.stringValue);
            }
            // Stop drawing this specific instance of the tag property
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight;
    }
}