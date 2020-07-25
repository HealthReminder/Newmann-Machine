using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable] public class Planetoid
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
        GeneratePlanetoid();
        yield return StartCoroutine(terrain.DeformTrianglesRandomly());
        yield return StartCoroutine(terrain.ColorHeight(current_planetoid.terrain_colors));
        //Smoothes colors
        yield return StartCoroutine(terrain.ColorAverage());
        //Does not work as intended
        //yield return StartCoroutine(terrain.ColorMajority());
        yield break;
    }

    void GeneratePlanetoid ()
    {
        current_planetoid = new Planetoid();
        current_planetoid.terrain_colors = possible_terrain_colors[Random.Range(0,possible_terrain_colors.Length)];
    }
}
