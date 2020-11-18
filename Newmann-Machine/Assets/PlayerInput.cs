using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public bool is_spawned = false;
    public bool is_input = false;
    public Transform spawn_pod;
    public ItemManager building_manager;
    public GameObject placing_building;
    public Camera main_camera;
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
        is_input = true;
        yield break;
    }
    private void Update()
    {
        if (!is_spawned)
            return;
        //Do calculations and automatic actions
        //In this part
        if (!is_input)
            return;
        //Do stuff regarding play input
        //In this part
        if (placing_building)
            PlaceBuilding();
    }

    void PlaceBuilding()
    {

        //Player is cancelling input on map
        if (Input.GetMouseButtonDown(1))
            placing_building = null;

        RaycastHit hit;
        Ray ray = main_camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {

            if(hit.transform.tag == "Terrain")
            {
                building_manager.DisplayPlacement(placing_building, hit.point);
            }
        }

        //Preview bulding placement
        if (placing_building != null)
        {
            //Display placement accordingly to its validity
            //if (building_manager)
                //DisplayPlacemenet()


        }
        //Player is cancelling input on map
        if (Input.GetMouseButtonUp(0))
        {
            //If placement is valid
            //Place!
        }
    }
}
