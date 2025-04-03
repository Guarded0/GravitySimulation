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
        public InspectorSetting(Transform panel) {
            this.panel = panel;
            // TODO: account for better detection
            this.settingName = panel.GetComponentInChildren<InspectorSettingName>();
            this.inputField = panel.GetComponentInChildren<TMP_InputField>();

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
        float numInput = float.Parse(input);
        if (setting.fieldInfo.FieldType != typeof(float))
            throw new System.Exception("Variable type is not a float");
        setting.fieldInfo.SetValue(Cible.current.GetComponent<CelestialBody>().planetSettings, numInput);
        Cible.current.GetComponent<CelestialBody>().shouldUpdateSettings = true;
    }
    void UpdateFromCible(Transform cible)
    {
        for (int i = 0; i < settings.Count; i++)
        { 
            InspectorSetting setting = settings[i];
            var value = setting.fieldInfo.GetValue(cible.GetComponent<CelestialBody>().planetSettings);
            setting.inputField.SetTextWithoutNotify(value.ToString());
        }
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
