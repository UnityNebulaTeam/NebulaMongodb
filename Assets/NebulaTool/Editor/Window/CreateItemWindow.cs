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
        private IconSO icons;

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
            
            if (createType is CreateItemType.Item)
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
            icons = AssetDatabase.LoadAssetAtPath<IconSO>(NebulaPath.DataPath + NebulaResourcesName.IconsDataName);
            
            rootVisualElement.styleSheets.Add(mainStyle);
        }

        [Obsolete]
        private void CreateGUI()
        {
            switch (createType)
            {
                case CreateItemType.Database:
                    CreateDatabaseUI();
                    break;

                case CreateItemType.Collection:
                    CreateCollectionUI();
                    break;

                case CreateItemType.Item:
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

            var dbContainer = NebulaExtention.Create<VisualElement>("ContainerPropItem");
            var dbName = NebulaExtention.Create<TextField>("CustomValueField");
            dbName.SetPlaceholderText(CustomValidation.CreateDbPlaceHolder);
            var dbTitle = NebulaExtention.Create<Label>("CustomLabel","CustomPropField");
            dbTitle.text = "Database";
            
            var collectionContainer = NebulaExtention.Create<VisualElement>("ContainerPropItem");
            var collectionName = NebulaExtention.Create<TextField>("CustomValueField");
            collectionName.SetPlaceholderText(CustomValidation.CreateCollectionPlaceHolder);
            var colletionTitle = NebulaExtention.Create<Label>("CustomLabel","CustomPropField");
            colletionTitle.text = "Collection";

            dbContainer.Add(dbTitle);
            dbContainer.Add(dbName);
            
            collectionContainer.Add(colletionTitle);
            collectionContainer.Add(collectionName);
            
            container.Add(dbContainer);
            container.Add(collectionContainer);

            var CreateButton = NebulaExtention.Create<Button>("CustomOperationButtonOkey");
            var helpBoxContainer = NebulaExtention.Create<VisualElement>("HelpboxContainer");

            CreateButton.style.backgroundImage = icons.GetIcon(IconType.Okey).texture;
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

            var dbTitle = NebulaExtention.Create<Label>("CustomLabel","DatabaseTitle");
            dbTitle.text = $"{EditorPrefs.GetString("dbname")} Database";
            container.Add(dbTitle);
            
            var collectionContainer = NebulaExtention.Create<VisualElement>("ContainerPropItem");
            var colletionTitle = NebulaExtention.Create<Label>("CustomLabel","CustomPropField");
            colletionTitle.text = "Collection";
            
            var collectionNameInput = NebulaExtention.Create<TextField>("CustomValueField");
            collectionNameInput.SetPlaceholderText(CustomValidation.CreateCollectionPlaceHolder);
            collectionContainer.Add(colletionTitle);
            collectionContainer.Add(collectionNameInput);
            
            container.Add(collectionContainer);

            var createOperationButton = NebulaExtention.Create<Button>("CustomOperationButtonOkey");
            var helpBoxContainer = NebulaExtention.Create<VisualElement>("HelpboxContainer");
            createOperationButton.style.backgroundImage = icons.GetIcon(IconType.Okey).texture;
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
                    apiController.CreateTable(SelectedDatabase, collectionNameInput.value);
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

                var createOperationButton = NebulaExtention.Create<Button>("CustomOperationButtonOkey");
                var helpBoxContainer = NebulaExtention.Create<VisualElement>("HelpboxContainer");


                createOperationButton.style.backgroundImage = icons.GetIcon(IconType.Okey).texture;
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
                container.Add(createOperationButton);
                root.Add(container);
                root.Add(helpBoxContainer);
            }

            if (doesNotExistDoc)
            {
                var propFieldContainer = NebulaExtention.Create<VisualElement>("AddMinusFieldContainer");
                var fieldCountLabel = NebulaExtention.Create<Label>("CustomLabel");
                fieldCountLabel.text = $"Field Count {fieldCount}";

                var addFieldButton = NebulaExtention.Create<Button>("AddFieldButton");
                addFieldButton.text = "+";
                addFieldButton.clicked += delegate
                {
                    fieldCount++;
                    CreateItemUI();
                };

                var minusFieldCount = NebulaExtention.Create<Button>("MinusFieldButton");
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
                    
                    fields.Add(fieldValuePair);
                }

                var createOperationButton = NebulaExtention.Create<Button>("CustomOperationButtonOkey");
                var helpBoxContainer = NebulaExtention.Create<VisualElement>("HelpboxContainer");

                createOperationButton.style.backgroundImage = icons.GetIcon(IconType.Okey).texture;
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
                container.Add(createOperationButton);
                root.Add(container);
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