using System.IO;
using UnityEditor;
using NebulaTool.Path;
using UnityEngine.UIElements;
using NebulaTool.DTO;
using MongoDB.Bson.IO;
using MongoDB.Bson;
using System.Collections.Generic;

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


    public static class CustomValidation
    {
        public static string userNamePlaceHolder = "Enter your username";
        public static string emailPlaceHolder = "Enter your email";
        public static string passwordPlaceHolder = "Enter your password";
        public static string urlPlaceHolder = "Enter your connection url";

        public static List<ValidationType> IsValid(Dictionary<ValidationType, string> values)
        {
            List<ValidationType> invalidTypes = new List<ValidationType>();

            foreach (var pair in values)
            {
                string placeHolder = "";
                ValidationType placeType = ValidationType.None;

                switch (pair.Key)
                {
                    case ValidationType.UserName:
                        placeHolder = userNamePlaceHolder;
                        placeType = ValidationType.UserName;
                        break;
                    case ValidationType.Email:
                        placeHolder = emailPlaceHolder;
                        placeType = ValidationType.Email;
                        break;
                    case ValidationType.Password:
                        placeHolder = passwordPlaceHolder;
                        placeType = ValidationType.Password;
                        break;
                    case ValidationType.ConnectionURL:
                        placeHolder = urlPlaceHolder;
                        placeType = ValidationType.ConnectionURL;
                        break;
                }

                if (string.IsNullOrWhiteSpace(pair.Value) || pair.Value == placeHolder)
                {
                    invalidTypes.Add(placeType);
                }
            }

            return invalidTypes;
        }

    }



    public enum ValidationType
    {
        None,
        UserName,
        Email,
        Password,
        ConnectionURL
    }
}