using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TMPro;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class CATerrain : MonoBehaviour
{
    public int xOffset, zOffset;
    public int xDimension, zDimension;
    public float noiseScale = 0.1f;

    [Range(0, 1)]
    public float noiseThreshold = 0.6f;

    [Range(0, 1)]
    public float P = 0.5f;


    public int maxHeight = 24;




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



    void RenderMesh() {
        List<Vector3> verts = new();
        List<int> tris = new();

        bool[,,] points = new bool[xDimension, maxHeight, zDimension];
        
            
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

        for (int p = 0; p < maxHeight; p++) {
            for (int i = 0; i < xDimension; i++) {
                for (int j = 0; j < zDimension; j++) {
                    float noise = Mathf.PerlinNoise(noiseScale * (i + xOffset) * 0.6237f, noiseScale*(j + zOffset) * 0.5663f);

                    if (p == 0) {
                        points[i, p, j] = true;
                    } else if (p == 1){
                        if(noise >= noiseThreshold) {
                            points[i, p, j] = true;
                        }
                    } else {
                        
                    }                    
                }
            }
        }
        


        // Debug.Log(mesh.vertices.Length);

        (verts, tris) = RenderCubes(points);

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
        mesh.RecalculateNormals();
    }   

    


    (List<Vector3>, List<int>) RenderCubes(bool[,,] points) {
        List<Vector3> verts = new();
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

        Action<Vector3, Vector3, Vector3, Vector3> drawPlane = (Vector3 tl, Vector3 tr, Vector3 bl, Vector3 br) => {
            verts.AddRange(new Vector3[] {
                tl, tr, bl, br
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

                    if (!left) {
                        drawPlane(
                            new(x - 0.5f, y + 0.5f, z - 0.5f),
                            new(x - 0.5f, y + 0.5f, z + 0.5f),
                            new(x - 0.5f, y - 0.5f, z - 0.5f),
                            new(x - 0.5f, y - 0.5f, z + 0.5f)
                        );
                    }

                    if (!right) {
                        drawPlane(
                            new(x + 0.5f, y + 0.5f, z - 0.5f),
                            new(x + 0.5f, y - 0.5f, z - 0.5f),
                            new(x + 0.5f, y + 0.5f, z + 0.5f),
                            new(x + 0.5f, y - 0.5f, z + 0.5f)
                        );
                    }


                    if (!up) {
                        drawPlane(
                            new(x + 0.5f, y + 0.5f, z - 0.5f),
                            new(x + 0.5f, y + 0.5f, z + 0.5f),
                            new(x - 0.5f, y + 0.5f, z - 0.5f),
                            new(x - 0.5f, y + 0.5f, z + 0.5f)
                        );
                    }

                    if (!down) {
                        drawPlane(
                            new(x + 0.5f, y - 0.5f, z - 0.5f),
                            new(x - 0.5f, y - 0.5f, z - 0.5f),
                            new(x + 0.5f, y - 0.5f, z + 0.5f),
                            new(x - 0.5f, y - 0.5f, z + 0.5f)
                        );
                    }


                    if (!forward) {
                        drawPlane(
                            new(x + 0.5f, y + 0.5f, z + 0.5f),
                            new(x + 0.5f, y - 0.5f, z + 0.5f),
                            new(x - 0.5f, y + 0.5f, z + 0.5f),
                            new(x - 0.5f, y - 0.5f, z + 0.5f)
                        );
                    }

                    if (!backward) {
                        drawPlane(
                            new(x + 0.5f, y + 0.5f, z - 0.5f),
                            new(x - 0.5f, y + 0.5f, z - 0.5f),
                            new(x + 0.5f, y - 0.5f, z - 0.5f),
                            new(x - 0.5f, y - 0.5f, z - 0.5f)
                        );
                    }
                }
            }
        }


        return (verts, tris);
    }
}

