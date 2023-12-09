using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using NebulaTool.Enum;
using NebulaTool.Struct;

namespace NebulaTool.ScritableSO
{
    [CreateAssetMenu(fileName = "StylesheetsData", menuName = "Nebula/Create Stylesheets Data")]
    public class StyleSO : ScriptableObject
    {
        public List<StyleDatas> datas;
        public StyleSheet GetStyle(StyleType type) => type switch
        {
            StyleType.Manager => datas.FirstOrDefault(x => x.styleType == type).styles[0],
            StyleType.ApiConnection => datas.FirstOrDefault(x => x.styleType == type).styles[0],
            StyleType.CreateWindow => datas.FirstOrDefault(x => x.styleType == type).styles[0],
            StyleType.InformationsWindow => datas.FirstOrDefault(x => x.styleType == type).styles[0],
            _ => throw new ArgumentNullException("Could not found this type style")
        };
    }
}
