using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public List<Item> buildings;

    #region Player Display

    Item item_display = null;
    public bool DisplayPlacement(GameObject displaying_item, Vector3 pos)
    {
        if (!displaying_item.GetComponent<Item>())
            Debug.LogError("Cannot instantiate object with no Placeable script.");

        //Check if it was displaying something already
        if (item_display)
        {
            //Was previewing an item already and the item is different
            if (item_display.item_name != displaying_item.GetComponent<Item>().item_name)
            {
                Destroy(item_display.gameObject);
                item_display = Instantiate(displaying_item, pos, Quaternion.identity, transform).GetComponent<Item>();

            }

        }
        else
            //Was not previewing any item
            item_display = Instantiate(displaying_item, pos, Quaternion.identity, transform).GetComponent<Item>();

        item_display.transform.position = pos;

        item_display.ChangeColors(Color.red);
        //Check validity

        return false;
    }
    #endregion
}
