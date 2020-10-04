﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.XR;
using UnityEngine;

namespace GameDevTV.Inventories
{
    /// <summary>
    /// A ScriptableObject that represents any item that can be put in an
    /// inventory.
    /// </summary>
    /// <remarks>
    /// In practice, you are likely to use a subclass such as `ActionItem` or
    /// `EquipableItem`.
    /// </remarks>
    public abstract class InventoryItem : ScriptableObject, ISerializationCallbackReceiver
    {
        // CONFIG DATA
        [Tooltip("Auto-generated UUID for saving/loading. Clear this field if you want to generate a new one.")]
        [SerializeField] string itemID = null;
        [Tooltip("Item name to be displayed in UI.")]
        [SerializeField] string displayName = null;
        [Tooltip("Item description to be displayed in UI.")]
        [SerializeField][TextArea] string description = null;
        [Tooltip("The UI icon to represent this item in the inventory.")]
        [SerializeField] Sprite icon = null;
        [Tooltip("The prefab that should be spawned when this item is dropped.")]
        [SerializeField] Pickup pickup = null;
        [Tooltip("If true, multiple items of this type can be stacked in the same inventory slot.")]
        [SerializeField] bool stackable = false;

        // STATE
        static Dictionary<string, InventoryItem> itemLookupCache;

        // PUBLIC

        /// <summary>
        /// Get the inventory item instance from its UUID.
        /// </summary>
        /// <param name="itemID">
        /// String UUID that persists between game instances.
        /// </param>
        /// <returns>
        /// Inventory item instance corresponding to the ID.
        /// </returns>
        public static InventoryItem GetFromID(string itemID)
        {
            if (itemLookupCache == null)
            {
                itemLookupCache = new Dictionary<string, InventoryItem>();
                var itemList = Resources.LoadAll<InventoryItem>("");
                foreach (var item in itemList)
                {
                    if (itemLookupCache.ContainsKey(item.itemID))
                    {
                        Debug.LogError(string.Format("Looks like there's a duplicate GameDevTV.UI.InventorySystem ID for objects: {0} and {1}", itemLookupCache[item.itemID], item));
                        continue;
                    }

                    itemLookupCache[item.itemID] = item;
                }
            }

            if (itemID == null || !itemLookupCache.ContainsKey(itemID)) return null;
            return itemLookupCache[itemID];
        }
        
        /// <summary>
        /// Spawn the pickup gameobject into the world.
        /// </summary>
        /// <param name="position">Where to spawn the pickup.</param>
        /// <param name="number">How many instances of the item does the pickup represent.</param>
        /// <returns>Reference to the pickup object spawned.</returns>
        public Pickup SpawnPickup(Vector3 position, int number)
        {
            var pickup = Instantiate(this.pickup);
            pickup.transform.position = position;
            pickup.Setup(this, number);
            return pickup;
        }

        public Sprite GetIcon()
        {
            return icon;
        }

        public string GetItemID()
        {
            return itemID;
        }

        public bool IsStackable()
        {
            return stackable;
        }
        
        public string GetDisplayName()
        {
            return displayName;
        }

        public string GetDescription()
        {
            return description;
        }

        // PRIVATE
        
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // Generate and save a new UUID if this is blank.
            if (string.IsNullOrWhiteSpace(itemID))
            {
                itemID = System.Guid.NewGuid().ToString();
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // Require by the ISerializationCallbackReceiver but we don't need
            // to do anything with it.
        }

        #region InventoryEditor Additions

        public Pickup GetPickup()
        {
            return pickup;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Convenience method, just to call EditorUtility.SetDirty(this);  It's optional, but it saves us some
        /// typing with so many methods to set dirty.  There is debate over the need to use SetDirty within editor
        /// code.  Extensive testing of this Editor in three different projects has shown me that without calling
        /// EditorUtility.SetDirty(this) in SerializedObject setters you can experience data loss.  Saving is very
        /// inconsistent.
        /// </summary>
        public void Dirty()
        {
            EditorUtility.SetDirty(this);
        }
        /// <summary>
        /// Another convenience method.  Simply calls Undo.RecordObject(this, message).  
        /// </summary>
        /// <param name="message"></param>
        public void SetUndo(string message)
        {
            Undo.RecordObject(this, message);
        }

        /// <summary>
        /// A handy float comparison function to test for equality.  As floats are imprecise, comparing two seemingly identical floats
        /// can yield false negatives.  This tests to a resolution of .001f
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public bool FloatEquals(float value1, float value2)
        {
            return Math.Abs(value1 - value2) < .001f;
        }

        public void SetDisplayName(string newDisplayName)
        {
            if (newDisplayName == displayName) return;
            SetUndo("Change Display Name");
            displayName = newDisplayName;
            Dirty();
        }

        public void SetDescription(string newDescription)
        {
            if (newDescription == description) return;
            SetUndo("Change Description");
            description = newDescription;
            Dirty();
        }

        public void SetIcon(Sprite newIcon)
        {
            if (icon == newIcon) return;
            SetUndo("Change Icon");
            icon = newIcon;
            Dirty();
        }

        public void SetPickup(Pickup newPickup)
        {
            if (pickup == newPickup) return;
            SetUndo("Change Pickup");
            pickup = newPickup;
            Dirty();
        }

        public void SetItemID(string newItemID)
        {
            if (itemID == newItemID) return;
            SetUndo("Change ItemID");
            itemID = newItemID;
            Dirty();
        }

        public void SetStackable(bool newStackable)
        {
            if (stackable == newStackable) return;
            SetUndo(stackable?"Set Not Stackable": "Set Stackable");
            stackable = newStackable;
            Dirty();
        }



        bool drawInventoryItem = true;
        public GUIStyle foldoutStyle;
        public virtual void DrawCustomInspector()
        {
            foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.fontStyle = FontStyle.Bold;
            drawInventoryItem = EditorGUILayout.Foldout(drawInventoryItem, "InventoryItem Data", foldoutStyle);
            if (!drawInventoryItem) return;
            SetItemID(EditorGUILayout.TextField("ItemID (clear to reset", GetItemID()));
            SetDisplayName(EditorGUILayout.TextField("Display name", GetDisplayName()));
            SetDescription(EditorGUILayout.TextField("Description", GetDescription()));
            SetIcon((Sprite)EditorGUILayout.ObjectField("Icon", GetIcon(), typeof(Sprite), false));
            SetPickup((Pickup)EditorGUILayout.ObjectField("Pickup", pickup, typeof(Pickup), false));
            SetStackable(EditorGUILayout.Toggle("Stackable", IsStackable()));
        }

#endif

#endregion
    }
}
