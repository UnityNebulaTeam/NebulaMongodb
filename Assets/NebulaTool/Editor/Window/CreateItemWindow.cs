using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using Unity.EditorCoroutines.Editor;
using MongoDB.Bson;
using System.Collections.Generic;
using NebulaTool.Enum;
using NebulaTool.ScritableSO;
using NebulaTool.DTO;
using NebulaTool.API;
using NebulaTool.Path;
using NebulaTool.Extension;

namespace NebulaTool.Window
{
    public class CreateItemWindow : EditorWindow
    {
        private  CreateItemType createType;
        private string SelectedDatabase;
        private string SelectedColection;
        private BsonDocument doc = new();
        private bool doesNotExistDoc;
        private ApiController apiController;
        private int fieldCount;

        public CreateItemWindow(CreateItemType _type)
        {
            createType = _type;
            var valueString = createType.ToString();
            EditorPrefs.SetString("createType", valueString);  
        }
        
        public CreateItemWindow(CreateItemType _type,string selectedDatabase)
        {
            createType = _type;
            var valueString = createType.ToString();
            EditorPrefs.SetString("createType", valueString);  
            EditorPrefs.SetString("dbname",selectedDatabase);
        }

        /// <summary>
        /// Item oluştururken kullanılan constructor
        /// </summary>
        /// <param name="_type">UI için tpye</param>
        /// <param name="selectedDatabase">Koleksiyonunun hangi veritabanına bağlı olduğunu belirt</param>
        /// <param name="selectedCollection">Veri hangi koleksiyon altında oluşturulacak</param>
        public CreateItemWindow(CreateItemType _type,
            string selectedDatabase, string selectedCollection)
        {
            createType = _type;
            var valueString = createType.ToString();
            EditorPrefs.SetString("createType", valueString);  
            EditorPrefs.SetString("dbname",selectedDatabase);
            EditorPrefs.SetString("collectionName",selectedCollection);
            
            
        } 

        private StyleSheet mainStyle;

        public void ShowWindow()
        {
            var window = GetWindow<CreateItemWindow>();
            window.titleContent = new GUIContent($"Create {createType.ToString()}");
            PrepareData();
            
            window.Show();
        }

        private void OnEnable()
        {
            PrepareData();
            InitializeUI();
            apiController.EditorDrawLoaded += DrawEditorLoad;
        }

        private void OnDestroy()
        {
            apiController.EditorDrawLoaded -= DrawEditorLoad;
            ClearAllPlayerPrefs();
        }

        private void DrawEditorLoad(EditorLoadType type)
        {
            DatabaseManager dbManager = GetWindow<DatabaseManager>();
            dbManager.RefreshPanel(type);
            CloseWindow();
        }

        private async void PrepareData()
        {
            if(apiController is null)
                apiController = new ApiController();
            if (EditorPrefs.HasKey("dbname"))
                SelectedDatabase = EditorPrefs.GetString("dbname");
            if (EditorPrefs.HasKey("collectionName"))
                SelectedColection = EditorPrefs.GetString("collectionName");
            
            if (EditorPrefs.HasKey("createType"))
            {
                var enumValue = EditorPrefs.GetString("createType");
                createType = (CreateItemType)System.Enum.Parse(typeof(CreateItemType), enumValue);
            }
            
            if (createType is CreateItemType.item)
            {
                doc = await apiController.GetFirstItem(SelectedDatabase, SelectedColection);
                if (doc is not null)
                {
                    doesNotExistDoc = false;
                    CreateItemUI();
                }
                else
                {
                    doesNotExistDoc = true;
                    CreateItemUI();
                }
            }
        }


        private void InitializeUI()
        {
            mainStyle = AssetDatabase.LoadAssetAtPath<StyleSO>(NebulaPath.DataPath + NebulaResourcesName.StylesheetsDataName).GetStyle(StyleType.CreateWindowStyle);
            rootVisualElement.styleSheets.Add(mainStyle);
        }

        [Obsolete]
        private void CreateGUI()
        {
            switch (createType)
            {
                case CreateItemType.db:
                    CreateDatabaseUI();
                    break;

                case CreateItemType.collection:
                    CreateCollectionUI();
                    break;

                case CreateItemType.item:
                    CreateItemUI();
                    break;
            }

            ;
        }

        [Obsolete]
        private void CreateDatabaseUI()
        {
            var root = rootVisualElement;
            var container = NebulaExtention.Create<VisualElement>("Container");

            var dbName = NebulaExtention.Create<TextField>();
            dbName.SetPlaceholderText(CustomValidation.CreateDbPlaceHolder);
            var dbTitle = NebulaExtention.Create<Label>("CustomLabel");

            var collectionName = NebulaExtention.Create<TextField>();
            collectionName.SetPlaceholderText(CustomValidation.CreateCollectionPlaceHolder);
            var colletionTitle = NebulaExtention.Create<Label>("CustomLabel");


            container.Add(dbTitle);
            container.Add(dbName);
            container.Add(colletionTitle);
            container.Add(collectionName);


            var CreateButton = NebulaExtention.Create<Button>("CustomOperationButton");
            var helpBoxContainer = NebulaExtention.Create<VisualElement>("HelpboxContainer");

            CreateButton.text = "+";
            CreateButton.clicked += delegate
            {
                helpBoxContainer.Clear();
                var values = new Dictionary<ValidationType, string>();
                values.Add(ValidationType.CreateDb, dbName.value);
                values.Add(ValidationType.CreateCollection, collectionName.value);
                var validationResult = CustomValidation.IsValid(values);
                if (validationResult.Count > 0)
                {
                    foreach (var result in validationResult)
                    {
                        var warningBox = NebulaExtention.Create<HelpBox>("CustomHelpBox");
                        warningBox.messageType = HelpBoxMessageType.Error;
                        warningBox.text = result switch
                        {
                            ValidationType.CreateDb => "You have to set db name",
                            ValidationType.CreateCollection => "You have to set collection name",
                            _ => warningBox.text
                        };
                        helpBoxContainer.Add(warningBox);
                    }

                    container.Add(helpBoxContainer);
                }
                else
                {
                    apiController.CreateDatabase(dbName.value, collectionName.value);
                }
            };

            container.Add(CreateButton);


            root.Add(container);
        }

        [Obsolete]
        private void CreateCollectionUI()
        {
            var root = rootVisualElement;
            var container = NebulaExtention.Create<VisualElement>("Container");

            var dbTitle = NebulaExtention.Create<Label>("CustomLabel");
            dbTitle.text = EditorPrefs.GetString("dbname");
            container.Add(dbTitle);

            var collectionNameInput = NebulaExtention.Create<TextField>("CustomTextField");
            collectionNameInput.SetPlaceholderText(CustomValidation.CreateCollectionPlaceHolder);
            container.Add(collectionNameInput);

            var createOperationButton = NebulaExtention.Create<Button>("CustomOperationButton");
            var helpBoxContainer = NebulaExtention.Create<VisualElement>("HelpboxContainer");
            createOperationButton.text = "+";
            createOperationButton.clicked += delegate
            {
                helpBoxContainer.Clear();
                var values = new Dictionary<ValidationType, string>
                {
                    {ValidationType.CreateCollection, collectionNameInput.value}
                };
                var validationResult = CustomValidation.IsValid(values);
                if (validationResult.Count > 0)
                {
                    foreach (var result in validationResult)
                    {
                        var warningBox = NebulaExtention.Create<HelpBox>("CustomHelpBox");
                        warningBox.messageType = HelpBoxMessageType.Error;
                        warningBox.text = result switch
                        {
                            ValidationType.CreateCollection => "You have to set collection name",
                            _ => warningBox.text
                        };
                        helpBoxContainer.Add(warningBox);
                    }

                    container.Add(helpBoxContainer);
                }
                else
                {
                    apiController.CreateTable(dbTitle.text, collectionNameInput.value);
                }
            };
            container.Add(createOperationButton);
            root.Add(container);
        }

        [Obsolete("Obsolete")]
        private void CreateItemUI()
        {
            var root = rootVisualElement;
            root.Clear();
            var container = NebulaExtention.Create<VisualElement>("Container");
            if (!doesNotExistDoc)
            {
                List<FieldValuePair> fields = new List<FieldValuePair>();
                foreach (var key in doc)
                {
                    if (key.Name is "_id") continue;
                    var fieldValuePair = new FieldValuePair(key.Name, CustomValidation.ItemValuePlaceHolder);
                    var propTextAndValueContainer = NebulaExtention.Create<VisualElement>("ContainerPropItem");
                    var propText = NebulaExtention.Create<TextField>("CustomPropField");
                    propText.SetPlaceholderText(key.Name);
                    propText.RegisterValueChangedCallback(x =>
                    {
                        if (string.IsNullOrEmpty(x.newValue))
                            fieldValuePair.FieldName = key.Name;
                        else
                            fieldValuePair.FieldName = x.newValue;
                    });

                    var propvalue = NebulaExtention.Create<TextField>("CustomValueField");
                    propvalue.SetPlaceholderText(CustomValidation.ItemValuePlaceHolder);

                    propvalue.RegisterValueChangedCallback(e =>
                    {
                        fieldValuePair.UpdatedValue = e.newValue;
                    });
                    propTextAndValueContainer.Add(propText);
                    propTextAndValueContainer.Add(propvalue);
                    container.Add(propTextAndValueContainer);

                    fields.Add(fieldValuePair);
                }

                var createOperationButton = NebulaExtention.Create<Button>("CustomOperationButton");
                var helpBoxContainer = NebulaExtention.Create<VisualElement>("HelpboxContainer");

                
                createOperationButton.text = "+";
                createOperationButton.clicked += delegate
                {
                    helpBoxContainer.Clear();
                    var validations = CustomValidation.IsValidItem(fields);
                    if (validations.Count > 0)
                    {
                        foreach (var valid in validations)
                        {
                            var warningBox = NebulaExtention.Create<HelpBox>("CustomHelpBox");
                            warningBox.messageType = HelpBoxMessageType.Error;
                            warningBox.text = $"{valid + 1}. satıra lütfen geçerli bir değer giriniz";
                            helpBoxContainer.Add(warningBox);
                        }
                    }
                    else
                    {
                        apiController.CreateItem(SelectedDatabase, SelectedColection, fields);
                    }
                };

                root.Add(container);
                root.Add(createOperationButton);
                root.Add(helpBoxContainer);
            }

            if (doesNotExistDoc)
            {
                var propFieldContainer = NebulaExtention.Create<VisualElement>("ContainerPropItem");
                var fieldCountLabel = NebulaExtention.Create<Label>("CustomLabel");
                fieldCountLabel.text = $"Field Count {fieldCount}";

                var addFieldButton = NebulaExtention.Create<Button>("CustomOperationButton");
                addFieldButton.text = "+";
                addFieldButton.clicked += delegate
                {
                    fieldCount++;
                    CreateItemUI();
                };

                var minusFieldCount = NebulaExtention.Create<Button>("CustomOperationButton");
                minusFieldCount.text = "-";
                minusFieldCount.clicked += delegate
                {
                    if (fieldCount > 0)
                        fieldCount--;
                    CreateItemUI();
                };
                propFieldContainer.Add(fieldCountLabel);
                ;
                propFieldContainer.Add(addFieldButton);
                propFieldContainer.Add(minusFieldCount);

                container.Add(propFieldContainer);
                root.Add(container);


                List<FieldValuePair> fields = new List<FieldValuePair>();
                for (int i = 0; i < fieldCount; i++)
                {
                    var propContainer = NebulaExtention.Create<VisualElement>("ContainerPropItem");
                    var fieldValuePair = new FieldValuePair(CustomValidation.ItemPropertyPlaceHolder, CustomValidation.ItemValuePlaceHolder);
                    var propName = NebulaExtention.Create<TextField>("CustomPropField");
                    propName.SetPlaceholderText(CustomValidation.ItemPropertyPlaceHolder);
                    var propValue = NebulaExtention.Create<TextField>("CustomValueField");
                    propValue.SetPlaceholderText(CustomValidation.ItemValuePlaceHolder);

                    propName.RegisterValueChangedCallback(e => { fieldValuePair.FieldName = e.newValue; });

                    propValue.RegisterValueChangedCallback(e => { fieldValuePair.UpdatedValue = e.newValue; });

                    propContainer.Add(propName);
                    propContainer.Add(propValue);

                    container.Add(propContainer);
                    root.Add(container);
                    fields.Add(fieldValuePair);
                }

                var createOperationButton = NebulaExtention.Create<Button>("CustomOperationButton");
                var helpBoxContainer = NebulaExtention.Create<VisualElement>("HelpboxContainer");
                
                createOperationButton.text = "Create";
                createOperationButton.clicked += delegate
                {
                    helpBoxContainer.Clear();
                    var validCount = CustomValidation.IsValidItem(fields);
                    if (validCount.Count > 0)
                    {
                        foreach (var validIndex in validCount)
                        {
                            var warningBox = NebulaExtention.Create<HelpBox>("CustomHelpBox");
                            warningBox.messageType = HelpBoxMessageType.Error;
                            warningBox.text = $"{validIndex + 1}. satıra lütfen geçerli bir değer giriniz";
                            helpBoxContainer.Add(warningBox);
                        }
                    }
                    else
                    {
                        apiController.CreateItem(SelectedDatabase, SelectedColection, fields);
                    }
                };
                root.Add(createOperationButton);
                root.Add(helpBoxContainer);
            }
        }

        private void CloseWindow()
        {
            ClearAllPlayerPrefs();
            Close();
        }

        private void ClearAllPlayerPrefs() => EditorPrefs.DeleteAll();
    }
}