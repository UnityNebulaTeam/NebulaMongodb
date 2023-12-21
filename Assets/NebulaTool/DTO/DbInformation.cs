using NebulaTool.Enum;

namespace NebulaTool.DTO
{
    public class DbInformation
    {
        public string connectionString { get; set; }
        public string keyIdentifier = DatabaseTypes.MONGODB.ToString();
    }
}
