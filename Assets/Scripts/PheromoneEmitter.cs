using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneEmitter : MonoBehaviour
{
    public ParticleSystem pheromoneParticleSystem;
    public float PheromoneEmittInterval = 0.5f;
    public float PheromoneEvaporationRate = 0.5f;
    
    void Start()
    {
        pheromoneParticleSystem = GetComponent<ParticleSystem>();
    }
}
