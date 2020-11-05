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
            ;yield return StartCoroutine(terrain.DeformPerlin(seed_perlin,5.0f));
            terrain.DrawTriangles();
            //Debug.Log("<color=orange> Offset grid output: </color>\n " + terrain.DebugTriangleValues());

            //Scale is how scatterred the dunes are. Lower decimal values mean further away dunes. Bigger numbers means EXPONENTIALLY more clustered dunes 0.1f -> large, 0.2f -> medium, 0.5f -> small, 1.0f -> tiny.
            //Point range is how much the dunes spread, how much their arch reaches.
            yield return StartCoroutine(terrain.DeformVoronoi(seed_voronoi, 0.5f, 25f));
            terrain.DrawTriangles();

            yield return StartCoroutine(terrain.ClampHeight(0, 40));
            terrain.DrawTriangles();

            //APPEARANCE
            yield return StartCoroutine(terrain.ColorHeight(current_planetoid.terrain_colors));
            terrain.DrawTriangles();
            //Smoothes colors
            yield return StartCoroutine(terrain.ColorAverage());
            terrain.DrawTriangles();
            //Does not work as intended
            //yield return StartCoroutine(terrain.ColorMajority());
            yield return new WaitForSeconds(5.0f);
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
