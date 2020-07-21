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
    #region BASE NOISES
    public static float ApplyPerlinNoise(Vector2 coord, Vector2 size, float strength,float scale, float seed)
    {
        //Should be the same in every noise
        strength *= 10;
        scale = 1 / scale;
        Vector2 scaled_coord = new Vector2(coord.x / (size.x ), (coord.y*2) / (size.y  * 2));
        float new_value = Mathf.PerlinNoise(seed + (scaled_coord.x * scale), seed + scaled_coord.y * scale) *strength;
        return new_value; 
    }
    #endregion
    #region COMPLEMENTARY NOISES
    public static float ApplyMagnitudeFilter(float current_value, float strength)
    {
        float og = current_value;
        strength *= 10;
        strength = 0.5f * strength;

        if (current_value < strength)
            current_value = (strength + (current_value - strength) *-1);

        Debug.Log(og + " -> " + current_value + " on "+ strength);
        return current_value;
    }
    #endregion
}
