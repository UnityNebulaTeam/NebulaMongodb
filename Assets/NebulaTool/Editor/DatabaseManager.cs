using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class DatabaseManager : EditorWindow
{
    private static DatabaseManager Window;
    private StyleSheet mainStyle;

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

        var root = rootVisualElement;
        root.styleSheets.Add(mainStyle);

        var Container = Create<VisualElement>("Container");
        root.Add(Container);

        var leftPanel = Create<VisualElement>("PanelStyle");

        var leftTitlePanel = Create<VisualElement>("Title");

        var databaseTitle = Create<Label>("CustomLabel");
        databaseTitle.text = "Databases";

        leftTitlePanel.Add(databaseTitle);

        var leftScrollView = Create<VisualElement>("ScrollView");

        leftPanel.Add(leftTitlePanel);
        leftPanel.Add(leftScrollView);
        Container.Add(leftPanel);

        var middlePanel = Create<VisualElement>("PanelStyle");

        var middleTitlePanel = Create<VisualElement>("Title");

        var colllectionsTitle = Create<Label>("CustomLabel");
        colllectionsTitle.text = "Collections";

        middleTitlePanel.Add(colllectionsTitle);
        var middlePanelScrollView = Create<VisualElement>("ScrollView");
        middlePanel.Add(middleTitlePanel);
        middlePanel.Add(middlePanelScrollView);

        Container.Add(middlePanel);

        var rightPanel = Create<VisualElement>("PanelStyle");

        var rightTitlePanel = Create<VisualElement>("Title");


        var rightPanelTitle = Create<Label>("CustomLabel");
        rightPanelTitle.text = "Items";
        var rightPanelScrollView = Create<VisualElement>("ScrollView");
        
        rightTitlePanel.Add(rightPanelTitle);
        rightPanel.Add(rightTitlePanel);
        rightPanel.Add(rightPanelScrollView);

        Container.Add(rightPanel);
    }



    private T Create<T>(params string[] classNames) where T : VisualElement, new()
    {
        var element = new T();
        foreach (var name in classNames)
            element.AddToClassList(name);

        return element;
    }
}
