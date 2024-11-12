using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunker : MonoBehaviour
{
    public int xChunks = 4;
    public int zChunks = 4;

    public int chunkWidth = 40;
    public int chunkHeight = 40;

    public GameObject terrainPrefab;

    // Update is called once per frame
    void Start()
    {
        for(int i = 0; i < xChunks; i++) {
            for(int j = 0; j < zChunks; j++) {
                GameObject g = Instantiate(terrainPrefab);
                // g.transform.position = new Vector3(i * chunkWidth, 0, j * chunkHeight);

                CATerrain cA = g.GetComponent<CATerrain>();
                cA.xDimension = chunkWidth;
                cA.zDimension = chunkHeight;
                cA.xOffset = chunkWidth * i;
                cA.zOffset = chunkHeight * j;
                cA.chunker = this;
            }
        }
    }



    public bool BlockExistsAt(int x, int y, int z) {
        return true;
    }
}
