using UnityEngine;
using NebulaTool.DTO;

namespace NebulaTool.ScritableSO
{
    [CreateAssetMenu(fileName = "ApiConnectionSO", menuName = "Nebula/ApiConnection SO")]
    public class ApiConnectionSO : ScriptableObject
    {
        public ApiInformation userInformation;
    }
}