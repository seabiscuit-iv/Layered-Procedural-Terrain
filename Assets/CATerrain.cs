using System;
using System.Collections;
using System.Collections.Generic;
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

        GetComponent<MeshFilter>().mesh = mesh;

        RenderMesh();
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

    enum BlockType {GRASS, WATER, STONE};

    void RenderMesh() {
        List<Vector3> verts = new();
        List<Color> colors = new();
        List<int> tris = new();

        bool[,,] points = new bool[xDimension, maxHeight, zDimension];
        BlockType[,,] blockTypes = new BlockType[xDimension, maxHeight, zDimension];
        
            
        Func<int, int, int, bool> checkIfCubeExist = (int x, int y, int z) => {
            if (x >= points.GetLength(0) || x < 0) {
                return chunker.BlockExistsAt(x, y, z);
            }
            if (y >= points.GetLength(1) || y < 0) {
                return chunker.BlockExistsAt(x, y, z);
            }

            if (z >= points.GetLength(2) || z < 0) {
                return chunker.BlockExistsAt(x, y, z);
            }

            return points[x, y, z];
        };

        for (int p = 0; p < maxHeight; p++) {
            for (int i = 0; i < xDimension; i++) {
                for (int j = 0; j < zDimension; j++) {
                    float noise = Mathf.PerlinNoise(noiseScale * (i + xOffset) * 0.6237f, noiseScale*(j + zOffset) * 0.5663f);
                    float terrainType = Mathf.PerlinNoise(terrainTypeScale * (i + xOffset) * 0.437f, terrainTypeScale*(j + zOffset) * 0.4363f);

                    if (p == 0) {
                        points[i, p, j] = true;
                        blockTypes[i, p, j] = BlockType.GRASS;
                    } else if (p == 1){
                        if(noise >= noiseThreshold) {
                            points[i, p, j] = true;
                            blockTypes[i, p, j] = BlockType.GRASS;
                        }
                    } else {

                        float probability = 0f;

                        noise -= noiseThreshold;
                        BlockType type;

                        if(terrainType < 0.3f) {
                            noise *= (terrainType + 0.7f);
                            noise -= 0.15f * p; 
                            type = BlockType.GRASS; 
                        } else if(terrainType < 0.5f) {
                            noise *= terrainType + 0.5f;
                            noise -= 0.05f * p;  
                            type = BlockType.GRASS;
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
                }
            }
        }

        // Debug.Log(mesh.vertices.Length);

        (verts, colors, tris) = RenderCubes(points, blockTypes);

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

        Func<int, int, int, bool> checkIfCubeExist = (int x, int y, int z) => {
            if (x >= points.GetLength(0) || x < 0) {
                return false;
            }

            if (y >= points.GetLength(1) || y < 0) {
                return false;
            }

            if (z >= points.GetLength(2) || z < 0) {
                return false;
            }

            return points[x, y, z];
        };

        int offset = 0;

        Action<Vector3, Vector3, Vector3, Vector3, Color> drawPlane = (Vector3 tl, Vector3 tr, Vector3 bl, Vector3 br, Color c) => {
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
        };



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
                        c = Color.green;
                    } else if (types[x, y, z] == BlockType.WATER) {
                        c = new Color(0.3f, 0.3f, 1.0f, 0.6f);
                    } else {
                        c = Color.grey;
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
}

