using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.ExceptionServices;
using UnityEngine;

public class CATerrain : MonoBehaviour
{
    public int xOffset, zOffset;
    public int xDimension, zDimension;
    public float noiseScale = 0.1f;
    public float terrainTypeScale = 0.01f;

    [Range(0, 1)]
    public float noiseThreshold = 0.6f;

    [Range(0, 1)]
    public float P = 0.5f;


    public int maxHeight = 24;

    [HideInInspector]
    public Chunker chunker;




    Mesh mesh;


    private void Start() {
        mesh = new();

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

        if (xDimension < 1 || zDimension < 1) {
            return;
        }


        
        // RenderMesh();
    }

    enum BlockType {NONE, STONE, WATER, GRASS};

    void RenderMesh() {
        List<Vector3> verts;
        List<Color> colors;
        List<int> tris;

        bool[,,] points = new bool[xDimension, maxHeight, zDimension];
        BlockType[,,] blockTypes = new BlockType[xDimension, maxHeight, zDimension];
        float[,,] values = new float[xDimension, maxHeight, zDimension];


        bool checkIfCubeExist(int x, int y, int z)
        {
            if (x >= points.GetLength(0) || x < 0)
            {
                // return chunker.BlockExistsAt(x, y, z);
                return true;
            }
            if (y >= points.GetLength(1) || y < 0)
            {
                // return chunker.BlockExistsAt(x, y, z);
                return true;
            }

            if (z >= points.GetLength(2) || z < 0)
            {
                // return chunker.BlockExistsAt(x, y, z);
                return true;
            }

            return points[x, y, z];
        }

        for (int p = 0; p < maxHeight; p++) {
            for (int i = 0; i < xDimension; i++) {
                for (int j = 0; j < zDimension; j++) {
                    float noise = Mathf.PerlinNoise(noiseScale * (i + xOffset) * 0.6237f, noiseScale*(j + zOffset) * 0.5663f);
                    float terrainType = Mathf.PerlinNoise(terrainTypeScale * (i + xOffset) * 0.437f, terrainTypeScale*(j + zOffset) * 0.4363f);

                    if (p == 0) {
                        points[i, p, j] = true;
                        blockTypes[i, p, j] = BlockType.GRASS;
                        // blockTypes[i, p, j] = BlockType.STONE;
                        values[i, p, j] = 1;
                    } else if (p == 1){
                        if(noise >= noiseThreshold) {
                            points[i, p, j] = true;
                            blockTypes[i, p, j] = BlockType.GRASS;
                            // blockTypes[i, p, j] = BlockType.STONE;
                            values[i, p, j] = noise;
                        }
                    } else {

                        float probability = 0f;

                        noise -= noiseThreshold;
                        BlockType type;

                        if(terrainType < 0.3f) {
                            noise *= (terrainType + 0.7f);
                            noise -= 0.15f * p; 
                            type = BlockType.GRASS; 
                            // type = BlockType.STONE;
                        } else if(terrainType < 0.5f) {
                            noise *= terrainType + 0.5f;
                            noise -= 0.05f * p;  
                            type = BlockType.GRASS;
                            // type = BlockType.STONE;
                        } else {
                            // noise *= terrainType;
                            noise -= 0.015f * p;  
                            type = BlockType.STONE;
                        }

                        if (checkIfCubeExist(i, p-1, j)) {
                            probability += 0.6f;
                        }

                        if (checkIfCubeExist(i-1, p-1, j)) {
                            probability += 0.1f;
                        }
                        
                        if (checkIfCubeExist(i+1, p-1, j)) {
                            probability += 0.1f;
                        }

                        if (checkIfCubeExist(i, p-1, j-1)) {
                            probability += 0.1f;
                        }

                        if (checkIfCubeExist(i, p-1, j+1)) {
                            probability += 0.1f;
                        }

                        if(noise > ( 1 - probability)) {
                            points[i, p, j] = true;
                            blockTypes[i, p, j] = type;
                            values[i, p, j] = noise;
                        }
                    }                    
                }
            }
        }
        

        for(int i = 0; i < xDimension ; i++) {
            for (int j = 0; j < zDimension; j++) {
                if(!points[i, 1, j]) {
                    points[i, 1, j] = true;
                    blockTypes[i, 1, j] = BlockType.WATER;
                    // blockTypes[i, 1, j] = BlockType.STONE;
                }
            }
        }

        // Debug.Log(mesh.vertices.Length);

        // (verts, colors, tris) = RenderCubes(points, blockTypes);
        (verts, colors, tris) = MarchingCubes(points, blockTypes, values);

        for(int i = 0; i < verts.Count; i++) {
            verts[i] += new Vector3(xOffset, 0, zOffset);
        }

        // for(int i = 0; i < verts.Count; i++) {
        //     changeAmt = 0.25f*Mathf.Sin(Time.time + (verts[i].x * 234));
            
        //     float xChange = changeAmt * PerlinNoise.Noise(verts[i].x / xDimension * 500.234f + 654.254f, verts[i].y / 12 * 500.23f + 61.163f, verts[i].z / zDimension * 500.23f + 235.65f) - 0.5f * changeAmt;
        //     float yChange = changeAmt * PerlinNoise.Noise(verts[i].x / xDimension * 500.24325f + 25.24654f, verts[i].y / 12 * 500.23f + 2.623f, verts[i].z / zDimension * 500.23f + 2.655f) - 0.5f * changeAmt;
        //     float zChange = changeAmt * PerlinNoise.Noise(verts[i].x / xDimension * 500.23f + 64.2754f, verts[i].y / 12 * 500.23f + 67.7623f, verts[i].z / zDimension * 500.23f + 75.323f) - 0.5f * changeAmt;

        //     verts[i] += new Vector3(xChange, yChange, zChange);
        // }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
    }   

    


    (List<Vector3>, List<Color>, List<int>) RenderCubes(bool[,,] points, BlockType[,,] types) {
        List<Vector3> verts = new();
        List<Color> colors = new();
        List<int> tris = new();

        bool checkIfCubeExist(int x, int y, int z)
        {
            if (x >= points.GetLength(0) || x < 0)
            {
                return false;
            }

            if (y >= points.GetLength(1) || y < 0)
            {
                return false;
            }

            if (z >= points.GetLength(2) || z < 0)
            {
                return false;
            }

            return points[x, y, z];
        }

        int offset = 0;

        void drawPlane(Vector3 tl, Vector3 tr, Vector3 bl, Vector3 br, Color c)
        {
            verts.AddRange(new Vector3[] {
                tl, tr, bl, br
            });

            colors.AddRange(new Color[] {
                c, c, c, c
            });

            tris.AddRange(new int[] {
                2 + offset, 1 + offset, 0 + offset,
                3 + offset, 1 + offset, 2 + offset
            });

            offset += 4;
        }



        for (int x = 0; x < points.GetLength(0); x++) {
            for (int y = 0; y < points.GetLength(1); y++) {
                for (int z = 0; z < points.GetLength(2); z++) {
                    if (!points[x, y, z]) {
                        continue;
                    }

                    bool left = checkIfCubeExist(x-1, y, z);
                    bool right = checkIfCubeExist(x+1, y, z);
                    bool up = checkIfCubeExist(x, y+1, z);
                    bool down = checkIfCubeExist(x, y-1, z);
                    bool forward = checkIfCubeExist(x, y, z+1);
                    bool backward = checkIfCubeExist(x, y, z-1);

                    Color c;
                    if(types[x, y, z] == BlockType.GRASS) {
                        c = new Color(0.3f, 1.0f, 0.3f, 0.6f);
                    } else if (types[x, y, z] == BlockType.WATER) {
                        c = new Color(0.3f, 0.3f, 1.0f, 0.6f);
                    } else {
                        c = new Color(0.3f, 0.3f, 0.3f, 0.6f);
                    }

                    if (!left) {
                        drawPlane(
                            new(x - 0.5f, y + 0.5f, z - 0.5f),
                            new(x - 0.5f, y + 0.5f, z + 0.5f),
                            new(x - 0.5f, y - 0.5f, z - 0.5f),
                            new(x - 0.5f, y - 0.5f, z + 0.5f),
                            c
                        );
                    }

                    if (!right) {
                        drawPlane(
                            new(x + 0.5f, y + 0.5f, z - 0.5f),
                            new(x + 0.5f, y - 0.5f, z - 0.5f),
                            new(x + 0.5f, y + 0.5f, z + 0.5f),
                            new(x + 0.5f, y - 0.5f, z + 0.5f),
                            c
                        );
                    }


                    if (!up) {
                        drawPlane(
                            new(x + 0.5f, y + 0.5f, z - 0.5f),
                            new(x + 0.5f, y + 0.5f, z + 0.5f),
                            new(x - 0.5f, y + 0.5f, z - 0.5f),
                            new(x - 0.5f, y + 0.5f, z + 0.5f),
                            c
                        );
                    }

                    if (!down) {
                        drawPlane(
                            new(x + 0.5f, y - 0.5f, z - 0.5f),
                            new(x - 0.5f, y - 0.5f, z - 0.5f),
                            new(x + 0.5f, y - 0.5f, z + 0.5f),
                            new(x - 0.5f, y - 0.5f, z + 0.5f),
                            c
                        );
                    }


                    if (!forward) {
                        drawPlane(
                            new(x + 0.5f, y + 0.5f, z + 0.5f),
                            new(x + 0.5f, y - 0.5f, z + 0.5f),
                            new(x - 0.5f, y + 0.5f, z + 0.5f),
                            new(x - 0.5f, y - 0.5f, z + 0.5f),
                            c
                        );
                    }

                    if (!backward) {
                        drawPlane(
                            new(x + 0.5f, y + 0.5f, z - 0.5f),
                            new(x - 0.5f, y + 0.5f, z - 0.5f),
                            new(x + 0.5f, y - 0.5f, z - 0.5f),
                            new(x - 0.5f, y - 0.5f, z - 0.5f),
                            c
                        );
                    }
                }
            }
        }


        return (verts, colors, tris);
    }



    
     (List<Vector3>, List<Color>, List<int>) MarchingCubes(bool[,,] points, BlockType[,,] types, float[,,] values) {
        List<Vector3> verts = new();
        List<int> indicies = new();
        List<Color> colors = new();

        for(int x = 0; x < points.GetLength(0); x++) {
            for(int y = 0; y < points.GetLength(1); y++) {
                for(int z = 0; z < points.GetLength(2); z++) {

                    Color c = Color.black;

                    // xyz will be the bottom left corner
                    int num = 0;
                    for(int i = MarchingCubesTables.cubeCorners.Length-1; i >= 0; i--) {
                        Vector3 cube = MarchingCubesTables.cubeCorners[i];
                        bool t;
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

                        if(c == Color.black) {
                            if(types[x + xOff, y + yOff, z + zOff] == BlockType.GRASS) {
                                c = new Color(0.3f, 1.0f, 0.3f, 0.6f);
                            } else if (types[x + xOff, y + yOff, z + zOff] == BlockType.WATER) {
                                c = new Color(0.3f, 0.3f, 1.0f, 0.6f);
                            } else if (types[x + xOff, y + yOff, z + zOff] == BlockType.STONE) {
                                c = new Color(0.3f, 0.3f, 0.3f, 0.6f);
                            }
                        }

                        num <<= 1;
                        if (t) {
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

                        Vector3 og = new Vector3(x, y, z);

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
                        
                        // float val00 = values[(int)v00.x, (int)v00.y, (int)v00.z];
                        // float val01 = values[(int)v01.x, (int)v01.y, (int)v01.z];
                        // float val10 = values[(int)v10.x, (int)v10.y, (int)v10.z];
                        // float val11 = values[(int)v11.x, (int)v11.y, (int)v11.z];
                        // float val20 = values[(int)v20.x, (int)v20.y, (int)v20.z];
                        // float val21 = values[(int)v21.x, (int)v21.y, (int)v21.z];

                        // val00 /= val00 + val01;
                        // val01 /= val00 + val01;
                        // val10 /= val10 + val11;
                        // val11 /= val10 + val11;
                        // val20 /= val20 + val21;
                        // val21 /= val20 + val21;

                        // Vector3 v0 = val00*v00 + val01 * v01;
                        // Vector3 v1 = val10*v10 + val11 * v11;
                        // Vector3 v2 = val20*v20 + val21 * v21;


                        int count = verts.Count;

                        verts.AddRange(new Vector3[] {
                            // v0, v1, v2,
                            v1, v0, v2,
                        });

                        indicies.AddRange(new int[] {
                            count, count+1, count+2,
                            // count+3, count+4, count+5
                        });

                        colors.AddRange(new Color[] {
                            c, c, c, 
                            // c, c, c
                        });
                    }
                }
            }   
        }

        return (verts, colors, indicies);
    }

}

