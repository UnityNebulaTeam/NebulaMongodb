using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.TerrainTools;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using MongoDB.Bson;

public class DatabaseManager : EditorWindow
{
    private static DatabaseManager Window;
    private StyleSheet mainStyle;
    private string selectedDatabase;
    private string selectedCollection;

    private ApiController apiController = new();

    private List<DatabaseDto> databaseList = new();
    private List<CollectionDto> collectionList = new();
    private TableItemDto itemList;

    [MenuItem("Nebula/Mongodb Manager")]
    public static void Initialize()
    {
        Window = GetWindow<DatabaseManager>("Database Manager");
        EditorCoroutineUtility.StartCoroutineOwnerless(Window.InitializeApiCoroutine());
        Window.minSize = new Vector2(300, 200);
        Window.Show();
    }

    private void OnEnable()
    {
        InitializeUI();
        apiController.DatabaseListLoaded += GetDatabaseList;
        apiController.collectionListLoaded += GetCollectionList;
        apiController.itemListLoaded += GetİtemList;
    }

    private void OnDestroy()
    {
        apiController.DatabaseListLoaded -= GetDatabaseList;
        apiController.collectionListLoaded -= GetCollectionList;
        apiController.itemListLoaded -= GetİtemList;
    }

    private void GetİtemList(TableItemDto dto)
    {
        itemList = dto;
        CustomRepaint();
    }

    private void GetDatabaseList(List<DatabaseDto> list)
    {
        databaseList.Clear();
        databaseList = list;
        CustomRepaint();
    }

    private void GetCollectionList(List<CollectionDto> list)
    {
        collectionList.Clear();
        collectionList = list;
        CustomRepaint();
    }

    private void OnFocus()
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(InitializeApiCoroutine());
    }

    private IEnumerator InitializeApiCoroutine()
    {
        yield return apiController.GetAllDatabases();

        if (!string.IsNullOrEmpty(selectedDatabase))
            yield return apiController.GetAllCollections(selectedDatabase);

        if (!string.IsNullOrEmpty(selectedCollection))
            yield return apiController.GetAllItems(selectedDatabase, selectedCollection);
    }

    private void InitializeUI()
    {
        mainStyle = AssetDatabase.LoadAssetAtPath<StyleSO>("Assets/NebulaTool/Editor/StylesheetsData.asset").GetStyle(StyleType.Manager);
    }

    public void CreateGUI()
    {
        var Wrapper = InitializeRootVisualElement();

        var dbContainer = Create<VisualElement>("Container");
        dbContainer.Add(LeftPanel());
        dbContainer.Add(MiddlePanel());
        dbContainer.Add(RightPanel());

        Wrapper.Add(UpPanel());
        Wrapper.Add(dbContainer);
    }


    private VisualElement UpPanel()
    {
        var upPanel = Create<VisualElement>("UpPanelStyle");

        var refreshButton = Create<Button>("CustomOperationButton");
        refreshButton.text = "R";
        refreshButton.clicked += delegate
        {
            selectedDatabase = string.Empty;
            selectedCollection = string.Empty;
            collectionList.Clear();
            databaseList.Clear();
            EditorCoroutineUtility.StartCoroutineOwnerless(InitializeApiCoroutine());
        };

        var toolTitle = Create<Label>("CustomLabel");
        toolTitle.text = "Nebula Tool Mongodb";


        upPanel.Add(refreshButton);
        upPanel.Add(toolTitle);

        return upPanel;
    }

    public void CustomRepaint()
    {
        rootVisualElement.Clear();
        var Wrapper = InitializeRootVisualElement();

        var dbContainer = Create<VisualElement>("Container");
        dbContainer.Add(LeftPanel());
        dbContainer.Add(MiddlePanel());
        dbContainer.Add(RightPanel());

        Wrapper.Add(UpPanel());
        Wrapper.Add(dbContainer);
    }
    private VisualElement InitializeRootVisualElement()
    {
        var root = rootVisualElement;
        root.styleSheets.Add(mainStyle);

        var Container = Create<VisualElement>("Wrapper");
        root.Add(Container);

        return Container;
    }
    private VisualElement LeftPanel()
    {
        var leftPanel = Create<VisualElement>("PanelStyle");

        var leftTitlePanel = Create<VisualElement>("Title");

        var databaseTitle = Create<Label>("CustomTitle");
        databaseTitle.text = "Databases";

        var createDButton = Create<Button>("CustomOperationButtonAdd");
        createDButton.text = "+";
        createDButton.clicked += delegate
        {
            CreateItemWindow windowCreate = new CreateItemWindow(CreateItemType.db);
            windowCreate.ShowWindow();
        };

        leftTitlePanel.Add(databaseTitle);
        leftTitlePanel.Add(createDButton);

        var leftScrollView = Create<VisualElement>("ScrollView");

        var databaseScroll = Create<ScrollView>();

        foreach (var db in databaseList)
        {
            var itemContainer = Create<VisualElement>("itemContainer");
            var databaseButton = Create<Button>("CustomItemButton");
            databaseButton.text = $"{db.name}";
            databaseButton.style.backgroundColor = databaseButton.text == selectedDatabase ? Color.green : Color.gray;
            databaseButton.clicked += delegate
            {
                if (selectedDatabase == databaseButton.text)
                {
                    selectedDatabase = string.Empty;
                    collectionList.Clear();
                }
                else
                    selectedDatabase = databaseButton.text;

                EditorCoroutineUtility.StartCoroutineOwnerless(InitializeApiCoroutine());
            };
            var deleteOperationButtonOperation = Create<Button>("CustomOperationButton");
            deleteOperationButtonOperation.text = "X";
            deleteOperationButtonOperation.clicked += delegate { Debug.Log(databaseButton.text); };
            var updateOperationButton = Create<Button>("CustomOperationButton");
            updateOperationButton.text = "U";
            updateOperationButton.clicked += delegate { Debug.Log(databaseButton.text); };
            itemContainer.Add(databaseButton);
            itemContainer.Add(deleteOperationButtonOperation);
            itemContainer.Add(updateOperationButton);

            databaseScroll.Add(itemContainer);
        }


        leftScrollView.Add(databaseScroll);



        leftPanel.Add(leftTitlePanel);
        leftPanel.Add(leftScrollView);

        return leftPanel;

    }
    private VisualElement MiddlePanel()
    {
        var middlePanel = Create<VisualElement>("PanelStyle");

        var middleTitlePanel = Create<VisualElement>("Title");

        var colllectionsTitle = Create<Label>("CustomTitle");
        colllectionsTitle.text = "Collections";
        var createCollectionButton = Create<Button>("CustomOperationButtonAdd");
        createCollectionButton.text = "+";
        middleTitlePanel.Add(colllectionsTitle);
        middleTitlePanel.Add(createCollectionButton);
        var middlePanelScrollView = Create<VisualElement>("ScrollView");
        var collectionScroll = Create<ScrollView>();

        foreach (var collection in collectionList)
        {
            var itemContainer = Create<VisualElement>("itemContainer");
            var collectionButton = Create<Button>("CustomItemButton");
            collectionButton.text = $"{collection.name}";
            collectionButton.style.backgroundColor = collectionButton.text == selectedCollection ? Color.green : Color.gray;
            collectionButton.clicked += delegate
            {
                if (selectedCollection == collectionButton.text)
                    selectedCollection = string.Empty;
                else
                {
                    selectedCollection = collectionButton.text;
                    EditorCoroutineUtility.StartCoroutineOwnerless(InitializeApiCoroutine());
                }
            };

            var deleteOperationButtonOperation = Create<Button>("CustomOperationButton");
            deleteOperationButtonOperation.text = "X";
            deleteOperationButtonOperation.clicked += delegate { Debug.Log(collectionButton.text); };


            var updateOperationButton = Create<Button>("CustomOperationButton");
            updateOperationButton.text = "U";
            updateOperationButton.clicked += delegate { Debug.Log(collectionButton.text); };

            itemContainer.Add(collectionButton);
            itemContainer.Add(deleteOperationButtonOperation);
            itemContainer.Add(updateOperationButton);

            collectionScroll.Add(itemContainer);
        }


        middlePanelScrollView.Add(collectionScroll);

        middlePanel.Add(middleTitlePanel);
        middlePanel.Add(middlePanelScrollView);

        return middlePanel;

    }
    private VisualElement RightPanel()
    {
        var rightPanel = Create<VisualElement>("PanelStyle");

        var rightTitlePanel = Create<VisualElement>("Title");

        var rightPanelTitle = Create<Label>("CustomTitle");
        rightPanelTitle.text = "Items";

        var createItemButton = Create<Button>("CustomOperationButtonAdd");
        createItemButton.text = "+";
        createItemButton.clicked += delegate
        {
            if (string.IsNullOrEmpty(selectedCollection))
            {
                EditorUtility.DisplayDialog("Null Collection", "You have to choose one collection", "Ok");
            }
        };

        var rightPanelScrollView = Create<VisualElement>("ScrollView");
        var itemsScroll = Create<ScrollView>();

        if (itemList != null && itemList.Docs != null)
        {
            foreach (var collection in itemList.Docs)
            {
                var document = collection.AsBsonDocument;
                var containerWrapper = Create<VisualElement>("ContainerWrapper");
                var itemContainer = Create<VisualElement>("CustomContainer");
                List<FieldValuePair> fieldValues = new List<FieldValuePair>();
                foreach (var item in document)
                {
                    var fieldValuePair = new FieldValuePair(item.Name, item.Value.ToString());
                    if (item.Name == "_id")
                    {
                        var idLabel = Create<Label>("CustomLabel");
                        idLabel.text = $"ObjectId ({item.Value.ToString()})";
                        itemContainer.Add(idLabel);
                    }
                    else
                    {
                        var itemInputField = Create<TextField>();
                        itemInputField.value = fieldValuePair.UpdatedValue;
                        itemInputField.RegisterValueChangedCallback(e =>
                        {
                            fieldValuePair.UpdatedValue = e.newValue;
                        });
                        fieldValues.Add(fieldValuePair);
                        itemContainer.Add(itemInputField);

                    }

                    containerWrapper.Add(itemContainer);
                }

                var operationContainer = Create<VisualElement>("operationContainer");
                var updateOperationButton = Create<Button>("CustomOperationButton");
                updateOperationButton.text = "U";
                updateOperationButton.clicked += delegate
                {
                    foreach (var item in fieldValues)
                    {
                        if (item.OriginalValue != item.UpdatedValue)
                        {
                            document[item.FieldName] = item.UpdatedValue;
                            Debug.Log($"Güncellenecek kısım {item.FieldName}");

                            UpdateTableItemDto dto = new UpdateTableItemDto();
                            dto.DbName=selectedDatabase;
                            dto.TableName=selectedCollection;
                            dto.doc=document;
                            EditorCoroutineUtility.StartCoroutineOwnerless(apiController.UpdateItem(dto));
                        }
                        else
                            Debug.Log($"Güncellenecek satır YOK! {item.FieldName}");
                    }
                };

                var deleteOperationButton = Create<Button>("CustomOperationButton");
                deleteOperationButton.text = "X";
                operationContainer.Add(updateOperationButton);
                operationContainer.Add(deleteOperationButton);
                containerWrapper.Add(operationContainer);
                itemsScroll.Add(containerWrapper);
            }
            rightPanelScrollView.Add(itemsScroll);
        }

        rightTitlePanel.Add(rightPanelTitle);
        rightTitlePanel.Add(createItemButton);

        rightPanel.Add(rightTitlePanel);
        rightPanel.Add(rightPanelScrollView);

        return rightPanel;
    }
    private T Create<T>(params string[] classNames) where T : VisualElement, new()
    {
        var element = new T();
        foreach (var name in classNames)
            element.AddToClassList(name);

        return element;
    }
}

public class FieldValuePair
{
    public string FieldName { get; }
    public string OriginalValue { get; }
    public string UpdatedValue { get; set; }

    public FieldValuePair(string fieldName, string originalValue)
    {
        FieldName = fieldName;
        OriginalValue = originalValue;
        UpdatedValue = originalValue;
    }
}
