using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.TerrainTools;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class DatabaseManager : EditorWindow
{
    private static DatabaseManager Window;
    private StyleSheet mainStyle;
    private string selectedDatabase;
    private string selectedCollection;

    private ApiController apiController = new();

    private List<DatabaseDto> databaseList = new();
    private List<CollectionDto> collectionList = new();

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
    }

    private void OnDestroy()
    {
        apiController.DatabaseListLoaded -= GetDatabaseList;
        apiController.collectionListLoaded -= GetCollectionList;
    }

    private void GetDatabaseList(List<DatabaseDto> list)
    {
        databaseList.Clear();
        databaseList = list;
    }

    private void GetCollectionList(List<CollectionDto> list)
    {
        collectionList.Clear();
        collectionList = list;
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
            EditorCoroutineUtility.StartCoroutineOwnerless(InitializeApiCoroutine());
            CustomRepaint();
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
                    selectedDatabase = string.Empty;
                else
                    selectedDatabase = databaseButton.text;

                EditorCoroutineUtility.StartCoroutineOwnerless(InitializeApiCoroutine());
                CustomRepaint();
            };
            var deleteOperation = Create<Button>("CustomOperationButton");
            deleteOperation.text = "X";
            deleteOperation.clicked += delegate { Debug.Log(databaseButton.text); };
            var updateOperation = Create<Button>("CustomOperationButton");
            updateOperation.text = "U";
            updateOperation.clicked += delegate { Debug.Log(databaseButton.text); };
            itemContainer.Add(databaseButton);
            itemContainer.Add(deleteOperation);
            itemContainer.Add(updateOperation);

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
                    selectedCollection = collectionButton.text;
                CustomRepaint();
            };

            var deleteOperation = Create<Button>("CustomOperationButton");
            deleteOperation.text = "X";
            deleteOperation.clicked += delegate { Debug.Log(collectionButton.text); };


            var updateOperation = Create<Button>("CustomOperationButton");
            updateOperation.text = "U";
            updateOperation.clicked += delegate { Debug.Log(collectionButton.text); };



            itemContainer.Add(collectionButton);
            itemContainer.Add(deleteOperation);
            itemContainer.Add(updateOperation);

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
