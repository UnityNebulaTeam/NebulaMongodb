using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using NebulaTool.ScritableSO;
using NebulaTool.Path;
using NebulaTool.Enum;
using NebulaTool.Extension;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using NebulaTool.API;


namespace NebulaTool.Window
{
    public class UpdateConnectionStringWindow : EditorWindow
    {
        private StyleSheet mainStyle;
        private ApiController apiController=new();

        [MenuItem("Nebula/Update Connection String", priority = (int)CustomWindowPriorty.UpdateConnectionString)]
        private static void ShowWindow()
        {
            if (!NebulaExtention.IsConnectionDataExist())
            {
                NebulaExtention.DisplayConnectionDataDoesnotExistMessage();
                return;
            }
            var window = GetWindow<UpdateConnectionStringWindow>();
            window.titleContent = new GUIContent("Update Connection String Window");
            window.minSize = new Vector2(300, 200);
            window.Show();
        }

        private void OnEnable()
        {
            mainStyle = AssetDatabase.LoadAssetAtPath<StyleSO>
              (NebulaPath.DataPath +
               NebulaResourcesName.StylesheetsDataName).GetStyle(StyleType.UpdateDbStyle);
        }

        private void CreateGUI()
        {

            var root = rootVisualElement;
            root.styleSheets.Add(mainStyle);

            var container = Create<VisualElement>("Container");



            var connectionStringContainer = Create<VisualElement>("CustomPropFieldContainer");
            var connectionStringTitle = Create<Label>("CustomLabel");
            connectionStringTitle.text = "URL";
            var connectionStringTextField = Create<TextField>("CustomTextField");
            connectionStringTextField.SetPlaceholderText(CustomValidation.urlPlaceHolder);
            connectionStringContainer.Add(connectionStringTitle);
            connectionStringContainer.Add(connectionStringTextField);


            container.Add(connectionStringContainer);

            var SendContainer = Create<VisualElement>("CustomPropFieldContainer");

            var UpdateButton = Create<Button>("CustomButton");
            UpdateButton.text = "Update";

            var helpBoxContainer = NebulaExtention.Create<VisualElement>("HelpboxContainer");

            UpdateButton.clicked += () =>
            {
                helpBoxContainer.Clear();
                var values = new Dictionary<ValidationType, string>();
                values.Add(ValidationType.ConnectionURL, connectionStringTextField.value);
                var validationResult = CustomValidation.IsValid(values);
                if (validationResult.Count > 0)
                {
                    foreach (var result in validationResult)
                    {
                        var warningBox = NebulaExtention.Create<HelpBox>("CustomHelpBox");
                        warningBox.messageType = HelpBoxMessageType.Error;
                        switch (result)
                        {
                            case ValidationType.Password:
                                warningBox.text = "Password is not empty or invalid";
                                break;
                            case ValidationType.Email:
                                warningBox.text = "Email is not empty or invalid";
                                break;
                            case ValidationType.ConnectionURL:
                                warningBox.text = "ConnectionURL is not empty or invalid";
                                break;
                        }
                        helpBoxContainer.Add(warningBox);
                    }
                    container.Add(helpBoxContainer);
                }
                else
                {
                    apiController.UpdateConnectionURL(connectionStringTextField.value);
                }
            };

            SendContainer.Add(UpdateButton);

            container.Add(SendContainer);
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