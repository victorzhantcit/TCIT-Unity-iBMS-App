using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorMappingData", menuName = "iBMSApp/Color Mapping Data")]
public class ColorMappingData : ScriptableObject
{
    [System.Serializable]
    public class NamedColor
    {
        public Color color;
        public List<string> names;
    }

    public List<NamedColor> colorMappings;
}
