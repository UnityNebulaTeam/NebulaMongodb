using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class ApiController
{
    private const string GetDatabasesUri = "http://localhost:5135/api/Mongo/db";
    private const string GetAllCollectionsUri = "http://localhost:5135/api/Mongo/table?dbName=";
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
}
