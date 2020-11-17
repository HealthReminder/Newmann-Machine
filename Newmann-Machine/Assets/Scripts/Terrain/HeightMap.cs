using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class HeightMap
{
    #region BASE NOISES
    public static float[] ApplyBorder(float[] values, Vector2 mesh_size, float default_height = 0, int width = 1)
    {
        //This function creates a border around the image
        float[] new_values = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
            new_values[i] = values[i];

        for (int y = 0; y < mesh_size.y; y++)
            for (int x = 0; x < mesh_size.x; x++)
                if (x < width * 2 || x >= mesh_size.x - width * 2 || y < width || y >= mesh_size.y - width)
                    new_values[(int)(y * mesh_size.x) + x] = default_height;

        return new_values;
    }
    public static float[] ApplyVoronoiNoise(int seed, float[] values, Vector2 size, float scale, float point_range, float strength)
    {
        //STRETCH MIGHT NOT BE WORKING AS INTENDED DUE TO DOUBLE TRIANGLES ON X AXIS
        //This function applies a voronoi filter to the image
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
                    if (x % frequency == 0)
                    {
                        Vector2 p = new Vector2(
                            x - 2 + Random.Range(point_range / 2, point_range),
                            y / 2 - 2 + Random.Range(point_range / 4, point_range / 2));
                        points.Add(p);
                    }
        }

        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
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

                result[i] += (value * strength);


            }
        return result;
    }
    public static float[] ApplyPerlinNoise(int seed, float[] values, Vector2 mesh_size, float scale, float strength)
    {
        //This function applies a perlin filter to the image
        float[] new_values = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
            new_values[i] = values[i];

        scale = 1 / scale;
        for (int y = 0; y < mesh_size.y; y++)
            for (int x = 0; x < mesh_size.x; x++)
            {
                Vector2 coord = new Vector2(x, y);
                Vector2 scaled_coord = new Vector2(coord.x / (mesh_size.x), (coord.y * 2) / (mesh_size.y * 2));
                float result = strength * Mathf.PerlinNoise((scaled_coord.x * scale) + seed, (scaled_coord.y * scale) + seed);
                new_values[(int)(y * mesh_size.x) + x] += result;

            }
        return new_values;
    }
    #endregion
    #region COMPLEMENTARY NOISES
    public static float[] ApplySquarePadding(float[] values, Vector2 mesh_size, int size)
    {
        //This filter modifies the values on the edges of the image to fade to 0

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

        int padding_start = ((int)mesh_size.x - size) - 1;
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


        padding_start = ((int)mesh_size.y - size) - 1;
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

    public static float[] ApplyBoxBlur(int size, float[] values, Vector2 mesh_size)
    {
        //This filter applies a box blur algorithm to the image

        float[] new_values = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
            new_values[i] = values[i];

        float[][] arr = new float[(int)mesh_size.x][];
        for (int x = 0; x < mesh_size.x; x++)
            arr[x] = new float[(int)mesh_size.y];

        //This list contains the coordinates corresponding to the area of a single unit of blur defined by the size parameter
        List<List<Vector2>> blur_cells = new List<List<Vector2>>();

        //For each cell check if its the initital point of a new blur cel
        for (int y = 0; y < mesh_size.y; y++)
        {
            int x_skip = 0;
            for (int x = 0; x < mesh_size.x; x++)
            {

                arr[x][y] = new_values[(int)(y * mesh_size.x) + x];
                if (x_skip == size * 2 && y % size == 0)
                {
                    //If these conditions match it means that this is the coordinate for a new blur cell
                    x_skip = 0;
                    blur_cells.Add(new List<Vector2>());
                    for (int o = 0; o < size / 2; o++)
                        for (int i = 0; i < size; i++)
                        {
                            Vector2 coord = new Vector2(x + i, y + o);
                            if (coord.x >= 0 && coord.x < mesh_size.x && coord.y >= 0 && coord.y < mesh_size.y)
                                blur_cells[blur_cells.Count - 1].Add(coord);
                        }



                }
                else
                    x_skip++;

            }
        }
        Debug.Log("Box blur quantity of blur cells is " + blur_cells.Count);
        for (int i = 0; i < blur_cells.Count; i++)
        {
            float total_sum = 0;
            for (int k = 0; k < blur_cells[i].Count; k++)
            {
                int x = (int)blur_cells[i][k].x;
                int y = (int)blur_cells[i][k].y;
                total_sum += arr[x][y];
            }
            float avg_value = total_sum / blur_cells[i].Count;
            for (int k = 0; k < blur_cells[i].Count; k++)
            {
                int x = (int)blur_cells[i][k].x;
                int y = (int)blur_cells[i][k].y;
                arr[x][y] = avg_value;
            }
        }

        //This is for debugging to check if the cells are in order
        //for (int y = 0; y < mesh_size.y; y++)
        //{
        //for (int x = 0; x < mesh_size.x; x++)
        //{
        //    arr[x][y] = ((int)(y * mesh_size.x) + x);
        //}
        //}

        for (int y = 0; y < mesh_size.y; y++)
            for (int x = 0; x < mesh_size.x; x++)
                new_values[(int)(y * mesh_size.x) + x] = arr[x][y];


        return new_values;
    }
    public static float[] ApplyBubbleFilter(float[] values, Vector2 mesh_size)
    {
        //This filter multiplies existing values of the image to fit a square root curve
        //It is kind of obsolete due to the use of AnimationCurves by the terrain script
        float[] new_values = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
            new_values[i] = values[i];

        for (int y = 0; y < mesh_size.y; y++)
            for (int x = 0; x < mesh_size.x; x++)
            {
                float old_value = new_values[(int)(y * mesh_size.x) + x];
                if (old_value > 1)
                    Debug.LogWarning("Filters should only be applied to normalized values.");
                float new_value;
                new_value = Mathf.Sqrt(old_value);
                new_values[(int)(y * mesh_size.x) + x] += new_value;

            }
        return new_values;
    }
    #endregion
}
