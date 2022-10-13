using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;

[RequireComponent(typeof(TMP_Text))]
public class NumberPlaceholderAlpha : MonoBehaviour
{
    /// <summary>
    /// 占位符透明度
    /// </summary>
    [Tooltip("占位符透明度")]
    [Range(0, 255)]
    public byte placeholderAlpha = 128;
    /// <summary>
    /// 非数字透明度
    /// </summary>
    [Tooltip("非数字透明度")]
    [Range(0, 255)]
    public byte notNumberAlpha = 128;
    /// <summary>
    /// 是否包含多串数字
    /// </summary>
    [Tooltip("是否包含多串数字")]
    public bool multiNumber = false;

    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        text.OnPreRenderText += SetPlaceholderAlpha;
    }

    /// <summary>
    /// 改变TMP Text占位符的alpha
    /// </summary>
    /// <param name="text">TMP Text</param>
    /// <param name="placeholderAlpha">占位符的alpha(0-255)</param>
    /// 
    private void SetPlaceholderAlpha(TMP_TextInfo textInfo)
    {
        ChangePlaceholderAlpha(textInfo, multiNumber, placeholderAlpha, notNumberAlpha);
    }

    /// <summary>
    /// 改变TMP Text占位符的alpha
    /// </summary>
    /// <param name="textInfo">TMP Text</param>
    /// <param name="multiNumber">是否包含多串数字</param>
    /// <param name="placeholderAlpha">占位符的alpha(0-255)</param>
    /// <param name="notNumberAlpha">非数字的alpha(0-255)</param>
    public static void ChangePlaceholderAlpha(TMP_TextInfo textInfo, bool multiNumber, byte placeholderAlpha, byte notNumberAlpha)
    {
        if (textInfo == null)
        {
            return;
        }

        bool validNumber = false;
        bool findNumber = false;
        bool numberBreak = false;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (textInfo.characterInfo[i].isVisible)
            {
                bool isNumber;
                if (int.TryParse(textInfo.characterInfo[i].character.ToString(), out int result))
                {
                    isNumber = true;
                    findNumber = true;
                    if (result > 0)
                    {
                        validNumber = true;
                    }
                }
                else
                {
                    isNumber = false;
                    validNumber = false;
                    if (findNumber)
                    {
                        numberBreak = true;
                    }
                }

                if ((!multiNumber && !numberBreak) || multiNumber)
                {
                    if (isNumber)
                    {
                        SetCharAlpha(textInfo, i, !validNumber ? placeholderAlpha : byte.MaxValue);
                    }
                    else
                    {
                        SetCharAlpha(textInfo, i, notNumberAlpha);
                    }
                }
                else
                {
                    SetCharAlpha(textInfo, i, notNumberAlpha);
                }

            }
        }
        textInfo.textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private static void SetCharAlpha(TMP_TextInfo textInfo, int charId, byte alpha)
    {
        int materialIndex = textInfo.characterInfo[charId].materialReferenceIndex;
        Color32[] charVertexColors = textInfo.meshInfo[materialIndex].colors32;
        int charVertexIndex = textInfo.characterInfo[charId].vertexIndex;


        charVertexColors[charVertexIndex + 0].a = alpha;
        charVertexColors[charVertexIndex + 1].a = alpha;
        charVertexColors[charVertexIndex + 2].a = alpha;
        charVertexColors[charVertexIndex + 3].a = alpha;
    }
}
