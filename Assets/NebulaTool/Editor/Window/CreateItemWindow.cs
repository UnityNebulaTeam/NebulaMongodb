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
        private readonly CreateItemType createType;
        private string SelectedDatabase;
        private string SelectedColection;
        private BsonDocument doc = new();
        private bool doesNotExistDoc;
        private ApiController apiController = new();
        private int fieldCount;
        public CreateItemWindow(CreateItemType _type) => createType = _type;

        /// <summary>
        /// Item oluştururken kullanılan constructor
        /// </summary>
        /// <param name="_type">UI için tpye</param>
        /// <param name="selectedDatabase">Koleksiyonunun hangi veritabanına bağlı olduğunu belirt</param>
        /// <param name="selectedCollection">Veri hangi koleksiyon altında oluşturulacak</param>
        public CreateItemWindow(CreateItemType _type, string selectedDatabase, string selectedCollection) => createType = _type;
        private StyleSheet mainStyle;
        public void ShowWindow()
        {
            var window = GetWindow<CreateItemWindow>();
            window.titleContent = new GUIContent($"Create {createType.ToString()}");
            if (EditorPrefs.GetString("dbname") is not null)
                SelectedDatabase = EditorPrefs.GetString("dbname");
            if (EditorPrefs.GetString("collectionName") is not null)
                SelectedColection = EditorPrefs.GetString("collectionName");

            if (createType is CreateItemType.item)
                EditorCoroutineUtility.StartCoroutineOwnerless(apiController.GetAllItemsTypeBsonDocument(SelectedDatabase, SelectedColection));
            window.Show();
        }

        private void OnEnable()
        {
            InitializeUI();
            apiController.itemLoaded += ItemLoad;
            apiController.NoneItemLoaded += NoneItemLoad;
            apiController.EditorDrawLoaded += DrawEditorLoad;
        }

        private void DrawEditorLoad(EditorLoadType type)
        {
            DatabaseManager dbManager = GetWindow<DatabaseManager>();
            dbManager.RefreshPanel(type);
            CloseWindow();
        }

        private void NoneItemLoad(bool result)
        {
            doesNotExistDoc = result;
            CreateItemUI();
        }

        private void OnDestroy()
        {
            apiController.itemLoaded -= ItemLoad;
            apiController.NoneItemLoaded -= NoneItemLoad;
            apiController.EditorDrawLoaded -= DrawEditorLoad;
        }

        private void ItemLoad(BsonDocument document)
        {
            Debug.Log(document);
            document["_id"] = new ObjectId(document["_id"].AsString);
            doc = document;
            CreateItemUI();
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

                default:
                    break;
            };
        }

        [Obsolete]
        private void CreateDatabaseUI()
        {
            var root = rootVisualElement;
            var container = NebulaExtention.Create<VisualElement>("Container");

            var dbName = NebulaExtention.Create<TextField>();
            var dbTitle = NebulaExtention.Create<Label>("CustomLabel");
            dbTitle.text = "Database Name";

            var collectionName = NebulaExtention.Create<TextField>();
            var colletionTitle = NebulaExtention.Create<Label>("CustomLabel");
            colletionTitle.text = "Collection Name";


            container.Add(dbTitle);
            container.Add(dbName);
            container.Add(colletionTitle);
            container.Add(collectionName);


            var CreateButton = NebulaExtention.Create<Button>("CustomOperationButton");
            CreateButton.text = "+";
            CreateButton.clicked += delegate
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(
                     apiController.CreateDatabase(dbName.value, collectionName.value)
                                                             );
            };

            container.Add(CreateButton);

            var dbHelperBox = NebulaExtention.Create<HelpBox>();
            dbHelperBox.messageType = HelpBoxMessageType.Info;
            dbHelperBox.text = "Veritabanı Adı Boş Olamaz";
            var collectionHelperBox = NebulaExtention.Create<HelpBox>();
            collectionHelperBox.messageType = HelpBoxMessageType.Info;
            collectionHelperBox.text = "Collection Adı Boş Olamaz. Collection oluşturmamız zorunlu çünkü Mongodb sisteminde veritabanı oluşturabilmeniz için bir adet koleksiyon oluşturmak zorundasınız";

            container.Add(dbHelperBox);
            container.Add(collectionHelperBox);

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
            collectionNameInput.value = "Collection Name";
            container.Add(collectionNameInput);

            var createOperationButton = NebulaExtention.Create<Button>("CustomOperationButton");
            createOperationButton.text = "+";
            createOperationButton.clicked += delegate
            {
                Debug.Log(dbTitle.text);
                Debug.Log(collectionNameInput.value);
                if (!string.IsNullOrEmpty(collectionNameInput.value))
                {
                    EditorCoroutineUtility.StartCoroutineOwnerless(apiController.CreateTable(dbTitle.text, collectionNameInput.value));
                }
            };
            container.Add(createOperationButton);
            root.Add(container);
        }

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
                    var fieldValuePair = new FieldValuePair(key.Name, key.Value.ToString());
                    var propTextAndValueContainer = NebulaExtention.Create<VisualElement>("ContainerPropItem");
                    var propText = NebulaExtention.Create<TextField>("CustomPropField");
                    propText.value = key.Name;

                    var propvalue = NebulaExtention.Create<TextField>("CustomValueField");
                    propvalue.value = "";

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
                createOperationButton.text = "+";
                createOperationButton.clicked += delegate
                {
                    EditorCoroutineUtility.StartCoroutineOwnerless(apiController.CreateItem(SelectedDatabase, SelectedColection, fields));
                };

                root.Add(container);
                root.Add(createOperationButton);
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
                propFieldContainer.Add(fieldCountLabel); ;
                propFieldContainer.Add(addFieldButton);
                propFieldContainer.Add(minusFieldCount);

                container.Add(propFieldContainer);
                root.Add(container);



                List<FieldValuePair> fields = new List<FieldValuePair>();
                for (int i = 0; i < fieldCount; i++)
                {
                    var propContainer = NebulaExtention.Create<VisualElement>("ContainerPropItem");
                    var fieldValuePair = new FieldValuePair("", "");
                    var propName = NebulaExtention.Create<TextField>("CustomPropField");
                    var propValue = NebulaExtention.Create<TextField>("CustomValueField");

                    propName.RegisterValueChangedCallback(e =>
                    {
                        fieldValuePair.FieldName = e.newValue;
                    });

                    propValue.RegisterValueChangedCallback(e =>
                   {
                       fieldValuePair.UpdatedValue = e.newValue;
                   });

                    propContainer.Add(propName);
                    propContainer.Add(propValue);

                    container.Add(propContainer);
                    root.Add(container);
                    fields.Add(fieldValuePair);
                }

                var createOperationButton = NebulaExtention.Create<Button>("CustomOperationButton");
                createOperationButton.text = "Create";
                createOperationButton.clicked += delegate
                {
                    EditorCoroutineUtility.StartCoroutineOwnerless(apiController.CreateItem(SelectedDatabase, SelectedColection, fields));
                };
                root.Add(createOperationButton);
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
