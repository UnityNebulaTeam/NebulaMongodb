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
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using NebulaTool;
using NebulaTool.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using Formatting = Newtonsoft.Json.Formatting;

public class ApiController
{
    public event Action<List<DatabaseDto>> DatabaseListLoaded;
    public event Action<List<CollectionDto>> collectionListLoaded;
    public event Action<TableItemDto> itemListLoaded;
    public event Action<BsonDocument> itemLoaded;
    public event Action<bool> NoneItemLoaded;

    public event Action<EditorLoadType> EditorDrawLoaded;

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
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(NebulaURL.MongoDB.databaseURL, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                Debug.Log($"{request.downloadHandler.text} Isimli veritabanı başarıyla oluşturuldu");
                EditorDrawLoaded?.Invoke(EditorLoadType.Database);
                Debug.Log("VERİTABANI OLUŞTU");
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
        using (UnityWebRequest request = UnityWebRequest.Put(NebulaURL.MongoDB.databaseURL, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                Debug.Log($"{_name} veritabanı {request.downloadHandler.text} ismine başarıyla güncellendi");
                EditorDrawLoaded?.Invoke(EditorLoadType.Database);
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
        //Custom Error Handler Test Edildi
        using (UnityWebRequest request = UnityWebRequest.Delete(NebulaURL.MongoDB.databaseURL + "?Name=" + _dbName))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                Debug.Log($"{_dbName} isimli veritabanı başarıyla silindi");
                EditorDrawLoaded?.Invoke(EditorLoadType.Database);
            }
            else
            {
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Veritabanı silinemedi - ApiErrorMessage: {exception.Message}");
            }
        }
    }

    public IEnumerator GetAllDatabases()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(NebulaURL.MongoDB.databaseURL))
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
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(NebulaURL.MongoDB.tableURL, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                Debug.Log($"{_dbName} veritabanına bağlı {_name} adında yeni bir koleksiyon oluşturuldu");
                EditorDrawLoaded?.Invoke(EditorLoadType.Table);
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
        UpdateTableDto dto = new UpdateTableDto {dbName = _dbName, name = _tableName, newTableName = _newTableName};
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);

        using (UnityWebRequest request = UnityWebRequest.Put(NebulaURL.MongoDB.tableURL, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                Debug.Log($"{_dbName} veritabanına bağlı {_tableName} koleksiyonu {_newTableName} olarak güncellendi");
                EditorDrawLoaded?.Invoke(EditorLoadType.Table);
            }
            else
            {
                MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Koleksiyon Güncellenmedi  - ApiErrorMessage {exception.Message}");
            }
        }
    }

    public IEnumerator DeleteTable(string _dbName, string _tableName)
    {
        //Custom Error Handler test edildi 
        string uri = NebulaURL.MongoDB.tableURL + "?DbName=" + _dbName + "&" + "Name=" + _tableName;
        using (UnityWebRequest request = UnityWebRequest.Delete(uri))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                Debug.Log($"{_dbName} 'e bağlı {_tableName} koleksiyonu başarıyla silindi");
                EditorDrawLoaded?.Invoke(EditorLoadType.Table);
            }
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
        using (UnityWebRequest request = UnityWebRequest.Get(NebulaURL.MongoDB.tableURL + "?dbName=" + dbName))
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
        using (UnityWebRequest request = UnityWebRequest.Get(NebulaURL.MongoDB.itemURL + "?DbName=" + dbName + "&TableName=" + collectionName))
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
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(NebulaURL.MongoDB.itemURL, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                Debug.Log($"Veri oluşturuldu");
                EditorDrawLoaded?.Invoke(EditorLoadType.Item);
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
        using (UnityWebRequest request = UnityWebRequest.Put(NebulaURL.MongoDB.itemURL, json))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                PropertyId id = Newtonsoft.Json.JsonConvert.DeserializeObject<PropertyId>(request.downloadHandler.text);
                EditorDrawLoaded?.Invoke(EditorLoadType.Item);
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
        string uri = NebulaURL.MongoDB.itemURL + "?DbName=" + _dbName + "&" + "Name=" + _tableName + "&" + "Id=" + Id;
        using (UnityWebRequest request = UnityWebRequest.Delete(uri))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                EditorDrawLoaded?.Invoke(EditorLoadType.Item);
                Debug.Log($"{_dbName} 'e bağlı {_tableName} koleksiyonu bağlı {Id} numaralı veri başarıyla silindi");
            }
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
        using (UnityWebRequest request = UnityWebRequest.Get(NebulaURL.MongoDB.itemURL + "?DbName=" + dbName + "&TableName=" + collectionName))
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


    public IEnumerator SignUp(string _username, string _email, string _password)
    {
        var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
            (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
        ApiConnectionDto dto = new ApiConnectionDto
        {
            username = _username,
            email = _email,
            password = _password
        };
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(NebulaURL.MongoDB.RegisterURL, json))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                Debug.Log("Connected");
                apiConnectData.userInformation.userName = _username;
                apiConnectData.userInformation.password = _password;
                apiConnectData.userInformation.eMail = _email;
            }
            else
            {
                MessageErrorException customExp = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Api Erro Message {customExp.Message}");
            }
        }
    }

    public IEnumerator Login()
    {
        var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
            (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
        ApiConnectionDto dto = new ApiConnectionDto
        {
            username = apiConnectData.userInformation.userName,
            email = apiConnectData.userInformation.eMail,
            password = apiConnectData.userInformation.password
        };
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(NebulaURL.MongoDB.LoginURL, json))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result is UnityWebRequest.Result.Success)
            {
                ApiToken token = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiToken>(request.downloadHandler.text);
                apiConnectData.userInformation.token = token.Token;
                apiConnectData.userInformation.refreshToken = token.RefreshToken;
            }
            else
            {
                MessageErrorException customExp = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Api Erro Message {customExp.Message}");
            }
        }
    }
    
    
    public IEnumerator GetUserDatabasesFromApi()
    {
        var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
            (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
        using (UnityWebRequest request = UnityWebRequest.Get(NebulaURL.MongoDB.ApiDatabaseURL))
        {
            request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
            yield return request.SendWebRequest();
            if (request.result is UnityWebRequest.Result.Success)
            {
                Debug.Log(request.downloadHandler.text);
            }
            else
            {
                //TODO: Custom Error Exception Loglama yap
            }
        }
    }

    public IEnumerator AddNewDbForApiAccount( string _keyIdentifier, string connectionString)
    {
        var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
            (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);

        CreateDbApi dto = new CreateDbApi
        {
            keyIdentifier = _keyIdentifier,
            connectionString = connectionString
        };

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(NebulaURL.MongoDB.ApiDatabaseURL, json))
        {
            request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            var result = request.result;
            if (result is UnityWebRequest.Result.Success)
            {
                CreateDbApi dto2 = Newtonsoft.Json.JsonConvert.DeserializeObject<CreateDbApi>(request.downloadHandler.text);

                Debug.Log(dto2.keyIdentifier);
                Debug.Log(dto2.connectionString);

                DatabaseTypes dbType = (DatabaseTypes) Enum.Parse(typeof(DatabaseTypes), _keyIdentifier, true);
                var dbInformation = new DatabaseInformation
                {
                    dbType = dbType,
                    dbConnectionURL = connectionString
                };

                if (apiConnectData.userInformation.databaseInformations.Contains(dbInformation))
                {
                    var data = apiConnectData.userInformation.databaseInformations.FirstOrDefault(x => x.dbType == dbType);
                    data.dbType = dbInformation.dbType;
                    data.dbConnectionURL = dbInformation.dbConnectionURL;
                }
                else
                {
                    apiConnectData.userInformation.databaseInformations.Add(dbInformation);
                }
            }
            else
            {
                MessageErrorException customExp = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                Debug.Log($"Api Error Message : {customExp.Message}");
            }
        }
    }

    static string ConvertTableItemDtoToJson(UpdateTableItemDto tableItemDto)
    {
        var bsonDocument = tableItemDto;
        var settings = new JsonWriterSettings {Indent = true};
        var jsonOutput = bsonDocument.ToJson(settings);
        return jsonOutput;
    }
}

public class ApiConnectionDto
{
    [CanBeNull] public string username { get; set; }
    [CanBeNull] public string email { get; set; }
    public string password { get; set; }
}


public class ApiToken
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}

public class CreateDbApi
{
    public string keyIdentifier { get; set; }
    public string connectionString { get; set; }
}