using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
public class ApiController
{
    private const string GetDatabasesUri = "http://localhost:5135/api/Mongo/db";
    private const string GetAllCollectionsUri = "http://localhost:5135/api/Mongo/table?dbName=";
    private const string GetAllItemsUri = "http://localhost:5135/api/Mongo/item?DbName=";
    private const string PutItemUri = "";
    public event Action<List<DatabaseDto>> DatabaseListLoaded;
    public event Action<List<CollectionDto>> collectionListLoaded;
    public event Action<TableItemDto> itemListLoaded;
    public IEnumerator GetAllDatabases()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(GetDatabasesUri))
        {
            yield return request.SendWebRequest();

            if (request.result is UnityWebRequest.Result.Success)
            {
                var data = request.downloadHandler.text;
                try
                {
                    var dbList = JsonUtility.FromJson<DatabaseListWrapper>("{\"databases\":" + data + "}").databases;
                    DatabaseListLoaded?.Invoke(dbList);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Exception during JSON deserialization: {e}");
                }
            }
            else
            {
                Debug.LogError(request.error);
            }
        }
    }
    public IEnumerator GetAllCollections(string dbName)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(GetAllCollectionsUri + dbName))
        {
            yield return request.SendWebRequest();

            if (request.result is UnityWebRequest.Result.Success)
            {
                var data = request.downloadHandler.text;
                try
                {
                    var collectionList = JsonUtility.FromJson<CollectionListWrapper>("{\"collections\":" + data + "}");
                    collectionListLoaded?.Invoke(new List<CollectionDto>(collectionList.collections));
                }
                catch (Exception e)
                {
                    Debug.LogError($"Exception during JSON deserialization: {e}");
                }
            }
            else
            {
                Debug.LogError(request.error);
            }
        }
    }

    public IEnumerator GetAllItems(string dbName, string collectionName)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(GetAllItemsUri + dbName + "&TableName=" + collectionName))
        {
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                var data = request.downloadHandler.text;
                var items = BsonSerializer.Deserialize<BsonArray>(data);
                TableItemDto itemDto = new TableItemDto(items);
                itemListLoaded?.Invoke(itemDto);
            }
            else
            {
                Debug.LogError(request.error);
            }
        }
    }

    public IEnumerator UpdateItem(UpdateTableItemDto dto)
    {
        string jsonData = "";
        //TODO: putItemUri'yi değiştireceksin hatalı uri ---
        using (UnityWebRequest request = UnityWebRequest.Put(PutItemUri, jsonData))
        {
            yield return request.SendWebRequest();
            var result = request.result;
            if(result is UnityWebRequest.Result.Success)
                Debug.Log("Veri Güncellendi");
            else
                Debug.Log("Veri Güncellenmedi");
        }
    }
}

public class TableItemDto
{
    public BsonArray? Docs;

    public TableItemDto(BsonArray _items)
    {
        Docs = new();
        Docs = _items;
    }
}

public class UpdateTableItemDto
{
    public string DbName;
    public string TableName;
    public BsonDocument newDoc;
}


