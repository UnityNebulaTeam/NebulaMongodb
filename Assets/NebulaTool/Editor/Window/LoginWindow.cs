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

        private EnumField selectedDBType;

        private ApiController apiController = new();

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
        }

        private void CreateGUI()
        {
            var eMailTextField = new TextField();
            eMailTextField.value = "E-Mail Or UserName";

            var passwordTextField = new TextField();
            passwordTextField.value = "password";

            var LoginButton = new Button();
            LoginButton.text = "Login";
            LoginButton.clicked += () =>
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(
                    apiController.Login());
                
            };
            rootVisualElement.Add(eMailTextField);
            rootVisualElement.Add(passwordTextField);
            rootVisualElement.Add(LoginButton);
        }
    }
}