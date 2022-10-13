using System.Collections;
using System.Collections.Generic;
using Opsive.UltimateInventorySystem.UI.CompoundElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectTextColor : MonoBehaviour
{
    [Tooltip("ActionButton")]
    [SerializeField] private ActionButton m_ActionButton;
    [Tooltip("Text���")]
    [SerializeField] private TMP_Text text;
    [Tooltip("δѡ����ɫ")]
    [SerializeField] private Color m_Color = Color.white;
    [Tooltip("ѡ����ɫ")]
    [SerializeField] private Color m_SelectedColor = Color.white;

    private void Awake()
    {
        if (m_ActionButton == null)
        {
            m_ActionButton = GetComponent<ActionButton>();
        }

        if (m_ActionButton == null)
        {
            return;
        }

        m_ActionButton.OnSelectE += () => SetTextColor(true);
        m_ActionButton.OnDeselectE += () => SetTextColor(false);
    }

    /// <summary>
    /// ����Text��ɫ
    /// </summary>
    /// <param name="select">�Ƿ�ѡ��</param>
    public void SetTextColor(bool select)
    {
        if (!select)
        {
            text.color = m_Color;
        }
        else
        {
            text.color = m_SelectedColor;
        }
    }
}
