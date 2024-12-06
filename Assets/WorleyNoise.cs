using System;
using UnityEngine;

public class WorleyNoise
{
    // Simple pseudo-random hash function
    private static Vector3 Hash3(int x, int y, int z)
    {
        int seed = x * 73856093 ^ y * 19349663 ^ z * 83492791;
        seed = (seed << 13) ^ seed;
        float fx = (1.0f - ((seed * (seed * seed * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f);
        seed = (seed << 13) ^ seed;
        float fy = (1.0f - ((seed * (seed * seed * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f);
        seed = (seed << 13) ^ seed;
        float fz = (1.0f - ((seed * (seed * seed * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f);
        return new Vector3(fx, fy, fz);
    }

    public static float SampleWorleyNoise(Vector3 position, float cellSize)
    {
        Vector3 cellCoord = new(Mathf.Floor((position / cellSize).x), Mathf.Floor((position / cellSize).y), Mathf.Floor((position / cellSize).z));
        float minDist = float.MaxValue;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3 neighborCell = cellCoord + new Vector3(x, y, z);
                    Vector3 randomPoint = neighborCell * cellSize + Hash3((int)neighborCell.x, (int)neighborCell.y, (int)neighborCell.z) * cellSize;

                    float dist = Vector3.Distance(position, randomPoint);
                    if (dist < minDist)
                    {
                        minDist = dist;
                    }
                }
            }
        }

        return minDist / cellSize; // Normalized distance
    }
}
