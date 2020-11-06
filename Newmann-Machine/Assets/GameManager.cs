using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
        StartCoroutine(SetupRoutine());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(0);
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
            //yield return StartCoroutine(terrain.DeformPerlin(seed_perlin,5.0f));
            //terrain.DrawTriangles();
            //Debug.Log("<color=orange> Offset grid output: </color>\n " + terrain.DebugTriangleValues());

            //Point range is how much variance in its position a point can have         1 = tight grid like epicenters   20   25 = irregular dunes       50 = bubbly islands      
            yield return StartCoroutine(terrain.DeformVoronoi(seed_voronoi, 0.5f, 20f));
            terrain.DrawTriangles();
            yield return new WaitForSeconds(1.0f);

            //Before applying filters make sure the terrain is normalized

            yield return StartCoroutine(terrain.FilterArch());
            terrain.DrawTriangles();

            yield return StartCoroutine(terrain.ClampHeight(0, 25f));
            terrain.DrawTriangles();

            //APPEARANCE
            yield return StartCoroutine(terrain.ColorHeight(current_planetoid.terrain_colors));
            terrain.DrawTriangles();
            //Smoothes colors
            yield return StartCoroutine(terrain.ColorAverage());
            terrain.DrawTriangles();
            //Does not work as intended
            //yield return StartCoroutine(terrain.ColorMajority());
            yield return new WaitForSeconds(3.0f);
            yield return null;

        }
        yield break;
    }

    void GeneratePlanetoid()
    {
        current_planetoid = new Planetoid();
        current_planetoid.terrain_colors = possible_terrain_colors[Random.Range(0, possible_terrain_colors.Length)];
    }
}
