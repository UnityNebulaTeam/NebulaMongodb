using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;

public class CreateItemWindow : EditorWindow
{
    private readonly CreateItemType createType;
    public CreateItemWindow(CreateItemType _type) => createType = _type;
    /// <summary>
    /// Collection Oluştururken kullanılan constructor
    /// </summary>
    /// <param name="_type">UI için type</param>
    /// <param name="selectedDatabase">Koleksiyon hangi veritabanına bağlı olacak</param>
    public CreateItemWindow(CreateItemType _type,string selectedDatabase) => createType = _type;
    /// <summary>
    /// Item oluştururken kullanılan constructor
    /// </summary>
    /// <param name="_type">UI için tpye</param>
    /// <param name="selectedDatabase">Koleksiyonunun hangi veritabanına bağlı olduğunu belirt</param>
    /// <param name="selectedCollection">Veri hangi koleksiyon altında oluşturulacak</param>
    public CreateItemWindow(CreateItemType _type,string selectedDatabase, string selectedCollection) => createType = _type;
    private StyleSheet mainStyle;
    public void ShowWindow()
    {
        var window = GetWindow<CreateItemWindow>();
        window.titleContent = new GUIContent($"Create {createType.ToString()}");
        window.Show();
    }

    private void OnEnable()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        mainStyle = AssetDatabase.LoadAssetAtPath<StyleSO>("Assets/NebulaTool/Editor/StylesheetsData.asset").GetStyle(StyleType.CreateWindow);
        rootVisualElement.styleSheets.Add(mainStyle);
    }

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

    private void CreateDatabaseUI()
    {
        throw new NotImplementedException();
    }

    private void CreateCollectionUI()
    {
        throw new NotImplementedException();
    }

    private void CreateItemUI()
    {
        throw new NotImplementedException();
    }

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