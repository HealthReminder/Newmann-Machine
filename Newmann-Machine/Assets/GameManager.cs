using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Planetoid
{
    public Gradient terrain_colors;
}
public class GameManager : MonoBehaviour
{
    [Header("Instanced for generation")]
    public Terrain terrain;
    [Header("Cached info for generation")]
    public Gradient[] possible_terrain_colors;
    [Header("Planetoid Info")]
    [SerializeField] public Planetoid current_planetoid;

    [Header("Public Variables")]
    public float routine_time = 0;
    [Range(0, 1)] public float strength_perlin;
    [Range(0, 1)] public float strength_voronoi;

    void Start()
    {
        routine_time = 0;
        strength_perlin = 1;
        StartCoroutine(SetupRoutine());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(0);
    }
    void ChangePublicVariables(float f)
    {
        strength_perlin = 1.0f - routine_time;
        strength_voronoi = 0.0f + routine_time;
        routine_time += f;
    }

    IEnumerator SetupRoutine()
    {
        while (true) {
            //ERROR SOMETHIGN MAKES IT NOT RANDOM
            int seed_perlin = Random.Range(0, 99999);
            int seed_voronoi = Random.Range(0, 99999);


            terrain.Reset();
            GeneratePlanetoid();
            //Height range is decided by the strength of gravity in the planet
            //Passes make the planet rougher 
            //Debug.Log("<color=yellow> Offset grid output: </color>\n " + terrain.DebugTriangleValues());
            yield return StartCoroutine(terrain.DeformPerlin(seed_perlin,5.0f, strength_perlin));
            //terrain.DrawTriangles();
            //Debug.Log("<color=orange> Offset grid output: </color>\n " + terrain.DebugTriangleValues());

            //Point range is how much variance in its position a point can have         1 = tight grid like epicenters   20   25 = irregular dunes       50 = bubbly islands      
            yield return StartCoroutine(terrain.DeformVoronoi(seed_voronoi, 0.5f, 20f,strength_voronoi));
            terrain.DrawTriangles();

            //Before applying filters make sure the terrain is normalized

            yield return StartCoroutine(terrain.FilterArch());
            terrain.DrawTriangles();

            yield return StartCoroutine(terrain.ClampHeight(0, 45f));
            terrain.DrawTriangles();

            //APPEARANCE
            yield return StartCoroutine(terrain.ColorHeight(current_planetoid.terrain_colors));
            terrain.DrawTriangles();
            //Smoothes colors
            yield return StartCoroutine(terrain.ColorAverage());
            terrain.DrawTriangles();
            //Does not work as intended
            //yield return StartCoroutine(terrain.ColorMajority());
            yield return new WaitForSeconds(4.2f);
            yield return null;
            ChangePublicVariables(0.1f);
        }
        yield break;
    }

    void GeneratePlanetoid()
    {
        current_planetoid = new Planetoid();
        current_planetoid.terrain_colors = possible_terrain_colors[Random.Range(0, possible_terrain_colors.Length)];
    }
}
