using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public bool is_spawned = false;
    public bool is_input = false;
    public Transform spawn_pod;
    public ItemManager item_manager;
    public GameObject item_placing;
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
        if (!is_input)
            return;
        if (item_placing)
            PlacingItem();
    }
    bool is_placement_valid = false;
    void PlacingItem()
    {

        //Player is cancelling input on map
        if (Input.GetMouseButtonDown(1))
        {
            item_placing = null;
            item_manager.ClearDisplay();
        }

        //Preview bulding placement and place it if needed
        if (item_placing != null)
        {
            RaycastHit hit;
            Ray ray = main_camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
                if (hit.transform.tag == "Terrain")
                    if (item_manager.DisplayPlacement(item_placing, hit.point, hit.normal))
                        //Place if player clicks
                        if (Input.GetMouseButtonDown(0))
                        {
                             Debug.Log("Player constructed a " + item_placing.GetComponent<Item>().item_name);
                             item_manager.PlaceItem(item_placing,hit.point, hit.normal);
                            item_manager.ClearDisplay();
                        }
        }
    }
}
