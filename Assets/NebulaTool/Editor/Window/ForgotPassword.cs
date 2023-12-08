using UnityEditor;
using UnityEngine;

namespace NebulaTool.Editor
{
    public class ForgotPassword : EditorWindow
    {
        [MenuItem("Nebula/SignIn/Forgot Password",priority = (int)CustomWindowPriorty.ChilOfdSignIn_ForgotPassword)]
        private static void ShowWindow()
        {
            var window = GetWindow<ForgotPassword>();
            window.titleContent = new GUIContent("Forgot Password Window");
            window.Show();
        }
    }
}