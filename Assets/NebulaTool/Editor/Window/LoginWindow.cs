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
    public class LoginWindow : EditorWindow
    {
        private ApiConnectionSO apiConnectionSo;
        private ApiController apiController = new();
        private StyleSheet mainStyle;

        [MenuItem("Nebula/SignIn/Login", priority = (int)CustomWindowPriorty.ChilOfdSignIn_Login)]
        private static void ShowWindow()
        {
            if (!NebulaExtention.IsConnectionDataExist())
            {
                NebulaExtention.DisplayConnectionDataDoesnotExistMessage();
                return;
            }

            var window = GetWindow<LoginWindow>();
            window.titleContent = new GUIContent("Login Window");
            window.minSize = new Vector2(300, 200);
            window.Show();
        }

        private void OnEnable()
        {
            apiConnectionSo = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
                (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);

            mainStyle = AssetDatabase.LoadAssetAtPath<StyleSO>
              (NebulaPath.DataPath +
               NebulaResourcesName.StylesheetsDataName).GetStyle(StyleType.LoginStyle);

        }

        private void CreateGUI()
        {

            var root = rootVisualElement;
            root.styleSheets.Add(mainStyle);

            var container = NebulaExtention.Create<VisualElement>("Container");

            var userNameContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");
            var userNameTitle = NebulaExtention.Create<Label>("CustomLabel");
            userNameTitle.text = "Username";
            var userNameTextField = NebulaExtention.Create<TextField>("CustomTextField");
            userNameTextField.SetPlaceholderText(CustomValidation.userNamePlaceHolder);
            userNameContainer.Add(userNameTitle);
            userNameContainer.Add(userNameTextField);

            var mailContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");
            var mailTitle = NebulaExtention.Create<Label>("CustomLabel");
            mailTitle.text = "MAİL";
            var eMailTextField = NebulaExtention.Create<TextField>("CustomTextField");
            eMailTextField.SetPlaceholderText(CustomValidation.emailPlaceHolder);
            mailContainer.Add(mailTitle);
            mailContainer.Add(eMailTextField);


            var passwordContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");
            var passwordTitle = NebulaExtention.Create<Label>("CustomLabel");
            passwordTitle.text = "Password";
            var passwordTextField = NebulaExtention.Create<TextField>("CustomTextField");
            passwordTextField.SetPlaceholderText(CustomValidation.passwordPlaceHolder);
            passwordContainer.Add(passwordTitle);
            passwordContainer.Add(passwordTextField);

            container.Add(userNameContainer);
            container.Add(mailContainer);
            container.Add(passwordContainer);

            var loginContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");

            var LoginButton = NebulaExtention.Create<Button>("CustomButton");
            LoginButton.text = "Login";
            var helpboxContainer = NebulaExtention.Create<VisualElement>("HelpboxContainer");
            LoginButton.clicked += () =>
            {
                helpboxContainer.Clear();
                var values = new Dictionary<ValidationType, string>();
                values.Add(ValidationType.UserName, userNameTextField.value);
                values.Add(ValidationType.Email, eMailTextField.value);
                values.Add(ValidationType.Password, passwordTextField.value);

                var validationResult = CustomValidation.IsValid(values);
                if (validationResult.Count > 0)
                {
                    foreach (var result in validationResult)
                    {
                        var warningBox = NebulaExtention.Create<HelpBox>("CustomHelpBox");
                        warningBox.messageType = HelpBoxMessageType.Error;
                        switch (result)
                        {
                            case ValidationType.UserName:
                                warningBox.text = "Username is not empty or invalid";
                                break;
                            case ValidationType.Email:
                                warningBox.text = "Email is not empty or invalid";
                                break;
                            case ValidationType.Password:
                                warningBox.text = "Password is not empty or invalid";
                                break;
                        }
                        helpboxContainer.Add(warningBox);
                    }
                    container.Add(helpboxContainer);
                }
                else
                {
                    apiConnectionSo.userInformation.eMail = eMailTextField.value;
                    apiConnectionSo.userInformation.userName = userNameTextField.value;
                    apiConnectionSo.userInformation.password = passwordTextField.value;
                    EditorUtility.SetDirty(apiConnectionSo);
                    apiController.Login();
                }

            };

            loginContainer.Add(LoginButton);

            container.Add(loginContainer);
            root.Add(container);

        }

    }
}