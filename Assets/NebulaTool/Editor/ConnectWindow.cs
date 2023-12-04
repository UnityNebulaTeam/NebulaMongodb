using System;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NebulaTool.Editor
{
    public class ConnectWindow : EditorWindow
    {
        private StyleSheet mainStyle;
        private ApiController apiController = new();

        [MenuItem("Nebula/Connect Api",priority = 0)]
        private static void ShowWindow()
        {
            var window = GetWindow<ConnectWindow>();
            window.titleContent = new GUIContent("Connect Api Window");
            window.Show();
        }


        private void OnEnable()
        {
            mainStyle = mainStyle = AssetDatabase.LoadAssetAtPath<StyleSO>(NebulaPath.DataPath +
                                                                           NebulaResourcesName.StylesheetsDataName).GetStyle(StyleType.ApiConnection);
        }


        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.styleSheets.Add(mainStyle);
            
            var container = Create<VisualElement>("Container");

            var email = Create<TextField>("CustomTextField");
            var password = Create<TextField>("CustomTextField");
            
            
            var connectButton = Create<Button>("CustomButton");
            connectButton.text = "Connect";

            connectButton.clicked += () =>
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(apiController.ConnectionApi(email.value,password.value));
            };

            var urlLbl = Create<Label>("CustomLabel");
            urlLbl.text = "Connection Url";
            
            container.Add(urlLbl);
            container.Add(email);
            container.Add(password);
            container.Add(connectButton);
            
            
            root.Add(container);
        }

        private T Create<T>(params string[] classNames) where T : VisualElement, new()
        {
            var element = new T();
            foreach (var name in classNames)
                element.AddToClassList(name);

            return element;
        }
    }
}