using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using NebulaTool.Enum;
using NebulaTool.DTO;
using NebulaTool.ScritableSO;
using NebulaTool.API;
using NebulaTool.Path;
using NebulaTool.Extension;

namespace NebulaTool.Window
{
    public class DatabaseManager : EditorWindow
    {
        public static DatabaseManager Window;
        private StyleSheet mainStyle;
        private IconSO icons;
        private string selectedDatabase;
        private string selectedCollection;
        private ApiController apiController = new();
        private List<DatabaseDto> databaseList = new();
        private List<CollectionDto> collectionList = new();
        private TableItemDto itemList;
        private bool isEditDB { get; set; }
        private bool isEditCollection { get; set; }
        private const string DatabaseManagerTitle = "Database Manager";

        #region DefaultFuncs

        [MenuItem("Nebula/Mongodb Manager", priority = (int) CustomWindowPriorty.DatabaseManager)]
        public static void Initialize()
        {
            if (!NebulaExtention.IsConnectionDataExist())
            {
                NebulaExtention.DisplayConnectionDataDoesnotExistMessage();
                return;
            }

            Window = GetWindow<DatabaseManager>(DatabaseManagerTitle);
            EditorCoroutineUtility.StartCoroutineOwnerless(Window.InitializeApiCoroutine());
            Window.Show();
        }

        private void OnEnable()
        {
            PrepareData();
            
            if (!string.IsNullOrEmpty(selectedDatabase) && !string.IsNullOrEmpty(selectedCollection))
                         apiController
                             .GetAllItems(selectedDatabase, selectedCollection);
          
            apiController.DatabaseListLoaded += GetDatabaseList;
            apiController.collectionListLoaded += GetCollectionList;
            apiController.itemListLoaded += GetItemList;
            apiController.EditorDrawLoaded += DrawEditorLoad;
        }

        private void OnDestroy()
        {
            apiController.DatabaseListLoaded -= GetDatabaseList;
            apiController.collectionListLoaded -= GetCollectionList;
            apiController.itemListLoaded -= GetItemList;
            apiController.EditorDrawLoaded -= DrawEditorLoad;

            EditorPrefs.DeleteAll();
        }

        // private void OnFocus()
        // {
        //     if (!string.IsNullOrEmpty(selectedDatabase) && !string.IsNullOrEmpty(selectedCollection))
        //         apiController
        //             .GetAllItems(selectedDatabase, selectedCollection);
        // }

        public void CreateGUI()
        {
            var Wrapper = InitializeRootVisualElement();
            var dbContainer = NebulaExtention.Create<VisualElement>("Container");
            dbContainer.Add(LeftPanel());
            dbContainer.Add(MiddlePanel());
            dbContainer.Add(RightPanel());

            Wrapper.Add(UpPanel());
            Wrapper.Add(dbContainer);
        }

        #endregion

        #region Event Listeners

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

        private void GetItemList(TableItemDto dto)
        {
            itemList = dto;
            CustomRepaint();
        }

        private void DrawEditorLoad(EditorLoadType editorType)
        {
            switch (editorType)
            {
                case EditorLoadType.Database:
                    apiController.GetAllDatabases();
                    break;
                case EditorLoadType.Table:
                    apiController.GetAllCollections(selectedDatabase);
                    break;
                case EditorLoadType.Item:
                    apiController.GetAllItems(selectedDatabase, selectedCollection);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(editorType), editorType, null);
            }

            CustomRepaint();
        }

        #endregion

        #region Panels

        private VisualElement InitializeRootVisualElement()
        {
            var root = rootVisualElement;
            root.styleSheets.Add(mainStyle);

            var Container = NebulaExtention.Create<VisualElement>("Wrapper");
            root.Add(Container);

            return Container;
        }

        private VisualElement UpPanel()
        {
            var upPanel = NebulaExtention.Create<VisualElement>("UpPanelStyle");

            var refreshButton = NebulaExtention.Create<Button>("CustomOperationButtonRefresh");
            refreshButton.style.backgroundImage = icons.GetIcon(IconType.Refresh).texture;
            refreshButton.clicked += delegate { Refresh(); };

            var toolTitle = NebulaExtention.Create<Label>("CustomLabel");
            toolTitle.text = "Nebula Tool Mongodb";


            upPanel.Add(refreshButton);
            upPanel.Add(toolTitle);

            return upPanel;
        }

        private VisualElement LeftPanel()
        {
            var leftPanel = NebulaExtention.Create<VisualElement>("PanelStyle");

            var leftTitlePanel = NebulaExtention.Create<VisualElement>("Title");

            var databaseTitle = NebulaExtention.Create<Label>("CustomTitle");
            databaseTitle.text = "Databases";

            var createDButton = NebulaExtention.Create<Button>("CustomOperationButtonAdd");
            createDButton.text = "+";
            createDButton.clicked += delegate
            {
                CreateItemWindow windowCreate = new CreateItemWindow(CreateItemType.Database);
                windowCreate.ShowWindow();
            };

            leftTitlePanel.Add(databaseTitle);
            leftTitlePanel.Add(createDButton);

            var leftScrollView = NebulaExtention.Create<VisualElement>("ScrollView");

            var databaseScroll = NebulaExtention.Create<ScrollView>();

            foreach (var db in databaseList)
            {
                var itemContainer = NebulaExtention.Create<VisualElement>("itemContainer");
                var buttonWrapper = NebulaExtention.Create<VisualElement>("buttonWrapper");

                if (selectedDatabase == db.name && isEditDB)
                {
                    var dbTextField = NebulaExtention.Create<TextField>("CustomTextField");
                    dbTextField.value = selectedDatabase;
                    buttonWrapper.Add(dbTextField);

                    var cancelOperationButton = NebulaExtention.Create<Button>("CustomOperationButtonCancel");
                    cancelOperationButton.style.backgroundImage = icons.GetIcon(IconType.Cancel).texture;
                    cancelOperationButton.clicked += delegate
                    {
                        isEditDB = !isEditDB;
                        CustomRepaint();
                    };


                    var updateItemOperationButton = NebulaExtention.Create<Button>("ConfirmUpdateButton");
                    updateItemOperationButton.style.backgroundImage = icons.GetIcon(IconType.Okey).texture;
                    updateItemOperationButton.clicked += delegate
                    {
                        if (selectedDatabase != dbTextField.value)
                        {
                            apiController.UpdateDatabase(selectedDatabase, dbTextField.value);
                            isEditDB = !isEditDB;
                        }
                        else
                            Debug.Log("Herhangi bir veri güncellemesi yok");
                    };

                    buttonWrapper.Add(cancelOperationButton);
                    buttonWrapper.Add(updateItemOperationButton);
                    itemContainer.Add(buttonWrapper);
                }
                else
                {
                    var databaseButton = NebulaExtention.Create<Button>("CustomItemButton");
                    databaseButton.text = $"{db.name}";
                    databaseButton.style.color = databaseButton.text == selectedDatabase ? Color.black : Color.white;
                    databaseButton.style.backgroundColor = databaseButton.text == selectedDatabase ? Color.cyan : Color.gray;
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
                            ClearEditorDataForTable();
                            //seçtiğim veritabanına bağlı koleksiyonlar gelsin diye
                            apiController.GetAllCollections(selectedDatabase);
                        }
                    };
                    buttonWrapper.Add(databaseButton);
                    var deleteOperationButtonOperation = NebulaExtention.Create<Button>("CustomOperationButtonDelete");
                    deleteOperationButtonOperation.style.backgroundImage = icons.GetIcon(IconType.Delete).texture;
                    deleteOperationButtonOperation.clicked += delegate
                    {
                        if (NebulaExtention.ShowDisplayDialogForDelete("Are You Sure for delete this database ?", "Do you want to delete this db"))
                        {
                           apiController.DeleteDatabase(db.name);
                            ClearEditorDataForDatabase();
                        }
                    };
                    var updateOperationButton = NebulaExtention.Create<Button>("CustomOperationButtonUpdate");
                    updateOperationButton.style.backgroundImage = icons.GetIcon(IconType.Update).texture;
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
            var middlePanel = NebulaExtention.Create<VisualElement>("PanelStyle");

            var middleTitlePanel = NebulaExtention.Create<VisualElement>("Title");

            var colllectionsTitle = NebulaExtention.Create<Label>("CustomTitle");
            colllectionsTitle.text = "Collections";
            var createCollectionButton = NebulaExtention.Create<Button>("CustomOperationButtonAdd");
            createCollectionButton.text = "+";
            createCollectionButton.clicked += delegate
            {
                if (string.IsNullOrEmpty(selectedDatabase))
                {
                    EditorUtility.DisplayDialog("Selected Database",
                        "You have to choose one database for this operation", "ok");
                    return;
                }
                else
                {
                    EditorPrefs.SetString("dbname", selectedDatabase);
                    CreateItemWindow windowCreate = new CreateItemWindow(CreateItemType.Collection,selectedDatabase);
                    windowCreate.ShowWindow();
                }
            };
            middleTitlePanel.Add(colllectionsTitle);
            middleTitlePanel.Add(createCollectionButton);
            var middlePanelScrollView = NebulaExtention.Create<VisualElement>("ScrollView");
            var collectionScroll = NebulaExtention.Create<ScrollView>();

            foreach (var collection in collectionList)
            {
                var itemContainer = NebulaExtention.Create<VisualElement>("itemContainer");
                if (!string.IsNullOrEmpty(selectedDatabase) && collection.name == selectedCollection && isEditCollection)
                {
                    var collectionTextField = NebulaExtention.Create<TextField>("CustomTextField");
                    collectionTextField.value = collection.name;
                    itemContainer.Add(collectionTextField);

                    var cancelOperationButton = NebulaExtention.Create<Button>("CustomOperationButtonCancel");
                    cancelOperationButton.style.backgroundImage = icons.GetIcon(IconType.Cancel).texture;
                    cancelOperationButton.clicked += delegate
                    {
                        isEditCollection = !isEditCollection;
                        CustomRepaint();
                    };


                    var updateItemOperationButton = NebulaExtention.Create<Button>("ConfirmUpdateButton");
                    updateItemOperationButton.style.backgroundImage = icons.GetIcon(IconType.Okey).texture;
                    updateItemOperationButton.clicked += delegate
                    {
                        if (selectedCollection != collectionTextField.value)
                           apiController.UpdateTable(selectedDatabase, selectedCollection, collectionTextField.value);
                    };

                    itemContainer.Add(cancelOperationButton);
                    itemContainer.Add(updateItemOperationButton);
                }
                else
                {
                    var collectionButton = NebulaExtention.Create<Button>("CustomItemButton");
                    collectionButton.text = $"{collection.name}";
                    collectionButton.style.color = collectionButton.text == selectedCollection ? Color.black : Color.white;
                    collectionButton.style.backgroundColor = collectionButton.text == selectedCollection ? Color.cyan : Color.gray;
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
                           
                                apiController.GetAllItems(selectedDatabase, selectedCollection);
                        }
                    };

                    var deleteOperationButtonOperation = NebulaExtention.Create<Button>("CustomOperationButtonDelete");
                    deleteOperationButtonOperation.style.backgroundImage = icons.GetIcon(IconType.Delete).texture;
                    deleteOperationButtonOperation.clicked += delegate
                    {
                        var msg = collectionList.Count > 1
                            ? "Are you sure delete this collection"
                            : "Are you sure delete this collection, because if you delete this collection you database delete too";
                        if (collectionList.Count > 1)
                        {
                            if (NebulaExtention.ShowDisplayDialogForDelete("Delete Collection", msg))
                                if (!string.IsNullOrEmpty(selectedDatabase))
                                {
                                    apiController.DeleteTable(selectedDatabase, collection.name);
                                    ClearEditorDataForTable();
                                }
                        }
                        else
                        {
                            if (NebulaExtention.ShowDisplayDialogForDelete("Delete Collection", msg))
                                if (!string.IsNullOrEmpty(selectedDatabase))
                                {
                                    apiController.DeleteTable(selectedDatabase, collection.name, true);
                                    ClearEditorDataForDatabase();
                                    ClearEditorDataForTable();
                                }
                        }
                    };

                    var updateOperationButton = NebulaExtention.Create<Button>("CustomOperationButtonUpdate");
                    updateOperationButton.style.backgroundImage = icons.GetIcon(IconType.Update).texture;
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
            var rightPanel = NebulaExtention.Create<VisualElement>("PanelStyle");

            var rightTitlePanel = NebulaExtention.Create<VisualElement>("Title");

            var rightPanelTitle = NebulaExtention.Create<Label>("CustomTitle");
            rightPanelTitle.text = "Items";

            var createItemButton = NebulaExtention.Create<Button>("CustomOperationButtonAdd");
            createItemButton.text = "+";
            createItemButton.clicked += delegate
            {
                if (string.IsNullOrEmpty(selectedCollection))
                {
                    EditorUtility.DisplayDialog("Null Collection", "You have to choose one collection", "Ok");
                }
                else
                {
                    CreateItemWindow windowCreate = new CreateItemWindow(CreateItemType.Item,selectedDatabase,selectedCollection);
                    windowCreate.ShowWindow();
                }
            };

            var rightPanelScrollView = NebulaExtention.Create<VisualElement>("ScrollView");
            var itemsScroll = NebulaExtention.Create<ScrollView>();

            if (itemList != null && itemList.Docs != null)
            {
                foreach (var collection in itemList.Docs)
                {
                    var document = collection.AsBsonDocument;
                    var containerWrapper = NebulaExtention.Create<VisualElement>("ContainerWrapper");
                    var itemContainer = NebulaExtention.Create<VisualElement>("CustomContainer");
                    List<FieldValuePair> fieldValues = new List<FieldValuePair>();
                    foreach (var item in document)
                    {
                        var fieldValuePair = new FieldValuePair(item.Name, item.Value.ToString());
                        if (item.Name == "_id")
                        {
                            var idLabel = NebulaExtention.Create<Label>("CustomLabel");
                            idLabel.text = $"ObjectId ({item.Value.ToString()})";
                            itemContainer.Add(idLabel);
                        }
                        else
                        {
                            var containerProp = NebulaExtention.Create<VisualElement>("ContainerPropItem");

                            var itemInputField = NebulaExtention.Create<TextField>("CustomValueField");
                            itemInputField.value = fieldValuePair.UpdatedValue;
                            itemInputField.RegisterValueChangedCallback(e => { fieldValuePair.UpdatedValue = e.newValue; });

                            var itemPropName = NebulaExtention.Create<Label>("CustomPropField");
                            itemPropName.text = item.Name.ToString();

                            containerProp.Add(itemPropName);
                            containerProp.Add(itemInputField);


                            fieldValues.Add(fieldValuePair);
                            itemContainer.Add(containerProp);
                        }

                        containerWrapper.Add(itemContainer);
                    }

                    var operationContainer = NebulaExtention.Create<VisualElement>("operationContainer");
                    var updateOperationButton = NebulaExtention.Create<Button>("CustomOperationButtonUpdate");
                    updateOperationButton.style.backgroundImage = icons.GetIcon(IconType.Update).texture;
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
                                apiController.UpdateItem(dto);
                            }
                        }
                    };

                    var deleteOperationButton = NebulaExtention.Create<Button>("CustomOperationButtonDelete");
                    deleteOperationButton.style.backgroundImage = icons.GetIcon(IconType.Delete).texture;
                    deleteOperationButton.clicked += delegate
                    {
                        if (NebulaExtention.ShowDisplayDialogForDelete("Delete Item", "Are you sure delete this Item"))
                            apiController.DeleteItem(selectedDatabase, selectedCollection, collection["_id"].AsString);
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

        #endregion

        #region CustomFunctions

        private IEnumerator InitializeApiCoroutine()
        {
            yield return apiController.GetAllDatabases();

            if (!string.IsNullOrEmpty(selectedDatabase))
                yield return apiController.GetAllCollections(selectedDatabase);

            if (!string.IsNullOrEmpty(selectedCollection))
            {
                yield return apiController.GetAllItems(selectedDatabase, selectedCollection);
            }
        }

        private void Refresh()
        {
            selectedDatabase = string.Empty;
            selectedCollection = string.Empty;
            collectionList.Clear();
            databaseList.Clear();
            itemList = null;
            EditorCoroutineUtility.StartCoroutineOwnerless(InitializeApiCoroutine());
        }


        private void ClearEditorDataForDatabase()
        {
            selectedDatabase = string.Empty;
            selectedCollection = string.Empty;
            collectionList.Clear();
            databaseList.Clear();
            itemList = null;
        }

        private void ClearEditorDataForTable()
        {
            selectedCollection = string.Empty;
            itemList = null;
        }

        public void CustomRepaint()
        {
            rootVisualElement.Clear();
            var Wrapper = InitializeRootVisualElement();

            var dbContainer = NebulaExtention.Create<VisualElement>("Container");
            dbContainer.Add(LeftPanel());
            dbContainer.Add(MiddlePanel());
            dbContainer.Add(RightPanel());

            Wrapper.Add(UpPanel());
            Wrapper.Add(dbContainer);
        }

        private void PrepareData()
        {
            mainStyle = AssetDatabase.LoadAssetAtPath<StyleSO>(NebulaPath.DataPath + NebulaResourcesName.StylesheetsDataName).GetStyle(StyleType.DatabaseManagerStyle);
            icons = AssetDatabase.LoadAssetAtPath<IconSO>(NebulaPath.DataPath + NebulaResourcesName.IconsDataName);
        }
        
        public void RefreshPanel(EditorLoadType panelType) => DrawEditorLoad(panelType);

        public BsonDocument GetFirstDoc()
        {
            if (itemList is not null && itemList.Docs.Count > 0)
            {
                Debug.Log("DOC GONDERİLİYOR");
                var doc = itemList.Docs[0].AsBsonDocument;
                return doc;
            }
                Debug.Log("DOC gonderilemedi");

            return null;
        }
        #endregion
    }
}