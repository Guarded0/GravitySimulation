using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Reflection;
public class Inspector : MonoBehaviour
{
    private RectTransform rectTransform;
    public Transform inspectorSettingParent;
    private List<InspectorSetting> settings;
    private bool isUIShown = false;
    private bool initialized = false;
    public struct InspectorSetting
    {
        public Transform panel;
        public InspectorSettingName settingName;
        public TMP_InputField inputField;
        public FieldInfo fieldInfo;
        public bool isNested;
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
            this.inputField = panel.GetComponentInChildren<TMP_InputField>();

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
    }
    void Init()
    {
        if (initialized) return;
        var children = gameObject.GetComponentsInChildren<Transform>();
        settings = new List<InspectorSetting>();
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].parent != inspectorSettingParent) continue;
            if (children[i].GetComponent<InspectorSettingName>() == null) continue;
            InspectorSetting setting = new InspectorSetting(children[i]);
            setting.inputField.onEndEdit.AddListener((string str) => OnNewValue(str, setting));
            settings.Add(setting);
               
        }
        if (Cible.cibleChanged != null)
        {
            Cible.cibleChanged.AddListener(OnCibleUpdate);
        }
        initialized = true;
    }
    void OnNewValue(string input, InspectorSetting setting)
    {
        float newValue = float.Parse(input);
        SetObjectValue(Cible.current, setting, newValue);
        Cible.current.GetComponent<CelestialBody>().shouldUpdateSettings = true;
    }
    void UpdateFromCible(Transform cible)
    {
        for (int i = 0; i < settings.Count; i++)
        { 
            InspectorSetting setting = settings[i];
            var value = GetValueFromObject(cible, setting);
            setting.inputField.SetTextWithoutNotify(value.ToString());
        }
    }
    void SetObjectValue(Transform cible, InspectorSetting setting, object value)
    {
        if (setting.isNested)
            SetNestedObjectValue(cible, setting, value);
        else
            setting.fieldInfo.SetValue(GetSettingObject(cible, setting), value);
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
        {
            HideUI();

        }
        else
        {
            ShowUI();
        }
        
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
