using System.Collections;
using System.Collections.Generic;
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
    Triangle[] disp_tri;
    void Start()
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
        disp_tri = new Triangle[tri_index_count/3];

        int[] mesh_tris_indexes = mesh_deforming.triangles;
        Vector3[] mesh_verts = mesh_deforming.vertices;

        Debug.Log("Triangle count: " + disp_tri.Length);
        Debug.Log("Triangle indexes count: " + mesh_tris_indexes.Length);
        Debug.Log("Vertices count: " + mesh_verts.Length);

        for (int i = 0; i < disp_tri.Length; i++)
            disp_tri[i] = new Triangle();

        int k = 0;
        for (int i = 0; i < disp_tri.Length; i ++)
        {
            disp_tri[i].p1 = mesh_verts[mesh_tris_indexes[i+k]];
            disp_tri[i].i1 = mesh_tris_indexes[i + k];
            disp_tri[i].p2 = mesh_verts[mesh_tris_indexes[i + 1 + k]];
            disp_tri[i].i2 = mesh_tris_indexes[i + 1 + k];
            disp_tri[i].p3 = mesh_verts[mesh_tris_indexes[i + 2 + k]];
            disp_tri[i].i3 = mesh_tris_indexes[i + 2 + k];

            //Get center of triangle
            //Centers will be used to find neighbours
            //Neighbours will be used to put the triangles in order
            Bounds bounds = new Bounds(disp_tri[i].p1, Vector3.zero);
            bounds.Encapsulate(disp_tri[i].p2);
            bounds.Encapsulate(disp_tri[i].p3);
            disp_tri[i].center = bounds.center;

            k +=2;
        }
        Deform();
        //StartCoroutine(DeformRoutine());
    }
    void Deform()
    {
        StartCoroutine(DeformTriangles());

    }
    IEnumerator DeformRoutine()
    {
        while (true)
        {
            StartCoroutine(DeformTriangles());
            yield return new WaitForSeconds(1);
        }
        yield return null;
    }

    public IEnumerator DeformTriangles()
    {
        yield return new WaitForSeconds(2);


       for (int i = 0; i < disp_tri.Length; i++)
        {
            disp_tri[i].Translate(Vector3.up);
        }
        foreach (Triangle t in disp_tri)
        {
            disp_vert[t.i1] = t.p1;
            disp_vert[t.i2] = t.p2;
            disp_vert[t.i3] = t.p3;

            yield return null;

            mesh_deforming.vertices = disp_vert;
            mesh_deforming.RecalculateNormals();
        }
        Debug.Log("Went through "+ disp_tri.Length+" triangles");

        mesh_deforming.vertices = disp_vert;
        mesh_deforming.RecalculateNormals();
        mesh_collider.sharedMesh = mesh_deforming;
        yield break;
    }
    Vector3[] SortPoints(Vector3[] points)
    {
        List<Vector3> result = new List<Vector3>();
        result.Add(points[0]);
        for (int i = 1; i < points.Length; i++)
        {
            result.Add(points[i]);
        }
        result.Sort();
        return result.ToArray();
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
}
