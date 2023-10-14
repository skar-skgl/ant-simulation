using System.Collections.Generic;
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

    private List<Pheromone> leavePheromones = new List<Pheromone>();
    private List<Pheromone> returnPheromones = new List<Pheromone>();
    public float maxPheromoneIntensity = 1f;
    public float minPheromoneIntensity = 0.1f;
    public float evaporationRate = 0.1f;


    public float viewRadius = 1f;
    public float viewAngle = 120f;

    private Transform targetFood;

    public LayerMask foodLayer;
    public LayerMask nestLayer;
    [SerializeField] private GameObject visualizerPrefab;
    private bool hasFood = false;
    private bool hasFoodInSight = false;
    private float timeSinceLastEmit = 0f;
    [SerializeField] private float pheromoneDensity;
    private Transform pheromoneParent;

    private void Start()
    {
        position = transform.position;
        pheromoneParent = GameObject.Find("Pheromones").transform;
    }

    void Update()
    {
        UpdatePheromones();
        DropPheromone(transform.position, maxPheromoneIntensity, hasFood);
        if (!hasFood && !hasFoodInSight)
        {
            // If the ant has no food, it will wander around.
            Wander();

            GatherFood();
        }
        else if (!hasFood && hasFoodInSight)
        {
            // If the ant has no food, but food in sight, it will gather food.
            Wander();

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
        for (int i = leavePheromones.Count - 1; i >= 0; i--)
        {
            Pheromone pheromone = leavePheromones[i];
            pheromone.intensity -= pheromone.evaporationRate * Time.deltaTime;


            if (pheromone.intensity <= minPheromoneIntensity)
            {
                // Remove pheromones that have faded away
                leavePheromones.RemoveAt(i);
                Destroy(pheromone.visualizer);
            }
            else
            {
                UpdatePheromoneVisualizer(pheromone);
            }
        }
        for (int i = returnPheromones.Count - 1; i >= 0; i--)
        {
            Pheromone pheromone = returnPheromones[i];
            pheromone.intensity -= pheromone.evaporationRate * Time.deltaTime;


            if (pheromone.intensity <= minPheromoneIntensity)
            {
                // Remove pheromones that have faded away
                returnPheromones.RemoveAt(i);
                Destroy(pheromone.visualizer);
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
        pheromone.visualizer.GetComponent<Renderer>().material.SetFloat("_intensity", intensityRatio);
        // pheromone.visualizer.GetComponent<TrailRenderer>().endColor = Color.Lerp(Color.Transparent, Color.Red, intensityRatio);
        // pheromone.visualizer.GetComponent<TrailRenderer>().startWidth = 0.1f + 0.5f * intensityRatio;
        // pheromone.visualizer.GetComponent<TrailRenderer>().endWidth = 0.1f + 0.5f * intensityRatio;
    }

    private void ReturnToColony()
    {
        // Calculate the direction to the nest (you need to specify how to find the nest)
        Vector2 directionToNest = CalculateDirectionToNest();

        // Set the desired direction to the direction to the nest
        desiredDirection = directionToNest.normalized;

        // Calculate desired velocity and steering force
        Vector2 desiredVelocity = desiredDirection * maxSpeed;
        Vector2 desiredSteeringForce = (desiredVelocity - velocity) * steerStrength;
        Vector2 acceleration = Vector2.ClampMagnitude(desiredSteeringForce, steerStrength) / 1f;

        // Update velocity and position
        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed);
        position += velocity * Time.deltaTime;

        // Rotate the ant to face its movement direction
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle));


    }

    private Vector2 CalculateDirectionToNest()
    {
        // Check if there are any return pheromones within the ant's view radius
        foreach (Pheromone pheromone in leavePheromones)
        {
            float distanceToPheromone = Vector2.Distance(transform.position, pheromone.position);

            // Check if the pheromone is within the ant's view radius
            if (distanceToPheromone <= viewRadius)
            {
                // Calculate the direction to the pheromone
                Vector2 directionToPheromone = (pheromone.position - (Vector2)transform.position).normalized;

                // Return the direction to the pheromone
                return directionToPheromone;
            }
        }

        // If no pheromones are within view radius, return a default direction (e.g., wander)
        return desiredDirection;
    }

    private void DropPheromone(Vector3 position, float intensity, bool hasFood)
    {
        timeSinceLastEmit += Time.deltaTime;
        if (timeSinceLastEmit <= pheromoneDensity)
            return;

        timeSinceLastEmit = 0f;
        Pheromone pheromone = new Pheromone(position, intensity, evaporationRate)
        {
            visualizer = Instantiate(visualizerPrefab, position, Quaternion.identity, pheromoneParent)
        };


        if (hasFood)
        {
            returnPheromones.Add(pheromone);
            pheromone.visualizer.GetComponent<Renderer>().material.SetInt("_hasFood", 1);
        }
        else
        {
            leavePheromones.Add(pheromone);
            pheromone.visualizer.GetComponent<Renderer>().material.SetInt("_hasFood", 0);
        }
    }

    private void Wander()
    {
        desiredDirection = (desiredDirection + Random.insideUnitCircle * wanderStrength).normalized + PheromoneBias();
        Vector2 desiredVelocity = desiredDirection * maxSpeed;
        Vector2 desiredSteeringForce = (desiredVelocity - velocity) * steerStrength;
        Vector2 acceleration = Vector2.ClampMagnitude(desiredSteeringForce, steerStrength) / 1f;

        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed);
        position += velocity * Time.deltaTime;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle));
    }
    private Vector2 PheromoneBias()
    {
        Vector2 bias = Vector2.zero;
        if (hasFood)
        {
            foreach (Pheromone pheromone in leavePheromones)
            {
                Vector2 dirToPheromone = (pheromone.position - (Vector2)transform.position).normalized;
                float angleToPheromone = Vector2.Angle(velocity.normalized, dirToPheromone);

                if (angleToPheromone < viewAngle / 2)
                {
                    bias += dirToPheromone * pheromone.intensity;
                }
            }
            return bias;
        }
        else
        {
            foreach (Pheromone pheromone in returnPheromones)
            {
                Vector2 dirToPheromone = (pheromone.position - (Vector2)transform.position).normalized;
                float angleToPheromone = Vector2.Angle(velocity.normalized, dirToPheromone);

                if (angleToPheromone < viewAngle / 2)
                {
                    bias += dirToPheromone * pheromone.intensity;
                }
            }
            return bias;
        }
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
            desiredDirection = (targetFood.position - antHead.position).normalized;

            const float foodPickupRadius = 0.05f;
            if (Vector2.Distance(targetFood.position, antHead.position) < foodPickupRadius)
            {
                targetFood.position = antHead.position;
                targetFood.parent = antHead;
                hasFood = true;
                targetFood = null;
                hasFoodInSight = false;
            }
        }
    }
    private void DropFood()
    {
        hasFood = false;
        targetFood.parent = null;
        Destroy(targetFood.gameObject);
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