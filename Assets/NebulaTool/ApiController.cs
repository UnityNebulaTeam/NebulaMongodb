using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using MongoDB.Bson.IO;
using System.Text;
using System.Linq;

public class ApiController
{
    private const string GetDatabasesUri = "http://localhost:5135/api/Mongo/db";
    private const string CreateDatabaseUri = "http://localhost:5135/api/Mongo/db";
    private const string UpdateDatabaseUri = "http://localhost:5135/api/Mongo/db";
    private const string GetAllCollectionsUri = "http://localhost:5135/api/Mongo/table?dbName=";
    private const string GetAllItemsUri = "http://localhost:5135/api/Mongo/item?DbName=";
    private const string itemUri = "http://localhost:5135/api/Mongo/item";
    private const string UpdateTableUri = "http://localhost:5135/api/Mongo/table";
    public event Action<List<DatabaseDto>> DatabaseListLoaded;
    public event Action<List<CollectionDto>> collectionListLoaded;
    public event Action<TableItemDto> itemListLoaded;
    public event Action<BsonDocument> itemLoaded;
    public event Action<bool> NoneItemLoaded;
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

    public IEnumerator GetAllItemsTypeBsonDocument(string dbName, string collectionName)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(GetAllItemsUri + dbName + "&TableName=" + collectionName))
        {
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                var data = request.downloadHandler.text;
                var items = BsonSerializer.Deserialize<BsonArray>(data);
                if (items.Count > 0)
                    itemLoaded?.Invoke(items[0].AsBsonDocument);
                else
                    NoneItemLoaded?.Invoke(true);
            }
            else
            {
                Debug.LogError(request.error);
            }
        }
    }

    [Obsolete]
    public IEnumerator CreateItem(string dbName, string tableName, List<FieldValuePair> fields)
    {
        Dictionary<string, string> docDictionary = new Dictionary<string, string>();
        foreach (var item in fields)
            docDictionary.Add(item.FieldName, item.UpdatedValue);

        CreateItemDto dto = new CreateItemDto
        {
            DbName = dbName,
            TableName = tableName,
            Doc = docDictionary
        };
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
        Debug.Log(json);
        using (UnityWebRequest request = UnityWebRequest.Post(itemUri, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                Debug.Log($"Veri oluşturuldu");
            }
            else
            {
                Debug.Log($"Veri oluşturulamadı {request.error}");
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
    public IEnumerator UpdateDatabase(string _name, string newdbName)
    {
        UpdateDatabaseDto dto = new UpdateDatabaseDto
        {
            Name = _name,
            NewDbName = newdbName
        };
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
        using (UnityWebRequest request = UnityWebRequest.Put(UpdateDatabaseUri, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                Debug.Log($"{_name} Isimli veritabanı {newdbName} ismine başarıyla güncellendi");
            }
            else
            {
                Debug.Log($"Veritabanı Güncellenemedi {request.error}");
            }
        }
    }

    public IEnumerator DeleteDatabase(string _dbName)
    {
        using (UnityWebRequest request = UnityWebRequest.Delete(UpdateDatabaseUri + "?Name=" + _dbName))
        {
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
                Debug.Log($"{_dbName} Silindi");
            else
                Debug.Log($"{_dbName} Silinemedi");
        }
    }

    public IEnumerator UpdateTable(string _dbName, string _tableName, string _newTableName)
    {
        UpdateTableDto dto = new UpdateTableDto { dbName = _dbName, name = _tableName, newTableName = _newTableName };
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);

        using (UnityWebRequest request = UnityWebRequest.Put(UpdateTableUri, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
                Debug.Log($"{_dbName} veritabanına bağlı {_tableName} koleksiyonu {_newTableName} olarak güncellendi");
            else
                Debug.LogError($"Koleksiyon Güncellenemedi HATA KODU ! : {request.error}");
        }
    }

    public IEnumerator DeleteTable(string _dbName, string _tableName)
    {
        string uri = UpdateTableUri + "?DbName=" + _dbName + "&" + "Name=" + _tableName;
        using (UnityWebRequest request = UnityWebRequest.Delete(uri))
        {
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
                Debug.Log($"{_dbName} 'e bağlı {_tableName} koleksiyonu başarıyla silindi");
            else
                Debug.Log($"{_dbName} 'e bağlı {_tableName} koleksiyonu silinemedi. HATA KODU {request.error}");
        }
    }


    public IEnumerator DeleteTableItem(string _dbName, string _tableName, string Id)
    {
        string uri = itemUri + "?DbName=" + _dbName + "&" + "Name=" + _tableName + "&" + "Id=" + Id;
        using (UnityWebRequest request = UnityWebRequest.Delete(uri))
        {
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
                Debug.Log($"{_dbName} 'e bağlı {_tableName} koleksiyonu bağlı {Id} numaralı veri başarıyla silindi");
            else
                Debug.Log($"{_dbName} 'e bağlı {_tableName} koleksiyonu bağlı {Id} numaralı veri silinemedi");
        }
    }

    [Obsolete]
    public IEnumerator CreateTable(string _dbName, string _name)
    {
        CreateTableDto dto = new CreateTableDto
        {
            dbName = _dbName,
            name = _name
        };

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
        using (UnityWebRequest request = UnityWebRequest.Post(UpdateTableUri, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                Debug.Log($"{_dbName} veritabanına bağlı {_name} adında yeni bir koleksiyon oluşturuldu");
            }
            else
            {
                Debug.LogError($"koleksiyon oluşturulamadı Hata kodu {request.error}");
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

public class UpdateDatabaseDto
{
    public string Name { get; set; }
    public string NewDbName { get; set; }
}

public class PropertyId
{
    public string id { get; set; }
}

public class UpdateTableDto
{
    public string dbName { get; set; }
    public string name { get; set; }
    public string newTableName { get; set; }
}

public class CreateTableDto
{
    public string dbName { get; set; }
    public string name { get; set; }
}

public class CreateItemDto
{
    public string DbName { get; set; }
    public string TableName { get; set; }
    public Dictionary<string, string> Doc { get; set; }
}