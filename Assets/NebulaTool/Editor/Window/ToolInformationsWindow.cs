using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using NebulaTool.Enum;
using NebulaTool.ScritableSO;
using NebulaTool.Path;
using NebulaTool.Extension;

namespace NebulaTool.Window
{
    public class ToolInformationsWindow : EditorWindow
    {
        private InformationStruct infos;
        private StyleSheet mainStyle;
        [MenuItem("Nebula/Informations", priority = (int)CustomWindowPriorty.ToolInformation)]
        private static void ShowWindow()
        {
            var window = GetWindow<ToolInformationsWindow>();
            window.titleContent = new GUIContent("Tool Informations Window");
            window.Show();
        }


        private void OnEnable()
        {
            var json = File.ReadAllText(NebulaPath.DataPath + NebulaResourcesName.InformationsJsonDataName);
            infos = Newtonsoft.Json.JsonConvert.DeserializeObject<InformationStruct>(json);
            mainStyle = AssetDatabase.LoadAssetAtPath<StyleSO>(NebulaPath.DataPath + NebulaResourcesName.StylesheetsDataName).GetStyle(StyleType.InformationsWindowStyle);
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.styleSheets.Add(mainStyle);
            var container = NebulaExtention.Create<VisualElement>("Container");

            var tittle = NebulaExtention.Create<Label>("CustomLabel");
            tittle.text = "Tool Informations";
            container.Add(tittle);

            var mainScrollView = NebulaExtention.Create<VisualElement>("ScrollView");

            var scroll = NebulaExtention.Create<ScrollView>();

            foreach (var information in infos.Informations)
            {
                var helperBox = NebulaExtention.Create<HelpBox>();
                helperBox.messageType = information.MessageType;
                helperBox.text = information.Message;
                scroll.Add(helperBox);
            }

            mainScrollView.Add(scroll);

            container.Add(mainScrollView);

            root.Add(container);

        }
    }

}

[Serializable]
public class InformationStruct
{
    public List<Informations> Informations { get; set; }
}
public class Informations
{
    public string Message { get; set; }
    public HelpBoxMessageType MessageType { get; set; }
}
