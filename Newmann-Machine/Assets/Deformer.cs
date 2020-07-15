using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        center += v;
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
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(0);
    }

    public IEnumerator DeformTriangles()
    {
        //yield return new WaitForSeconds(1);
        //This line will only work on square meshes
        //float[,] height_map = HeightMap.CreateHeightMap((int)mesh_size.x, (int)mesh_size.y);
        //if (height_map.GetLength(0) * height_map.GetLength(1) != triangles.Length)
            //Debug.LogError("Height map size does not match triangle size");
        Triangle current_triangle;
        float perlin_seed = Random.Range(0.0f, 1000.0f);

        //APPLY FOR ONE TRIANGLE
        /*
        for (int y = 0; y < mesh_size.y; y++)
            for (int x = 0; x < mesh_size.x; x++)
            {
                current_triangle = triangles[(int)(y * mesh_size.x) + x];
                float perlin = HeightMap.ApplyPerlinNoise(new Vector2(x, y), mesh_size, 0, 1.0f, 1, 1, perlin_seed);
                //perlin = 1;
                MoveTriangle(current_triangle, Vector3.up *perlin, true);
                Debug.Log(current_triangle.center.x);
                yield return null;
                Debug.Log(current_triangle.center + " | " +new Vector2(x,y)+ " | " + perlin);
            }
            */
        
        //APPLY FOR TWO TRIANGLES
        int odd = 0; float perlin = 0;
        for (int y = 0; y < mesh_size.y; y++)
            for (int x = 0; x < mesh_size.x; x++)
            {
                //if (x == 0 || y == 0 | x == mesh_size.x - 1 || y == mesh_size.y - 1)
                //{ 
                //} 
                //else { 


                current_triangle = triangles[(int)(y * mesh_size.x) + x];
                //if (odd == 0)
                perlin = HeightMap.ApplyPerlinNoise(new Vector2(x, y), mesh_size,100, 1, perlin_seed);
                //ERROR IS RIGHT HERE
                MoveTriangle(current_triangle, Vector3.up * perlin, true);
                //GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = new Vector3(x, perlin*100,y);
                //odd++;
                //if (odd == 1)
                //odd = 0;
                //Debug.Log(current_triangle.center + " | " + new Vector2(x, y) + " | " + perlin);
                //yield return new WaitForSeconds(0.1f);
                //yield return null;


                //}
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
            {
                Debug.Log("foun uniq");
                unique_z.Add(current_center.z);
            }
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
}
