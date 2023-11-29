using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using UnityEngine;
using UnityEngine.Networking;

public class ApiController
{
    private const string GetDatabasesUri = "http://localhost:5135/api/Mongo/db";
    private const string GetAllCollectionsUri = "http://localhost:5135/api/Mongo/table?dbName=";
    private const string GetAllItemsUri = "http://localhost:5135/api/Mongo/item?DbName=";
    public event Action<List<DatabaseDto>> DatabaseListLoaded;
    public event Action<List<CollectionDto>> collectionListLoaded;
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
                //var itemList = JsonUtility.FromJson<List<TableItemDto>>(data);
                //TODO: Deserialize Hatası alıyorum
                Debug.Log(data);
            }
            else
            {
                Debug.LogError(request.error);
            }
        }
    }
}

[Serializable]
public class TableItemDto
{
    public BsonDocument item;
}



