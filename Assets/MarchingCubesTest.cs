using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubesTest : MonoBehaviour
{

    Mesh mesh;


    // Start is called before the first frame update
    void Start()
    {

        bool[,,] diamond = new bool[4,4,4];

        for(int i = 0; i < diamond.GetLength(0); i++){
            for(int j = 0; j < diamond.GetLength(1); j++) {
                for(int k = 0; k < diamond.GetLength(2); k++) {
                    diamond[i, j, k] = i == 0 || i == diamond.GetLength(0) - 1 ||
                    j == 0 || j ==  diamond.GetLength(1) ||
                    k == 0 || k == diamond.GetLength(2);
                    ;
                }
            }
        }

        var (verts, tris) = MarchingCubes(diamond);

        mesh = new Mesh() {
            vertices = verts.ToArray(),
            triangles = tris.ToArray()
        }; 

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    
    (List<Vector3>, List<int>) MarchingCubes(bool[,,] points) {
        List<Vector3> verts = new();
        List<int> indicies = new();

        for(int x = 0; x < points.GetLength(0) - 1; x++) {
            for(int y = 0; y < points.GetLength(0) - 1; y++) {
                for(int z = 0; z < points.GetLength(0) - 1; z++) {
                    // xyz will be the bottom left corner
                    int num = 0;
                    for(int i = MarchingCubesTables.cubeCorners.Length-1; i >= 0; i--) {
                        Vector3 cube = MarchingCubesTables.cubeCorners[i];
                        bool t = points[x + ((int)cube.x), y + ((int)cube.y), z + ((int)cube.z)];
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

                        int count = verts.Count;

                        verts.AddRange(new Vector3[] {
                            v0, v1, v2
                        });

                        indicies.AddRange(new int[] {
                            count, count+1, count+2
                        });
                    }
                }
            }   
        }

        return (verts, indicies);
    }
}
