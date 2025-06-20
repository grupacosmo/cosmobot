using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwitch : MonoBehaviour
{
    public Material[] materials;
    int currentMatIndex = 0;
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q)){
            currentMatIndex --;
            if(currentMatIndex<0)
                currentMatIndex += materials.Length;
            updateSky();
        }
        if(Input.GetKeyDown(KeyCode.E)){
            currentMatIndex ++;
            if(currentMatIndex>=materials.Length)
                currentMatIndex -= materials.Length;
            updateSky();
        }
    }

    void updateSky(){
        RenderSettings.skybox = materials[currentMatIndex];
    }
}
