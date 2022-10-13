using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 武器数据显示
/// </summary>
public class WeaponDisplay : MonoBehaviour
{
    /// optional - the ID of the player associated to this display
    [SerializeField] private string playerID;
    /// the ID of the AmmoDisplay 
    [Tooltip("the ID of the AmmoDisplay ")]
    [SerializeField] private int weaponDisplayID = 0;
    /// <summary>
    /// 弹药进度条
    /// </summary>
    [Tooltip("弹药进度条")]
    [SerializeField] private MMProgressBar ammoBar;
    /// the Text object used to display the current ammo numbers
    [Tooltip("the Text object used to display the current ammo numbers")]
    [SerializeField] private TMP_Text currentAmmoText;
    /// the Text object used to display the current ammo numbers
    [Tooltip("the Text object used to display the total ammo numbers")]
    [SerializeField] private TMP_Text totalAmmoText;
    /// <summary>
    /// 武器图标UI图片
    /// </summary>
    [Tooltip("武器图标UI图片")]
    [SerializeField] private Image weaponIconImage;
    /// <summary>
    /// 弹药类型UI图片
    /// </summary>
    [Tooltip("弹药类型UI图片")]
    [SerializeField] private Image ammoTypeImage;
    /// <summary>
    /// 弹药类型UI背景图片
    /// </summary>
    [Tooltip("弹药类型UI背景图片")]
    [SerializeField] private Image ammoTypeBackGroundImage;
    /// <summary>
    /// 武器射击模式类型UI图片
    /// </summary>
    [Tooltip("武器射击模式类型UI图片")]
    [SerializeField] private Image weaponTriggerModeImage;
    /// <summary>
    /// 文本格式
    /// </summary>
    [Tooltip("文本格式")]
    [SerializeField] private string textPattern = "000";


    [Header("WeaponTriggerModeIcons")]
    /// <summary>
    /// 半自动模式图标
    /// </summary>
    [Tooltip("半自动模式图标")]
    [SerializeField] private Sprite semiAutoIcon;
    /// <summary>
    /// 全自动模式图标
    /// </summary>
    [Tooltip("全自动模式图标")]
    [SerializeField] private Sprite autoIcon;
    /// <summary>
    /// 泵动模式图标
    /// </summary>
    [Tooltip("泵动模式图标")]
    [SerializeField] private Sprite pumpActionIcon;


    protected int _totalAmmoLastTime, _maxAmmoLastTime, _ammoInMagazineLastTime, _magazineSizeLastTime;

    public int WeaponDisplayID { get => weaponDisplayID; }
    public string PlayerID { get => playerID; }

    protected virtual void Awake()
    {
        Initialization();
    }

    public virtual void Initialization()
    {

    }

    /// <summary>
    /// Updates the text display with the parameter string
    /// </summary>
    /// <param name="newText">New text.</param>
    public virtual void UpdateTextDisplay(string currentAmmoNewText, string totalAmmoNewText)
    {
        if (currentAmmoText != null)
        {
            currentAmmoText.text = currentAmmoNewText;
        }
        if (totalAmmoText != null)
        {
            totalAmmoText.text = totalAmmoNewText;
        }
    }

    /// <summary>
    /// Updates the ammo display's text and progress bar
    /// </summary>
    /// <param name="magazineBased">If set to <c>true</c> magazine based.</param>
    /// <param name="totalAmmo">Total ammo.</param>
    /// <param name="maxAmmo">Max ammo.</param>
    /// <param name="ammoInMagazine">Ammo in magazine.</param>
    /// <param name="magazineSize">Magazine size.</param>
    /// <param name="CD">update CD</param>
    public virtual void UpdateAmmoDisplays(bool magazineBased, int totalAmmo, int maxAmmo, int ammoInMagazine, int magazineSize, float CD)
    {
        // we make sure there's actually something to update
        if ((_totalAmmoLastTime == totalAmmo)
            && (_maxAmmoLastTime == maxAmmo)
            && (_ammoInMagazineLastTime == ammoInMagazine)
            && (_magazineSizeLastTime == magazineSize))
        {
            return;
        }

        if (magazineBased)
        {
            if (totalAmmo > 0)
            {
                this.UpdateTextDisplay(ammoInMagazine.ToString(textPattern), totalAmmo.ToString(textPattern));

            }
            else
            {
                this.UpdateTextDisplay(ammoInMagazine.ToString(textPattern), "\u221E");
            }
            ammoBar.BumpDuration = CD;
            ammoBar.UpdateBar(ammoInMagazine, 0, magazineSize);
        }
        else
        {
            if (totalAmmo > 0)
            {
                this.UpdateTextDisplay(totalAmmo.ToString(textPattern), totalAmmo.ToString(textPattern));
            }
            else
            {
                this.UpdateTextDisplay("\u221E", "\u221E");
            }
        }

        _totalAmmoLastTime = totalAmmo;
        _maxAmmoLastTime = maxAmmo;
        _ammoInMagazineLastTime = ammoInMagazine;
        _magazineSizeLastTime = magazineSize;
    }

    /// <summary>
    /// 设置武器图标
    /// </summary>
    /// <param name="icon">武器图标</param>
    public void SetWeaponImage(Sprite icon)
    {
        weaponIconImage.gameObject.SetActive(icon != null);
        weaponIconImage.sprite = icon;
    }

    /// <summary>
    /// 设置弹药类型图标
    /// </summary>
    /// <param name="icon">弹药类型图标</param>
    public void SetAmmoTypeImage(Sprite icon)
    {
        //ammoTypeImage.sprite = icon;
        //ammoTypeBackGroundImage.sprite = icon;
    }

    /// <summary>
    /// 设置射击模式图标
    /// </summary>
    /// <param name="icon">射击模式</param>
    public void SetWeaponTriggerModeImage(Weapon.TriggerModes triggerMode)
    {
        switch (triggerMode)
        {
            case Weapon.TriggerModes.SemiAuto:
                weaponTriggerModeImage.sprite = semiAutoIcon;
                break;
            case Weapon.TriggerModes.Auto:
                weaponTriggerModeImage.sprite = autoIcon;
                break;
            case Weapon.TriggerModes.PumpAction:
                weaponTriggerModeImage.sprite = pumpActionIcon;
                break;
            default:
                break;
        }
    }
}