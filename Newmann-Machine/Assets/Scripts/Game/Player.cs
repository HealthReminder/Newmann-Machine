using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool is_spawned = false;
    public Transform spawn_pod;
    public IEnumerator SpawnRoutine(Vector3 terrain_position)
    {
        //This routine should be part of a script that every artificial item or unit inherits and has functions appropriate for it like falling from the sky to position

        //Initiate player above center of terrain 
        spawn_pod.position = terrain_position + new Vector3(0, 100, 0);
        spawn_pod.gameObject.SetActive(true);

        RaycastHit hit;
        //Throw error if no terrain was found
        if (!Physics.Raycast(spawn_pod.position, Vector3.down, out hit, 500))
            Debug.LogError("Player spawn did not find terrain.");

        //Move pod towares hit point
        float progress = 0;
        Vector3 init_pos = spawn_pod.position;
        while (progress < 1)
        {
            if (GlobalConstants.INTRO_SPEED == 0)
                yield break;

            spawn_pod.position = Vector3.Lerp(init_pos, hit.point, progress);
            progress += Time.deltaTime* GlobalConstants.INTRO_SPEED;
            yield return null;
        }
        spawn_pod.position = hit.point;
        spawn_pod.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);


        is_spawned = true;
        yield break;
    }
}
