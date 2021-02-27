using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string item_name;
    public Vector2 angle_range;
    [HideInInspector] public bool is_instantiated;
    public Collider[] colls;
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
