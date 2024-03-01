using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int width, int height, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[width, height];
        float noiseX, noiseY;
        float[] halfSizes = new float[2];
        halfSizes[0] = width/2f;
        halfSizes[1] = height/2f;

        System.Random pRng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; i++)
        {
            float offsetX = pRng.Next(-10000,10000);
            float offsetY = pRng.Next(-10000,10000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;

        for(int x = 0; x<width; x++)
        {
            for(int y = 0; y<height; y++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for(int i=0; i<octaves;i++)
                {
                    noiseX = (x-halfSizes[0])*(1/scale) * frequency + octaveOffsets[i].x + offset.x;
                    noiseY = (y-halfSizes[1])*(1/scale) * frequency + octaveOffsets[i].y + offset.y;

                    float perlinResult = Mathf.PerlinNoise(noiseX, noiseY) * 2 - 1;
                    noiseHeight += perlinResult * amplitude;
                    
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > maxHeight)
                {
                    maxHeight = noiseHeight;
                }
                else if(noiseHeight < minHeight)
                {
                    minHeight = noiseHeight;
                }
                noiseMap[x,y] = noiseHeight;
                
            }
        }

        for(int x=0; x<width; x++)
        {
            for(int y=0; y<height; y++)
            {
                noiseMap[x,y] = Mathf.InverseLerp(minHeight, maxHeight, noiseMap[x,y]);
            }
        }


        return noiseMap;
    }

}
