using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TerrainGenerator terrain_gen;
    public ResourceManager resource_manager;
    public PlayerInput player_input;
    //Game loop
    Dictionary<string, Item> player_inventory; //This is basically the player economy
    private void Start()
    {
        StartCoroutine(GameRoutine());
    }
    IEnumerator GameRoutine()
    {
        while (true)
        {
            yield return terrain_gen.GenerateNewTerrain();
            yield return resource_manager.PopulateResources();
            yield return player_input.SpawnRoutine(terrain_gen.transform.position);

            //Game loop
            while (true)
            {
                //Match loop
                yield return null;

            }
            yield return null;
        }
        yield break;
    }
}
