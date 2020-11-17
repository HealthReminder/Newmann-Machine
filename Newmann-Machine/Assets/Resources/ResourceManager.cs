using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public GameObject resource_prefab;
    List<Resource> instances;
    public IEnumerator PopulateResources()
    {
        //Instantiate 100 resources
        for (int i = 0; i < 100; i++)
            instances.Add(Instantiate(resource_prefab, transform.position + new Vector3(Random.Range(0,100),100,Random.Range(0,100)), Quaternion.identity, transform).GetComponent<Resource>());

        for (int i = 0; i < instances.Count; i++)
        {
            Transform t = instances[i].transform;
            Resource r = t.GetComponent<Resource>();
            t.gameObject.SetActive(false);

            RaycastHit hit;
            //Throw error if no terrain was found
            if (!Physics.Raycast(t.position, Vector3.down, out hit, 500))
                Debug.LogWarning("Resource spawn did not find terrain.");
            else
            {
                //This routine should be part of a script that every natural or organic item or unit inherits and has functions appropriate for it like growing out of the ground inspired by the player script
                //Move pod towares hit point
                t.position = hit.point;
                t.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                r.is_instantiated = true;
                t.gameObject.SetActive(true);
            }
        }
        Debug.Log("Populated map with resource instances.");
        yield break;
    }
    private void Awake()
    {
        Setup();
    }
    void Setup()
    {
        instances = new List<Resource>();
    }
}
