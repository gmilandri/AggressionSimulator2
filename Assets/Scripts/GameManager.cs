using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : Singleton<GameManager>
{
    [Header("Prefabs and Arrays")]
    [SerializeField]
    private GameObject m_popDovePrefab;
    [SerializeField]
    private GameObject m_popHawkPrefab;
    [SerializeField]
    private GameObject m_foodPrefab;
    [SerializeField]
    private GameObject m_eggPrefab;

    public GameObject[] FoodObjectPool;
    public GameObject[] PopObjectPool;
    public Pop[] PopPool;
    public GameObject[] EggObjectPool;
    public EggSpawnerInformation[] EggPool;

    [Header("Simulation Variables")]
    [SerializeField]
    private int m_floorSize;
    public int TotalFoodBiomass = 200;
    public int StartingPops = 0;
    public int FoodPerBamboo = 2;
    public int StartingFoodPop = 4;
    public int StartingPopSpeed = 4;
    public int MinimumPopEnergy = 5;
    [Range(1, 100)]
    public int MutationChance = 5;
    public bool DoveGenerosity = true;
    private float SpeedMutationChange;
    public int DoveCount;
    public int HawkCount;

    [HideInInspector]
    public int SpeedFactor = 1;
    private float m_squareSize;
    private int m_startIndex = 0;
    private float m_availableBiomass = 0;


    public float GetMaxBoundaries() => m_squareSize;

    public void AvailableBiomassIncreaseBy(float amount) => m_availableBiomass += amount;

    void Start()
    {
        Debug.unityLogger.logEnabled = false;
        GameObject floor = GameObject.Find("Floor");
        floor.transform.localScale = new Vector3(m_floorSize, 1, m_floorSize);
        floor.transform.position = new Vector3(m_floorSize * 5, 0, m_floorSize * 5);
        m_squareSize = floor.transform.localScale.x * 10 - 2;

        Camera.main.transform.position = new Vector3(floor.transform.position.x, 15, -12);

        FoodObjectPool = new GameObject[TotalFoodBiomass / FoodPerBamboo];

        PopPool = new Pop[m_floorSize * 50];//TotalFoodBiomass / (MinimumPopEnergy * 2)];
        PopObjectPool = new GameObject[m_floorSize * 50];

        EggPool = new EggSpawnerInformation[m_floorSize * 50];
        EggObjectPool = new GameObject[m_floorSize * 50];

        EventManager.Instance.OnPopEaten.AddListener(m_gameManager_PopEaten);

        m_availableBiomass = TotalFoodBiomass;

        SpeedMutationChange = StartingPopSpeed / 5f;

        InizializeNavMesh();
        PlacePops();
        PlaceFood();
    }

    void Update()
    {
        if (m_startIndex == 0)
        {
            int activePops = 0;
            foreach (var pop in PopObjectPool)
                if (pop != null && pop.activeSelf)
                    activePops++;

            int HundredsofPops = activePops / 40;
            if (HundredsofPops == 0)
                SpeedFactor = 1;
            else
                SpeedFactor = HundredsofPops;
        }

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

        for (int i = 0; i < FoodObjectPool.Length; i++)
        {
            FoodObjectPool[i] = Instantiate(m_foodPrefab, new Vector3(Random.Range(1, GetMaxBoundaries()), 1f, Random.Range(1, GetMaxBoundaries())), Quaternion.identity, parent);
            if (m_availableBiomass >= FoodPerBamboo)
            {
                m_availableBiomass -= FoodPerBamboo;
            }
            else
            {
                FoodObjectPool[i].SetActive(false);
            }
        }
    }

    void PlacePops()
    {
        Transform parent = GameObject.Find("PopParent").transform;
        for (int i = 0; i < StartingPops; i++)
        {
            m_availableBiomass -= StartingFoodPop;
            PopPool[i] = Instantiate(m_popDovePrefab, new Vector3(Random.Range(1, GetMaxBoundaries()), 1f, Random.Range(1, GetMaxBoundaries())), Quaternion.identity, parent).GetComponent<Pop>();
            PopPool[i].gameObject.name = "Pop n." + i.ToString();
            PopObjectPool[i] = PopPool[i].gameObject;
        }
        DoveCount = StartingPops;
    }

    void InizializeNavMesh()
    {
        GameObject.Find("NavMesh").GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    public int ClosestPopDestination(Vector3 origin, float hunterSize)
    {
        float minDistance = float.MaxValue;
        int destination = -1;

        for (int i = 0; i < PopObjectPool.Length; i++)
        {
            if (PopObjectPool[i] == null)
                continue;
            if (!PopObjectPool[i].activeSelf)
                continue;
            if (PopPool[i].Size > hunterSize)
                continue;
            if (origin == PopObjectPool[i].transform.position)
                continue;
            var distance = Vector3.Distance(origin, PopObjectPool[i].transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                destination = i;
            }
        }

        return destination;
    }

    public void SpawnEgg(Pop original)
    {

        Transform parent = GameObject.Find("EggParent").transform;

        foreach (var egg in EggObjectPool)
        {
            if (egg != null && !egg.activeSelf)
            {
                egg.SetActive(true);
                var eggInfo = egg.GetComponent<EggSpawnerInformation>();
                eggInfo.SetPopType(original);
                eggInfo.SetOriginalSpeed(original.GetComponent<NavMeshAgent>().speed);
                eggInfo.SetOriginalMetabolism(original.MetabolismRate);
                eggInfo.SetOriginalSize(original.Size);
                egg.transform.position = new Vector3(original.gameObject.transform.position.x, egg.transform.localScale.y / 2, original.gameObject.transform.position.z);
                StartCoroutine(eggInfo.BirthCountdown());
                return;
            }
        }
        for (int i = 0; i < EggObjectPool.Length; i++)
        {
            if (EggObjectPool[i] == null)
            {
                EggPool[i] = Instantiate(m_eggPrefab, new Vector3(original.gameObject.transform.position.x, m_eggPrefab.transform.localScale.y / 2, original.gameObject.transform.position.z), Quaternion.identity, parent).GetComponent<EggSpawnerInformation>();
                EggPool[i].gameObject.name = "Egg n." + i.ToString();
                EggObjectPool[i] = EggPool[i].gameObject;
                EggPool[i].SetPopType(original);
                var eggInfo = EggPool[i].GetComponent<EggSpawnerInformation>();
                eggInfo.SetOriginalSpeed(original.GetComponent<NavMeshAgent>().speed);
                eggInfo.SetOriginalMetabolism(original.MetabolismRate);
                eggInfo.SetOriginalSize(original.Size);
                StartCoroutine(eggInfo.BirthCountdown());
                return;
            }
        }
    }

    private void DespawnPop(int eatenFood)
    {
        PopPool[eatenFood].ResetSelf();
    }

    private void RespawnFood()
    {
        m_availableBiomass -= FoodPerBamboo;
        for (int i = 0; i < FoodObjectPool.Length; i++)
        {
            if (!FoodObjectPool[i].activeSelf)
            {
                FoodObjectPool[i].SetActive(true);
                FoodObjectPool[i].transform.position = new Vector3(Random.Range(1, GetMaxBoundaries()), 1f, Random.Range(1, GetMaxBoundaries()));
                break;
            }
        }
    }

    void m_gameManager_PopEaten(int foodPos)
    {
        DespawnPop(foodPos);
    }

    public bool IsDestinationOnFloor(Vector3 destination)
    {
        if (destination.x < 1 || destination.x > GetMaxBoundaries())
            return false;
        if (destination.z < 1 || destination.z > GetMaxBoundaries())
            return false;
        return true;
    }

    public void CreateNewPop(EggSpawnerInformation original)
    {
        MutationType mutation = MutationType.None;

        if (RandomMutationChance)
            mutation = (MutationType)Random.Range(0, 8);

        switch (mutation)
        {
            case MutationType.None:
                SpawnSamePop(original);
                break;
            case MutationType.SwitchBehaviour:
                SpawnDifferentPop(original);
                break;
            case MutationType.SpeedIncrease:
                SpawnSamePop(original, shouldIncreaseSpeed: true);
                break;
            case MutationType.SpeedDecrease:
                SpawnSamePop(original, shouldDecreaseSpeed: true);
                break;
            case MutationType.MetabolismIncrease:
                SpawnSamePop(original, shouldIncreaseMetabolism: true);
                break;
            case MutationType.MetabolismDecrease:
                SpawnSamePop(original, shouldDecreaseMetabolism: true);
                break;
            case MutationType.IncreaseSize:
                SpawnSamePop(original, shouldIncreaseSize: true);
                break;
            case MutationType.DecreaseSize:
                SpawnSamePop(original, shouldDecreaseSize: true);
                break;

        }
    }

    private void SpawnSamePop(
        EggSpawnerInformation original,
        bool shouldIncreaseSpeed = false,
        bool shouldDecreaseSpeed = false,
        bool shouldIncreaseMetabolism = false,
        bool shouldDecreaseMetabolism = false,
        bool shouldIncreaseSize = false,
        bool shouldDecreaseSize = false)
    {
        Transform parent = GameObject.Find("PopParent").transform;

        foreach (var pop in PopPool)
        {
            if (pop != null && !pop.gameObject.activeSelf && SamePopType(original, pop))
            {
                pop.gameObject.SetActive(true);
                pop.transform.position = original.gameObject.transform.position;

                CopyInfoEggOnNewPop(original, pop);

                if (shouldIncreaseSpeed)
                    pop.GetComponent<NavMeshAgent>().speed += SpeedMutationChange;
                if (shouldDecreaseSpeed)
                {                
                    if (original.OriginalSpeed - SpeedMutationChange > 0)
                        pop.GetComponent<NavMeshAgent>().speed -= SpeedMutationChange;
                }
                if (shouldIncreaseMetabolism && pop.MetabolismRate < 100)
                    pop.MetabolismRate++;
                if (shouldDecreaseMetabolism)
                    pop.MetabolismRate--;
                if (shouldIncreaseSize && original.OriginalSpeed - SpeedMutationChange > 0)
                {
                    pop.Size += 0.1f;
                    pop.GetComponent<NavMeshAgent>().speed = original.OriginalSpeed - SpeedMutationChange;                    
                }
                if (shouldDecreaseSize && original.OriginalSize > 0.1f)
                {
                    pop.Size -= 0.1f;
                    pop.GetComponent<NavMeshAgent>().speed = original.OriginalSpeed + SpeedMutationChange;
                }

                pop.gameObject.transform.localScale = new Vector3(pop.Size, pop.Size, pop.Size);
                //pop.ResetTarget();

                if (pop is Dove)
                    DoveCount++;
                else
                    HawkCount++;
                return;
            }
        }
        for (int i = 0; i < PopPool.Length; i++)
        {
            if (PopPool[i] == null)
            {
                PopPool[i] = Instantiate(GetRightPrefab(original), original.gameObject.transform.position, Quaternion.identity, parent).GetComponent<Pop>();
                PopPool[i].gameObject.name = "Pop n." + i.ToString();
                PopObjectPool[i] = PopPool[i].gameObject;

                CopyInfoEggOnNewPop(original, PopPool[i]);

                if (shouldIncreaseSpeed)
                    PopPool[i].GetComponent<NavMeshAgent>().speed += SpeedMutationChange;
                if (shouldDecreaseSpeed)
                {
                    if (original.OriginalSpeed - SpeedMutationChange > 0)
                        PopPool[i].GetComponent<NavMeshAgent>().speed -= SpeedMutationChange;
                }
                if (shouldIncreaseMetabolism && PopPool[i].MetabolismRate < 100)
                    PopPool[i].MetabolismRate++;
                if (shouldDecreaseMetabolism)
                    PopPool[i].MetabolismRate--;
                if (shouldIncreaseSize && original.OriginalSpeed - SpeedMutationChange > 0)
                {
                    PopPool[i].Size += 0.1f;
                    PopPool[i].GetComponent<NavMeshAgent>().speed = original.OriginalSpeed - SpeedMutationChange;
                }
                if (shouldDecreaseSize && original.OriginalSize > 0.1f)
                {
                    PopPool[i].Size -= 0.1f;
                    PopPool[i].GetComponent<NavMeshAgent>().speed = original.OriginalSpeed + SpeedMutationChange;
                }

                PopObjectPool[i].transform.localScale = new Vector3(PopPool[i].Size, PopPool[i].Size, PopPool[i].Size);

                if (PopPool[i] is Dove)
                    DoveCount++;
                else
                    HawkCount++;
                break;
            }
        }
    }

    private void SpawnDifferentPop(EggSpawnerInformation original)
    {
        Transform parent = GameObject.Find("PopParent").transform;

        foreach (var pop in PopPool)
        {
            if (pop != null && !pop.gameObject.activeSelf && !SamePopType(original, pop))
            {
                pop.gameObject.SetActive(true);
                pop.transform.position = original.gameObject.transform.position;
                //pop.ResetTarget();

                CopyInfoEggOnNewPop(original, pop);

                pop.gameObject.transform.localScale = new Vector3(pop.Size, pop.Size, pop.Size);

                if (pop is Dove)
                    DoveCount++;
                else
                    HawkCount++;
                return;
            }
        }
        for (int i = 0; i < PopPool.Length; i++)
        {
            if (PopPool[i] == null)
            {
                PopPool[i] = Instantiate(GetOtherPrefab(original), original.gameObject.transform.position, Quaternion.identity, parent).GetComponent<Pop>();
                PopPool[i].gameObject.name = "Pop n." + i.ToString();
                PopObjectPool[i] = PopPool[i].gameObject;

                CopyInfoEggOnNewPop(original, PopPool[i]);

                PopObjectPool[i].transform.localScale = new Vector3(PopPool[i].Size, PopPool[i].Size, PopPool[i].Size);

                if (PopPool[i] is Dove)
                    DoveCount++;
                else
                    HawkCount++;
                break;
            }
        }
    }

    private bool SamePopType (EggSpawnerInformation one, Pop two)
    {
        if (one.IsDove && two is Dove)
            return true;
        if (!one.IsDove && two is Hawk)
            return true;
        return false;
    }

    private bool RandomMutationChance => Random.Range(0, 100) < MutationChance ? true : false;

    private GameObject GetRightPrefab(EggSpawnerInformation original) => original.IsDove ? m_popDovePrefab : m_popHawkPrefab;

    private GameObject GetOtherPrefab(EggSpawnerInformation original) => original.IsDove ? m_popHawkPrefab : m_popDovePrefab;

    private void CopyInfoEggOnNewPop(EggSpawnerInformation egg, Pop newPop)
    {
        newPop.MetabolismRate = egg.OriginalMetabolism;
        newPop.Size = egg.OriginalSize;
        newPop.m_agent.speed = egg.OriginalSpeed;
    }

    enum MutationType
    {
        None,
        SwitchBehaviour,
        SpeedIncrease,
        SpeedDecrease,
        MetabolismIncrease,
        MetabolismDecrease,
        IncreaseSize,
        DecreaseSize
    }

}
