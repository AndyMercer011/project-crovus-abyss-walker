using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opsive.UltimateInventorySystem.UI.Panels;

public class GameplayPanel : DisplayPanel
{
    /// 玩家状态显示
    [Tooltip("玩家状态显示")]
    [SerializeField] private BodyStatusDisplay playerStatusDisplay;
    /// 武器状态显示
    [Tooltip("武器状态显示")]
    [SerializeField] private WeaponDisplay weaponDisplay;

    /// <summary>
    /// Sets the weapon displays active or not
    /// </summary>
    /// <param name="state">If set to <c>true</c> state.</param>
    /// <param name="playerID">Player I.</param>
    public virtual void SetWeaponDisplays(bool state, string playerID, int weaponDisplayID)
    {
        if (weaponDisplay != null)
        {
            if ((weaponDisplay.PlayerID == playerID) && (weaponDisplayID == weaponDisplay.WeaponDisplayID))
            {
                weaponDisplay.gameObject.SetActive(state);
            }
        }
    }

    /// <summary>
    /// Updates the health bar.
    /// </summary>
    /// <param name="currentHealth">Current health.</param>
    /// <param name="minHealth">Minimum health.</param>
    /// <param name="maxHealth">Max health.</param>
    /// <param name="playerID">Player I.</param>
    public virtual void UpdateHealthBar(float currentHealth, float minHealth, float maxHealth, string playerID)
    {
        if (playerStatusDisplay != null)
        {
            if (playerStatusDisplay.PlayerID == playerID)
            {
                playerStatusDisplay.UpdateHealthBar(currentHealth, minHealth, maxHealth, playerID);
            }
        }
    }

    /// <summary>
    /// Updates the (optional) ammo displays.
    /// </summary>
    /// <param name="magazineBased">If set to <c>true</c> magazine based.</param>
    /// <param name="totalAmmo">Total ammo.</param>
    /// <param name="maxAmmo">Max ammo.</param>
    /// <param name="ammoInMagazine">Ammo in magazine.</param>
    /// <param name="magazineSize">Magazine size.</param>
    /// <param name="CD">update CD</param>
    /// <param name="playerID">Player I.</param>
    /// <param name="weaponDisplayID">WeaponDisplay ID</param>
    public void UpdateAmmoDisplays(bool magazineBased, int totalAmmo, int maxAmmo, int ammoInMagazine,
        int magazineSize, float CD, string playerID, int weaponDisplayID)
    {
        if (weaponDisplay == null) { return; }
        if ((weaponDisplay.PlayerID == playerID) && (weaponDisplayID == weaponDisplay.WeaponDisplayID))
        {
            weaponDisplay.UpdateAmmoDisplays(magazineBased, totalAmmo, maxAmmo, ammoInMagazine, magazineSize, CD);
        }
    }

    /// <summary>
    /// 更新武器UI图片
    /// </summary>
    /// <param name="weaponIcon">武器图标</param>
    /// <param name="ammoTypeIcon">弹药类型图标</param>
    /// <param name="weaponTriggerMode">武器射击模式</param>
    /// <param name="playerID">玩家ID</param>
    /// <param name="weaponDisplayID">武器UI ID</param>
    public virtual void UpdateWeaponIcons(Sprite weaponIcon, Sprite ammoTypeIcon, Weapon.TriggerModes weaponTriggerMode, string playerID, int weaponDisplayID)
    {
        if (weaponDisplay == null) { return; }
        if ((weaponDisplay.PlayerID == playerID) && (weaponDisplayID == weaponDisplay.WeaponDisplayID))
        {
            weaponDisplay.SetWeaponImage(weaponIcon);
            weaponDisplay.SetAmmoTypeImage(ammoTypeIcon);
            weaponDisplay.SetWeaponTriggerModeImage(weaponTriggerMode);
        }
    }
}
