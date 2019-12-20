using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private int m_floorSize;

    [SerializeField]
    private GameObject m_popPrefab;
    [SerializeField]
    private GameObject m_foodPrefab;
    private float m_squareSize;

    public int StartingPops = 0;
    public int StartingFood = 0;

    public float GetMaxBoundaries() => m_squareSize;

    public GameObject[] FoodObjectPool;
    public Vector2[] FoodPool;

    public Pop[] PopPool;

    private const int m_MaxPops = 1000;

    [Range(0f, 10f)]
    public float timeScale = 1f;


    void Start()
    {
        GameObject floor = GameObject.Find("Floor");
        floor.transform.localScale = new Vector3(m_floorSize, 1, m_floorSize);
        floor.transform.position = new Vector3(m_floorSize * 5, 0, m_floorSize * 5);
        m_squareSize = floor.transform.localScale.x * 10 - 2;

        FoodPool = new Vector2[StartingFood];
        FoodObjectPool = new GameObject[StartingFood];
        PopPool = new Pop[m_MaxPops];

        InizializeNavMesh();
        PlaceFood();
        PlacePops();
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;
        foreach (var pop in PopPool)
        {
            if (pop == null)
                continue;
            pop.MyUpdate();
        }
    }

    void PlaceFood()
    {
        Transform parent = GameObject.Find("FoodParent").transform;
        for (int i = 0; i < FoodPool.Length; i++)
        {
            FoodObjectPool[i] = Instantiate(m_foodPrefab, new Vector3(Random.Range(1, GetMaxBoundaries()), 1f, Random.Range(1, GetMaxBoundaries())), Quaternion.identity, parent);
            FoodPool[i] = new Vector2(FoodObjectPool[i].transform.position.x, FoodObjectPool[i].transform.position.z);    
        }
    }

    void PlacePops()
    {
        Transform parent = GameObject.Find("PopParent").transform;
        for (int i = 0; i < StartingPops; i++)
        {
            PopPool[i] = Instantiate(m_popPrefab, new Vector3(Random.Range(1, GetMaxBoundaries()), 1f, Random.Range(1, GetMaxBoundaries())), Quaternion.identity, parent).GetComponent<Pop>();
        }
    }

    void InizializeNavMesh()
    {
        GameObject.Find("NavMesh").GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    public Vector2 ClosestDestination (Vector2 origin)
    {
        float minDistance = float.MaxValue;
        Vector2 destination = new Vector2();

        foreach (var food in FoodPool)
        {
            var distance = Vector2.Distance(origin, food);
            if (distance < minDistance)
            {
                minDistance = distance;
                destination = food;
            }
        }

        return destination;
    }

    public void RespawnFood(Vector2 eatenFood)
    {
        for (int i = 0; i < FoodPool.Length; i++)
        {
            if (FoodPool[i] == eatenFood)
            {
                FoodObjectPool[i].transform.position = new Vector3(Random.Range(1, GetMaxBoundaries()), 1f, Random.Range(1, GetMaxBoundaries()));
                FoodPool[i] = new Vector2(FoodObjectPool[i].transform.position.x, FoodObjectPool[i].transform.position.z);
                break;
            }
        }
    }
}
