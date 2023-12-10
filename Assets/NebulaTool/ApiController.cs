using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
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
        private static ApiController instance;
        public static ApiController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ApiController();
                }
                return instance;
            }
        }
        public event Action<List<DatabaseDto>> DatabaseListLoaded;
        public event Action<List<CollectionDto>> collectionListLoaded;
        public event Action<TableItemDto> itemListLoaded;
        public event Action<BsonDocument> itemLoaded;
        public event Action<bool> NoneItemLoaded;
        public event Action<Action> GetTokenLoaded;
        public event Action<EditorLoadType> EditorDrawLoaded;

        #region Database

        [Obsolete]
        public IEnumerator CreateDatabase(string dbName, string _tableName)
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
                yield return request.SendWebRequest();
                var result = request.result;
                if (result is UnityWebRequest.Result.Success)
                {
                    Debug.Log($"{request.downloadHandler.text} Isimli veritabanı başarıyla oluşturuldu");
                    EditorDrawLoaded?.Invoke(EditorLoadType.Database);
                }
                else
                {
                    MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                    if (!exception.success)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                        EditorCoroutineUtility.StartCoroutineOwnerless(CreateDatabase(dbName, _tableName));
                        //TOOD:GET TOKEN
                    }
                    else
                        Debug.Log($"Veritabanı oluşturulamadı  - ApiErrorMessage {exception.Message}");
                }
            }
        }

        public IEnumerator UpdateDatabase(string _name, string newdbName)
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
                    if (!exception.success)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                        EditorCoroutineUtility.StartCoroutineOwnerless(UpdateDatabase(_name, newdbName));
                        //TOOD:GET TOKEN
                    }
                    else
                        Debug.Log($"Veritabanı Güncellenemedi  - ApiErrorMessage {exception.Message}");
                }
            }
        }

        public IEnumerator DeleteDatabase(string _dbName)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
               (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            //Custom Error Handler Test Edildi
            using (UnityWebRequest request = UnityWebRequest.Delete(NebulaURL.MongoDB.databaseURL + "?Name=" + _dbName))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
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
                    if (!exception.success)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                        EditorCoroutineUtility.StartCoroutineOwnerless(DeleteDatabase(_dbName));
                        //TOOD:GET TOKEN
                    }
                    else
                        Debug.Log($"Veritabanı silinemedi - ApiErrorMessage: {exception.Message}");
                }
            }
        }

        public IEnumerator GetAllDatabases()
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
                (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);

            using (UnityWebRequest request = UnityWebRequest.Get(NebulaURL.MongoDB.databaseURL))
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                yield return request.SendWebRequest();

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
                        EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                        EditorCoroutineUtility.StartCoroutineOwnerless(GetAllDatabases());
                    }
                    else
                    {
                        Debug.Log(request.responseCode);
                        MessageErrorException exception = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                        Debug.Log($"veritabanları alınamadı  - ApiErrorMessage {exception.Message}");
                    }
                }
            }
        }

        #endregion

        #region Collection

        [Obsolete]
        public IEnumerator CreateTable(string _dbName, string _name)
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
                    if (!exception.success)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                        EditorCoroutineUtility.StartCoroutineOwnerless(CreateTable(_dbName, _name));
                        //TOOD:GET TOKEN
                    }
                    else
                        Debug.Log($"Koleksiyon Oluşturulamadı  - ApiErrorMessage {exception.Message}");
                }
            }
        }

        public IEnumerator UpdateTable(string _dbName, string _tableName, string _newTableName)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            //Test edildi custom error handler 
            UpdateTableDto dto = new UpdateTableDto { dbName = _dbName, name = _tableName, newTableName = _newTableName };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Formatting.Indented);

            using (UnityWebRequest request = UnityWebRequest.Put(NebulaURL.MongoDB.tableURL, json))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
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
                    if (!exception.success)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                        EditorCoroutineUtility.StartCoroutineOwnerless(UpdateTable(_dbName, _tableName, _newTableName));
                        //TOOD:GET TOKEN
                    }
                    else
                        Debug.Log($"Koleksiyon Güncellenmedi  - ApiErrorMessage {exception.Message}");
                }
            }
        }

        public IEnumerator DeleteTable(string _dbName, string _tableName)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            //Custom Error Handler test edildi 
            string uri = NebulaURL.MongoDB.tableURL + "?DbName=" + _dbName + "&" + "Name=" + _tableName;
            using (UnityWebRequest request = UnityWebRequest.Delete(uri))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
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
                    if (!exception.success)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                        EditorCoroutineUtility.StartCoroutineOwnerless(DeleteTable(_dbName, _tableName));
                        //TOOD:GET TOKEN
                    }
                    else
                        Debug.Log($"Koleksiyon silinemedi  - ApiErrorMessage {exception.Message}");
                }
            }
        }

        public IEnumerator GetAllCollections(string dbName)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
                (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            //custmo handler test edildi
            using (UnityWebRequest request = UnityWebRequest.Get(NebulaURL.MongoDB.tableURL + "?dbName=" + dbName))
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
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
                    if (!exception.success)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                        EditorCoroutineUtility.StartCoroutineOwnerless(GetAllCollections(dbName));
                        //TOOD:GET TOKEN
                    }
                    else
                        Debug.Log($"koleksiyonlar alınamadı  - ApiErrorMessage {exception.Message}");
                }
            }
        }

        #endregion

        #region Item

        public IEnumerator GetAllItemsTypeBsonDocument(string dbName, string collectionName)
        {

            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);

            //Custom Handler test edildi
            using (UnityWebRequest request = UnityWebRequest.Get(NebulaURL.MongoDB.itemURL + "?DbName=" + dbName + "&TableName=" + collectionName))
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
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
                    if (!exception.success)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                        EditorCoroutineUtility.StartCoroutineOwnerless(GetAllItemsTypeBsonDocument(dbName, collectionName));
                        //TODO:GET TOKEN
                    }
                    else
                        Debug.Log($"Veri  - ApiErrorMessage {exception.Message}");
                }
            }
        }

        [Obsolete]
        public IEnumerator CreateItem(string dbName, string tableName, List<FieldValuePair> fields)
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
                    if (!exception.success)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                        EditorCoroutineUtility.StartCoroutineOwnerless(CreateItem(dbName, tableName, fields));
                        //TODO:GET TOKEn
                    }
                    else
                        Debug.Log($"Veri oluşturulamadı  - ApiErrorMessage {exception.Message}");
                }
            }
        }

        public IEnumerator UpdateItem(UpdateTableItemDto dto)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            //Custom error handler test edildi
            dto.doc["_id"] = dto.doc["_id"].ToString();
            var json = NebulaExtention.ConvertTableItemDtoToJson(dto);
            using (UnityWebRequest request = UnityWebRequest.Put(NebulaURL.MongoDB.itemURL, json))
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
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
                    if (!exception.success)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                        EditorCoroutineUtility.StartCoroutineOwnerless(UpdateItem(dto));
                        //TODO: GET TOKEN
                    }
                    else
                        Debug.Log($"Veri güncellenemedi  - ApiErrorMessage {exception.Message}");
                }
            }
        }

        public IEnumerator DeleteItem(string _dbName, string _tableName, string Id)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            string uri = NebulaURL.MongoDB.itemURL + "?DbName=" + _dbName + "&" + "Name=" + _tableName + "&" + "Id=" + Id;
            using (UnityWebRequest request = UnityWebRequest.Delete(uri))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
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
                    if (!exception.success)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                        EditorCoroutineUtility.StartCoroutineOwnerless(DeleteItem(_dbName, _tableName, Id));
                        //TODO: GET TOKEn
                    }
                    else
                    {
                        Debug.Log($"Veri silinemedi  - ApiErrorMessage {exception.Message}");
                    }
                }
            }
        }

        public IEnumerator GetAllItems(string dbName, string collectionName)
        {
            var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>(NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            //Custom error handler test edildi
            using (UnityWebRequest request = UnityWebRequest.Get(NebulaURL.MongoDB.itemURL + "?DbName=" + dbName + "&TableName=" + collectionName))
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
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
                    if (!exception.success)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                        EditorCoroutineUtility.StartCoroutineOwnerless(GetAllItems(dbName, collectionName));
                        //TODO: GET TOKEN
                    }
                    else
                    {
                        Debug.Log($"Veriler alınamadı  - ApiErrorMessage {exception.Message}");
                        Debug.LogError(request.error);
                    }
                }
            }
        }

        #endregion

        #region Api

        public IEnumerator SignUp(string _username, string _email, string _password, string _connectionURL)
        {

            // var apiConnectData = AssetDatabase.LoadAssetAtPath<ApiConnectionSO>
            //     (NebulaPath.DataPath + NebulaResourcesName.ApiConnectionData);
            var apiConnectionSO = ScriptableObject.CreateInstance<ApiConnectionSO>();


            ApiConnectionDto dto = new ApiConnectionDto
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
                yield return request.SendWebRequest();
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

                    EditorCoroutineUtility.StartCoroutineOwnerless(Login());
                    Debug.Log($"Kullanıcı oluşturuldu");

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
                    Debug.Log(request.downloadHandler.text);
                    ApiToken token = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiToken>(request.downloadHandler.text);
                    apiConnectData.userInformation.token = token.Token;
                    apiConnectData.userInformation.refreshToken = token.RefreshToken;
                }
                else
                {
                    MessageErrorException customExp = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                    if (!customExp.success)
                    {
                        Debug.Log($"APİ ERROR MESSAGE {customExp.Message}");
                        //TODO: GET TOKEN
                    }
                    else
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
                    MessageErrorException customExp = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageErrorException>(request.downloadHandler.text);
                    if (!customExp.success)
                    {

                    }
                    else
                    {

                    }
                }
            }
        }

        public IEnumerator RefreshToken()
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
                request.SetRequestHeader("Authorization", "Bearer " + apiConnectData.userInformation.token);
                yield return request.SendWebRequest();
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
                        Debug.Log($"GET TOKEN {customExp.Message}");
                        //TODO: GET TOKEN
                    }
                    else
                        Debug.Log($"Api Erro Message {customExp.Message}");
                }
            }
        }


        #endregion


    }
}
