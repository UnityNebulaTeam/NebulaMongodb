using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using Unity.EditorCoroutines.Editor;

public class CreateItemWindow : EditorWindow
{
    private readonly CreateItemType createType;
    private ApiController apiController = new();
    public CreateItemWindow(CreateItemType _type) => createType = _type;
    /// <summary>
    /// Collection Oluştururken kullanılan constructor
    /// </summary>
    /// <param name="_type">UI için type</param>
    /// <param name="selectedDatabase">Koleksiyon hangi veritabanına bağlı olacak</param>
    public CreateItemWindow(CreateItemType _type, string selectedDatabase) => createType = _type;
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
            EditorCoroutineUtility.StartCoroutineOwnerless(apiController.CreateDatabase(dbName.value,collectionName.value));
        };

        container.Add(CreateButton);

        var dbHelperBox = Create<HelpBox>();
        dbHelperBox.messageType=HelpBoxMessageType.Info;
        dbHelperBox.text="Veritabanı Adı Boş Olamaz";
        var collectionHelperBox = Create<HelpBox>();
        collectionHelperBox.messageType=HelpBoxMessageType.Info;
        collectionHelperBox.text="Collection Adı Boş Olamaz. Collection oluşturmamız zorunlu çünkü Mongodb sisteminde veritabanı oluşturabilmeniz için bir adet koleksiyon oluşturmak zorundasınız";
        
        container.Add(dbHelperBox);
        container.Add(collectionHelperBox);
        
        root.Add(container);
    }

    private void CreateCollectionUI()
    {
    }

    private void CreateItemUI()
    {
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