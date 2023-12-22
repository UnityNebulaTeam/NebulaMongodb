using System;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using NebulaTool.Enum;
using NebulaTool.ScritableSO;
using NebulaTool.API;
using NebulaTool.Path;
using NebulaTool.Extension;
using System.Collections.Generic;

namespace NebulaTool.Window
{
    public class SignUpWindow : EditorWindow
    {
        private StyleSheet mainStyle;
        private ApiController apiController = new();

        [MenuItem("Nebula/SignIn/SignUp", priority = (int)CustomWindowPriorty.SignUp)]
        private static void ShowWindow()
        {
            var window = GetWindow<SignUpWindow>();
            window.titleContent = new GUIContent("SignUp Window");
            window.minSize = new Vector2(300, 200);
            window.Show();
        }

        private void OnEnable()
        {
            mainStyle = AssetDatabase.LoadAssetAtPath<StyleSO>
            (NebulaPath.DataPath +
             NebulaResourcesName.StylesheetsDataName).GetStyle(StyleType.SignUpStyle);
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.styleSheets.Add(mainStyle);

            var container = NebulaExtention.Create<VisualElement>("Container");
            var userNameContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");

            var userName = NebulaExtention.Create<Label>("CustomLabel");
            userName.text = "Username : ";
            var userNameTextField = NebulaExtention.Create<TextField>("CustomTextField");
            userNameTextField.SetPlaceholderText(CustomValidation.userNamePlaceHolder);
            userNameContainer.Add(userName);
            userNameContainer.Add(userNameTextField);
            container.Add(userNameContainer);


            var emailLbl = NebulaExtention.Create<Label>("CustomLabel");
            emailLbl.text = "E:MAİL : ";
            var emailTextField = NebulaExtention.Create<TextField>("CustomTextField");
            emailTextField.SetPlaceholderText(CustomValidation.emailPlaceHolder);
            var emailContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");
            emailContainer.Add(emailLbl);
            emailContainer.Add(emailTextField);
            container.Add(emailContainer);


            var passWordLbl = NebulaExtention.Create<Label>("CustomLabel");
            passWordLbl.text = "Password : ";
            var passWordLblTextField = NebulaExtention.Create<TextField>("CustomTextField");
            passWordLblTextField.SetPlaceholderText(CustomValidation.passwordPlaceHolder);
            var passwordContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");
            passwordContainer.Add(passWordLbl);
            passwordContainer.Add(passWordLblTextField);
            container.Add(passwordContainer);

            var dbInfoContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");
            var databaseTitle = NebulaExtention.Create<Label>("CustomLabel");
            databaseTitle.text = DatabaseTypes.MONGODB.ToString();
            var connectionURLTextField = NebulaExtention.Create<TextField>("CustomTextField");
            connectionURLTextField.SetPlaceholderText(CustomValidation.urlPlaceHolder);
            dbInfoContainer.Add(databaseTitle);
            dbInfoContainer.Add(connectionURLTextField);
            container.Add(dbInfoContainer);



            var connectButton = NebulaExtention.Create<Button>("CustomButton");
            connectButton.text = "Sign Up";
            var helpBoxContainer = NebulaExtention.Create<VisualElement>("HelpboxContainer");
            connectButton.clicked += () =>
            {
                helpBoxContainer.Clear();
                var values = new Dictionary<ValidationType, string>();
                values.Add(ValidationType.UserName, userNameTextField.value);
                values.Add(ValidationType.Email, emailTextField.value);
                values.Add(ValidationType.Password, passWordLblTextField.value);
                values.Add(ValidationType.ConnectionURL, connectionURLTextField.value);
                var validationResult = CustomValidation.IsValid(values);
                if (validationResult.Count > 0)
                {
                    helpBoxContainer.Clear();
                    foreach (var result in validationResult)
                    {
                        var warningBox = NebulaExtention.Create<HelpBox>("CustomHelpBox");
                        warningBox.messageType = HelpBoxMessageType.Error;
                        switch (result)
                        {
                            case ValidationType.None:
                                break;
                            case ValidationType.UserName:
                                warningBox.text = "Username is not empty or invalid";
                                break;
                            case ValidationType.Email:
                                warningBox.text = "Email is not empty or invalid";
                                break;
                            case ValidationType.Password:
                                warningBox.text = "Password is not empty or invalid";
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
                    apiController.SignUp(userNameTextField.value,
                    emailTextField.value, passWordLblTextField.value,
                    connectionURLTextField.value);
                    Close();
                }
            };
            container.Add(connectButton);
            root.Add(container);
        }
    }
}