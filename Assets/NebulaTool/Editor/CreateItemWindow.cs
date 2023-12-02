using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using Unity.EditorCoroutines.Editor;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

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
        mainStyle = AssetDatabase.LoadAssetAtPath<StyleSO>("Assets/NebulaTool/Editor/StylesheetsData.asset").GetStyle(StyleType.CreateWindow);
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
        var container = Create<VisualElement>("Container");

        var dbName = Create<TextField>();
        var dbTitle = Create<Label>("CustomLabel");
        dbTitle.text = "Database Name";

        var collectionName = Create<TextField>();
        var colletionTitle = Create<Label>("CustomLabel");
        colletionTitle.text = "Collection Name";


        container.Add(dbTitle);
        container.Add(dbName);
        container.Add(colletionTitle);
        container.Add(collectionName);


        var CreateButton = Create<Button>("CustomOperationButton");
        CreateButton.text = "+";
        CreateButton.clicked += delegate
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(apiController.CreateDatabase(dbName.value, collectionName.value));
            CloseWindow();
        };

        container.Add(CreateButton);

        var dbHelperBox = Create<HelpBox>();
        dbHelperBox.messageType = HelpBoxMessageType.Info;
        dbHelperBox.text = "Veritabanı Adı Boş Olamaz";
        var collectionHelperBox = Create<HelpBox>();
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
        var container = Create<VisualElement>("Container");

        var dbTitle = Create<Label>("CustomLabel");
        dbTitle.text = EditorPrefs.GetString("dbname");
        container.Add(dbTitle);

        var collectionNameInput = Create<TextField>("CustomTextField");
        collectionNameInput.value = "Collection Name";
        container.Add(collectionNameInput);

        var createOperationButton = Create<Button>("CustomOperationButton");
        createOperationButton.text = "+";
        createOperationButton.clicked += delegate
        {
            Debug.Log(dbTitle.text);
            Debug.Log(collectionNameInput.value);
            if (!string.IsNullOrEmpty(collectionNameInput.value))
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(apiController.CreateTable(dbTitle.text, collectionNameInput.value));
                CloseWindow();
            }
        };
        container.Add(createOperationButton);
        root.Add(container);
    }

    [Obsolete]
    private void CreateItemUI()
    {
        var root = rootVisualElement;
        root.Clear();
        var container = Create<VisualElement>("Container");
        if (!doesNotExistDoc)
        {
            List<FieldValuePair> fields = new List<FieldValuePair>();
            foreach (var key in doc)
            {
                var fieldValuePair = new FieldValuePair(key.Name, key.Value.ToString());
                var propTextAndValueContainer = Create<VisualElement>("ContainerPropItem");
                var propText = Create<TextField>("CustomPropField");
                propText.value = key.Name;

                var propvalue = Create<TextField>("CustomValueField");
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

            var createOperationButton = Create<Button>("CustomOperationButton");
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
            var propFieldContainer = Create<VisualElement>("ContainerPropItem");
            var fieldCountLabel = Create<Label>("CustomLabel");
            fieldCountLabel.text = $"Field Count {fieldCount}";

            var addFieldButton = Create<Button>("CustomOperationButton");
            addFieldButton.text = "+";
            addFieldButton.clicked += delegate
            {
                fieldCount++;
                CreateItemUI();
            };

            var minusFieldCount = Create<Button>("CustomOperationButton");
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
                var propContainer = Create<VisualElement>("ContainerPropItem");
                var fieldValuePair = new FieldValuePair("", "");
                var propName = Create<TextField>("CustomPropField");
                var propValue = Create<TextField>("CustomValueField");

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

            var createOperationButton = Create<Button>("CustomOperationButton");
            createOperationButton.text = "Create";
            createOperationButton.clicked += delegate
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(apiController.CreateItem(SelectedDatabase, SelectedColection, fields));
                CloseWindow();
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

    private T Create<T>(params string[] classNames) where T : VisualElement, new()
    {
        var element = new T();
        foreach (var name in classNames)
            element.AddToClassList(name);

        return element;
    }
}

public enum CreateItemType
{
    db,
    collection,
    item
}
