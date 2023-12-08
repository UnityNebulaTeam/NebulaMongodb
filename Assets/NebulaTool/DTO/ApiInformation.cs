using System;
using UnityEngine;

namespace NebulaTool.Editor
{
    [Serializable]
    public class ApiInformation
    {
        [field: SerializeField] public string eMail { get; set; }
        [field: SerializeField] public string userName { get; set; }
        [field: SerializeField] public string password { get; set; }
        [field: SerializeField] public string token { get; set; }
        [field: SerializeField] public string refreshToken { get; set; }
        [field: SerializeField] public DatabaseTypes dbType { get; set; }
    }
}