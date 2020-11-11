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
public class GenerationTester : MonoBehaviour
{
    [Header("Instanced for generation")]
    public bool is_once = false;
    public Terrain terrain;
    public Transform water_transform;
    public AnimationCurve probability_curve;
    [Header("Cached info for generation")]
    public Gradient[] possible_terrain_colors;
    [Header("Planetoid Info")]
    [SerializeField] public Planetoid current_planetoid;
    [Header("Public Variables")]
    public float routine_time = 0;
    [Range(0, 1)] public float perlin_strength;
    [Range(0, 1)] public float voronoi_strength;

    void Start()
    {
        ChangePublicVariables(0.1f);
        StartCoroutine(SetupRoutine());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(0);
    }
    void ChangePublicVariables(float f)
    {
        //perlin_strength = 1.0f - routine_time;
        //voronoi_strength = 0.0f + routine_time;

        perlin_strength = Random.Range(0.0f, 1.0f);
        voronoi_strength = Random.Range(0.0f, 1.0f);

        perlin_strength = probability_curve.Evaluate(perlin_strength);
        voronoi_strength = probability_curve.Evaluate(voronoi_strength);

        routine_time += f;
    }

    IEnumerator SetupRoutine()
    {
        while (true) {
            //ERROR SOMETHIGN MAKES IT NOT RANDOM
            int seed_perlin = Random.Range(0, 99999);
            int seed_voronoi = Random.Range(0, 99999);



            //TERRAIN GENERATION
            terrain.Reset();
            GeneratePlanetoid();
            //Height range is decided by the strength of gravity in the planet
            //Passes make the planet rougher 
            //Debug.Log("<color=yellow> Offset grid output: </color>\n " + terrain.DebugTriangleValues());
            yield return StartCoroutine(terrain.DeformPerlin(seed_perlin,5.0f, perlin_strength));
            //terrain.DrawTriangles();
            //Debug.Log("<color=orange> Offset grid output: </color>\n " + terrain.DebugTriangleValues());

            //Point range is how much variance in its position a point can have         1 = tight grid like epicenters   20   25 = irregular dunes       50 = bubbly islands      
            yield return StartCoroutine(terrain.DeformVoronoi(seed_voronoi, 0.5f, 50f, voronoi_strength));
            //terrain.DrawTriangles();

            //Before applying filters make sure the terrain is normalized

            yield return StartCoroutine(terrain.FilterArch());
            //terrain.DrawTriangles();
            yield return StartCoroutine(terrain.ClampHeight(0, 50f));

            //Finally smooth out bounds to make it prettier
            //Can be modified to simulate an island-like generation
            //If has water make it an island
            yield return StartCoroutine(terrain.FilterSquarePadding(15));


            terrain.DrawTriangles();
            //TERRAIN GENERATION END

            //TERRAIN APPEARANCE
            yield return StartCoroutine(terrain.ColorHeight(current_planetoid.terrain_colors));
            //Smoothes colors
            yield return StartCoroutine(terrain.ColorAverage());
            //Does not work as intended
            //yield return StartCoroutine(terrain.ColorMajority());
            
            terrain.DrawTriangles();

            Debug.Log(("<color=green>Run routine for perlin values of {0}. Voronoi strength of {1}.</color>", perlin_strength, voronoi_strength));

            //TERRAIN APPEARANCE END

            yield return new WaitForSeconds(4.2f);
            yield return null;
            ChangePublicVariables(0.1f);
            if (is_once)
                yield break;
        }
        yield break;
    }

    void GeneratePlanetoid()
    {
        current_planetoid = new Planetoid();
        current_planetoid.terrain_colors = possible_terrain_colors[Random.Range(0, possible_terrain_colors.Length)];
    }
}
