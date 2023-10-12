using UnityEngine;

public class PheromoneGrid : MonoBehaviour
{
    public float[,] grid;
    public float decayRate = 0.99f;
    public float dropAmount = 1.0f;
    public int gridSize = 100;

    public GameObject pheromoneParticlePrefab;
    private ParticleSystem[,] particleSystems;

    void Start()
    {
        grid = new float[gridSize, gridSize];
        InitializeParticleSystems();
    }

    void Update()
    {
        DecayPheromones();
        UpdatePheromoneVisuals();
    }

    private void DecayPheromones()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                grid[i, j] *= decayRate;
            }
        }
    }

    public void DropPheromone(Vector2 position, float amount)
    {
        // Translate world position to grid position and drop pheromones
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);

        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
        {
            grid[x, y] += amount;
        }
    }

    void InitializeParticleSystems()
    {
        particleSystems = new ParticleSystem[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                GameObject particleInstance = Instantiate(pheromoneParticlePrefab, new Vector3(i, j, 0), Quaternion.identity, this.transform);
                particleSystems[i, j] = particleInstance.GetComponent<ParticleSystem>();
                particleInstance.SetActive(false); // Start them deactivated.
            }
        }
    }

    void UpdatePheromoneVisuals()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                // A threshold to decide whether to activate particle system or not. Adjust according to your needs.
                if (grid[i, j] > 0.1f)
                {
                    particleSystems[i, j].gameObject.SetActive(true);

                    // Modify particle properties based on grid values.
                    // Here, you may set emission, color, size, etc. depending on your design.
                    var emissionModule = particleSystems[i, j].emission;
                    emissionModule.rateOverTime = grid[i, j] * 10f; // Example, tweak as per requirement.
                }
                else
                {
                    particleSystems[i, j].gameObject.SetActive(false);
                }
            }
        }
    }
}
