namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using System.Text;
    using UnityEngine;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// Item View component that shows the item name.
    /// </summary>
    public class CategoryAndNameItemView : ItemViewModule
    {
        [Tooltip("The name text.")]
        [SerializeField] protected Text m_NameText;
        [Tooltip("Default text value.")]
        [SerializeField] protected string m_DefaultTextValue;
        [Tooltip("物品名称颜色")]
        [SerializeField] protected Color m_NameColor = Color.white;

        [Header("Equipment")]
        [Tooltip("Equiped text suffix.")]
        [SerializeField] protected bool m_ShowEquiped = true;
        [Tooltip("Equiped text suffix.")]
        [SerializeField] protected string m_EquipedSuffix;
        [Tooltip("The attribute name for the equipped state, has priority over the collection ID.")]
        [SerializeField] protected string m_EquippedAttributeName = "IsEquipped";
        [Tooltip("The equipment itemCollection.")]
        [SerializeField] protected ItemCollectionID m_EquipmentCollectionID = ItemCollectionPurpose.Equipped;


        private StringBuilder stringBuilder = new StringBuilder(30);

        protected bool m_IsEquipped = false;

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null)
            {
                Clear();
                return;
            }

            if (m_ShowEquiped)
            {
                if (info.Item.TryGetAttributeValue<bool>(m_EquippedAttributeName, out var isEquipped))
                {
                    m_IsEquipped = isEquipped;
                }
                else
                {

                    if (info.ItemCollection == null)
                    {
                        Clear();
                        return;
                    }

                    m_IsEquipped = m_EquipmentCollectionID.Compare(info.ItemCollection);
                }
            }

            stringBuilder.Clear();
            stringBuilder.Append(info.Item.ItemDefinition.Category.name);
            stringBuilder.Append(" | ");
            stringBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGB(m_NameColor)}>");
            stringBuilder.Append(info.Item.name);
            stringBuilder.Append($"</color>");
            if (m_ShowEquiped && m_IsEquipped)
            {
                stringBuilder.Append(m_EquipedSuffix);
            }
            m_NameText.text = stringBuilder.ToString();
        }

        /// <summary>
        /// Clear the value.
        /// </summary>
        public override void Clear()
        {
            m_NameText.text = m_DefaultTextValue;
            m_IsEquipped = false;
        }
    }
}