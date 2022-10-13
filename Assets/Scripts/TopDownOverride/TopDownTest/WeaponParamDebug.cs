using MoreMountains.TopDownEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WeaponParamDebug : MonoBehaviour
{
    [Serializable]
    private struct SliderGroup
    {
        public Slider XSlider;
        public Slider YSlider;
        public Text XText;
        public Text YText;

        public void SetValue(float x, float y, string textPattern)
        {
            XSlider.value = x;
            XText.text = x.ToString(textPattern);
            YSlider.value = y;
            YText.text = y.ToString(textPattern);
        }
        public Vector2 GetValue()
        {
            return new Vector2(XSlider.value, YSlider.value);
        }

        public void AddListener(UnityAction<float> action, UnityAction<string> textAction, string textPattern)
        {
            XSlider.onValueChanged.RemoveAllListeners();
            YSlider.onValueChanged.RemoveAllListeners();
            XSlider.onValueChanged.AddListener(action);
            YSlider.onValueChanged.AddListener(action);
            XSlider.onValueChanged.AddListener((value) => textAction.Invoke(textPattern));
            YSlider.onValueChanged.AddListener((value) => textAction.Invoke(textPattern));
        }

        public void UpateText(string textPattern)
        {
            XText.text = XSlider.value.ToString(textPattern);
            YText.text = YSlider.value.ToString(textPattern);
        }
    }
    [SerializeField] private RectTransform panel;
    [SerializeField] private SliderGroup RecoilGroup;
    [SerializeField] private SliderGroup RecoilRandomGroup;
    [SerializeField] private SliderGroup RevertGroup;
    [SerializeField] private SliderGroup SpreadGroup;
    [SerializeField] private SliderGroup AimSpreadGroup;
    [SerializeField] private string textPattern = "F2";

    private CharacterHandleWeapon characterHandleWeapon;
    private ProjectileWeaponOverride projectileWeaponOverride;

    private void OnEnable()
    {
        characterHandleWeapon = LevelManager.Instance.SceneCharacters[0]?.FindAbility<CharacterHandleWeapon>();
        projectileWeaponOverride = characterHandleWeapon.CurrentWeapon as ProjectileWeaponOverride;
        if (projectileWeaponOverride == null)
        {
            panel.gameObject.SetActive(false);
            return;
        }
        else
        {
            panel.gameObject.SetActive(true);
        }
        RecoilGroup.SetValue(projectileWeaponOverride.WeapomRecoilAngle.x, projectileWeaponOverride.WeapomRecoilAngle.y, textPattern);
        RecoilRandomGroup.SetValue(projectileWeaponOverride.WeapomRecoilRandomRange.x, projectileWeaponOverride.WeapomRecoilRandomRange.y, textPattern);
        RevertGroup.SetValue(projectileWeaponOverride.WeapomRecoilRevertSpeed.x, projectileWeaponOverride.WeapomRecoilRevertSpeed.y, textPattern);
        SpreadGroup.SetValue(projectileWeaponOverride.Spread.x, projectileWeaponOverride.Spread.y, textPattern);
        AimSpreadGroup.SetValue(projectileWeaponOverride.AimSpread.x, projectileWeaponOverride.AimSpread.y, textPattern);


        RecoilGroup.AddListener((value) => projectileWeaponOverride.WeapomRecoilAngle = RecoilGroup.GetValue(), (s) => RecoilGroup.UpateText(s), textPattern);

        RecoilRandomGroup.AddListener((value) => projectileWeaponOverride.WeapomRecoilRandomRange = RecoilRandomGroup.GetValue(), (s) => RecoilRandomGroup.UpateText(s), textPattern);

        RevertGroup.AddListener((value) => projectileWeaponOverride.WeapomRecoilRevertSpeed = RevertGroup.GetValue(), (s) => RevertGroup.UpateText(s), textPattern);

        SpreadGroup.AddListener((value) => projectileWeaponOverride.Spread = SpreadGroup.GetValue(), (s) => SpreadGroup.UpateText(s), textPattern);

        AimSpreadGroup.AddListener((value) => projectileWeaponOverride.AimSpread = AimSpreadGroup.GetValue(), (s) => AimSpreadGroup.UpateText(s), textPattern);
    }
}
