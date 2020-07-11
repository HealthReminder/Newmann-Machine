using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMap
{
    public static float[,] CreateHeightMap(int size_x, int size_y)
    {
        float[,] displaced_map = new float[size_x, size_y];

        for (int y = 0; y < size_y; y++)
        {
            for (int x = 0; x < size_x; x++)
            {
                displaced_map[x, y] = ApplyPerlinNoise(new Vector2(x, y), displaced_map[x,y],1);
            }
        }
        return displaced_map;
    }
    private static float ApplyPerlinNoise(Vector2 coord, float current_value, float strength)
    {
        float new_value = current_value + Mathf.PerlinNoise(coord.x/100,coord.y / 100)*strength;

        return new_value; 
    }
}
