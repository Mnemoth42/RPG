using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace GameDevTV.Inventories.Editor
{
    public class InventoryItemEditor : EditorWindow
    {
        [NonSerialized] InventoryItem selected = null;



        /// <summary>
        /// This is the boilerplate code that all EditorWindows need to have in order to function.
        /// The MenuItem directive adds an item to the Editor Menu system.
        /// </summary>
        [MenuItem("Window/InventoryItem Editor")]
        public static void ShowEditorWindow()
        {
            GetWindow(typeof(InventoryItemEditor), false, "InventoryItem");
        }

        /// <summary>
        /// This is a special version of ShowEditorWindow that takes a parameter InventoryItem candidate.
        /// This makes it so that when you double click on an InventoryItem to "open" it, the window will automatically
        /// have the correct file selected.
        /// </summary>
        /// <param name="candidate"></param>
        public static void ShowEditorWindow(InventoryItem candidate) { 
            InventoryItemEditor window = GetWindow(typeof(InventoryItemEditor), false, "Dialogue Editor") as InventoryItemEditor;
            if (candidate)
            {
                window.OnSelectionChange();
            }
        }

        /// <summary>
        /// The tag [OnOpenAsset(1)] hooks the OnOpenAsset method into the list of methods to check if
        /// an asset is the type of asset we should open.  Since we want to open any child of InventoryItem
        /// we're checking to see if the asset can be cast as an InventoryItem.
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            InventoryItem candidate = EditorUtility.InstanceIDToObject(instanceID) as InventoryItem;
            if (candidate != null)
            {
                ShowEditorWindow(candidate); //pass 

                return true;
            }
            return false;
        }

        void OnSelectionChange()
        {
            var candidate = EditorUtility.InstanceIDToObject(Selection.activeInstanceID) as InventoryItem;
            if (candidate == null) return;
            selected = candidate;
            Repaint();
        }

        void OnGUI()
        {
            if (!selected)
            {
                EditorGUILayout.HelpBox("No InventoryItem Selected", MessageType.Error);
                return;
            }
            EditorGUILayout.HelpBox($"{selected.name}/{selected.GetDisplayName()}", MessageType.Info);
            selected.DrawCustomInspector();
        }
    }
}