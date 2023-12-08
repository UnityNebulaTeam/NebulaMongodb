using UnityEngine;

namespace NebulaTool.Editor
{
    [CreateAssetMenu(fileName = "ApiConnectionSO", menuName = "Nebula/ApiConnection SO")]
    public class ApiConnectionSO : ScriptableObject
    {
        public ApiInformation userInformation;
    }
}