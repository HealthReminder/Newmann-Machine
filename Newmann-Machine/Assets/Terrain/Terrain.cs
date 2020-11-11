using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Triangle
{
    //Vertices indexes
    public int i1, i2, i3;
    //Displaced vertices
    public Vector3 v1, v2, v3;
    //Original vertices
    public Vector3 o1, o2, o3;
    //This will be used to find neighbours. It will only work on equilateral triangles
    //Otherwise the algorithm will fail when trying to find the edge triangles neighbours
    public Vector3 ocenter;
    public Vector3 center;
    public float value;
    public void Set(Vector3 v)
    {
        v1 = o1;
        v2 = o2;
        v3 = o3;
        center = ocenter;
    }
    public void Translate(Vector3 v)
    {
        v1 += v;
        v2 += v;
        v3 += v;
        center += v;
    }
}

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class Terrain : MonoBehaviour
{
    Mesh mesh_deforming;
    MeshCollider mesh_collider;
    //Original vertices, Displaced vertices
    Vector3[] og_vert, disp_vert;
    //Original triangles, Displaced triangles
    Triangle[] triangles;
    //Used to generate maps //read-only
    public Vector2 mesh_size;

    public Vector2 minmax_Y;
    void Awake()
    {
        Setup();
    }
    #region Appearance
    public IEnumerator ColorHeight(Gradient gradient)
    {
        Color[] vertex_colors = new Color[mesh_deforming.vertices.Length];
        for (int y = 0; y < mesh_size.y; y++)
        {
            yield return null;
            for (int x = 0; x < mesh_size.x; x++)
            {
                int i = (int)(y * mesh_size.x) + x;
                Triangle current_triangle = triangles[i];
                float lerp = Mathf.InverseLerp(minmax_Y.x, minmax_Y.y, current_triangle.value);
                vertex_colors[current_triangle.i1] = vertex_colors[current_triangle.i2] = vertex_colors[current_triangle.i3] = gradient.Evaluate(lerp);
                mesh_deforming.colors = vertex_colors;
            }
        }
        mesh_deforming.colors = vertex_colors;
        Debug.Log("Applied random colors");
        yield break;
    }
    public IEnumerator ColorMajority()
    {
        //colors.Evaluate(Random.Range(0.0f, 1.0f))
        Debug.Log("Averaged colors of terrain.");
        Color[] vertex_colors = new Color[mesh_deforming.vertices.Length];
        for (int k = 0; k < vertex_colors.Length; k++)
            vertex_colors[k] = mesh_deforming.colors[k];

        for (int y = 0; y < mesh_size.y; y++)
        {
            yield return null;
            for (int x = 0; x < mesh_size.x; x++)
            {
                int i = (int)(y * mesh_size.x) + x;
                Triangle t = triangles[i];

                Color[] c = new Color[3];
                c[0] = vertex_colors[t.i1];
                c[1] = vertex_colors[t.i2];
                c[2] = vertex_colors[t.i3];
                Color major_color = c[2];
                if (c[0] == c[1] || c[0] == c[2])
                    major_color = c[0];
                else
                    major_color = c[1];
                vertex_colors[t.i1] = vertex_colors[t.i2] = vertex_colors[t.i3] = major_color;
            }
        }
        mesh_deforming.colors = vertex_colors;
        Debug.Log("Applied random colors");
        yield break;
    }
    public IEnumerator ColorAverage()
    {
        Color[] vertex_colors = new Color[mesh_deforming.vertices.Length];
        for (int k = 0; k < vertex_colors.Length; k++)
            vertex_colors[k] = mesh_deforming.colors[k];

        for (int y = 0; y < mesh_size.y; y++)
        {
            yield return null;
            for (int x = 0; x < mesh_size.x; x++)
            {
                int i = (int)(y * mesh_size.x) + x;
                Triangle t = triangles[i];

                float[] c = new float[4];
                c[0] = vertex_colors[t.i1].r + vertex_colors[t.i2].r + vertex_colors[t.i3].r;
                c[1] = vertex_colors[t.i1].g + vertex_colors[t.i2].g + vertex_colors[t.i3].g;
                c[2] = vertex_colors[t.i1].b + vertex_colors[t.i2].b + vertex_colors[t.i3].b;
                c[3] = vertex_colors[t.i1].a + vertex_colors[t.i2].a + vertex_colors[t.i3].a;
                Color average_color = new Color(c[0] / 3, c[1] / 3, c[2] / 3, c[3] / 3);
                vertex_colors[t.i1] = vertex_colors[t.i2] = vertex_colors[t.i3] = average_color;
            }
        }
        mesh_deforming.colors = vertex_colors;
        Debug.Log("Averaged colors of terrain.");

        yield break;
    }
    public void DrawTriangles()
    {
        for (int y = 0; y < mesh_size.y; y++)
            for (int x = 0; x < mesh_size.x; x++)
            {
                if (x == 0 || x == 1 || x >= mesh_size.x - 2 || y == 0 || y >= mesh_size.y - 1)
                {
                }
                else
                {
                    int index = (int)(y * mesh_size.x) + x;
                    SetTriangle(triangles[index], (Vector3.up * triangles[index].value), true);
                }
            }

        mesh_deforming.vertices = disp_vert;
        mesh_deforming.RecalculateNormals();
        mesh_collider.sharedMesh = mesh_deforming;
    }
    #endregion
    #region Possible deformations
    public void Reset()
    {
        //Reset
        for (int i = 0; i < triangles.Length; i++)
            triangles[i].value = 1;

    }
    public IEnumerator DeformPerlin(int seed_perlin, float perlin_passes, float strength)
    {
        //Variables that affect the whole process. These variables should remain static for the most
        float scale = Random.Range(1.0f, 1.0f);
        //Reset
        //for (int i = 0; i < triangles.Length; i++)
        //triangles[i].value = 0;
        //All the heights of the triangles will bew stored in this array
        //It will be read to draw the triangles to the screen again
        float[] triangle_values = new float[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
            triangle_values[i] = 1;

        Debug.Log("Generated new terrain with passes of: " + perlin_passes + ". And scale of: " + scale);


        for (int i = 1; i <= perlin_passes; i++)
        {
            scale = scale / 2;
            float k = (float)i;
            triangle_values = HeightMap.ApplyPerlinNoise(seed_perlin, triangle_values, mesh_size, (float)scale / (k), strength / (k * k * k));
            //Debug.Log("Scale: " + scale+" k: "+k+ " scale result: " + (float)scale / (k) + " strength: " + 1 / (k * k * k));
        }

        //Apply array to the triangles themselves
        for (int i = 0; i < triangles.Length; i++)
            triangles[i].value *= triangle_values[i];

        yield break;
    }
    public IEnumerator DeformPixelate(int seed_perlin, float perlin_passes, float strength)
    {
        //Variables that affect the whole process. These variables should remain static for the most
        float scale = Random.Range(1.0f, 1.0f);
        //Reset
        //for (int i = 0; i < triangles.Length; i++)
        //triangles[i].value = 0;
        //All the heights of the triangles will bew stored in this array
        //It will be read to draw the triangles to the screen again
        float[] triangle_values = new float[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
            triangle_values[i] = 1;

        Debug.Log("Generated new terrain with passes of: " + perlin_passes + ". And scale of: " + scale);


        for (int i = 1; i <= perlin_passes; i++)
        {
            scale = scale / 2;
            float k = (float)i;
            triangle_values = HeightMap.ApplyPerlinNoise(seed_perlin, triangle_values, mesh_size, (float)scale / (k), strength / (k * k * k));
            //Debug.Log("Scale: " + scale+" k: "+k+ " scale result: " + (float)scale / (k) + " strength: " + 1 / (k * k * k));
        }

        //Apply array to the triangles themselves
        for (int i = 0; i < triangles.Length; i++)
            triangles[i].value *= triangle_values[i];

        yield break;
    }
    public IEnumerator DeformVoronoi(int seed, float scale, float point_range, float strength)
    {
        //Just to make outside use easier
        //Make it at least 10
        scale /= 10;

        //All the heights of the triangles will bew stored in this array
        //It will be read to draw the triangles to the screen again
        float[] triangle_values = new float[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
            triangle_values[i] = 1;

        Debug.Log("Generated new terrain with scale of: " + scale);

        triangle_values = HeightMap.ApplyVoronoiNoise(seed, triangle_values, mesh_size, scale, point_range, strength);

        //Apply array to the triangles themselves
        for (int i = 0; i < triangles.Length; i++)
            triangles[i].value *= triangle_values[i];

        Debug.Log("Applied voronoi noise");
        yield break;
    }
    public IEnumerator FilterSquarePadding(int padding_size)
    {
       
        //Array contains height of triangles and will be read to draw them
        float[] triangle_values = new float[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
            triangle_values[i] = triangles[i].value;

        triangle_values = HeightMap.ApplySquarePadding(triangle_values, mesh_size, padding_size);

        //Apply array to the triangles themselves
        for (int i = 0; i < triangles.Length; i++)
            triangles[i].value *= triangle_values[i];

        Debug.Log("Applied Arch filter");
        yield break;
    }
    public IEnumerator FilterArch()
    {
        Vector2 minmax = new Vector2(999, -999);
        for (int i = 0; i < triangles.Length; i++)
        {
            if (triangles[i].value < minmax.x)
                minmax.x = triangles[i].value;
            if (triangles[i].value > minmax.y)
                minmax.y = triangles[i].value;
        }
        //All the heights of the triangles will bew stored in this array
        //It will be read to draw the triangles to the screen again
        float[] triangle_values = new float[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
            triangle_values[i] = Mathf.InverseLerp(minmax.x, minmax.y, triangles[i].value);

        triangle_values = HeightMap.ApplyBubbleFilter(triangle_values, mesh_size);

        //Apply array to the triangles themselves
        for (int i = 0; i < triangles.Length; i++)
            triangles[i].value = triangle_values[i];

        Debug.Log("Applied Arch filter");
        yield break;
    }


    public IEnumerator ClampHeight(float min, float max)
    {
        GetMinMaxHeight();

        Triangle current_triangle;
        for (int y = 0; y < mesh_size.y; y++)
        {
            yield return null;
            for (int x = 0; x < mesh_size.x; x++)
            {
                if (x == 0 || x == 1 || x >= mesh_size.x - 2 || y == 0 || y >= mesh_size.y - 1)
                {
                }
                else
                {
                    current_triangle = triangles[(int)(y * mesh_size.x) + x];
                    float p = Mathf.InverseLerp(minmax_Y.x, minmax_Y.y, current_triangle.value);
                    p = Mathf.Lerp(min, max, p);
                    current_triangle.value = p;
                    //Debug.Log("Min Y: " + minmax_Y.x + " Original value: " + current_triangle.value);
                    //current_triangle.value -= minmax_Y.x - 1;
                    //Debug.Log("Changed value: " + current_triangle.value);

                    SetTriangle(current_triangle, (Vector3.up * current_triangle.value), false);
                }
            }
        }
        minmax_Y = new Vector2(min, max);
        Debug.Log("Clamped Height.");
        yield break;
    }
    #endregion
    #region Debug
    public string DebugTriangleValues()
    {
        float[] arr = new float[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
            arr[i] = triangles[i].value;


        return ReadArrayInt(arr, (int)mesh_size.x, (int)mesh_size.y);
    }
    string ReadArrayInt(float[] val, int y_size, int x_size)
    {
        string log = "";
        if (val == null)
            log += ("NULL INT ARRAY");
        else
        {
            for (int y = 0; y < y_size; y++)
            {
                for (int x = 0; x < x_size; x++)
                {
                    log += Mathf.RoundToInt(val[y * x + x]);
                }
                log += "\n";

            }
            log += "\n";
        }

        log += "\n";
        return log;
    }
    #endregion
    #region Internal functions

    public void GetMinMaxHeight()
    {
        float max = -9999;
        float min = 9999;
        foreach (Triangle t in triangles)
            if (t.value != 0)
            {
                if (t.value < min)
                    min = t.value;
                if (t.value > max)
                    max = t.value;
            }

        minmax_Y = new Vector2(min, max);
    }
    void SetTriangle(Triangle t, Vector3 v, bool update)
    {
        t.Set(v);
        disp_vert[t.i1] = t.o1 + v;
        disp_vert[t.i2] = t.o2 + v;
        disp_vert[t.i3] = t.o3 + v;

        if (!update)
            return;

        mesh_deforming.vertices = disp_vert;
        mesh_deforming.RecalculateNormals();
        mesh_deforming.RecalculateTangents();
        mesh_deforming.RecalculateBounds();

    }
    public void DeformVertices()
    {
        for (int i = 0; i < disp_vert.Length; i++)
        {
            disp_vert[i] += Vector3.one;
        }
        mesh_deforming.vertices = disp_vert;
        mesh_deforming.RecalculateNormals();
        mesh_collider.sharedMesh = mesh_deforming;
    }
    void Setup()
    {
        //Get required components
        mesh_deforming = GetComponent<MeshFilter>().mesh;
        mesh_collider = GetComponent<MeshCollider>();

        //Get vertices
        og_vert = mesh_deforming.vertices;
        disp_vert = new Vector3[og_vert.Length];
        for (int i = 0; i < og_vert.Length; i++)
            disp_vert[i] = og_vert[i];

        //Get triangles
        int tri_index_count = mesh_deforming.triangles.Length;
        triangles = new Triangle[tri_index_count / 3];

        int[] mesh_tris_indexes = mesh_deforming.triangles;
        Vector3[] mesh_verts = mesh_deforming.vertices;

        //This list will be used to order the triangle list
        //Cacheing it separately makes it easier to sort the distances;
        List<Vector3> centers = new List<Vector3>();
        for (int i = 0; i < triangles.Length; i++)
            triangles[i] = new Triangle();
        //Get a list of triangles from the mesh
        int k = 0;
        for (int i = 0; i < triangles.Length; i++)
        {
            Triangle t = triangles[i];
            t.o1 = t.v1 = mesh_verts[mesh_tris_indexes[i + k]];
            t.i1 = mesh_tris_indexes[i + k];
            t.o2 = t.v2 = mesh_verts[mesh_tris_indexes[i + 1 + k]];
            t.i2 = mesh_tris_indexes[i + 1 + k];
            t.o3 = t.v3 = mesh_verts[mesh_tris_indexes[i + 2 + k]];
            t.i3 = mesh_tris_indexes[i + 2 + k];
            t.value = 0;
            //Get center of triangle
            //Centers will be used to find neighbours
            //Neighbours will be used to put the triangles in order
            Bounds bounds = new Bounds(triangles[i].o1, Vector3.zero);
            bounds.Encapsulate(t.o2);
            bounds.Encapsulate(t.o3);
            t.center = t.ocenter = bounds.center;
            centers.Add(bounds.center);

            //Move at first
            for (int y = 0; y < mesh_size.y; y++)
                for (int x = 0; x < mesh_size.x; x++)
                    SetTriangle(t, Vector3.up * t.value, true);

            k += 2;
        }
        Debug.Log("Cached the centers of " + centers.Count + " triangles");

        //Order the list of centers we got
        List<Vector3> ordered_centers = new List<Vector3>();
        List<int> ordered_indexes = new List<int>();
        //Every time Y changes 
        mesh_size = Vector2.zero;
        //This list will be counted to find out the dimensions of the mesh
        List<float> unique_z = new List<float>();
        for (int i = 0; i < centers.Count; i++)
        {
            int new_index = ordered_centers.Count;
            Vector3 current_center = centers[i];
            for (int o = 0; o < ordered_centers.Count; o++)
            {
                if (current_center.x < ordered_centers[o].x)
                {
                    new_index = o;
                    break;
                }
                else if (current_center.x > ordered_centers[o].x)
                    new_index = o + 1;
                else
                {
                    if (current_center.z < ordered_centers[o].z)
                    {
                        new_index = o;
                        break;
                    }
                    else if (current_center.z > ordered_centers[o].z)
                        new_index = o + 1;
                }
            }
            ordered_centers.Insert(new_index, current_center);
            ordered_indexes.Insert(new_index, i);
            //Cached unique Ys
            bool unique = true;
            foreach (float z in unique_z)
                if (z == current_center.z)
                    unique = false;
            if (unique)
                unique_z.Add(current_center.z);
        }

        float y_size = Mathf.Sqrt(triangles.Length / 2);
        mesh_size = new Vector2(y_size * 2, y_size);
        Debug.Log("Ordered " + ordered_centers.Count + " centers");

        //Order triangles according to the ordered centers
        List<Triangle> ordered_triangles = new List<Triangle>();
        for (int i = 0; i < ordered_centers.Count; i++)
        {
            ordered_triangles.Add(triangles[ordered_indexes[i]]);

        }
        Debug.Log("Ordered " + ordered_triangles.Count + " triangles");

        triangles = ordered_triangles.ToArray();
        minmax_Y = Vector2.zero;
    }
    #endregion
}
