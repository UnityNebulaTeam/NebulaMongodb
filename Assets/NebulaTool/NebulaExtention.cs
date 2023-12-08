using System.IO;
using UnityEditor;

namespace NebulaTool
{
    public static class NebulaExtention
    {
        public static bool IsConnectionDataExist() => File.Exists(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);

        public static void DisplayConnectionDataDoesnotExistMessage()
        {
            EditorUtility.DisplayDialog("Sign Up Error", "You didn't sign up this plugin", "ok");
        }
    }
}