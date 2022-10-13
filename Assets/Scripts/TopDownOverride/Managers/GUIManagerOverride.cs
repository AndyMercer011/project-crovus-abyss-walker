using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains.TopDownEngine
{
    public class GUIManagerOverride : GUIManager
    {
        public static GUIManagerOverride InstanceOverride
        {
            get
            {
                if (Instance != null)
                {
                    if (instanceOverride == null)
                    {
                        instanceOverride = Instance as GUIManagerOverride;
                    }
                    return instanceOverride;
                }
                else
                {
                    return null;
                }
            }
        }

        protected static GUIManagerOverride instanceOverride;

        public static bool HasInstanceOverride => InstanceOverride != null;

        /// <summary>
        /// 是否开启了信息面板
        /// </summary>
        public bool EnableInfoPanel { get => enableInfoPanel; set => enableInfoPanel = value; }
        public GameplayPanel GameplayPanel { get => gameplayPanel; }
        public MainMenuPanel MainMenuPanel { get => mainMenuPanel; }

        /// 战斗面板
        [Tooltip("战斗面板")]
        [SerializeField] private GameplayPanel gameplayPanel;

        /// 信息面板
        [Tooltip("信息面板")]
        [SerializeField] private MainMenuPanel mainMenuPanel;

        protected bool enableInfoPanel;

        public virtual bool ToggleInfoPanel()
        {
            if (!enableInfoPanel)
            {
                enableInfoPanel = true;
                mainMenuPanel.Open();
                gameplayPanel.Close();
                EventSystem.current.sendNavigationEvents = true;
                return true;

            }
            else
            {
                enableInfoPanel = false;
                mainMenuPanel.Close();
                gameplayPanel.Open();
                EventSystem.current.sendNavigationEvents = false;
                return false;
            }
        }
    }
}
