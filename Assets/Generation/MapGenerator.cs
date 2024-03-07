using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColorMap, Mesh};

    public DrawMode drawMode;
    public const int chunkSize = 241;
    [Range(0, 6)]
    public int lod;
    public float scale;
    public float heightMultiplier;
    public AnimationCurve heightCurve;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public Region[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(chunkSize, chunkSize, seed, scale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[chunkSize * chunkSize];
        for(int x=0; x<chunkSize; x++)
        {
            for(int y=0; y<chunkSize; y++)
            {
                float currentHeight = noiseMap[x,y];
                for(int i=0; i<regions.Length;i++)
                {
                    if(currentHeight<=regions[i].height)
                    {
                        colorMap[y*chunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = GetComponent<MapDisplay>();
        if(drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureRenderer.TextureNoiseMap(noiseMap));
        }
        else if(drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureRenderer.TextureColorMap(colorMap, chunkSize, chunkSize));
        }
        else if(drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateMesh(noiseMap, heightMultiplier, heightCurve, lod), TextureRenderer.TextureColorMap(colorMap, chunkSize, chunkSize));
        }
    }

    void OnValidate()
    {
        if(lacunarity<0)
        {
            lacunarity = 0;
        }
        GenerateMap();
    }


}

[System.Serializable]
public struct Region
{
    public string name;
    public float height;
    public Color color;
}
