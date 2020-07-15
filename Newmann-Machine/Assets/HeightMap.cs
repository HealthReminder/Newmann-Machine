using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMap
{
    /*
    public static float[,] CreateHeightMap(int size_x, int size_y)
    {
        float[,] displaced_map = new float[size_x, size_y];

        for (int y = 0; y < size_y; y++)
        {
            for (int x = 0; x < size_x; x++)
            {
                displaced_map[x, y] = ApplyPerlinNoise(new Vector2(x, y), new Vector2(size_x, size_y), displaced_map[x, y], 1, 1);
            }
        }
        return displaced_map;
    }
    */
    public static float ApplyPerlinNoise(Vector2 coord, Vector2 size, float strength,float scale, float seed)
    {
        //strength *= 2; // For Unity plane
        strength *= 1; // For 2048 plane
        Vector2 scaled_coord = new Vector2(coord.x * scale / (size.x ), (coord.y * scale*2) / (size.y * scale * 2));
        float new_value = Mathf.PerlinNoise(seed + (scaled_coord.x), seed + scaled_coord.y) *strength;

        return new_value; 
    }
}
