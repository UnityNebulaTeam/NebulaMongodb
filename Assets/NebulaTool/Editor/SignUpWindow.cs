using System;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NebulaTool.Editor
{
    public class SignUpWindow : EditorWindow
    {
        private StyleSheet mainStyle;
        private ApiController apiController = new();
        private ApiConnectionSO apiConnectionSo;

        [MenuItem("Nebula/SignUp", priority = 0)]
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
             NebulaResourcesName.StylesheetsDataName).GetStyle(StyleType.ApiConnection);

            apiConnectionSo = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
                (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.styleSheets.Add(mainStyle);

            var container = Create<VisualElement>("Container");
            var customPropFieldContainer1 = Create<VisualElement>("CustomPropFieldContainer");

            var userName = Create<Label>("CustomLabel");
            userName.text = "UserName : ";

            var userNameTextField = Create<TextField>("CustomTextField");


            customPropFieldContainer1.Add(userName);
            customPropFieldContainer1.Add(userNameTextField);


            var emailLbl = Create<Label>("CustomLabel");
            emailLbl.text = "E:MAİL : ";

            var emailTextField = Create<TextField>("CustomTextField");
            var customPropFieldContainer2 = Create<VisualElement>("CustomPropFieldContainer");
            customPropFieldContainer2.Add(emailLbl);
            customPropFieldContainer2.Add(emailTextField);


            var passWordLbl = Create<Label>("CustomLabel");
            passWordLbl.text = "Password : ";

            var passWordLblTextField = Create<TextField>("CustomTextField");

            var customPropFieldContainer3 = Create<VisualElement>("CustomPropFieldContainer");

            customPropFieldContainer3.Add(passWordLbl);
            customPropFieldContainer3.Add(passWordLblTextField);


            container.Add(customPropFieldContainer1);
            container.Add(customPropFieldContainer2);
            container.Add(customPropFieldContainer3);

            var connectButton = Create<Button>("CustomButton");
            connectButton.text = "Sign Up";

            connectButton.clicked += () =>
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(
                    apiController.SignUp(userNameTextField.value, emailTextField.value,passWordLblTextField.value));
            };
            container.Add(connectButton);



            var loginButton = Create<Button>();
            loginButton.text = "LOGIN";
            loginButton.clicked += () =>
            {
                //EditorUtility.DisplayDialog("TOKEN ERROR", "Your Token INVALID ! You Have To Login", "ok");
                EditorCoroutineUtility.StartCoroutineOwnerless(apiController.Login());
            };
            
            container.Add(loginButton);

            var getDbList = Create<Button>();
            getDbList.text = "Get User Databases";
            getDbList.clicked += () =>
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(apiController.GetUserDatabasesFromApi());
            };
            
            container.Add(getDbList);
            
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