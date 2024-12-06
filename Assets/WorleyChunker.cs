using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorleyChunker : MonoBehaviour
{
    public int xChunks = 4;
    public int zChunks = 4;

    int chunkWidth = 20;
    int chunkHeight = 20;

    public GameObject terrainPrefab;

    // Update is called once per frame
    void Start()
    {
        for(int i = 0; i < xChunks; i++) {
            for(int j = 0; j < zChunks; j++) {
                Debug.Log("Loading Chunk " + i + ", " + j);

                GameObject g = Instantiate(terrainPrefab);
                // g.transform.position = new Vector3(i * chunkWidth, 0, j * chunkHeight);

                WorleyNoiseTerrain cA = g.GetComponent<WorleyNoiseTerrain>();
                cA.xOffset = chunkWidth * i;
                cA.zOffset = chunkHeight * j;
            }
        }
    }
}
