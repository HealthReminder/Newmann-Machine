using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public List<Item> buildings;
    #region Item Placement
    public void PlaceItem(GameObject item, Vector3 pos, Vector3 up_vector)
    {
        if (!item.GetComponent<Item>())
            Debug.LogError("Cannot instantiate item object with no item script.");

        GameObject new_obj = Instantiate(item, pos, Quaternion.FromToRotation(Vector3.up, up_vector), transform);
        Item new_item = new_obj.GetComponent<Item>();
        buildings.Add(new_item);
    }
    #endregion

    #region Player Display

    Item item_display = null;
    public void ClearDisplay()
    {
        if (item_display)
        {
            Destroy(item_display.gameObject);
            item_display = null;
        }
    }
    public bool DisplayPlacement(GameObject displaying_item, Vector3 pos, Vector3 normal)
    {
        if (!displaying_item)
            Debug.LogError("Cannot instantiate null item.");

        if (!displaying_item.GetComponent<Item>())
            Debug.LogError("Cannot instantiate item with no Placeable script.");

        //Check if it was displaying something already
        if (item_display)
        {
            //Was previewing an item already and the item is different
            if (item_display.item_name != displaying_item.GetComponent<Item>().item_name)
            {
                Destroy(item_display.gameObject);
                item_display = Instantiate(displaying_item, pos, Quaternion.identity, transform).GetComponent<Item>();
                item_display.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                for (int c = 0; c < item_display.coll.Length; c++)
                    item_display.coll[c].enabled = false;
            }
        }
        else
        {
            //Was not previewing any item
            item_display = Instantiate(displaying_item, pos, Quaternion.identity, transform).GetComponent<Item>();
            item_display.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            for (int c = 0; c < item_display.coll.Length; c++)
                item_display.coll[c].enabled = false;
        }

        //Move item and align to normal
        item_display.transform.position = pos;
        item_display.transform.rotation  = Quaternion.FromToRotation(Vector3.up, normal);

        //Change colors according to validity
        if (IsColliderIntersecting(item_display.coll))
        {
            item_display.ChangeColors(Color.red);
            return false;
        }
        else
        {
            item_display.ChangeColors(Color.green);
            return true;
        }

    }
    bool IsColliderIntersecting(Collider[] colls)
    {
        bool is_intersecting = false;
        for (int i = 0; i < colls.Length; i++)
        {
            Collider cur = colls[i];
            if (cur.GetComponent<SphereCollider>())
            {
                SphereCollider s_coll = cur.GetComponent<SphereCollider>();
                if (Physics.OverlapSphere(s_coll.center, s_coll.radius ).Length > 0)
                    is_intersecting = true;
            }
            else if (cur.GetComponent<BoxCollider>())
            {
                BoxCollider b_coll = cur.GetComponent<BoxCollider>();
                if ((Physics.OverlapBox(b_coll.transform.position, b_coll.size ,b_coll.transform.rotation).Length > 0))
                    is_intersecting = true;

            }
            else if (cur.GetComponent<CapsuleCollider>())
            {
                CapsuleCollider c_coll = cur.GetComponent<CapsuleCollider>();
                Vector3 top = new Vector3(c_coll.center.x, c_coll.center.y + c_coll.height / 2 - c_coll.radius, c_coll.center.z);
                Vector3 bottom = new Vector3(c_coll.center.x, c_coll.center.y - c_coll.height / 2 + c_coll.radius, c_coll.center.z);
                if ((Physics.OverlapCapsule(top,bottom,c_coll.radius ).Length > 0))
                    is_intersecting = true;

            }

        }
        return is_intersecting;
    }

    #endregion
}
