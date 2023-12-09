using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using NebulaTool.ScritableSO;
using NebulaTool.Path;
using NebulaTool.Enum;
using NebulaTool.Extension;


namespace NebulaTool.Window
{
    public class UpdateConnectionStringWindow : EditorWindow
    {
        private StyleSheet mainStyle;

        [MenuItem("Nebula/Update Connection String", priority = (int)CustomWindowPriorty.UpdateConnectionString)]
        private static void ShowWindow()
        {
            var window = GetWindow<UpdateConnectionStringWindow>();
            window.titleContent = new GUIContent("Update Connection String Window");
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

            var mailContainer = Create<VisualElement>("CustomPropFieldContainer");
            var mailTitle = Create<Label>("CustomLabel");
            mailTitle.text = "MAİL";
            var eMailTextField = Create<TextField>("CustomTextField");
            eMailTextField.SetPlaceholderText("Enter your email or username");
            mailContainer.Add(mailTitle);
            mailContainer.Add(eMailTextField);


            var passwordContainer = Create<VisualElement>("CustomPropFieldContainer");
            var passwordTitle = Create<Label>("CustomLabel");
            passwordTitle.text = "Password";
            var passwordTextField = Create<TextField>("CustomTextField");
            passwordTextField.SetPlaceholderText("Enter your password");
            passwordContainer.Add(passwordTitle);
            passwordContainer.Add(passwordTextField);


            var connectionStringContainer = Create<VisualElement>("CustomPropFieldContainer");
            var connectionStringTitle = Create<Label>("CustomLabel");
            connectionStringTitle.text = "Password";
            var connectionStringTextField = Create<TextField>("CustomTextField");
            connectionStringTextField.SetPlaceholderText("Enter your connection url");
            connectionStringContainer.Add(connectionStringTitle);
            connectionStringContainer.Add(connectionStringTextField);



            container.Add(mailContainer);
            container.Add(passwordContainer);
            container.Add(connectionStringContainer);

            var SendContainer = Create<VisualElement>("CustomPropFieldContainer");

            var UpdateButton = Create<Button>("CustomButton");
            UpdateButton.text = "Update";
            UpdateButton.clicked += () =>
            {

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