using System;
using UnityEditor;
using UnityEngine;

namespace GameDevTV.Inventories
{
    /// <summary>
    /// An inventory item that can be equipped to the player. Weapons could be a
    /// subclass of this.
    /// </summary>
    [CreateAssetMenu(menuName = ("GameDevTV/GameDevTV.UI.InventorySystem/Equipable Item"))]
    public class EquipableItem : InventoryItem
    {
        // CONFIG DATA
        [Tooltip("Where are we allowed to put this item.")]
        [SerializeField] EquipLocation allowedEquipLocation = EquipLocation.Weapon;

        // PUBLIC

        public EquipLocation GetAllowedEquipLocation()
        {
            return allowedEquipLocation;
        }

        #region InventoryItemEditor Additions

#if UNITY_EDITOR
        public void SetAllowedEquipLocation(EquipLocation newLocation)
        {
            if (allowedEquipLocation == newLocation) return;
            SetUndo("Change Equip Location");
            allowedEquipLocation = newLocation;
            Dirty();
        }

        bool drawEquipableItem = true;
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();
            drawEquipableItem = EditorGUILayout.Foldout(drawEquipableItem, "EquipableItem Data", foldoutStyle);
            if (!drawEquipableItem) return;
            EditorGUILayout.BeginVertical(contentStyle);
            SetAllowedEquipLocation((EquipLocation)EditorGUILayout.EnumPopup(new GUIContent("Equip Location"),allowedEquipLocation,IsLocationSelectable,false));
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Override this is a class derived from Equipable Item to restrict the locations an item can be assigned. 
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public virtual  bool IsLocationSelectable(Enum location)
        {
            EquipLocation candidate = (EquipLocation)location;
            return candidate != EquipLocation.Weapon;
        }

#endif
        #endregion

    }
}