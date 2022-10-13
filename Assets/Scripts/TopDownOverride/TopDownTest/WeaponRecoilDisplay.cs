using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 武器后坐力显示
/// </summary>
public class WeaponRecoilDisplay : MonoBehaviour
{
    /// <summary>
    /// 武器后坐力面板
    /// </summary>
    [Tooltip("武器后坐力面板")]
    [SerializeField] private RectTransform Panel;
    /// <summary>
    /// 当前子弹散射角度
    /// </summary>
    [Tooltip("当前子弹散射角度")]
    [SerializeField] private Text SpreadText;
    /// <summary>
    /// 当前后坐力角度文本
    /// </summary>
    [Tooltip("当前后坐力角度文本")]
    [SerializeField] private Text RecoilText;
    /// <summary>
    /// 当前角度回复速度文本
    /// </summary>
    [Tooltip("当前角度回复速度文本")]
    [SerializeField] private Text RevertText;
    /// <summary>
    /// 文本格式
    /// </summary>
    [Tooltip("文本格式")]
    [SerializeField] private string textPattern = "F2";

    private CharacterHandleWeapon characterHandleWeapon;
    private Vector2 lastRecoil;
    private Vector2 lastRevert;
    private Vector3 lastSpread;
    private bool lastAiming;

    private void Start()
    {
        characterHandleWeapon = LevelManager.Instance.SceneCharacters[0]?.FindAbility<CharacterHandleWeapon>();
    }

    private void Update()
    {
        Panel.gameObject.SetActive(true);
        if (characterHandleWeapon == null || characterHandleWeapon.CurrentWeapon == null)
        {
            Panel.gameObject.SetActive(false);
            return;
        }
        WeaponAim3DOverride weaponAim3D = (characterHandleWeapon.CurrentWeapon as WeaponOverride).WeaponAim as WeaponAim3DOverride;
        if (weaponAim3D == null)
        {
            Panel.gameObject.SetActive(false);
            return;
        }
        if (lastRecoil != weaponAim3D.CurrentWeaponRecoilAngle)
        {
            lastRecoil = weaponAim3D.CurrentWeaponRecoilAngle;
            RecoilText.text = weaponAim3D.CurrentWeaponRecoilAngle.ToString(textPattern);
        }
        if (lastRevert != weaponAim3D.CurrentweapomRecoilRevertSpeed)
        {
            lastRevert = weaponAim3D.CurrentweapomRecoilRevertSpeed;
            RevertText.text = weaponAim3D.CurrentweapomRecoilRevertSpeed.ToString(textPattern);
        }

        ProjectileWeaponOverride projectileWeapon = characterHandleWeapon.CurrentWeapon as ProjectileWeaponOverride;
        if (projectileWeapon == null)
        {
            Panel.gameObject.SetActive(false);
            return;
        }
        Vector3 currentSpread = projectileWeapon.Aiming ? projectileWeapon.AimSpread : projectileWeapon.Spread;

        if (lastSpread != currentSpread || lastAiming != projectileWeapon.Aiming)
        {
            lastAiming = projectileWeapon.Aiming;
            lastSpread = currentSpread;
            SpreadText.text = currentSpread.ToString(textPattern);
        }
    }
}
