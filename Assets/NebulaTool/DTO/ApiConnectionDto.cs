
namespace NebulaTool.DTO
{
    public class ApiSignUpDto
    {
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public DbInformation db { get; set; }
    }

}
