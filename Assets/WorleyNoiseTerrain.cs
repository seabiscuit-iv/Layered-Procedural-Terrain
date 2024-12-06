using System;
using System.Collections.Generic;
using UnityEngine;

public class WorleyNoiseTerrain : MonoBehaviour
{
    Mesh mesh;

    public float scale;
    public float threshold = 0.5f;

    public float cellsize = 1.0f;

    public float xOffset = 0.0f;
    public float zOffset = 0.0f;


    private void Start() {
        mesh = new();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        // GetComponent<MeshCollider>().sharedMesh = mesh;

        RenderMesh();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }


    private void OnEnable() {
        Start();
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    enum BlockType {NONE, STONE, WATER, GRASS};

    void RenderMesh() {
        List<Vector3> verts = new();
        List<int> tris = new();

        float[,,] points = new float[160, 40, 160];

        for (int i = 0; i < 160; i++) {
            for (int j = 0; j < 40; j++) {
                for (int k = 0; k < 160; k++) {
                    Vector3 samplePos = new(i + xOffset, j, k + zOffset);
                    

                    points[i, j, k] = 1 - (Mathf.PerlinNoise(samplePos.x * scale * 0.13f, samplePos.z * scale * 0.13f) - (samplePos.y/20.0f));
                }
            }
        }

        (verts, tris) = MarchingCubes(points);

        for (int i =  0; i < verts.Count; i++) {
            verts[i] += new Vector3(xOffset, 0, zOffset);
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
    }   
    
     (List<Vector3>, List<int>) MarchingCubes(float[,,] points) {
        List<Vector3> verts = new();
        List<int> indicies = new();

        for(int x = 0; x < points.GetLength(0); x++) {
            for(int y = 0; y < points.GetLength(1); y++) {
                for(int z = 0; z < points.GetLength(2); z++) {

                    Color c = Color.black;

                    // xyz will be the bottom left corner
                    int num = 0;
                    for(int i = MarchingCubesTables.cubeCorners.Length-1; i >= 0; i--) {
                        Vector3 cube = MarchingCubesTables.cubeCorners[i];
                        float t;
                        int xOff = ((int)cube.x), yOff = ((int)cube.y), zOff = ((int)cube.z);

                        if(x + xOff >= points.GetLength(0)) {
                            xOff = 0;
                        }

                        if(y + yOff >= points.GetLength(1)) {
                            yOff = 0;
                        }

                        if(z + zOff >= points.GetLength(2)) {
                            zOff = 0;
                        }

                        t = points[x + xOff, y + yOff, z + zOff];

                        num <<= 1;
                        if (t > threshold) {
                            num += 1;
                        }
                    }

                    int[] edges = MarchingCubesTables.triTable[num];

                    for(int i = 0; i < edges.Length && edges[i] != -1; i += 3) {
                        int e00 = MarchingCubesTables.edgeConnections[edges[i]][0];
                        int e01 = MarchingCubesTables.edgeConnections[edges[i]][1];

                        int e10 = MarchingCubesTables.edgeConnections[edges[i + 1]][0];
                        int e11 = MarchingCubesTables.edgeConnections[edges[i + 1]][1];

                        int e20 = MarchingCubesTables.edgeConnections[edges[i + 2]][0];
                        int e21 = MarchingCubesTables.edgeConnections[edges[i + 2]][1];

                        Vector3 og = new(x, y, z);

                        Vector3 v00 = og + MarchingCubesTables.cubeCorners[e00];
                        Vector3 v01 = og + MarchingCubesTables.cubeCorners[e01];
                        Vector3 v10 = og + MarchingCubesTables.cubeCorners[e10];
                        Vector3 v11 = og + MarchingCubesTables.cubeCorners[e11];
                        Vector3 v20 = og + MarchingCubesTables.cubeCorners[e20];
                        Vector3 v21 = og + MarchingCubesTables.cubeCorners[e21];

                        //try and make this a weighted interpolation rather than an average
                        Vector3 v0 = (v00 + v01) / 2.0f;
                        Vector3 v1 = (v10  + v11) / 2.0f;
                        Vector3 v2 = (v20 + v21) / 2.0f;


                        int count = verts.Count;

                        verts.AddRange(new Vector3[] {
                            // v0, v1, v2,
                            v0, v1, v2,
                        });

                        indicies.AddRange(new int[] {
                            count, count+1, count+2,
                            // count+3, count+4, count+5
                        });
                    }
                }
            }   
        }

        return (verts, indicies);
    }

}

