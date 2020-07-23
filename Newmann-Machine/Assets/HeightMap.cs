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
    public static float[] ApplyVoronoiNoise(float [] values, Vector2 size, float scale,float radius, float point_range, float strength)
    {
        float[] result = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
            result[i] = values[i];
        

        Debug.Log("Applying voronoi of scale: " + scale + " radius: " +radius +" with range of: "+ point_range + " strength: " + strength);
        //Point equal distribution
        List<Vector2> points = new List<Vector2>();
        int frequency = (int)(1 / scale);
        Debug.Log(frequency);
        //It generates one third more points than neeeded so they can me shifted around
        for (int y = 0; y < size.y * 2; y++)
        {
            if (y % frequency == 0)
                for (int x = 0; x < size.x * 2; x++)
                {
                    if (x % frequency == 0)
                    {
                        Vector2 p = new Vector2(
                            x - 2 + Random.Range(point_range / 20, point_range / 10),
                            y / 2 - 2 + Random.Range(point_range / 20, point_range / 10));
                        points.Add(p);
                    }
                }
        }

        /*for (int y = 0; y < (size.y*scale)+1; y++)
        {
            for (int x = 0; x < (size.x*scale); x++)
            {
                Vector2 p = new Vector2(x, y);

                //Distribute accordingly
                p.x = x * 1 / scale;
                p.y = y * 1 / scale ;

                p.x += size.x * scale;
                p.y += size.y * scale*2;


                points.Add(p);
            }
        }*/



        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                //For each of the values of the triangles
                if (!(x == 0 || x == 1 || x >= size.x - 2 || y == 0 || y >= size.y - 1))
                {
                    int i = (int)(y * size.x) + x;

                    Vector2 closest = points[0];
                    float distance_to_closest = Vector2.Distance(closest, new Vector2(x, y));
                    //Find the closest point
                    for (int k = 1; k < points.Count; k++)
                    {
                        if (Vector2.Distance(points[k], new Vector2(x, y)) < distance_to_closest)
                        {
                            closest = points[k];
                            distance_to_closest = Vector2.Distance(closest, new Vector2(x, y));
                        }
                    }

                    //Get the value based on the distance and the radius
                    float value = 1-Mathf.InverseLerp(0, radius, distance_to_closest);

                    //if( value <= 0)
                        //This is a low land value
                        //Could be used for interesting exotic planets

                    result[i] = result[i] + (value*strength);
                }
                
            }
        }
        return result;
    }
    public static float ApplyPerlinNoise(Vector2 coord, Vector2 size, float strength, float scale, float seed)
    {
        //Should be the same in every noise
        scale = 1 / scale;
        Vector2 scaled_coord = new Vector2(coord.x / (size.x), (coord.y * 2) / (size.y * 2));
        float new_value = Mathf.PerlinNoise(seed + (scaled_coord.x * scale), seed + scaled_coord.y * scale) * 1;
        return new_value;
    }
    #endregion
    #region COMPLEMENTARY NOISES
    public static float ApplyMagnitudeFilter(float current_value, float strength,float invert_ratio)
    {
        float og = current_value;
        strength = invert_ratio * strength;

        if (current_value < strength)
            current_value = (strength + (current_value - strength) *-1);
        return current_value;
    }
    #endregion
}
