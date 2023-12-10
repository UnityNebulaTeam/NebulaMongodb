using System.IO;
using UnityEditor;
using NebulaTool.Path;
using UnityEngine.UIElements;
using NebulaTool.DTO;
using MongoDB.Bson.IO;
using MongoDB.Bson;

namespace NebulaTool.Extension
{
    public static class NebulaExtention
    {
        public static bool IsConnectionDataExist() => File.Exists(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);

        public static void DisplayConnectionDataDoesnotExistMessage()
        {
            EditorUtility.DisplayDialog("Sign Up Error", "You didn't sign up this plugin", "ok");
        }

        public static bool ShowDisplayDialogForDelete(string title, string msg)
        {
            var result = EditorUtility.DisplayDialog(title, msg, "ok", "cancel");
            return result;
        }

        public static void SetPlaceholderText(this TextField textField, string placeholder)
        {
            string placeholderClass = TextField.ussClassName + "__placeholder";

            onFocusOut();
            textField.RegisterCallback<FocusInEvent>(evt => onFocusIn());
            textField.RegisterCallback<FocusOutEvent>(evt => onFocusOut());

            void onFocusIn()
            {
                if (textField.ClassListContains(placeholderClass))
                {
                    textField.value = string.Empty;
                    textField.RemoveFromClassList(placeholderClass);
                }
            }

            void onFocusOut()
            {
                if (string.IsNullOrEmpty(textField.text))
                {
                    textField.SetValueWithoutNotify(placeholder);
                    textField.AddToClassList(placeholderClass);
                }
            }
        }

        public static T Create<T>(params string[] classNames) where T : VisualElement, new()
        {
            var element = new T();
            foreach (var name in classNames)
                element.AddToClassList(name);

            return element;
        }

        public static string ConvertTableItemDtoToJson(UpdateTableItemDto tableItemDto)
        {
            var bsonDocument = tableItemDto;
            var settings = new JsonWriterSettings { Indent = true };
            var jsonOutput = bsonDocument.ToJson(settings);
            return jsonOutput;
        }
    }
}