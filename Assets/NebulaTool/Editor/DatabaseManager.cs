using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.TerrainTools;

public class DatabaseManager : EditorWindow
{
    private static DatabaseManager Window;
    private StyleSheet mainStyle;
    private string selectedDatabase;
    private string selectedCollection;

    [MenuItem("Nebula/Mongodb Manager")]
    public static void Initialize()
    {
        Window = GetWindow<DatabaseManager>("Database Manager");
        Window.minSize = new Vector2(300, 200);
        Window.Show();
    }
    private void OnEnable()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        mainStyle = AssetDatabase.LoadAssetAtPath<StyleSO>("Assets/NebulaTool/Editor/StylesheetsData.asset").GetStyle(StyleType.Manager);
    }

    public void CreateGUI()
    {
        var Container = InitializeRootVisualElement();
        Container.Add(LeftPanel());
        Container.Add(MiddlePanel());
        Container.Add(RightPanel());
    }

    public void CustomRepaint()
    {
        rootVisualElement.Clear();
        var Container = InitializeRootVisualElement();
        Container.Add(LeftPanel());
        Container.Add(MiddlePanel());
        Container.Add(RightPanel());
    }

    private VisualElement InitializeRootVisualElement()
    {
        var root = rootVisualElement;
        root.styleSheets.Add(mainStyle);

        var Container = Create<VisualElement>("Container");
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

        leftTitlePanel.Add(databaseTitle);
        leftTitlePanel.Add(createDButton);

        var leftScrollView = Create<VisualElement>("ScrollView");

        var databaseScroll = Create<ScrollView>();

        for (int i = 0; i < 50; i++)
        {
            var itemContainer = Create<VisualElement>("itemContainer");
            var databaseButton = Create<Button>("CustomItemButton");
            databaseButton.text = $"DbName  {i}";
            databaseButton.style.backgroundColor = databaseButton.text == selectedDatabase ? Color.green : Color.gray;
            databaseButton.clicked += delegate
            {
                selectedDatabase = databaseButton.text;
                CustomRepaint();
                Debug.Log(databaseButton.text);
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

        for (int i = 0; i < 50; i++)
        {
            var itemContainer = Create<VisualElement>("itemContainer");
            var collectionButton = Create<Button>("CustomItemButton");
            collectionButton.text = $"CollectionName {i}";
            collectionButton.style.backgroundColor = collectionButton.text == selectedCollection ? Color.green : Color.gray;
            collectionButton.clicked += delegate
            {
                selectedCollection = collectionButton.text;
                CustomRepaint();
                Debug.Log(collectionButton.text);
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
