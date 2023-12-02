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

public class ApiController
{
    private const string itemUri = "http://localhost:5135/api/Mongo/item";
    private const string dbUri = "http://localhost:5135/api/Mongo/db";
    private const string tableUri = "http://localhost:5135/api/Mongo/table";
    public event Action<List<DatabaseDto>> DatabaseListLoaded;
    public event Action<List<CollectionDto>> collectionListLoaded;
    public event Action<TableItemDto> itemListLoaded;
    public event Action<BsonDocument> itemLoaded;
    public event Action<bool> NoneItemLoaded;
    public event Action DrawLeftPanelListener;
    public event Action DrawMiddlePanelListener;
    public event Action DrawRightPanelListener;


    #region Database

    [Obsolete]
    public IEnumerator CreateDatabase(string dbName, string _tableName)
    {
        CreateDatabaseDto dto = new CreateDatabaseDto
        {
            Name = dbName,
            TableName = _tableName
        };

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
        using (UnityWebRequest request = UnityWebRequest.Post(dbUri, json))
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
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Veritabanı oluşturulamadı  - ApiErrorMessage {exception.Message}");
            }
        }
    }
    public IEnumerator UpdateDatabase(string _name, string newdbName)
    {
        //TEST EDİLDİ CUSTOM HANDLER
        UpdateDatabaseDto dto = new UpdateDatabaseDto
        {
            Name = _name,
            NewDbName = newdbName
        };
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
        using (UnityWebRequest request = UnityWebRequest.Put(dbUri, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                Debug.Log($"{_name} veritabanı {request.downloadHandler.text} ismine başarıyla güncellendi");
            }
            else
            {
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Veritabanı Güncellenemedi  - ApiErrorMessage {exception.Message}");
            }
        }
    }

    public IEnumerator DeleteDatabase(string _dbName)
    {
        //TODO: Custom Error Handler'da hata var
        using (UnityWebRequest request = UnityWebRequest.Delete(dbUri + "?Name=" + _dbName))
        {
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
                Debug.Log($"{_dbName} isimli veritabanı başarıyla silindi");
            else
            {
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Veritabanı silinemedi  - ApiErrorMessage {exception.Message}");
            }
        }
    }

    public IEnumerator GetAllDatabases()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(dbUri))
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
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"veritabanları alınamadı  - ApiErrorMessage {exception.Message}");
            }
        }
    }
    #endregion

    #region Collection

    [Obsolete]
    public IEnumerator CreateTable(string _dbName, string _name)
    {
        //Custom handler test edildi
        CreateTableDto dto = new CreateTableDto
        {
            dbName = _dbName,
            name = _name
        };

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
        using (UnityWebRequest request = UnityWebRequest.Post(tableUri, json))
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
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Koleksiyon Oluşturulamadı  - ApiErrorMessage {exception.Message}");
            }
        }

    }
    public IEnumerator UpdateTable(string _dbName, string _tableName, string _newTableName)
    {
        //Test edildi custom error handler 
        UpdateTableDto dto = new UpdateTableDto { dbName = _dbName, name = _tableName, newTableName = _newTableName };
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);

        using (UnityWebRequest request = UnityWebRequest.Put(tableUri, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
                Debug.Log($"{_dbName} veritabanına bağlı {_tableName} koleksiyonu {_newTableName} olarak güncellendi");
            else
            {
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Koleksiyon Güncellenmedi  - ApiErrorMessage {exception.Message}");
            }
        }
    }

    public IEnumerator DeleteTable(string _dbName, string _tableName)
    {
        //TODO: Custom Error Handler'da hata var
        string uri = tableUri + "?DbName=" + _dbName + "&" + "Name=" + _tableName;
        using (UnityWebRequest request = UnityWebRequest.Delete(uri))
        {
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
                Debug.Log($"{_dbName} 'e bağlı {_tableName} koleksiyonu başarıyla silindi");
            else
            {
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Koleksiyon silinemedi  - ApiErrorMessage {exception.Message}");
            }
        }
    }
    public IEnumerator GetAllCollections(string dbName)
    {
        //custmo handler test edildi
        using (UnityWebRequest request = UnityWebRequest.Get(tableUri + "?dbName=" + dbName))
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
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"koleksiyonlar alınamadı  - ApiErrorMessage {exception.Message}");
            }
        }
    }
    #endregion

    #region Item
    public IEnumerator GetAllItemsTypeBsonDocument(string dbName, string collectionName)
    {
        //Custom Handler test edildi
        using (UnityWebRequest request = UnityWebRequest.Get(itemUri + "?DbName=" + dbName + "&TableName=" + collectionName))
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
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Veri  - ApiErrorMessage {exception.Message}");
            }
        }
    }

    [Obsolete]
    public IEnumerator CreateItem(string dbName, string tableName, List<FieldValuePair> fields)
    {
        //Custom Error Handler test edildi
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
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Veri oluşturulamadı  - ApiErrorMessage {exception.Message}");
            }
        }
    }
    public IEnumerator UpdateItem(UpdateTableItemDto dto)
    {
        //Custom error handler test edildi
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
            {
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Veri güncellenemedi  - ApiErrorMessage {exception.Message}");
            }
        }
    }

    public IEnumerator DeleteItem(string _dbName, string _tableName, string Id)
    {
        string uri = itemUri + "?DbName=" + _dbName + "&" + "Name=" + _tableName + "&" + "Id=" + Id;
        using (UnityWebRequest request = UnityWebRequest.Delete(uri))
        {
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
                Debug.Log($"{_dbName} 'e bağlı {_tableName} koleksiyonu bağlı {Id} numaralı veri başarıyla silindi");
            else
            {
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Veri silinemedi  - ApiErrorMessage {exception.Message}");
            }
        }
    }
    public IEnumerator GetAllItems(string dbName, string collectionName)
    {
        //Custom error handler test edildi
        using (UnityWebRequest request = UnityWebRequest.Get(itemUri + "?DbName=" + dbName + "&TableName=" + collectionName))
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
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Veriler alınamadı  - ApiErrorMessage {exception.Message}");
                Debug.LogError(request.error);
            }
        }
    }


    #endregion
    static string ConvertTableItemDtoToJson(UpdateTableItemDto tableItemDto)
    {
        var bsonDocument = tableItemDto;
        var settings = new JsonWriterSettings { Indent = true };
        var jsonOutput = bsonDocument.ToJson(settings);

        return jsonOutput;
    }
}
