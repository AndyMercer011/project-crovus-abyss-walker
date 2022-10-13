using Opsive.UltimateInventorySystem.UI.Panels;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : DisplayPanel
{
    /// <summary>
    /// 玩家状态面板
    /// </summary>
    [Tooltip("玩家状态面板")]
    [SerializeField] private StateOverviewPanel stateOverviewPanel;

    [Space]
    /// <summary>
    /// 状态总览面板切换
    /// </summary>
    [Tooltip("状态总览面板切换")]
    [SerializeField] private Toggle stateOverviewPanelToggle;

    /// <summary>
    /// 脑部神经面板切换
    /// </summary>
    [Tooltip("脑部神经面板切换")]
    [SerializeField] private Toggle brainNervePanelToggle;

    /// <summary>
    /// 日志面板切换
    /// </summary>
    [Tooltip("日志面板切换")]
    [SerializeField] private Toggle logPanelToggle;

    /// <summary>
    /// 区域地图面板切换
    /// </summary>
    [Tooltip("区域地图面板切换")]
    [SerializeField] private Toggle localMapPanelToggle;

    protected virtual void Awake()
    {
        Initialization();
    }

    protected virtual void OnEnable()
    {
    }

    public virtual void Initialization()
    {
        RegisterPanelToggle(stateOverviewPanelToggle, stateOverviewPanel);
        //RegisterPanelToggle(brainNervePanelToggle, brainNervePanel);
        //RegisterPanelToggle(logPanelToggle, logPanel);
        //RegisterPanelToggle(localMapPanelToggle, localMapPanel);
    }

    private void RegisterPanelToggle(Toggle toggle, DisplayPanel displayPanel)
    {
        toggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                displayPanel.Open();
            }
            else
            {
                displayPanel.Close();
            }
        });
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
        if (stateOverviewPanel != null)
        {
            stateOverviewPanel.UpdateHealthBar(currentHealth, minHealth, maxHealth, playerID);
        }
    }
}

