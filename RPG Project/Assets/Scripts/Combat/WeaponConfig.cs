using System;
using UnityEngine;
using RPG.Attributes;
using GameDevTV.Inventories;
using RPG.Stats;
using System.Collections.Generic;
using UnityEditor;


namespace RPG.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
    public class WeaponConfig : EquipableItem, IModifierProvider
    {
        [SerializeField] AnimatorOverrideController animatorOverride = null;
        [SerializeField] Weapon equippedPrefab = null;
        [SerializeField] float weaponDamage = 5f;
        [SerializeField] float percentageBonus = 0;
        [SerializeField] float weaponRange = 2f;
        [SerializeField] bool isRightHanded = true;
        [SerializeField] Projectile projectile = null;

        const string weaponName = "Weapon";

        public Weapon Spawn(Transform rightHand, Transform leftHand, Animator animator)
        {
            DestroyOldWeapon(rightHand, leftHand);

            Weapon weapon = null;

            if (equippedPrefab != null)
            {
                Transform handTransform = GetTransform(rightHand, leftHand);
                weapon = Instantiate(equippedPrefab, handTransform);
                weapon.gameObject.name = weaponName;
            }

            var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (animatorOverride != null)
            {
                animator.runtimeAnimatorController = animatorOverride; 
            }
            else if (overrideController != null)
            {
                animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
            }

            return weapon;
        }

        private void DestroyOldWeapon(Transform rightHand, Transform leftHand)
        {
            Transform oldWeapon = rightHand.Find(weaponName);
            if (oldWeapon == null)
            {
                oldWeapon = leftHand.Find(weaponName);
            }
            if (oldWeapon == null) return;

            oldWeapon.name = "DESTROYING";
            Destroy(oldWeapon.gameObject);
        }

        private Transform GetTransform(Transform rightHand, Transform leftHand)
        {
            Transform handTransform;
            if (isRightHanded) handTransform = rightHand;
            else handTransform = leftHand;
            return handTransform;
        }

        public bool HasProjectile()
        {
            return projectile != null;
        }

        public void LaunchProjectile(Transform rightHand, Transform leftHand, Health target, GameObject instigator, float calculatedDamage)
        {
            Projectile projectileInstance = Instantiate(projectile, GetTransform(rightHand, leftHand).position, Quaternion.identity);
            projectileInstance.SetTarget(target, instigator, calculatedDamage);
        }

        public float GetDamage()
        {
            return weaponDamage;
        }

        public float GetPercentageBonus()
        {
            return percentageBonus;
        }

        public float GetRange()
        {
            return weaponRange;
        }

        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return weaponDamage;
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return percentageBonus;
            }
        }

        #region InventoryItemEditor Additions

        public override string GetDescription()
        {
            string result = projectile ? "Ranged Weapon" : "Melee Weapon";
            result += $"\n\n{GetRawDescription()}\n";
            result += $"\nRange {weaponRange} meters";
            result += $"\nBase Damage {weaponDamage} points";
            if ((int)percentageBonus != 0)
            {
                string bonus = percentageBonus > 0 ? "<color=#8888ff>bonus</color>" : "<color=#ff8888>penalty</color>";
                result += $"\n{(int) percentageBonus} percent {bonus} to attack.";
            }
            return result;
        }

#if UNITY_EDITOR

        void OnValidate()
        {
            if (GetAllowedEquipLocation() != EquipLocation.Weapon)
            {
                SetAllowedEquipLocation(EquipLocation.Weapon);
            }
        }

        void SetWeaponRange(float newWeaponRange)
        {
            if (FloatEquals(weaponRange, newWeaponRange)) return;
            SetUndo("Set Weapon Range");
            weaponRange = newWeaponRange;
            Dirty();
        }

        void SetWeaponDamage(float newWeaponDamage)
        {
            if (FloatEquals(weaponDamage, newWeaponDamage)) return;
            SetUndo("Set Weapon Damage");
            weaponDamage = newWeaponDamage;
            Dirty();
        }

        void SetPercentageBonus(float newPercentageBonus)
        {
            if (FloatEquals(percentageBonus, newPercentageBonus)) return;
            SetUndo("Set Percentage Bonus");
            percentageBonus = newPercentageBonus;
            Dirty();
        }

        void SetIsRightHanded(bool newRightHanded)
        {
            if (isRightHanded == newRightHanded) return;
            SetUndo(newRightHanded?"Set as Right Handed":"Set as Left Handed");
            isRightHanded = newRightHanded;
            Dirty();
        }

        void SetAnimatorOverride(AnimatorOverrideController newOverride)
        {
            if (newOverride == animatorOverride) return;
            SetUndo("Change AnimatorOverride");
            animatorOverride = newOverride;
            Dirty();
        }

        void SetEquippedPrefab(GameObject potentialnewWeapon)
        {
            if (!potentialnewWeapon)
            {
                SetUndo("No Equipped Prefab");
                equippedPrefab = null;
                Dirty();
                return;
            }
            if (!potentialnewWeapon.TryGetComponent(out Weapon newWeapon)) return;
            if (newWeapon == equippedPrefab) return;
            SetUndo("Set Equipped Prefab");
            equippedPrefab = newWeapon;
            Dirty();
        }

        void SetProjectile(GameObject potentialNewProjectile)
        {
            if (!potentialNewProjectile)
            {
                SetUndo("No Projectile");
                projectile = null;
                Dirty();
                return;
            }
            if (!potentialNewProjectile.TryGetComponent(out Projectile newProjectile))
            {
                return;
            }
            
            if (newProjectile == projectile) return;
            SetUndo("Set Projectile");
            projectile = newProjectile;
            Dirty();
        }

        public override bool IsLocationSelectable(Enum location)
        {
            EquipLocation candidate = (EquipLocation) location;
            return candidate == EquipLocation.Weapon;
        }

        bool drawWeaponConfigData = true;
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();
            drawWeaponConfigData = EditorGUILayout.Foldout(drawWeaponConfigData, "WeaponConfig Data",foldoutStyle);
            if (!drawWeaponConfigData) return;
            EditorGUILayout.BeginVertical(contentStyle);
            //Trick to allow searching for the prefab using the . button instead of having to drag it in
            GameObject potentialPrefab = equippedPrefab?equippedPrefab.gameObject:null;
            SetEquippedPrefab((GameObject)EditorGUILayout.ObjectField("Equipped Prefab", potentialPrefab,typeof(GameObject), false));

            SetWeaponDamage(EditorGUILayout.Slider("Weapon Damage", weaponDamage, 0, 100));
            SetWeaponRange(EditorGUILayout.Slider("Weapon Range", weaponRange, 1,40));
            SetPercentageBonus(EditorGUILayout.IntSlider("Percentage Bonus", (int)percentageBonus, -10, 100));
            SetIsRightHanded(EditorGUILayout.Toggle("Is Right Handed", isRightHanded));
            SetAnimatorOverride((AnimatorOverrideController)EditorGUILayout.ObjectField("Animator Override", animatorOverride, typeof(AnimatorOverrideController), false));

            GameObject potentialProjectile = projectile ? projectile.gameObject : null;
            SetProjectile((GameObject)EditorGUILayout.ObjectField("Projectile", potentialProjectile, typeof(GameObject), false));
            EditorGUILayout.EndVertical();
        }

#endif
#endregion

    }
}
