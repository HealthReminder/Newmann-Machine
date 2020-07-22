using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

class Triangle
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

[RequireComponent(typeof(MeshFilter),typeof(MeshCollider))]
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
    void Awake()
    {
        Setup();
    }
    public IEnumerator ColorHeight(Gradient colors)
    {
        //colors.Evaluate(Random.Range(0.0f, 1.0f))
        Color32[] c = new Color32[mesh_deforming.colors32.Length];
        for (int i = 0; i < c.Length; i++)
        {
            c[i] = Color.red;
            yield return null;

        }
        mesh_deforming.colors32 = c;
        mesh_deforming.SetColors(c);
        mesh_deforming.Optimize();
        mesh_deforming.RecalculateTangents();
        mesh_deforming.RecalculateNormals();
        Debug.Log("Applied random colors");
        yield break;
    }
    public IEnumerator DeformTrianglesRandomly()
    {
        Triangle current_triangle;

        float value;
        float modifier = 3;
        float strength = Random.Range(1.5f, 2.2f);
        float scale = Random.Range(0.1f, 0.3f);
        Debug.Log("Generated new terrain with strength: " + strength + " and scale: " + scale);
        //8
        for (int iteration = 1; iteration <= 0; iteration++)
        {
            float perlin_seed = Random.Range(0.0f, 1000.0f);
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
                        value = (HeightMap.ApplyPerlinNoise(new Vector2(x, y), mesh_size, strength / iteration * modifier, scale / iteration * modifier, perlin_seed));
                        current_triangle.value = value;
                        MoveTriangle(current_triangle, Vector3.up * 10 * current_triangle.value, true);
                    }
                }
            }
        }
        Debug.Log("Applied perlin noise");
        float [] voronoi = new float[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
            voronoi[i] = triangles[i].value;
        
        for (int iteration = 1; iteration <= 1; iteration++)
        {
            voronoi = HeightMap.ApplyVoronoiNoise(voronoi, mesh_size, Random.Range(0.1f, 0.05f) , Random.Range(5.0f, 20.0f) , Random.Range(2.0f, 3.5f));
            float perlin_seed = Random.Range(0.0f, 1000.0f);
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
                        int i = (int)(y * mesh_size.x) + x;
                        triangles[i].value = voronoi[i];
                        SetTriangle(triangles[i], Vector3.up * 10 * triangles[i].value, true);
                    }
                }
            }
        }
        Debug.Log("Applied voronoi noise");
        for (int r = 0; r < 0; r++)
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
                        value = HeightMap.ApplyMagnitudeFilter(current_triangle.value, strength,0.7f);
                        current_triangle.value = value;
                        MoveTriangle(current_triangle, Vector3.up * current_triangle.value, true);
                    }

                }
            }
        Debug.Log("Applied magnitude filter");


        yield return null;

        Debug.Log("Went through "+ triangles.Length+" triangles");

        mesh_deforming.vertices = disp_vert;
        mesh_deforming.RecalculateNormals();
        mesh_collider.sharedMesh = mesh_deforming;
        yield break;
    }

    #region Internal functions
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
    void MoveTriangle(Triangle t, Vector3 v, bool update)
    {
        t.Translate(v);
        disp_vert[t.i1] = t.v1;
        disp_vert[t.i2] = t.v2;
        disp_vert[t.i3] = t.v3;

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

        //Debug.Log("Triangle count: " + triangles.Length);
        //Debug.Log("Triangle indexes count: " + mesh_tris_indexes.Length);
        //Debug.Log("Vertices count: " + mesh_verts.Length);

       

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
        mesh_size = new Vector2(y_size * 2, y_size ) ;
        Debug.Log("Ordered " + ordered_centers.Count + " centers");

        //Order triangles according to the ordered centers
        List<Triangle> ordered_triangles = new List<Triangle>();
        for (int i = 0; i < ordered_centers.Count; i++)
        {
            ordered_triangles.Add(triangles[ordered_indexes[i]]);

        }
        Debug.Log("Ordered " + ordered_triangles.Count + " triangles");

        triangles = ordered_triangles.ToArray();
    }
    #endregion
}
