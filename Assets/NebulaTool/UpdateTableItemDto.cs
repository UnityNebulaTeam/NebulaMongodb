using MongoDB.Bson;

public class UpdateTableItemDto
{
    public string DbName { get; set; }
    public string TableName { get; set; }
    public BsonDocument doc { get; set; }
}
