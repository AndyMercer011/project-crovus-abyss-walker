using Opsive.UltimateInventorySystem.UI.Panels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状态总览面板
/// </summary>
public class StateOverviewPanel : DisplayPanel
{
    /// 玩家状态显示
    [Tooltip("玩家状态显示")]
    [SerializeField] private BodyStatusDisplay playerStatusDisplay;

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
}
