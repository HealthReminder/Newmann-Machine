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
public class TerrainGenerator : MonoBehaviour
{
    //This script has to be used by the game manager to generate the desired terrains
    [Header("Instanced for generation")]
    public bool is_once = false;
    public Terrain terrain;
    public Transform water_transform;
    public AnimationCurve probability_curve;
    public AnimationCurve leveling_curve; //Make beaches flatter and mountains steeper using an exponential function

    [Header("Cached info for generation")]
    public Gradient[] possible_terrain_colors;
    [Header("Planetoid Info")]
    [SerializeField] public Planetoid current_planetoid;
    [Header("Public Variables")]
    public float routine_time = 0;
    [Range(0, 1)] public float perlin_strength;
    [Range(0, 1)] public float voronoi_strength;

    public IEnumerator GenerateNewTerrain()
    {
        ChangePublicVariables(0.1f);

        while (true) {
            int seed_perlin = Random.Range(0, 99999);
            int seed_voronoi = Random.Range(0, 99999);



            //TERRAIN GENERATION ----------------------------------------------------------------------------------
            terrain.Reset();
            yield return StartCoroutine(terrain.ColorHeight(new Gradient()));
            GeneratePlanetoid();
            //Height range is decided by the strength of gravity in the planet
            //Passes make the planet rougher 
            //Debug.Log("<color=yellow> Offset grid output: </color>\n " + terrain.DebugTriangleValues());
            yield return StartCoroutine(terrain.DeformPerlin(seed_perlin,5.0f, perlin_strength));
            //terrain.DrawTriangles();
            //Debug.Log("<color=orange> Offset grid output: </color>\n " + terrain.DebugTriangleValues());

            //Point range is how much variance in its position a point can have         1 = tight grid like epicenters   20   25 = irregular dunes       50 = bubbly islands      
            yield return StartCoroutine(terrain.DeformVoronoi(seed_voronoi, 0.5f, 75f, voronoi_strength));
            //Finally smooth out bounds to make it look like an isolated piece of land
            yield return StartCoroutine(terrain.FilterSquarePadding(10));
            //Make terrain values arch like the dunes of a desert
            //yield return StartCoroutine(terrain.FilterArch());
            //Make it blocky
            //yield return StartCoroutine(terrain.FilterPixelate(4));
            //yield return terrain.DrawTriangles();

            //COLOR CHANGES --------------------------------------------------------------------------------------

            //Clamp to normalized values to make sure colors work properly
            yield return StartCoroutine(terrain.ClampHeight(0.0f, 1.0f));
            //Color by height
            yield return StartCoroutine(terrain.ColorHeight(current_planetoid.terrain_colors, 0.4f));
            //Color by angle
            //yield return StartCoroutine(terrain.ColorAngle());
            //Smoothes colors
            yield return StartCoroutine(terrain.ColorAverage());
            //Does not work as intended
            //yield return StartCoroutine(terrain.ColorMajority());
            yield return StartCoroutine(terrain.ApplyCurve(leveling_curve));

            //POST COLOR CHANGES --------------------------------------------------------------------------------

            //Stretch terrain height between min and max values
            yield return StartCoroutine(terrain.ClampHeight(0, 50f));
            //Draw results
            yield return terrain.DrawTriangles();

            Debug.Log(string.Format("<color=green>Run routine for perlin values of {0}. Voronoi strength of {1}.</color>", perlin_strength, voronoi_strength));


            yield return new WaitForSeconds(4.2f);
            yield return null;
            //Change values for new level generation
            ChangePublicVariables(0.1f);
            if (is_once)
                yield break;
        }
        yield break;
    }


    void ChangePublicVariables(float f)
    {
        //perlin_strength = 1.0f - routine_time;
        //voronoi_strength = 0.0f + routine_time;

        perlin_strength = Random.Range(0.4f, 1.0f);
        voronoi_strength = Random.Range(0.1f, 0.7f);

        //perlin_strength = probability_curve.Evaluate(perlin_strength);
        //voronoi_strength = probability_curve.Evaluate(voronoi_strength);

        routine_time += f;
    }

    void GeneratePlanetoid()
    {
        current_planetoid = new Planetoid();
        current_planetoid.terrain_colors = possible_terrain_colors[Random.Range(0, possible_terrain_colors.Length)];
    }
}
