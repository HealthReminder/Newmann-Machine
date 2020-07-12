using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

class Triangle
{
    public int i1, i2, i3;
    public Vector3 p1, p2, p3;
    //This will be used to find neighbours. It will only work on equilateral triangles
    //Otherwise the algorithm will fail when trying to find the edge triangles neighbours
    public Vector3 center;
    public void Translate(Vector3 v)
    {
        p1 += v;
        p2 += v;
        p3 += v;
    }
}

[RequireComponent(typeof(MeshFilter),typeof(MeshCollider))]
public class Deformer : MonoBehaviour
{
    Mesh mesh_deforming;
    MeshCollider mesh_collider;
    //Original vertices, Displaced vertices
    Vector3[] og_vert, disp_vert;
    //Original triangles, Displaced triangles
    Triangle[] triangles;
    //Used to generate maps //read-only
    public Vector2 mesh_size;
    void Start()
    {
        Setup();

        StartCoroutine(DeformTriangles());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            MoveTriangle(triangles[0], Vector3.up, true);
        if (Input.GetKeyDown(KeyCode.W))
            MoveTriangle(triangles[1], Vector3.up, true);
        if (Input.GetKeyDown(KeyCode.A))
            MoveTriangle(triangles[2], Vector3.up, true);
        if (Input.GetKeyDown(KeyCode.S))
            MoveTriangle(triangles[3], Vector3.up, true);
        if (Input.GetKeyDown(KeyCode.Z))
            MoveTriangle(triangles[4], Vector3.up, true);
        if (Input.GetKeyDown(KeyCode.X))
            MoveTriangle(triangles[5], Vector3.up, true);
        if (Input.GetKeyDown(KeyCode.E))
            MoveTriangle(triangles[triangles.Length - 1], Vector3.up, true);
        if (Input.GetKeyDown(KeyCode.R))
            MoveTriangle(triangles[Random.Range(0, triangles.Length - 1)], Vector3.up, true);
        if (Input.GetKeyDown(KeyCode.F))
            MoveTriangle(triangles[200], Vector3.up, true);
    }

    public IEnumerator DeformTriangles()
    {
        yield return new WaitForSeconds(1);
        //This line will only work on square meshes
        //float[,] height_map = HeightMap.CreateHeightMap((int)mesh_size.x, (int)mesh_size.y);
        //if (height_map.GetLength(0) * height_map.GetLength(1) != triangles.Length)
            //Debug.LogError("Height map size does not match triangle size");
        Triangle current_triangle;
        for (int y = 0; y < mesh_size.y; y++)
            for (int x = 0; x < mesh_size.x; x++)
            {
                current_triangle = triangles[(int)(y * mesh_size.x) + x];
                MoveTriangle(current_triangle, Vector3.up *HeightMap.ApplyPerlinNoise(new Vector2(x,y), mesh_size, 0,1, 1), true);
                Debug.Log(current_triangle.center);
                yield return null;
            }
        yield return null;

        Debug.Log("Went through "+ triangles.Length+" triangles");

        mesh_deforming.vertices = disp_vert;
        mesh_deforming.RecalculateNormals();
        mesh_collider.sharedMesh = mesh_deforming;
        yield break;
    }
    void MoveTriangle(Triangle t, Vector3 v, bool update)
    {
        t.Translate(v);
        disp_vert[t.i1] = t.p1;
        disp_vert[t.i2] = t.p2;
        disp_vert[t.i3] = t.p3;

        if (!update)
            return;

        mesh_deforming.vertices = disp_vert;
        mesh_deforming.RecalculateNormals();

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

        Debug.Log("Triangle count: " + triangles.Length);
        Debug.Log("Triangle indexes count: " + mesh_tris_indexes.Length);
        Debug.Log("Vertices count: " + mesh_verts.Length);

        for (int i = 0; i < triangles.Length; i++)
            triangles[i] = new Triangle();

        //This list will be used to order the triangle list
        //Cacheing it separately makes it easier to sort the distances;
        List<Vector3> centers = new List<Vector3>();

        //Get a list of triangles from the mesh
        int k = 0;
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i].p1 = mesh_verts[mesh_tris_indexes[i + k]];
            triangles[i].i1 = mesh_tris_indexes[i + k];
            triangles[i].p2 = mesh_verts[mesh_tris_indexes[i + 1 + k]];
            triangles[i].i2 = mesh_tris_indexes[i + 1 + k];
            triangles[i].p3 = mesh_verts[mesh_tris_indexes[i + 2 + k]];
            triangles[i].i3 = mesh_tris_indexes[i + 2 + k];

            //Get center of triangle
            //Centers will be used to find neighbours
            //Neighbours will be used to put the triangles in order
            Bounds bounds = new Bounds(triangles[i].p1, Vector3.zero);
            bounds.Encapsulate(triangles[i].p2);
            bounds.Encapsulate(triangles[i].p3);
            triangles[i].center = bounds.center;
            centers.Add(bounds.center);

            k += 2;
        }
        Debug.Log("Cached the centers of " + centers.Count + " triangles");

        //Order the list of centers we got
        List<Vector3> ordered_centers = new List<Vector3>();
        List<int> ordered_indexes = new List<int>();
        //Every time Y changes 
        mesh_size = Vector2.zero;
        //This list will be counted to find out the dimensions of the mesh
        List<float> unique_y = new List<float>();
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
                    if (current_center.y < ordered_centers[o].y)
                    {
                        new_index = o;
                        break;
                    }
                    else if (current_center.y > ordered_centers[o].y)
                        new_index = o + 1;
                }
            }
            ordered_centers.Insert(new_index, current_center);
            ordered_indexes.Insert(new_index, i);
            //Cached unique Ys
            bool unique = true;
            foreach (float y in unique_y)
                if (y == current_center.y)
                    unique = false;
            if (unique)
                unique_y.Add(current_center.y);
        }
        float y_size = unique_y.Count;
        mesh_size = new Vector2(triangles.Length / y_size, y_size);
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
}
