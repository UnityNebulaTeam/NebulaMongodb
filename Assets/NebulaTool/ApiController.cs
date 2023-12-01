using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using MongoDB.Bson.IO;
using System.IO;
using UnityEditor.Overlays;
using System.Text;
public class ApiController
{
    private const string GetDatabasesUri = "http://localhost:5135/api/Mongo/db";
    private const string CreateDatabaseUri = "http://localhost:5135/api/Mongo/db";
    private const string GetAllCollectionsUri = "http://localhost:5135/api/Mongo/table?dbName=";
    private const string GetAllItemsUri = "http://localhost:5135/api/Mongo/item?DbName=";
    private const string itemUri = "http://localhost:5135/api/Mongo/item";
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
        dto.doc["_id"] = dto.doc["_id"].ToString();
        var json = ConvertTableItemDtoToJson(dto);
        using (UnityWebRequest request = UnityWebRequest.Put(itemUri, json))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                PropertyId id = Newtonsoft.Json.JsonConvert.DeserializeObject<PropertyId>(request.downloadHandler.text);
                Debug.Log($"{id.id} Numaralı Veri Güncellendi ");
            }
            else
                Debug.Log($"Veri Güncellenmedi {request.error}");
        }
    }

    [Obsolete]
    public IEnumerator CreateDatabase(string dbName, string _tableName)
    {
        /*
        WWWForm form = new WWWForm();
        form.AddField("name", dbName);
        form.AddField("tableName", tableName);
        */
        CreateDatabaseDto dto = new CreateDatabaseDto
        {
            Name = dbName,
            TableName = _tableName
        };

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
        using (UnityWebRequest request = UnityWebRequest.Post(CreateDatabaseUri, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                Debug.Log($"{request.downloadHandler.text} Isimli veritabanı başarıyla oluşturuldu");
            }
            else
            {
                Debug.Log($"Veritabanı Oluşturulamadı {request.error}");
            }
        }
    }
    static string ConvertTableItemDtoToJson(UpdateTableItemDto tableItemDto)
    {
        var bsonDocument = tableItemDto;
        var settings = new JsonWriterSettings { Indent = true };
        var jsonOutput = bsonDocument.ToJson(settings);

        return jsonOutput;
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
    public string DbName { get; set; }
    public string TableName { get; set; }
    public BsonDocument doc { get; set; }
}

public class CreateDatabaseDto
{
    public string Name;
    public string TableName;
}


public class PropertyId
{
    public string id { get; set; }
}





