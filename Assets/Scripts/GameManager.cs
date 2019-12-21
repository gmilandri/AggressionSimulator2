
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : Singleton<GameManager>
{
    [Header("Prefabs and Arrays")]
    [SerializeField]
    private GameObject m_popPrefab;
    [SerializeField]
    private GameObject m_foodPrefab;
    //public Material DoveMaterial;
    public Material HawkMaterial;
    public GameObject[] FoodObjectPool;
    public Vector2[] FoodPool;
    public Pop[] PopPool;


    [Header("Simulation Variables")]
    [SerializeField]
    private int m_floorSize;
    public int TotalFoodBiomass = 200;
    public int StartingPops = 0;
    public int FoodPerBamboo = 2;
    public int StartingFoodPop = 4;
    public int MinPopSpeed = 4;
    public int MaxPopSpeed = 8;
    public int MinimumPopEnergy = 5;


    [Header("Performance")]
    [Range(0f, 10f)]
    public float timeScale = 1f;
    [Range(1, 10)]
    public int SpeedFactor = 2;

    private float m_squareSize;
    private int m_startIndex = 0;
    private int m_availableBiomass = 0;


    public float GetMaxBoundaries() => m_squareSize;

    public void AvailableBiomassIncreaseBy(int amount) => m_availableBiomass += amount;

    void Start()
    {
        GameObject floor = GameObject.Find("Floor");
        floor.transform.localScale = new Vector3(m_floorSize, 1, m_floorSize);
        floor.transform.position = new Vector3(m_floorSize * 5, 0, m_floorSize * 5);
        m_squareSize = floor.transform.localScale.x * 10 - 2;

        Camera.main.transform.position = new Vector3(floor.transform.position.x, 20, -12);

        FoodPool = new Vector2[TotalFoodBiomass / FoodPerBamboo];
        FoodObjectPool = new GameObject[TotalFoodBiomass / FoodPerBamboo];

        PopPool = new Pop[TotalFoodBiomass / MinimumPopEnergy];

        EventManager.Instance.OnFoodEaten.AddListener(m_gameManager_FoodEaten);

        m_availableBiomass = TotalFoodBiomass;

        InizializeNavMesh();
        PlacePops();
        PlaceFood();
    }

    void Update()
    {
        Time.timeScale = timeScale;

        if (m_availableBiomass >= FoodPerBamboo)
            RespawnFood();

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
            if (m_availableBiomass >= FoodPerBamboo)
            {
                FoodPool[i] = new Vector2(FoodObjectPool[i].transform.position.x, FoodObjectPool[i].transform.position.z);
                m_availableBiomass -= FoodPerBamboo;
            }
            else
            {
                FoodObjectPool[i].SetActive(false);
                FoodPool[i] = Vector2.zero;
            }
        }
    }

    void PlacePops()
    {
        Transform parent = GameObject.Find("PopParent").transform;
        for (int i = 0; i < StartingPops; i++)
        {
            m_availableBiomass -= StartingFoodPop;
            PopPool[i] = Instantiate(m_popPrefab, new Vector3(Random.Range(1, GetMaxBoundaries()), 1f, Random.Range(1, GetMaxBoundaries())), Quaternion.identity, parent).GetComponent<Pop>();
            PopPool[i].gameObject.name = "Pop n." + i.ToString();
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
            if (food == Vector2.zero)
                continue;
            var distance = Vector2.Distance(origin, food);
            if (distance < minDistance)
            {
                minDistance = distance;
                destination = food;
            }
        }

        return destination;
    }

    private void DespawnFood(Vector2 eatenFood)
    {
        for (int i = 0; i < FoodPool.Length; i++)
        {
            if (FoodPool[i] == eatenFood)
            {
                FoodObjectPool[i].SetActive(false);
                FoodPool[i] = Vector2.zero;
                break;
            }
        }
    }

    private void RespawnFood()
    {
        m_availableBiomass -= FoodPerBamboo;
        for (int i = 0; i < FoodPool.Length; i++)
        {
            if (FoodPool[i] == Vector2.zero)
            {
                FoodObjectPool[i].SetActive(true);
                FoodObjectPool[i].transform.position = new Vector3(Random.Range(1, GetMaxBoundaries()), 1f, Random.Range(1, GetMaxBoundaries()));
                FoodPool[i] = new Vector2(FoodObjectPool[i].transform.position.x, FoodObjectPool[i].transform.position.z);
                break;
            }
        }
    }

    void m_gameManager_FoodEaten(Vector2 foodPos)
    {
        DespawnFood(foodPos);

        foreach (var pop in PopPool)
        {
            if (pop == null)
                continue;
            if (!pop.gameObject.activeSelf)
                continue;
            if (pop.MyDestination == foodPos)
                pop.ResetTarget();
        }
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
                pop.ResetTarget();
                //pop.SetDestination();
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
                    PopPool[i].gameObject.name = "Pop n." + i.ToString();
                    break;
                }
            }
        }
    }


}
