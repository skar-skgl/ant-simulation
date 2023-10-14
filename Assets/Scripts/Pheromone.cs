using UnityEngine;

public class Pheromone
{
    public Vector2 position;
    public float intensity;
    public float evaporationRate;
    public GameObject visualizer;


    public Pheromone(Vector2 pos, float initialIntensity, float evapRate)
    {
        position = pos;
        intensity = initialIntensity;
        evaporationRate = evapRate;
    }

}
