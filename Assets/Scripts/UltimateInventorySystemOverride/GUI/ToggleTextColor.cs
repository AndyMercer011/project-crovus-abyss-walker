using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleTextColor : MonoBehaviour
{
    private Toggle toggle;
    [Tooltip("Text组件")]
    [SerializeField] private TMP_Text text;
    [Tooltip("开启颜色")]
    [SerializeField] private Color OnColor = Color.white;
    [Tooltip("关闭颜色")]
    [SerializeField] private Color offColor = Color.white;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();

        if (toggle == null)
        {
            return;
        }

        SetTextColor(toggle.isOn);

        toggle.onValueChanged.AddListener((on) =>
        {
            SetTextColor(on);
        });
    }

    /// <summary>
    /// 设置Text颜色
    /// </summary>
    /// <param name="on">是否开启</param>
    public void SetTextColor(bool on)
    {
        if (on)
        {
            text.color = OnColor;
        }
        else
        {
            text.color = offColor;
        }
    }
}
