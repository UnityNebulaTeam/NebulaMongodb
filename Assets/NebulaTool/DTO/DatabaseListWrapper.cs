using System;
using System.Collections.Generic;

namespace NebulaTool.DTO
{
    [Serializable]
    public class DatabaseListWrapper
    {
        public List<DatabaseDto> databases { get; set; }
    }
}