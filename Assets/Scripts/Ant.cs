using UnityEngine;

public class Ant : MonoBehaviour
{
    public float maxSpeed = 2f;
    public float steerStrength = 2f;
    public float wanderStrength = 1f;

    Vector2 position;
    Vector2 velocity;
    Vector2 desiredDirection;

    public float viewRadius = 1f;
    public float viewAngle = 120f;

    void Update()
    {
        MoveAnt();
    }

    private void MoveAnt()
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
}