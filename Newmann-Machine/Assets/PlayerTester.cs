using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTester : MonoBehaviour
{
    public Transform terrain_transform;
    public Player player;
    private void Start()
    {
        player.Spawn(terrain_transform.position);
    }
}
