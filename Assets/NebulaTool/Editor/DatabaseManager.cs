using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.Collections.Generic;
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
    private bool isEditDB;
    private bool isEditCollection;

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
            var buttonWrapper = Create<VisualElement>("buttonWrapper");


            if (selectedDatabase == db.name && isEditDB)
            {
                var dbTextField = Create<TextField>("CustomTextField");
                dbTextField.value = selectedDatabase;
                itemContainer.Add(dbTextField);


                var cancelOperationButton = Create<Button>("CustomOperationButtonCancel");
                cancelOperationButton.text = "C";
                cancelOperationButton.clicked += delegate
                {
                    isEditDB = !isEditDB;
                    CustomRepaint();
                };


                var updateItemOperationButton = Create<Button>("CustomOperationButton");
                updateItemOperationButton.text = "✓";
                updateItemOperationButton.clicked += delegate
                {
                    if (selectedDatabase != dbTextField.value)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(apiController.UpdateDatabase(selectedDatabase, dbTextField.value));
                        isEditDB = !isEditDB;
                        EditorCoroutineUtility.StartCoroutineOwnerless(InitializeApiCoroutine());
                    }
                    else
                        Debug.Log("Herhangi bir veri güncellemesi yok");
                };

                itemContainer.Add(cancelOperationButton);
                buttonWrapper.Add(updateItemOperationButton);
            }
            else
            {
                var databaseButton = Create<Button>("CustomItemButton");
                databaseButton.text = $"{db.name}";
                databaseButton.style.color = databaseButton.text == selectedDatabase ? Color.black : Color.white;
                databaseButton.style.backgroundColor = databaseButton.text == selectedDatabase ? Color.green : Color.gray;
                databaseButton.clicked += delegate
                {
                    if (selectedDatabase == databaseButton.text)
                    {
                        selectedDatabase = string.Empty;
                        collectionList.Clear();
                        CustomRepaint();
                    }
                    else
                    {
                        selectedDatabase = databaseButton.text;
                        EditorCoroutineUtility.StartCoroutineOwnerless(InitializeApiCoroutine());
                    }
                };
                buttonWrapper.Add(databaseButton);
                var deleteOperationButtonOperation = Create<Button>("CustomOperationButtonDelete");
                deleteOperationButtonOperation.text = "X";
                deleteOperationButtonOperation.clicked += delegate
                {
                    if (ShowDisplayDialogForDelete("Are You Sure for delete this database ?", "Do you want to delete this db"))
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(apiController.DeleteDatabase(db.name));
                        EditorCoroutineUtility.StartCoroutineOwnerless(InitializeApiCoroutine());
                        CustomRepaint();
                    }
                    else
                    {
                        Debug.Log("GO ON silme");
                    }
                };
                var updateOperationButton = Create<Button>("CustomOperationButton");
                updateOperationButton.text = "U";
                updateOperationButton.clicked += delegate
                {
                    if (!string.IsNullOrEmpty(selectedDatabase) && selectedDatabase == db.name)
                    {
                        isEditDB = !isEditDB;
                        CustomRepaint();
                    }
                };
                buttonWrapper.Add(deleteOperationButtonOperation);
                buttonWrapper.Add(updateOperationButton);
            }

            itemContainer.Add(buttonWrapper);
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
        createCollectionButton.clicked += delegate
        {
            EditorPrefs.SetString("dbname", selectedDatabase);
            CreateItemWindow windowCreate = new CreateItemWindow(CreateItemType.collection);
            windowCreate.ShowWindow();
        };
        middleTitlePanel.Add(colllectionsTitle);
        middleTitlePanel.Add(createCollectionButton);
        var middlePanelScrollView = Create<VisualElement>("ScrollView");
        var collectionScroll = Create<ScrollView>();

        foreach (var collection in collectionList)
        {
            var itemContainer = Create<VisualElement>("itemContainer");
            if (!string.IsNullOrEmpty(selectedDatabase) && collection.name == selectedCollection && isEditCollection)
            {
                var collectionTextField = Create<TextField>("CustomTextField");
                collectionTextField.value = collection.name;
                itemContainer.Add(collectionTextField);

                var cancelOperationButton = Create<Button>("CustomOperationButtonCancel");
                cancelOperationButton.text = "C";
                cancelOperationButton.clicked += delegate
                {
                    isEditCollection = !isEditCollection;
                    CustomRepaint();
                };


                var updateItemOperationButton = Create<Button>("CustomOperationButton");
                updateItemOperationButton.text = "✓";
                updateItemOperationButton.clicked += delegate
                {
                    if (selectedCollection != collectionTextField.value)
                        EditorCoroutineUtility.StartCoroutineOwnerless(apiController.UpdateTable(selectedDatabase, selectedCollection, collectionTextField.value));
                };

                itemContainer.Add(cancelOperationButton);
                itemContainer.Add(updateItemOperationButton);
            }
            else
            {
                var collectionButton = Create<Button>("CustomItemButton");
                collectionButton.text = $"{collection.name}";
                collectionButton.style.color = collectionButton.text == selectedCollection ? Color.black : Color.white;
                collectionButton.style.backgroundColor = collectionButton.text == selectedCollection ? Color.green : Color.gray;
                collectionButton.clicked += delegate
                {
                    if (selectedCollection == collectionButton.text)
                    {
                        selectedCollection = string.Empty;
                        itemList = null;
                        CustomRepaint();
                    }
                    else
                    {
                        selectedCollection = collectionButton.text;
                        EditorCoroutineUtility.StartCoroutineOwnerless(InitializeApiCoroutine());
                    }
                };

                var deleteOperationButtonOperation = Create<Button>("CustomOperationButtonDelete");
                deleteOperationButtonOperation.text = "X";
                deleteOperationButtonOperation.clicked += delegate
                {

                    if (ShowDisplayDialogForDelete("Delete Collection", "Are you sure delete this collection"))
                        if (!string.IsNullOrEmpty(selectedDatabase))
                            EditorCoroutineUtility.StartCoroutineOwnerless(apiController.DeleteTable(selectedDatabase, collection.name));
                        else
                            Debug.Log("OKEYY LETS GO");
                };

                var updateOperationButton = Create<Button>("CustomOperationButton");
                updateOperationButton.text = "U";
                updateOperationButton.clicked += delegate
                {
                    if (!string.IsNullOrEmpty(selectedDatabase) && !string.IsNullOrEmpty(selectedCollection) && selectedCollection == collection.name)
                    {
                        isEditCollection = !isEditCollection;
                        CustomRepaint();
                    }
                };

                itemContainer.Add(collectionButton);
                itemContainer.Add(deleteOperationButtonOperation);
                itemContainer.Add(updateOperationButton);

            }

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
            else
            {
                EditorPrefs.SetString("dbname", selectedDatabase);
                EditorPrefs.SetString("collectionName", selectedCollection);
                CreateItemWindow windowCreate = new CreateItemWindow(CreateItemType.item);
                windowCreate.ShowWindow();
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
                        var containerProp = Create<VisualElement>("ContainerPropItem");

                        var itemInputField = Create<TextField>("CustomValueField");
                        itemInputField.value = fieldValuePair.UpdatedValue;
                        itemInputField.RegisterValueChangedCallback(e =>
                        {
                            fieldValuePair.UpdatedValue = e.newValue;
                        });

                        var itemPropName = Create<Label>("CustomPropField");
                        itemPropName.text = item.Name.ToString();

                        containerProp.Add(itemPropName);
                        containerProp.Add(itemInputField);


                        fieldValues.Add(fieldValuePair);
                        itemContainer.Add(containerProp);

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
                        //Eğer mevcut veri üzerinde bir değişiklik yaptıysa apiye istek atsın
                        if (item.OriginalValue != item.UpdatedValue)
                        {
                            document[item.FieldName] = item.UpdatedValue;
                            UpdateTableItemDto dto = new UpdateTableItemDto();
                            dto.DbName = selectedDatabase;
                            dto.TableName = selectedCollection;
                            dto.doc = document;
                            EditorCoroutineUtility.StartCoroutineOwnerless(apiController.UpdateItem(dto));
                        }
                    }
                };

                var deleteOperationButton = Create<Button>("CustomOperationButtonDelete");
                deleteOperationButton.text = "X";
                deleteOperationButton.clicked += delegate
                {
                    if (ShowDisplayDialogForDelete("Delete Item", "Are you sure delete this Item"))
                        EditorCoroutineUtility.StartCoroutineOwnerless(apiController.DeleteTableItem(selectedDatabase, selectedCollection, collection["_id"].AsString));
                };
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


    private bool ShowDisplayDialogForDelete(string title, string msg)
    {
        var result = EditorUtility.DisplayDialog(title, msg, "ok", "cancel");
        return result;
    }


    private T Create<T>(params string[] classNames) where T : VisualElement, new()
    {
        var element = new T();
        foreach (var name in classNames)
            element.AddToClassList(name);

        return element;
    }
}
