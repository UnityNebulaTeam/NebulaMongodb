using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using Formatting = Newtonsoft.Json.Formatting;
using NebulaTool.Enum;
using NebulaTool.ScritableSO;
using NebulaTool.DTO;
using NebulaTool.URL;
using NebulaTool.Path;
using NebulaTool.Extension;

namespace NebulaTool.API
{
    public class ApiController
    {
        public event Action<List<DatabaseDto>> DatabaseListLoaded;
        public event Action<List<CollectionDto>> collectionListLoaded;
        public event Action<TableItemDto> itemListLoaded;
        public event Action<EditorLoadType> EditorDrawLoaded;

        #region Database

        [Obsolete]
        public async Task CreateDatabase(string dbName, string _tableName)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
                (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);

            CreateDatabaseDto dto = new CreateDatabaseDto
            {
                Name = dbName,
                TableName = _tableName
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(NebulaURL.MongoDB.databaseURL, json))
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);

                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                var result = request.result;
                if (result is UnityWebRequest.Result.Success)
                {
                    EditorDrawLoaded?.Invoke(EditorLoadType.Database);
                }
                else
                {
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz CreateDatabase {request.responseCode}");
                        AddRefreshTokenHandler(() => CreateDatabase(dbName, _tableName));
                    }
                    else
                    {
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"Veritabanı oluşturulamadı  - ApiErrorMessage {exception.Message}");
                    }
                }
            }
        }

        public async Task UpdateDatabase(string _name, string newdbName)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
                (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);

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
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                var result = request.result;
                if (result is UnityWebRequest.Result.Success)
                {
                    Debug.Log($"{_name} veritabanı {newdbName} ismine başarıyla güncellendi");
                    EditorDrawLoaded?.Invoke(EditorLoadType.Database);
                }
                else
                {
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz CreateDatabase {request.responseCode}");
                        AddRefreshTokenHandler(() => UpdateDatabase(_name, newdbName));
                    }
                    else
                    {
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"Veritabanı Güncellenemedi  - ApiErrorMessage {exception.Message}");
                    }
                }
            }
        }

        public async Task DeleteDatabase(string _dbName)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
                (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            //Custom Error Handler Test Edildi
            using (UnityWebRequest request = UnityWebRequest.Delete(NebulaURL.MongoDB.databaseURL + "?Name=" + _dbName))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                var result = request.result;
                if (result is UnityWebRequest.Result.Success)
                {
                    Debug.Log($"{_dbName} isimli veritabanı başarıyla silindi");
                    EditorDrawLoaded?.Invoke(EditorLoadType.Database);
                }
                else
                {
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz DeleteDatabase  {request.responseCode}");
                        AddRefreshTokenHandler(() => DeleteDatabase(_dbName));
                    }
                    else
                    {
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"Veritabanı silinemedi - ApiErrorMessage: {exception.Message}");
                    }
                }
            }
        }

        public async Task GetAllDatabases()
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
                (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);

            using (UnityWebRequest request = UnityWebRequest.Get(NebulaURL.MongoDB.databaseURL))
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                if (request.result is UnityWebRequest.Result.Success)
                {
                    var data = request.downloadHandler.text;
                    var dbList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DatabaseDto>>(data);
                    DatabaseListLoaded?.Invoke(dbList);
                }
                else
                {
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz GetAllDatabases {request.responseCode}");
                        AddRefreshTokenHandler(() => GetAllDatabases());
                    }
                    else
                    {
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"veritabanları alınamadı  - ApiErrorMessage {exception.Message}");
                    }
                }
            }
        }

        #endregion

        #region Collection

        [Obsolete]
        public async Task CreateTable(string _dbName, string _name)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
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
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                request.SetRequestHeader("Content-Type", "application/json");
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                var result = request.result;
                if (result is UnityWebRequest.Result.Success)
                {
                    Debug.Log($"{_dbName} veritabanına bağlı {_name} adında yeni bir koleksiyon oluşturuldu");
                    EditorDrawLoaded?.Invoke(EditorLoadType.Table);
                }
                else
                {
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz CreateTable {request.responseCode}");
                        AddRefreshTokenHandler(() => CreateTable(_dbName, _name));
                    }
                    else
                    {
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"Koleksiyon Oluşturulamadı  - ApiErrorMessage {exception.Message}");
                    }
                }
            }
        }

        public async Task UpdateTable(string _dbName, string _tableName, string _newTableName)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            //Test edildi custom error handler 
            UpdateTableDto dto = new UpdateTableDto {dbName = _dbName, name = _tableName, newTableName = _newTableName};
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);

            using (UnityWebRequest request = UnityWebRequest.Put(NebulaURL.MongoDB.tableURL, json))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                request.SetRequestHeader("Content-Type", "application/json");
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                var result = request.result;
                if (result is UnityWebRequest.Result.Success)
                {
                    Debug.Log($"{_dbName} veritabanına bağlı {_tableName} koleksiyonu {_newTableName} olarak güncellendi");
                    EditorDrawLoaded?.Invoke(EditorLoadType.Table);
                }
                else
                {
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz UpdateTable {request.responseCode}");
                        AddRefreshTokenHandler(() => UpdateTable(_dbName, _tableName, _newTableName));
                    }
                    else
                    {
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"Koleksiyon Güncellenmedi  - ApiErrorMessage {exception.Message}");
                    }
                }
            }
        }

        public async Task DeleteTable(string _dbName, string _tableName, bool isLastCollection = false)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            //Custom Error Handler test edildi 
            string uri = NebulaURL.MongoDB.tableURL + "?DbName=" + _dbName + "&" + "Name=" + _tableName;
            using (UnityWebRequest request = UnityWebRequest.Delete(uri))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                var result = request.result;
                if (result is UnityWebRequest.Result.Success)
                {
                    Debug.Log($"{_dbName} 'e bağlı {_tableName} koleksiyonu başarıyla silindi");
                    if (isLastCollection)
                        EditorDrawLoaded?.Invoke(EditorLoadType.Database);
                    else
                        EditorDrawLoaded?.Invoke(EditorLoadType.Table);
                }
                else
                {
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz DeleteTable {request.responseCode}");
                        AddRefreshTokenHandler(() => DeleteTable(_dbName, _tableName));
                    }
                    else
                    {
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"Koleksiyon silinemedi  - ApiErrorMessage {exception.Message}");
                    }
                }
            }
        }


        public async Task GetAllCollections(string dbName)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
                (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            //custmo handler test edildi
            using (UnityWebRequest request = UnityWebRequest.Get(NebulaURL.MongoDB.tableURL + "?dbName=" + dbName))
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                if (request.result is UnityWebRequest.Result.Success)
                {
                    var collectionList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CollectionDto>>(request.downloadHandler.text);
                    collectionListLoaded?.Invoke(new List<CollectionDto>(collectionList));
                }
                else
                {
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz GetAllCollections {request.responseCode}");
                        AddRefreshTokenHandler(() => GetAllCollections(dbName));
                    }
                    else
                    {
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"koleksiyonlar alınamadı  - ApiErrorMessage {exception.Message}");
                    }
                }
            }
        }

        #endregion

        #region Item

        [Obsolete]
        public async Task CreateItem(string dbName, string tableName, List<FieldValuePair> fields)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
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
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                request.SetRequestHeader("Content-Type", "application/json");
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                var result = request.result;
                if (result is UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Veri oluşturuldu");
                    EditorDrawLoaded?.Invoke(EditorLoadType.Item);
                }
                else
                {
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz CreateItem {request.responseCode}");
                        AddRefreshTokenHandler(() => CreateItem(dbName, tableName, fields));
                    }
                    else
                    {
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"Veri oluşturulamadı  - ApiErrorMessage {exception.Message}");
                    }
                }
            }
        }

        public async Task UpdateItem(UpdateTableItemDto dto)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            //Custom error handler test edildi
            dto.doc["_id"] = dto.doc["_id"].ToString();
            var json = NebulaExtention.ConvertTableItemDtoToJson(dto);
            using (UnityWebRequest request = UnityWebRequest.Put(NebulaURL.MongoDB.itemURL, json))
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                request.SetRequestHeader("Content-Type", "application/json");
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                var result = request.result;
                if (result is UnityWebRequest.Result.Success)
                {
                    EditorDrawLoaded?.Invoke(EditorLoadType.Item);
                    Debug.Log($"{dto.doc["_id"]} Numaralı Veri Güncellendi ");
                }
                else
                {
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz UpdateItem {request.responseCode}");
                        AddRefreshTokenHandler(() => UpdateItem(dto));
                    }
                    else
                    {
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"Veri güncellenemedi  - ApiErrorMessage {exception.Message}");
                    }
                }
            }
        }

        public async Task DeleteItem(string _dbName, string _tableName, string Id)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            string uri = NebulaURL.MongoDB.itemURL + "?DbName=" + _dbName + "&" + "Name=" + _tableName + "&" + "Id=" + Id;
            using (UnityWebRequest request = UnityWebRequest.Delete(uri))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                var result = request.result;
                if (result is UnityWebRequest.Result.Success)
                {
                    EditorDrawLoaded?.Invoke(EditorLoadType.Item);
                    Debug.Log($"{_dbName} 'e bağlı {_tableName} koleksiyonu bağlı {Id} numaralı veri başarıyla silindi");
                }
                else
                {
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz DeleteItem {request.responseCode}");
                        AddRefreshTokenHandler(() => DeleteItem(_dbName, _tableName, Id));
                    }
                    else
                    {
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"Veri silinemedi  - ApiErrorMessage {exception.Message}");
                    }
                }
            }
        }

        public async Task GetAllItems(string dbName, string collectionName)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            //Custom error handler test edildi
            using (UnityWebRequest request = UnityWebRequest.Get(NebulaURL.MongoDB.itemURL + "?DbName=" + dbName + "&TableName=" + collectionName))
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();
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
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz GetAllItems {request.responseCode}");
                        AddRefreshTokenHandler(() => GetAllItems(dbName, collectionName));
                    }
                    else
                    {
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"Veriler alınamadı  - ApiErrorMessage {exception.Message}");
                    }
                }
            }
        }

        public async Task<BsonDocument> GetFirstItem(string dbName, string collectionName)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            //Custom error handler test edildi
            using (UnityWebRequest request = UnityWebRequest.Get(NebulaURL.MongoDB.itemURL + "?DbName=" + dbName + "&TableName=" + collectionName))
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();
                var result = request.result;
                if (result is UnityWebRequest.Result.Success)
                {
                    var data = request.downloadHandler.text;
                    var items = BsonSerializer.Deserialize<BsonArray>(data);
                    if (items.Count > 0)
                    {
                        var bsonDoc = items[0].AsBsonDocument;
                        return bsonDoc;
                    }
                }
                else
                {
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz GetAllItems {request.responseCode}"); 
                        AddRefreshTokenHandler(() => GetAllItems(dbName, collectionName));
                    }
                    else
                    {
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"Veriler alınamadı  - ApiErrorMessage {exception.Message}");
                        return null;
                    }
                }
            }

            return null;
        }
        
        #endregion

        #region Api
        public async Task SignUp(string _username, string _email, string _password, string _connectionURL)
        {
            var apiConnectionSO = ScriptableObject.CreateInstance<ApiConnectionSO>();

            ApiSignUpDto dto = new ApiSignUpDto
            {
                username = _username,
                email = _email,
                password = _password,
                db = new DbInformation
                {
                    connectionString = _connectionURL,
                },
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(NebulaURL.MongoDB.RegisterURL, json))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                var result = request.result;
                if (result is UnityWebRequest.Result.Success)
                {
                    AssetDatabase.CreateAsset(apiConnectionSO, NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    var data = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
                        (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
                    data.userInformation.userName = _username;
                    data.userInformation.password = _password;
                    data.userInformation.eMail = _email;
                    EditorUtility.SetDirty(data);
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = data;
                    await Login();
                    Debug.Log($"Kullanıcı oluşturuldu");
                }
                else
                {
                    MessageErrorException customExp = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                    Debug.Log($"Api Erro Message {customExp.Message}");
                }
            }
        }
        public async Task Login()
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            ApiLoginDto dto = new ApiLoginDto
            {
                email = apiConnectData.userInformation.eMail,
                password = apiConnectData.userInformation.password
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(NebulaURL.MongoDB.LoginURL, json))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

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
        public async Task RefreshToken()
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
                (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);

            ApiToken dto = new ApiToken
            {
                Token = apiConnectData.userInformation.token,
                RefreshToken = apiConnectData.userInformation.refreshToken
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);

            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(NebulaURL.MongoDB.RefreshTokenURL, json))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                if (request.result is UnityWebRequest.Result.Success)
                {
                    ApiToken tokens = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiToken>(request.downloadHandler.text);
                    apiConnectData.userInformation.token = tokens.Token;
                    apiConnectData.userInformation.refreshToken = tokens.RefreshToken;
                    EditorUtility.SetDirty(apiConnectData);
                    Debug.Log(request.downloadHandler.text);
                }
                else
                {
                    MessageErrorException customExp = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                    if (!customExp.success)
                    {
                        Debug.Log($"Token Geçersiz RefreshToken {customExp.Message}");
                        await Login();
                    }
                    else
                    {
                        Debug.Log($"Api Erro Message {customExp.Message}");
                    }
                }
            }
        }
        public async Task UpdateConnectionURL(string newconnectionURL)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
                (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);

            UpdateConnectionUrlDTO dto = new UpdateConnectionUrlDTO
            {
                connectionString = newconnectionURL
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);
            using (UnityWebRequest request = UnityWebRequest.Put(NebulaURL.MongoDB.UpdateConnectionURL, json))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                var progress = request.SendWebRequest();
                while(!progress.isDone)
                    await Task.Yield();

                if (request.result is UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Connection URL Güncellendi");
                }
                else
                {
                    if (request.responseCode is 401)
                    {
                        Debug.Log($"Token Geçersiz UpdateConnectionURL {request.responseCode}");
                        AddRefreshTokenHandler(() => UpdateConnectionURL(newconnectionURL));
                    }
                    else
                    {
                        MessageErrorException customExp = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"Api Erro Message {customExp.Message}");
                    }
                }
            }
        }

        #endregion

        #region Custom

        public async void AddRefreshTokenHandler(Func<Task> method)
        {
            Debug.Log("Login Yapıyor");
            await RefreshToken();
            Debug.Log("Login Yapıldı");

            if (method != null)
            {
                Debug.Log($"{method.Method.Name} Operasyon başladı");
                await method();
                Debug.Log($"{method.Method.Name} Operasyon bitti");
            }
        }

        #endregion
    }
}