using System;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NebulaTool.Editor
{
    public class SelectDatabaseWindow : EditorWindow
    {
        private ApiConnectionSO apiConnectionSo;

        private EnumField selectedDBType;

        private ApiController apiController = new();

        [MenuItem("Nebula/Select Database")]
        private static void ShowWindow()
        {
            var window = GetWindow<SelectDatabaseWindow>();
            window.titleContent = new GUIContent("Select Database Window");
            window.Show();
        }

        private void OnEnable()
        {
            apiConnectionSo = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
                (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
        }

        private void CreateGUI()
        {
            selectedDBType = new EnumField("Select Database Type",DatabaseTypes.MONGO);
            selectedDBType.label = "Select an option";
            selectedDBType.RegisterValueChangedCallback(OnEnumValueChanged);


            var connectionURLTextField = new TextField();
            connectionURLTextField.value = "Enter Your Connection String";

            var addDbTypeButton = new Button();
            addDbTypeButton.text = "Add New Database Controller";
            addDbTypeButton.clicked += () =>
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(
                    apiController.AddNewDbForApiAccount( 
                        selectedDBType.value.ToString(),
                        connectionURLTextField.value));
            };
            rootVisualElement.Add(selectedDBType);
            rootVisualElement.Add(connectionURLTextField);
            rootVisualElement.Add(addDbTypeButton);
        }

        private void OnEnumValueChanged(ChangeEvent<Enum> evt)
        {
            selectedDBType.value = evt.newValue;
            Debug.Log(selectedDBType.value);
        }
    }
}