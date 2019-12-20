
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
    public int SpeedFactor = 2;

    public Material DoveMaterial;
    public Material HawkMaterial;

    [Range(0f, 10f)]
    public float timeScale = 1f;

    private int m_startIndex = 0;


    void Start()
    {
        GameObject floor = GameObject.Find("Floor");
        floor.transform.localScale = new Vector3(m_floorSize, 1, m_floorSize);
        floor.transform.position = new Vector3(m_floorSize * 5, 0, m_floorSize * 5);
        m_squareSize = floor.transform.localScale.x * 10 - 2;

        FoodPool = new Vector2[StartingFood];
        FoodObjectPool = new GameObject[StartingFood];
        PopPool = new Pop[m_MaxPops];

        EventManager.Instance.OnFoodEaten.AddListener(m_gameManager_FoodEaten);

        InizializeNavMesh();
        PlaceFood();
        PlacePops();
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;
        for (int i = m_startIndex; i < PopPool.Length; i += SpeedFactor)
        {
            if (PopPool[i] == null)
                continue;
            if (!PopPool[i].gameObject.activeSelf)
                continue;
            PopPool[i].MyUpdate();
        }

        m_startIndex++;
        if (m_startIndex == SpeedFactor)
            m_startIndex = 0;
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

    private void RespawnFood(Vector2 eatenFood)
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

    void m_gameManager_FoodEaten(Vector2 foodPos)
    {
        foreach (var pop in PopPool)
        {
            if (pop == null)
                continue;
            if (!pop.gameObject.activeSelf)
                continue;
            if (pop.MyDestination == foodPos)
                pop.ResetTarget();
        }
        RespawnFood(foodPos);
    }

    public void CreateNewPop(Pop original)
    {
        Transform parent = GameObject.Find("PopParent").transform;
        var createdPop = false;
        foreach (var pop in PopPool)
        {
            if (pop != null && !pop.gameObject.activeSelf)
            {
                pop.gameObject.SetActive(true);
                pop.transform.position = original.transform.position;
                pop.SetDestination();
                createdPop = true;
                break;
            }
        }
        if (!createdPop)
        {
            for (int i= 0; i < PopPool.Length; i++)
            {
                if (PopPool[i] == null)
                {
                    PopPool[i] = Instantiate(m_popPrefab, original.transform.position, Quaternion.identity, parent).GetComponent<Pop>();
                    break;
                }
            }
        }
    }


}
