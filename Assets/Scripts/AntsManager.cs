using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntsManager : MonoBehaviour
{
    [SerializeField] private GameObject antPrefab;
    private int antCount = 0;
    [SerializeField] private int numberOfAnts = 100;
    private Transform antParent;
    
    void Start()
    {
        antParent = GameObject.Find("Ants").transform;
        SpawnAnts();
    }

    private void SpawnAnts()
    {
        for (int i = 0; i < numberOfAnts; i++)
        {
            Instantiate(antPrefab, new Vector3(UnityEngine.Random.Range(-10, 10), 0, UnityEngine.Random.Range(-10, 10)), Quaternion.identity);
            antCount++;
        }
    }
}
