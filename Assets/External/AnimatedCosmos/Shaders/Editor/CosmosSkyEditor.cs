using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CosmosSkyEditor : ShaderGUI {
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMat = materialEditor.target as Material;
        bool animationEnabled = false;
        for (int i = 0; i < properties.Length; i++)
        {
            if(properties[i].name == "_AnimOn"){
                animationEnabled = properties[i].floatValue > 0.5f;
            }
            if(!animationEnabled){
                string propName = properties[i].name;
                if (propName == "_D1Distort") continue;
                if (propName == "_D1Speed") continue;
                if (propName == "_D2Distort") continue;
                if (propName == "_D2Speed") continue;
            }
            materialEditor.ShaderProperty(properties[i], properties[i].displayName);
        }
    }
}