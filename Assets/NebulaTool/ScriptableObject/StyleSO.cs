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
            StyleType.DatabaseManagerStyle => datas.FirstOrDefault(x => x.styleType == type).styles[0],
            StyleType.SignUpStyle => datas.FirstOrDefault(x => x.styleType == type).styles[0],
            StyleType.CreateWindowStyle => datas.FirstOrDefault(x => x.styleType == type).styles[0],
            StyleType.InformationsWindowStyle => datas.FirstOrDefault(x => x.styleType == type).styles[0],
            StyleType.LoginStyle => datas.FirstOrDefault(x => x.styleType == type).styles[0],
            StyleType.UpdateDbStyle => datas.FirstOrDefault(x => x.styleType == type).styles[0],
            StyleType.ForgotStlye => datas.FirstOrDefault(x => x.styleType == type).styles[0],
            _ => throw new ArgumentNullException("Could not found this type style")
        };
    }
}
