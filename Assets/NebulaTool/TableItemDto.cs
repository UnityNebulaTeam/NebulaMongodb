using MongoDB.Bson;

public class TableItemDto
{
    public BsonArray Docs;
    public TableItemDto(BsonArray _items)
    {
        Docs = new();
        Docs = _items;
    }
}
