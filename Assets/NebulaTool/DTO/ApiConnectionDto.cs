
public class ApiConnectionDto
{
    public string username { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public DbInformation db { get; set; }
}

public class DbInformation
{
    public string connectionString { get; set; }
    public string keyIdentifier = DatabaseTypes.MONGO.ToString();
}