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

    public static float[] ApplyVoronoiNoise(int seed, float[] values, Vector2 size, float scale, float point_range, float strength)
    {
        Random.InitState(seed);

        bool is_smooth = true;
        //Variables below should not change often
        float max_radius = 20f;

        float[] result = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
            result[i] = values[i];


        Debug.Log("Applying voronoi of scale: " + scale + " radius: " + max_radius + " with point range of: " + point_range);
        //Point equal distribution
        List<Vector2> points = new List<Vector2>();
        int frequency = (int)(1 / scale) + 1;
        //It generates one third more points than neeeded so they can me shifted around
        for (int y = 0; y < size.y * 2; y++)
        {
            if (y % frequency == 0)
                for (int x = 0; x < size.x * 2; x++)
                {
                    if (x % frequency == 0)
                    {
                        Vector2 p = new Vector2(
                            x - 2 + Random.Range(point_range / 2, point_range),
                            y / 2 - 2 + Random.Range(point_range / 4, point_range/2)) ;
                        points.Add(p);
                    }
                }
        }

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
                    float value;
                    if (is_smooth)
                        value = 1 - Mathf.InverseLerp(0, max_radius, distance_to_closest);
                    else
                        value = 1 - Mathf.SmoothStep(0, max_radius, distance_to_closest);



                    //if( value <= 0)
                    //This is a low land value
                    //Could be used for interesting exotic planets

                    result[i] += (value* strength);
                }

            }
        }
        return result;
    }
    public static float[] ApplyPerlinNoise(int seed, float[] values, Vector2 mesh_size, float scale, float strength)
    {
        float[] new_values = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
            new_values[i] = values[i];

        scale = 1 / scale;
        for (int y = 0; y < mesh_size.y; y++)
        {
            for (int x = 0; x < mesh_size.x; x++)
            {
                if (x == 0 || x == 1 || x >= mesh_size.x - 2 || y == 0 || y >= mesh_size.y - 1)
                {
                }
                else
                {
                    Vector2 coord = new Vector2(x, y);
                    Vector2 scaled_coord = new Vector2(coord.x / (mesh_size.x), (coord.y * 2) / (mesh_size.y * 2));
                    new_values[(int)(y * mesh_size.x) + x] += strength * Mathf.PerlinNoise((scaled_coord.x * scale) + seed, (scaled_coord.y * scale) + seed);
                }
            }
        }
        return new_values;
    }
    #endregion
    #region COMPLEMENTARY NOISES
    public static float[] ApplySquarePadding(float[] values, Vector2 mesh_size, int size)
    {
        //This filter affects the existing normalized values of the mesh triangle array

        //Confirm if is closest side
        //If in-between keep one
        //If not the closest side leave
        //If it is closest size change value

        if (mesh_size.x < size || mesh_size.y < size)
            Debug.LogError("Padding size cannot be greater than array length.");


        float[] new_values = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
            new_values[i] = 1;


        //For each side on X axis
        for (int y = 0; y < mesh_size.y; y++)
            for (int x = 0; x < size; x++)
            {
                //new_values[(int)(y * mesh_size.x) + x] *= Mathf.InverseLerp(0.0f, size, x);
                new_values[(int)(y * mesh_size.x) + x] *= Mathf.Sqrt(Mathf.InverseLerp(0.0f, size, x));
                if (x == 0 || x == mesh_size.x || y == 0 || y == mesh_size.y)
                    new_values[(int)(y * mesh_size.x) + x] = 0;
            }

        int padding_start = ((int)mesh_size.x - size)-1;
        for (int y = 0; y < mesh_size.y; y++)
            for (int x = padding_start; x < mesh_size.x; x++)
            {
                //new_values[(int)(y * mesh_size.x) + x] *= Mathf.InverseLerp(size, 0.0f, x - padding_start);
                new_values[(int)(y * mesh_size.x) + x] *= Mathf.Sqrt(Mathf.InverseLerp(size, 0.0f, x - padding_start));
                if (x == 0 || x == mesh_size.x || y == 0 || y == mesh_size.y)
                    new_values[(int)(y * mesh_size.x) + x] = 0;
            }

        //For each side on Y axis
        for (int y = 0; y < size; y++)
            for (int x = 0; x < mesh_size.x; x++)
            {
                //new_values[(int)(y * mesh_size.x) + x] *= Mathf.InverseLerp(0.0f, size, y);
                new_values[(int)(y * mesh_size.x) + x] *= Mathf.Sqrt(Mathf.InverseLerp(0.0f, size, y));
                if (x == 0 || x == mesh_size.x || y == 0 || y == mesh_size.y)
                    new_values[(int)(y * mesh_size.x) + x] = 0;
            }
        

        padding_start = ((int)mesh_size.y - size)-1;
        for (int y = padding_start; y < mesh_size.y; y++)
            for (int x = 0; x < mesh_size.x; x++)
            {
                //new_values[(int)(y * mesh_size.x) + x] *= Mathf.InverseLerp(size, 0.0f, y - padding_start);
                new_values[(int)(y * mesh_size.x) + x] *= Mathf.Sqrt(Mathf.InverseLerp(size, 0.0f, y - padding_start));
                if (x == 0 || x == mesh_size.x || y == 0 || y == mesh_size.y)
                    new_values[(int)(y * mesh_size.x) + x] = 0;
            }
        //new_values[(int)(y * mesh_size.x) + x] *= Mathf.Sqrt(Mathf.InverseLerp(size, 0.0f, y - padding_start));



        return new_values;
    }
    public static float[] ApplyBubbleFilter(float[] values, Vector2 mesh_size)
    {
        //This filter affects the existing normalized values of the mesh triangle array
        //
        float[] new_values = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
            new_values[i] = values[i];

        for (int y = 0; y < mesh_size.y; y++)
        {
            for (int x = 0; x < mesh_size.x; x++)
            {
                if (x == 0 || x == 1 || x >= mesh_size.x - 2 || y == 0 || y >= mesh_size.y - 1)
                {
                }
                else
                {
                    float old_value = new_values[(int)(y * mesh_size.x) + x];
                    if (old_value > 1)
                        Debug.LogWarning("Filters should only be applied to normalized values.");
                    float new_value;
                    new_value = Mathf.Sqrt(old_value);

                    new_values[(int)(y * mesh_size.x) + x] += new_value;
                    //if(x== 5&& y == 5)
                    //Debug.Log(new_values[(int)(y * mesh_size.x) + x] +" plus "+ strength * Mathf.PerlinNoise(seed + (scaled_coord.x * scale), seed + scaled_coord.y * scale) * 1);
                }
            }
        }
        return new_values;
    }
    #endregion
}
