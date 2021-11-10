using System;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace TeddyToolKit.Core.Editor
{
    /// <summary>
    /// Custom Editor Window with functionality to rename selected object in n batch
    /// </summary>
    public class ObjectRenamer : EditorWindow
    {
        private string prefix = "Object_";
        private int startNum = 1;
        private int incrementBy = 1;
        private int digits = 3;

        private string suffix = ".x";
        
        private string searchstr = "";
        private string replacestr = "";

        /// <summary>
        /// add a menu along the File Edit Assets ...
        /// </summary>
        [MenuItem("TeddyTK/ObjectRenamer")]
        public static void ShowWindow()
        {
            GetWindow<ObjectRenamer>("ObjectRenamer");
        }

        /// <summary>
        /// called when Unity GUI is drawn/script is loaded
        /// </summary>
        private void OnGUI()
        {
            RenameSequenceGUI();
            RenameAppendGUI();
            RenameReplaceGUI();
        }

        private void RenameAppendGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                // brackets just for code readability
                suffix = EditorGUILayout.TextField("Suffix", suffix);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.HelpBox(
                    "* Add suffix to selected objects",
                    MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Append"))
            {
                RenameAppend();
            }
        }

        private void RenameSequenceGUI()
        {
            GUILayout.Label("Rename the selected Objects", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                // brackets just for code readability
                prefix = EditorGUILayout.TextField("Prefix", prefix);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                startNum = EditorGUILayout.IntField("Start At", startNum);
                incrementBy = EditorGUILayout.IntField("Increment By", incrementBy);
                digits = EditorGUILayout.IntField("Digits", digits);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.HelpBox(
                    "* Select Objects in the Hierarchy to rename\n* Selection click order is important\n* Positive numbers only.\n* Prefix can't be empty",
                    MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Rename Sequence"))
            {
                RenameSequence();
            }
        }

        private void RenameReplaceGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                // brackets just for code readability
                searchstr = EditorGUILayout.TextField("Search for String", searchstr);
                replacestr = EditorGUILayout.TextField("Replace with String", replacestr);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.HelpBox(
                    "* Search and replace",
                    MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Search and Replace"))
            {
                RenameReplace();
            }
        }
        
        /// <summary>
        /// replace text to the selected objects
        /// </summary>
        private void RenameReplace()
        {
            foreach (GameObject obj in Selection.objects)
            {
                if (
                    searchstr == ""
                    || replacestr == ""
                )
                {
                    throwError();
                }

                obj.name = obj.name.Replace(searchstr, replacestr);
            }
        }
        
        /// <summary>
        /// append text to the selected objects
        /// </summary>
        private void RenameAppend()
        {
            foreach (GameObject obj in Selection.objects)
            {
                obj.name = $"{obj.name}{suffix}";
            }
        }
        
        /// <summary>
        /// loop through all the selected object and rename
        /// </summary>
        private void RenameSequence()
        {
            var i = startNum;
            var padChar = "0";
            var padStr = "";
            var maxDigits = Selection.count;
            var curDigits = 1;

            //sanity check
            if (
                (prefix == "")
                || (startNum < 0)
                || (maxDigits < 1)
                || (incrementBy < 1)
                || (digits < 1)
            )
            {
                throwError();
            }

            foreach (GameObject obj in Selection.objects)
            {
                padStr = "";

                if (i > 9) curDigits = i.ToString().Length;

                //only add padding if current digit is less than the digits specified 
                for (int d = curDigits; d < digits; d++)
                {
                    padStr += padChar;
                }

                obj.name = $"{prefix}{padStr}{i}";
                i += incrementBy;
            }
        }

        /// <summary>
        /// throw exception and abort
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void throwError()
        {
            var msg =
                "usage: Start At, Increment By, Digits must be positive number. Selection and Prefix cant be empty";
            throw new Exception(msg);
        }
    }
}