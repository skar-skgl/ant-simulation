using UnityEngine;
using System.Collections.Generic;

public class PheromoneManager : MonoBehaviour
{
    public static PheromoneManager instance;
    [SerializeField] private Transform pheromoneParent;

    private void Awake()
    {
        instance = this;
    }
    [HideInInspector] public List<Pheromone> returnPheromones = new List<Pheromone>();
    [HideInInspector] public List<Pheromone> leavePheromones = new List<Pheromone>();
    public float minPheromoneIntensity;
    public float maxPheromoneIntensity;

    // Create a method to add pheromones to the manager.
    public void AddLeavePheromone(Pheromone pheromone)
    {
        leavePheromones.Add(pheromone);
        pheromone.visualizer.GetComponent<Renderer>().material.SetInt("_hasFood", 0);
    }
    public void AddReturnPheromone(Pheromone pheromone)
    {
        returnPheromones.Add(pheromone);
        pheromone.visualizer.GetComponent<Renderer>().material.SetInt("_hasFood", 1);
    }

    // Create a method to update pheromones (e.g., handle evaporation).
    private void UpdatePheromones()
    {
        for (int i = leavePheromones.Count - 1; i >= 0; i--)
        {
            Pheromone pheromone = leavePheromones[i];
            pheromone.intensity -= pheromone.evaporationRate * Time.deltaTime;
            pheromone.visualizer.GetComponent<Renderer>().material.SetFloat("_intensity", pheromone.intensity);


            if (pheromone.intensity <= 0f)
            {
                // Remove pheromones that have faded away.
                leavePheromones.RemoveAt(i);
                Destroy(pheromone.visualizer);
            }
        }
        for (int i = returnPheromones.Count - 1; i >= 0; i--)
        {
            Pheromone pheromone = returnPheromones[i];
            pheromone.intensity -= pheromone.evaporationRate * Time.deltaTime;
            pheromone.visualizer.GetComponent<Renderer>().material.SetFloat("_intensity", pheromone.intensity);


            if (pheromone.intensity <= 0f)
            {
                // Remove pheromones that have faded away.
                returnPheromones.RemoveAt(i);
                Destroy(pheromone.visualizer);
            }
        }
    }

    private void Update()
    {
        UpdatePheromones();
    }
}
