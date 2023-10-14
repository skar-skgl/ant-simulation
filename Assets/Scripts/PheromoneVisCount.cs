using UnityEngine;

public class PheromoneVisCount : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Debug.Log("PheromoneVisCount" + transform.childCount);
    }
}
