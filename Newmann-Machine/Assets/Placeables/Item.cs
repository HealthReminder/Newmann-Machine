using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string item_name;
    public bool is_instantiated;
    public Collider[] coll;
    public Renderer[] renderers;
    public void ChangeColors(Color new_color)
    {
        MaterialPropertyBlock prop_block;
        for (int i = 0; i < renderers.Length; i++)
        {
            prop_block = new MaterialPropertyBlock();
            // Get the current value of the material properties in the renderer.
            renderers[i].GetPropertyBlock(prop_block);
            // Assign our new value.
            prop_block.SetColor("_Color", new_color);
            // Apply the edited values to the renderer.
            renderers[i].SetPropertyBlock(prop_block);
        }
    }
}
