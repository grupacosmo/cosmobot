using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public static class MeshGenerator
{

    public static MeshData GenerateMesh(float[,] noiseMap, float heightMultiplier, AnimationCurve heightCurve, int lod)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / -2f;

       
        int vertexIndex = 0;
        int simplification = (lod==0) ? 1 : lod*2;
        int vertciesPerLine = (width-1)/simplification+1;
        MeshData meshData = new MeshData(vertciesPerLine, vertciesPerLine);

        for(int y=0; y<height; y+=simplification)
        {
            for(int x=0; x<width; x+=simplification)
            {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(noiseMap[x, y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x/(float)width, y/(float)height);

                if(x < width-1 && y < height-1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex+vertciesPerLine+1, vertexIndex+vertciesPerLine);
                    meshData.AddTriangle(vertexIndex+vertciesPerLine+1, vertexIndex, vertexIndex+1);
                }
                vertexIndex++;
            }
        }
        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public Vector2[] uvs;
    public int[] triangles;
    int triangleID;

    public MeshData(int width, int height)
    {
        vertices = new Vector3[width * height];
        triangles = new int[(width-1)*(height-1)*6];
        uvs = new Vector2[width * height];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleID] = a;
        triangles[++triangleID] = b;
        triangles[++triangleID] = c;
        triangleID++;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
