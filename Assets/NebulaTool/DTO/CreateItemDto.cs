using System.Collections.Generic;

namespace NebulaTool.DTO
{
    public class CreateItemDto
    {
        public string DbName { get; set; }
        public string TableName { get; set; }
        public Dictionary<string, string> Doc { get; set; }
    }
}
