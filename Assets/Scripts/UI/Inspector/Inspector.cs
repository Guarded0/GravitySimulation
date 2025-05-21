using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Reflection;
using System;
public class Inspector : MonoBehaviour
{
    private RectTransform rectTransform;

    public Transform inspectorSettingParent;
    public TMP_Dropdown presetDropdown;
    public List<PlanetShapePreset> planetShapePresets;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button deleteButton;
    [SerializeField] private GameObject transformSetting;
    private TMP_InputField xInputField;
    private TMP_InputField yInputField;
    private TMP_InputField zInputField;
    private List<InspectorSetting> settings;
    private bool isUIShown = false;
    private bool initialized = false;

    public enum SettingType
    {
        Float,
        Color,
        Bool,
        Unkown
    }
    public struct InspectorSetting
    {
        public Transform panel;
        public InspectorSettingName settingName;
        public FieldInfo fieldInfo;
        public Component targetComponent;
        public bool isNested;
        public SettingType settingType;
        private static FieldInfo GetNestedFieldInfo(string fieldName)
        {
            FieldInfo field = typeof(CelestialBody).GetField("planetSettings");
            string[] parts = fieldName.Split('/');
            foreach (string part in parts)
            {
                field = field.FieldType.GetField(part);
            }
            return field;
        }
        public InspectorSetting(Transform panel) {
            this.panel = panel;
            // TODO: account for better detection
            this.settingName = panel.GetComponentInChildren<InspectorSettingName>();

            if (panel.GetComponentInChildren<TMP_InputField>() != null)
            {
                this.targetComponent = panel.GetComponentInChildren<TMP_InputField>();
                this.settingType = SettingType.Float;
            }
            else if (panel.GetComponentInChildren<ColorField>() != null)
            {
                this.targetComponent = panel.GetComponentInChildren<ColorField>();
                this.settingType = SettingType.Color;
            }
            else if (panel.GetComponentInChildren<Toggle>() != null)
            {
                this.targetComponent = panel.GetComponentInChildren<Toggle>();
                this.settingType = SettingType.Bool;
            }
            else
            {
                this.targetComponent = null;
                this.settingType = SettingType.Unkown;
            }


            string variableName = settingName.variableName;
            this.isNested = variableName.Contains("/");
            if (isNested)
                fieldInfo = GetNestedFieldInfo(variableName);
            else
                this.fieldInfo = typeof(PlanetSettings).GetField(settingName.variableName);
           

            
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
    void Update()
    {
        if (!initialized) return;
        if (isUIShown && Cible.current != null)
        {
            UpdateFromCible(Cible.current);
        }
    }
    void Init()
    {
        if (initialized) return;
        var children = gameObject.GetComponentsInChildren<InspectorSettingName>();
        settings = new List<InspectorSetting>();
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].transform.parent != inspectorSettingParent && children[i].transform.parent.parent != inspectorSettingParent) continue;
            InspectorSetting setting = new InspectorSetting(children[i].transform);
            switch(setting.settingType)
            {
                case SettingType.Float:
                    ((TMP_InputField)setting.targetComponent).onEndEdit.AddListener((string str) => OnNewValue(float.Parse(str), setting));
                    break;
                case SettingType.Color:
                    ((ColorField)setting.targetComponent).onColorChanged.AddListener((Color color) => OnNewValue(color, setting));
                    break;
                case SettingType.Bool:
                    ((Toggle)setting.targetComponent).onValueChanged.AddListener((bool value) => OnNewValue(value, setting));
                    break;
            }
            settings.Add(setting);
               
        }

        if (presetDropdown != null)
        {
            presetDropdown.AddOptions(planetShapePresets.ConvertAll(p => p.name));
            presetDropdown.onValueChanged.AddListener(OnDropdownNewValue);
        }
        if (transformSetting != null)
        {
            xInputField = transformSetting.transform.Find("X").GetComponent<TMP_InputField>();
            yInputField = transformSetting.transform.Find("Y").GetComponent<TMP_InputField>();
            zInputField = transformSetting.transform.Find("Z").GetComponent<TMP_InputField>();
            xInputField.onEndEdit.AddListener((string str) =>
            {
                if (Cible.current == null) return;
                var value = Cible.current.position;
                value.x = float.Parse(str);
                Cible.current.position = value;
            });
            yInputField.onEndEdit.AddListener((string str) =>
            {
                if (Cible.current == null) return;
                var value = Cible.current.position;
                value.y = float.Parse(str);
                Cible.current.position = value;
            });
            zInputField.onEndEdit.AddListener((string str) =>
            {
                if (Cible.current == null) return;
                var value = Cible.current.position;
                value.z = float.Parse(str);
                Cible.current.position = value;
            });
        }

        if (Cible.cibleChanged != null)
        {
            Cible.cibleChanged.AddListener(OnCibleUpdate);
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(OnDeleteButtonClick);
        }


        initialized = true;
    }
    void OnDropdownNewValue(int index)
    {
        if (Cible.current == null) return;
        var preset = planetShapePresets[index];
        Cible.current.GetComponent<CelestialBody>().planetSettings.planetShapeSettings = preset;
        Cible.current.GetComponent<CelestialBody>().shouldUpdateSettings = true;
    }
    void OnNewValue(object input, InspectorSetting setting)
    {
        SetObjectValue(Cible.current, setting, input);
    }
    void UpdateFromCible(Transform cible)
    {
        titleText.text = cible.name;
        for (int i = 0; i < settings.Count; i++)
        { 
            InspectorSetting setting = settings[i];
            var value = GetValueFromObject(cible, setting);
            switch (setting.settingType)
            {
                case SettingType.Float:
                    {
                        var field = setting.panel.GetComponentInChildren<TMP_InputField>();
                        if (field.isFocused) break;
                        field.SetTextWithoutNotify(value.ToString());
                    }
                    break;
                case SettingType.Color:
                    {
                        var field = setting.panel.GetComponentInChildren<ColorField>();
                        if (field.isFocused) break;
                        setting.panel.GetComponentInChildren<ColorField>().SetColor((Color)value);
                    }
                    break;
                case SettingType.Bool:
                    setting.panel.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify((bool)value);
                    break;
            }
        }
        // Dropdown
        if (presetDropdown != null)
        {
            presetDropdown.value = planetShapePresets.IndexOf(Cible.current.GetComponent<CelestialBody>().planetSettings.planetShapeSettings);
        }
        // Transform
        if (transformSetting != null)
        {
            if (!xInputField.isFocused) xInputField.SetTextWithoutNotify(cible.position.x.ToString());
            if (!yInputField.isFocused) yInputField.SetTextWithoutNotify(cible.position.y.ToString());
            if (!zInputField.isFocused) zInputField.SetTextWithoutNotify(cible.position.z.ToString());
        }

    }
    void SetObjectValue(Transform cible, InspectorSetting setting, object value)
    {
        if (setting.isNested)
            SetNestedObjectValue(cible, setting, value);
        else
            setting.fieldInfo.SetValue(cible.GetComponent<CelestialBody>().planetSettings, value);

        Cible.current.GetComponent<CelestialBody>().shouldUpdateSettings = true;
    }
    void SetNestedObjectValue(Transform cible, InspectorSetting setting, object value)
    {
        string[] parts = setting.settingName.variableName.Split("/");
        object currentObject = cible.GetComponent<CelestialBody>().planetSettings;
        Stack<(object parent, FieldInfo field)> fieldPath = new Stack<(object, FieldInfo)>();
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (currentObject == null) return;
            string part = parts[i];
            FieldInfo field = currentObject.GetType().GetField(part);
            fieldPath.Push((currentObject, field));
            currentObject = field.GetValue(currentObject);
        }
        currentObject.GetType().GetField(parts[^1], BindingFlags.Public | BindingFlags.Instance).SetValue(currentObject, value);

        // walk the path back up to the root object as reflection creates copies of modified struct rather than references
        while (fieldPath.Count > 0)
        {
            var (parent, field) = fieldPath.Pop();
            field.SetValue(parent, currentObject);
            currentObject = parent;
        }

        cible.GetComponent<CelestialBody>().planetSettings = (PlanetSettings)currentObject;
    }
    // gets the value of the setting from the object
    object GetValueFromObject(Transform cible, InspectorSetting setting)
    {
        return setting.fieldInfo.GetValue(GetSettingObject(cible, setting));
    }
    // gets the object that contains the setting
    object GetSettingObject(Transform cible, InspectorSetting setting)
    {
        if (cible == null) return null;
        if (setting.isNested)
            return GetNestedSettingObject(cible, setting);

        return cible.GetComponent<CelestialBody>().planetSettings;
    }
    // walks the path of the nested object to get the value
    object GetNestedSettingObject(Transform cible, InspectorSetting setting)
    {
        string[] parts = setting.settingName.variableName.Split("/");
        object currentObject = cible.GetComponent<CelestialBody>().planetSettings;
        for (int i = 0; i < parts.Length-1; i++)
        {
            if (currentObject == null) return null;
            string part = parts[i];
            currentObject = currentObject.GetType().GetField(part).GetValue(currentObject);
        }
        return currentObject;
    }
    void OnCibleUpdate(Transform newCible)
    {
        if (newCible == null)
            HideUI();
        else
            ShowUI();
    }
    void OnDeleteButtonClick()
    {
        if (Cible.current == null) return;
        NBodySimulation.Instance.DestroyBody(Cible.current.gameObject);
        HideUI();
    }
    void ShowUI()
    {
        if (!isUIShown)
            LeanTween.move(rectTransform, Vector3.zero, 0.5f).setEase(LeanTweenType.easeOutExpo);

        UpdateFromCible(Cible.current);
        isUIShown = true;
        
    }
    void HideUI()
    {
        if (!isUIShown) return;
        LeanTween.move(rectTransform, new Vector3(-300,0,0), 0.5f).setEase(LeanTweenType.easeOutExpo);
        isUIShown = false;
    }
}
