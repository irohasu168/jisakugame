using UnityEngine;

public class InputAliveProbe : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Debug.Log("[InputAlive] MouseDown");

        if (Input.GetKeyDown(KeyCode.Space))
            Debug.Log("[InputAlive] SpaceDown");

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            Debug.Log("[InputAlive] TouchBegan");
    }
}
