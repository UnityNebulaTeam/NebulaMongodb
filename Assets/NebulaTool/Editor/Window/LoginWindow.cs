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

            var mailContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");
            var mailTitle = NebulaExtention.Create<Label>("CustomLabel");
            mailTitle.text = "MAİL";
            var eMailTextField = NebulaExtention.Create<TextField>("CustomTextField");
            eMailTextField.SetPlaceholderText("Enter your email");
            mailContainer.Add(mailTitle);
            mailContainer.Add(eMailTextField);


            var passwordContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");
            var passwordTitle = NebulaExtention.Create<Label>("CustomLabel");
            passwordTitle.text = "Password";
            var passwordTextField = NebulaExtention.Create<TextField>("CustomTextField");
            passwordTextField.SetPlaceholderText("Enter your password");
            passwordContainer.Add(passwordTitle);
            passwordContainer.Add(passwordTextField);

            container.Add(mailContainer);
            container.Add(passwordContainer);

            var loginContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");

            var LoginButton = NebulaExtention.Create<Button>("CustomButton");
            LoginButton.text = "Login";
            LoginButton.clicked += () =>
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(
                    apiController.Login());

            };

            loginContainer.Add(LoginButton);

            container.Add(loginContainer);
            root.Add(container);

        }
       
    }
}