using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    void Update()
    {
        // Does the player exist?
        if(PlayerController.me && !PlayerController.me.dead)
        {
            Vector3 targetPos = PlayerController.me.transform.position;
            targetPos.z = -10;

            transform.position = targetPos;
        }
    }
}
