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
    public class SignUpWindow : EditorWindow
    {
        private StyleSheet mainStyle;
        private ApiController apiController = new();

        [MenuItem("Nebula/SignIn/SignUp", priority = (int)CustomWindowPriorty.SignUp)]
        private static void ShowWindow()
        {
            var window = GetWindow<SignUpWindow>();
            window.titleContent = new GUIContent("SignUp Window");
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
            userNameTextField.SetPlaceholderText("Enter Your Username");
            userNameContainer.Add(userName);
            userNameContainer.Add(userNameTextField);
            container.Add(userNameContainer);


            var emailLbl = NebulaExtention.Create<Label>("CustomLabel");
            emailLbl.text = "E:MAİL : ";
            var emailTextField = NebulaExtention.Create<TextField>("CustomTextField");
            emailTextField.SetPlaceholderText("Enter your mail");
            var emailContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");
            emailContainer.Add(emailLbl);
            emailContainer.Add(emailTextField);
            container.Add(emailContainer);


            var passWordLbl = NebulaExtention.Create<Label>("CustomLabel");
            passWordLbl.text = "Password : ";
            var passWordLblTextField = NebulaExtention.Create<TextField>("CustomTextField");
            passWordLblTextField.SetPlaceholderText("Enter your password pls");
            var passwordContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");
            passwordContainer.Add(passWordLbl);
            passwordContainer.Add(passWordLblTextField);
            container.Add(passwordContainer);

            var dbInfoContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");
            var databaseTitle = NebulaExtention.Create<Label>("CustomLabel");
            databaseTitle.text = DatabaseTypes.MONGO.ToString();
            var connectionURLTextField = NebulaExtention.Create<TextField>("CustomTextField");
            connectionURLTextField.SetPlaceholderText("Enter database connection url");
            dbInfoContainer.Add(databaseTitle);
            dbInfoContainer.Add(connectionURLTextField);
            container.Add(dbInfoContainer);



            var connectButton = NebulaExtention.Create<Button>("CustomButton");
            connectButton.text = "Sign Up";

            connectButton.clicked += () =>
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(
                    apiController.SignUp(userNameTextField.value, emailTextField.value, passWordLblTextField.value, connectionURLTextField.value));
            };
            container.Add(connectButton);

            root.Add(container);
        }
    }
}