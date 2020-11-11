using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool is_spawned = false;
    public Transform spawn_pod;
    public void Spawn(Vector3 terrain_position)
    {
        StartCoroutine(SpawnRoutine(terrain_position));
    }
    IEnumerator SpawnRoutine(Vector3 terrain_position)
    {
        yield return new WaitForSeconds(2.0f);
        //In the future the player must be spawned using an initial pod
        spawn_pod.position = terrain_position + new Vector3(0, 100, 0);
        RaycastHit hit;
        if (Physics.Raycast(spawn_pod.position, Vector3.down, out hit, 500))
        {
            spawn_pod.position = hit.point;
            spawn_pod.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
        else
            Debug.LogError("Player spawn did not find terrain.");
        spawn_pod.gameObject.SetActive(true);

        is_spawned = true;
        yield break;
    }
}
