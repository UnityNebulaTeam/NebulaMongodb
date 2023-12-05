using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "IconData", menuName = "Nebula/Create Icon Data")]
public class IconSO : ScriptableObject
{
    public List<IconData> icons;

    public Sprite GetStyle(IconType type) => type switch
    {
        IconType.Refresh => icons.FirstOrDefault(x => x.iconType == type).Icon,
        IconType.Cancel => icons.FirstOrDefault(x => x.iconType == type).Icon,
        IconType.Delete => icons.FirstOrDefault(x => x.iconType == type).Icon,
        IconType.Update => icons.FirstOrDefault(x => x.iconType == type).Icon,
        IconType.Okey => icons.FirstOrDefault(x => x.iconType == type).Icon,
        _ => throw new ArgumentNullException("Could not found this type style"),
    };
}



[Serializable]
public struct IconData
{
    public IconType iconType;
    public Sprite Icon;
}

public enum IconType
{
    Refresh,
    Cancel,
    Delete,
    Update,
    Okey
}