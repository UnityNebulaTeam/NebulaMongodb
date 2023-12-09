using UnityEditor;
using UnityEngine;
using NebulaTool.Enum;
using UnityEngine.UIElements;
using NebulaTool.ScritableSO;
using NebulaTool.Path;
using NebulaTool.Extension;
namespace NebulaTool.Window
{
    public class ForgotPassword : EditorWindow
    {

        private StyleSheet mainStyle;

        [MenuItem("Nebula/SignIn/Forgot Password", priority = (int)CustomWindowPriorty.ChilOfdSignIn_ForgotPassword)]
        private static void ShowWindow()
        {
            var window = GetWindow<ForgotPassword>();
            window.titleContent = new GUIContent("Forgot Password Window");
            window.Show();
        }

        private void OnEnable()
        {
            mainStyle = AssetDatabase.LoadAssetAtPath<StyleSO>
              (NebulaPath.DataPath +
               NebulaResourcesName.StylesheetsDataName).GetStyle(StyleType.ForgotStlye);

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
            eMailTextField.SetPlaceholderText("Enter your email or username");
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

            var SendContainer = NebulaExtention.Create<VisualElement>("CustomPropFieldContainer");

            var SendButton = NebulaExtention.Create<Button>("CustomButton");
            SendButton.text = "Send";
            SendButton.clicked += () =>
            {

            };

            SendContainer.Add(SendButton);

            container.Add(SendContainer);
            root.Add(container);

        }
    }
}