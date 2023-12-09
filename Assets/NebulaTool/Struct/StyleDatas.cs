using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using NebulaTool.Enum;

namespace NebulaTool.Struct
{
    [Serializable]
    public struct StyleDatas
    {
        public StyleType styleType;
        public List<StyleSheet> styles;
    }
}
