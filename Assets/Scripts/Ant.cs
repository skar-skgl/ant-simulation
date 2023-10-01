using Unity.VisualScripting;
using UnityEngine;

public class Ant : MonoBehaviour
{
    [SerializeField] private Transform antHead;
    public float maxSpeed = 2f;
    public float steerStrength = 2f;
    public float wanderStrength = 1f;

    Vector2 position;
    Vector2 velocity;
    Vector2 desiredDirection;

    public float viewRadius = 1f;
    public float viewAngle = 120f;

    public LayerMask foodLayer;
    private bool hasFood = false;
    private bool hasFoodInSight = false;


    void Update()
    {
        if (!hasFood && !hasFoodInSight)
        {
            // If the ant has no food, it will wander around.
            Wander();
            GatherFood();
        }
        else if (hasFood)
        {
            // If the ant has food, but no food in sight, it will return to the nest.
            ReturnToNest();
        }
    }

    private void ReturnToNest()
    {
        throw new System.NotImplementedException();
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
        Collider2D[] foodsInSight = Physics2D.OverlapCircleAll(antHead.position, viewRadius, foodLayer);

        foreach (var foodCollider in foodsInSight)
        {
            Vector2 foodPosition = foodCollider.transform.position;
            Vector2 directionToFood = (foodPosition - (Vector2)antHead.position).normalized;

            // Calculate the angle between the ant's forward direction and the direction to the food
            float angleToFood = Vector2.Angle(velocity.normalized, directionToFood);

            // Check if food is within the ant's view angle
            if (angleToFood < viewAngle / 2)
            {
                desiredDirection = directionToFood;

                // If the ant is very close to the food, consume or collect the food.
                if (Vector2.Distance(antHead.position, foodPosition) < 0.1f)
                {
                    CollectFood(foodCollider);
                }
                break; // The ant will move to the first food it sees within its view angle. If you want the ant to prioritize closer food or some other logic, you'd adjust here.
            }
        }
    }

    private void CollectFood(Collider2D foodInSight)
    {
        hasFood = true;
        foodInSight.transform.SetParent(antHead);
    }

    #region Visualizations
    void OnDrawGizmos()
    {
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