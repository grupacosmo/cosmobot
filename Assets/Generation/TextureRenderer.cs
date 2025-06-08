using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public static class TextureRenderer
{
    public static Texture2D TextureColorMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Color[] colorGrid = new Color[width*height];


        for(int x=0; x<width; x++)
        {
            for(int y=0; y<height; y++)
            {
                colorGrid[y*width+x] = Color.Lerp(Color.black, Color.white, noiseMap[x,y]);
            }
        }

        return TextureColorMap(colorGrid, width, height);
    }
    
}
