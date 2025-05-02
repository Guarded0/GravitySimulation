using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Reflection;
using System;
using UnityEngine.UI;
using static SimulationSettingUI;
public class SimulationSettingUI : MonoBehaviour
{
    private RectTransform rectTransform;
    public Transform simulationSettingParent;

    private List<SimulationSetting> settings;
    private bool initialized = false;
    public struct SimulationSetting
    {
        public Transform panel;
        public InspectorSettingName settingName;
        public TMP_InputField inputField;
        public Slider slider;
        public FieldInfo fieldInfo;
        public Toggle toggle;
        public bool isToggle;
        public SimulationSetting(Transform panel)
        {
            this.panel = panel;
            // TODO: account for better detection
            this.settingName = panel.GetComponentInChildren<InspectorSettingName>();
            this.inputField = panel.GetComponentInChildren<TMP_InputField>();
            this.slider = panel.GetComponentInChildren<Slider>();
            this.isToggle = false;//
            if (this.inputField == null)
            {
                this.isToggle = true;
                this.toggle = panel.GetComponentInChildren<Toggle>();
            }else
            {
                this.isToggle = false;
                this.toggle = null;
            }
                string variableName = settingName.variableName;
            this.fieldInfo = typeof(NBodySimulation).GetField(settingName.variableName);

        }
    }
    private void Awake()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();

    }

    // Update is called once per frame
    void Init()
    {
        if (this.initialized) return;
        var children = gameObject.GetComponentsInChildren<Transform>();
        settings = new List<SimulationSetting>();
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].parent != simulationSettingParent) continue;
            if (children[i].GetComponent<InspectorSettingName>() == null) continue;
            SimulationSetting setting = new SimulationSetting(children[i]);
            if (setting.isToggle)
                setting.toggle.onValueChanged.AddListener((bool value) => SetObjectValue(setting, value));
            else
            {
                setting.slider.onValueChanged.AddListener((float value) => SetObjectValue(setting, value));
                setting.inputField.onEndEdit.AddListener((string str) => OnNewStringValue(str, setting));
            }
                
            settings.Add(setting);

        }
        initialized = true;
    }
    void OnNewStringValue(string input, SimulationSetting setting)
    {
        float newValue = float.Parse(input);
        SetObjectValue(setting, newValue);
    }
    void SetObjectValue(SimulationSetting setting, object value)
    {
        setting.fieldInfo.SetValue(NBodySimulation.Instance, value);
        if (setting.isToggle==false)
        {
            setting.inputField.SetTextWithoutNotify(value.ToString());
            setting.slider.SetValueWithoutNotify((float)value);
        }
    }
}
