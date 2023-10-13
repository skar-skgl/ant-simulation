using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ant : MonoBehaviour
{
    public bool drawGizmos = true;
    [SerializeField] private Transform antHead;
    public float maxSpeed = 2f;
    public float steerStrength = 2f;
    public float wanderStrength = 1f;

    Vector2 position;
    Vector2 velocity;
    Vector2 desiredDirection;

    private List<Pheromone> pheromones = new List<Pheromone>();
    public float maxPheromoneIntensity = 1f;
    public float minPheromoneIntensity = 0.1f;

    private PheromoneEmitter pheromoneParticleSystem;

    public float viewRadius = 1f;
    public float viewAngle = 120f;

    private Transform targetFood;

    public LayerMask foodLayer;
    public LayerMask nestLayer;
    private bool hasFood = false;
    private bool hasFoodInSight = false;
    private float timeSinceLastEmit = 0f;

    void Start()
    {
        pheromoneParticleSystem = FindObjectOfType<PheromoneEmitter>();
    }

    void Update()
    {
        LeavePheromone(transform.position, maxPheromoneIntensity);

        UpdatePheromones();
        if (!hasFood && !hasFoodInSight)
        {
            // If the ant has no food, it will wander around.
            Wander();
            GatherFood();
        }
        else if (!hasFood && hasFoodInSight)
        {
            // If the ant has no food, but food in sight, it will gather food.
            GatherFood();
        }
        else if (hasFood)
        {
            // If the ant has food, but no food in sight, it will return to the nest.
            ReturnToColony();
        }

    }

    private void UpdatePheromones()
    {
        for (int i = pheromones.Count - 1; i >= 0; i--)
        {
            Pheromone pheromone = pheromones[i];
            pheromone.intensity -= pheromone.evaporationRate * Time.deltaTime;
            

            if (pheromone.intensity <= minPheromoneIntensity)
            {
                // Remove pheromones that have faded away
                pheromones.RemoveAt(i);
                pheromone.visualizer.remainingLifetime = 0f;
            }
            else
            {
                UpdatePheromoneVisualizer(pheromone);
            }
        }
    }
    private void UpdatePheromoneVisualizer(Pheromone pheromone)
    {
        // Update the visualizer's position to match the pheromone's position
        //pheromone.visualizer.transform.position = new Vector3(pheromone.position.x, pheromone.position.y, 0f);

        // Update the visualizer's appearance based on pheromone intensity
        float intensityRatio = (pheromone.intensity - minPheromoneIntensity) / (maxPheromoneIntensity - minPheromoneIntensity);

        // Adjust the visualizer's color, size, or other properties based on intensityRatio
        // For example, change the color of the trail renderer or adjust the particle size
        // pheromone.visualizer.GetComponent<TrailRenderer>().startColor = Color.Lerp(Color.Transparent, Color.Red, intensityRatio);
        // pheromone.visualizer.GetComponent<TrailRenderer>().endColor = Color.Lerp(Color.Transparent, Color.Red, intensityRatio);
        // pheromone.visualizer.GetComponent<TrailRenderer>().startWidth = 0.1f + 0.5f * intensityRatio;
        // pheromone.visualizer.GetComponent<TrailRenderer>().endWidth = 0.1f + 0.5f * intensityRatio;
    }

    private void ReturnToColony()
    {
        // Implement this method
        LeavePheromone(transform.position, maxPheromoneIntensity);

    }

    private void LeavePheromone(Vector3 position, float intensity)
    {
        timeSinceLastEmit += Time.deltaTime;
        if (timeSinceLastEmit <= pheromoneParticleSystem.PheromoneEmittInterval)
            return;
            
        timeSinceLastEmit = 0f;
        Pheromone pheromone = new Pheromone(position, intensity, pheromoneParticleSystem.PheromoneEvaporationRate);

        // Instantiate the pheromone visualizer prefab
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
        {
            //particle = pheromone.visualizer,
            position = position,
            startColor = Color.red
        };
        pheromoneParticleSystem.pheromoneParticleSystem.Emit(emitParams, 1);

        pheromones.Add(pheromone);
    }

    private void Wander()
    {
        desiredDirection = (desiredDirection + Random.insideUnitCircle * wanderStrength).normalized;
        Vector2 desiredVelocity = desiredDirection * maxSpeed;
        Vector2 desiredSteeringForce = (desiredVelocity - velocity) * steerStrength;
        Vector2 acceleration = Vector2.ClampMagnitude(desiredSteeringForce, steerStrength) / 1f;

        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed);
        position += velocity * Time.deltaTime;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle));
    }

    private void GatherFood()
    {
        if (targetFood == null)
        {
            Collider2D[] foodsInSight = Physics2D.OverlapCircleAll(antHead.position, viewRadius, foodLayer);

            if (foodsInSight.Length > 0)
            {
                hasFoodInSight = true;
                // Pick a random food in sight
                Transform food = foodsInSight[Random.Range(0, foodsInSight.Length)].transform;
                Vector2 dirToFood = (food.position - antHead.position).normalized;
                float angleToFood = Vector2.Angle(velocity.normalized, dirToFood);

                if (angleToFood < viewAngle / 2)
                {
                    food.gameObject.layer = LayerMask.NameToLayer("TakenFood");
                    targetFood = food;
                }
            }
        }
        else
        {
            hasFoodInSight = false;
            desiredDirection = (targetFood.position - antHead.position).normalized;

            const float foodPickupRadius = 0.05f;
            if (Vector2.Distance(targetFood.position, antHead.position) < foodPickupRadius)
            {
                targetFood.position = antHead.position;
                targetFood.parent = antHead;
                hasFood = true;
                targetFood = null;
            }
        }
    }

    #region Visualizations
    void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;
        // Define the color for the field of view visualization
        Gizmos.color = Color.blue;

        // Visualize the view angle using segments
        int numSegments = 50; // Increase for smoother arc
        Vector2 prevDir = DirFromAngle(-viewAngle / 2, false);

        for (int i = 0; i <= numSegments; i++)
        {
            float t = (float)i / (float)numSegments;
            float currAngle = Mathf.Lerp(-viewAngle / 2, viewAngle / 2, t);
            Vector2 currDir = DirFromAngle(currAngle, false);

            // Draw a segment of the field of view "slice"
            Gizmos.DrawLine((Vector2)antHead.position + prevDir * viewRadius, (Vector2)antHead.position + currDir * viewRadius);

            prevDir = currDir;
        }

        // Draw the boundary lines of the slice
        Vector2 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector2 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.DrawLine((Vector2)antHead.position, (Vector2)antHead.position + viewAngleA * viewRadius);
        Gizmos.DrawLine((Vector2)antHead.position, (Vector2)antHead.position + viewAngleB * viewRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(antHead.position, viewRadius);
    }

    Vector2 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }
        return new Vector2(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), Mathf.Sin(angleInDegrees * Mathf.Deg2Rad));
    }


    #endregion
}