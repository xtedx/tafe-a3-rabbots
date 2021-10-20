using UnityEditor;
using UnityEngine;
using TeddyToolKit.Core.Attribute;
using TeddyToolKit.HighScore;

namespace TeddyToolKit.Editor.HighScore
{
    /// <summary>
    /// this makes it possible to display custom inspector for the Score class which contains int score and string name displayed in one line
    /// </summary>
    [CustomPropertyDrawer(typeof(Score))]
    public class ScoreDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Start drawing this specific instance of the tag property
            EditorGUI.BeginProperty(position, label, property);
            // Indicates the block of code is part of the property
            {
                // Draw the default label
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                
                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                // Calculate rects
                var scoreRect = new Rect(position.x, position.y, 80, position.height);
                var nameRect = new Rect(position.x + 85, position.y, 80, position.height);

                // Draw fields - pass GUIContent.none to each so they are drawn without labels
                EditorGUI.PropertyField(scoreRect, property.FindPropertyRelative("_value"), GUIContent.none);
                EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("_name"), GUIContent.none);

                // Set indent back to what it was
                EditorGUI.indentLevel = indent;
            }
            // Stop drawing this specific instance of the tag property
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight;
    }
}