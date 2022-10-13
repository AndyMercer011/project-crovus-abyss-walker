using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 玩家身体状态显示
/// </summary>
public class BodyStatusDisplay : MonoBehaviour
{
    /// optional - the ID of the player associated to this display
    [SerializeField] private string playerID;
    /// <summary>
    /// 血量进度条
    /// </summary>
    [Tooltip("血量进度条")]
    [SerializeField] private MMProgressBar healthBar;
    /// <summary>
    /// 血量文本
    /// </summary>
    [Tooltip("血量文本")]
    [SerializeField] private TMP_Text healthText;
    /// <summary>
    /// 文本格式
    /// </summary>
    [Tooltip("文本格式")]
    [SerializeField] private string textPattern = "000";

    public string PlayerID { get => playerID; }
    public MMProgressBar HealthBar { get => healthBar; }

    protected virtual void Awake()
    {
        Initialization();
    }

    public virtual void Initialization()
    {

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
        healthText.text = currentHealth.ToString(textPattern);

        if (healthBar == null) { return; }
        if (healthBar.PlayerID != playerID) { return; }
        healthBar.UpdateBar(currentHealth, minHealth, maxHealth);
    }
}

