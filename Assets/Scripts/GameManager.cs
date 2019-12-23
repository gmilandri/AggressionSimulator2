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
    public List<GameObject> PopObjectPool;
    public List<Pop> PopPool;
    public List<GameObject> EggObjectPool;
    public List<EggSpawnerInformation> EggPool;

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
    [Header("Debugging")]
    public int DoveCount;
    public int HawkCount;
    public bool Debugging;
    public float RealTotalBiomass;
    public float RealDoveBiomass;
    public float RealHawkBiomass;
    public float RealEggBiomass;
    public float RealFoodBiomass;

    [HideInInspector]
    public int SpeedFactor = 1;
    private float m_squareSize;
    private int m_startIndex = 0;
    private float m_availableBiomass = 0;


    public float GetMaxBoundaries() => m_squareSize;

    //public void AvailableBiomassIncreaseBy(float amount) => m_availableBiomass += amount;

    void Start()
    {
        Debug.unityLogger.logEnabled = false;
        GameObject floor = GameObject.Find("Floor");
        floor.transform.localScale = new Vector3(m_floorSize, 1, m_floorSize);
        floor.transform.position = new Vector3(m_floorSize * 5, 0, m_floorSize * 5);
        m_squareSize = floor.transform.localScale.x * 10 - 2;

        Camera.main.transform.position = new Vector3(floor.transform.position.x, 15, -12);

        FoodObjectPool = new GameObject[TotalFoodBiomass / FoodPerBamboo];

        PopPool = new List<Pop>();
        PopObjectPool = new List<GameObject>();

        EggPool = new List<EggSpawnerInformation>();
        EggObjectPool = new List<GameObject>();

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

            int HundredsofPops = activePops / 100;
            if (HundredsofPops == 0)
                SpeedFactor = 1;
            else
                SpeedFactor = HundredsofPops;
        }

        if (m_availableBiomass >= FoodPerBamboo)
            RespawnFood();

        for (int i = m_startIndex; i < PopPool.Count; i += SpeedFactor)
        {
            if (PopPool[i] == null)
                break;
            if (!PopPool[i].gameObject.activeSelf)
                continue;
            PopPool[i].MyUpdate();            
        }

        RealTotalBiomass = 0;
        RealDoveBiomass = 0;
        RealHawkBiomass = 0;
        RealFoodBiomass = 0;
        RealEggBiomass = 0;

        foreach (var food in FoodObjectPool)
            if (food.activeSelf)
                RealFoodBiomass += FoodPerBamboo;

        for (int i = 0; i < PopObjectPool.Count; i++)
        {
            if (PopObjectPool[i] == null)
                break;
            if (PopObjectPool[i].activeSelf)
            {
                if (PopPool[i] is Dove)
                {
                    RealDoveBiomass += PopPool[i].m_Energy.Energy;
                }
                else
                {
                    RealHawkBiomass += PopPool[i].m_Energy.Energy;
                }
            }
        }

        foreach (var egg in EggObjectPool)
        {
            if (egg == null)
                break;
            if (egg.activeSelf)
                RealEggBiomass += StartingFoodPop;
        }

        RealTotalBiomass = RealDoveBiomass + RealHawkBiomass + RealEggBiomass + RealFoodBiomass;

        m_availableBiomass = TotalFoodBiomass - RealTotalBiomass;

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
            Pop newPop = Instantiate(m_popDovePrefab, new Vector3(Random.Range(1, GetMaxBoundaries()), 1f, Random.Range(1, GetMaxBoundaries())), Quaternion.identity, parent).GetComponent<Pop>();
            PopPool.Add(newPop);
            newPop.gameObject.name = "Pop n." + PopPool.Count.ToString();
            PopObjectPool.Add(newPop.gameObject);
        }
        DoveCount = StartingPops;
    }

    void InizializeNavMesh()
    {
        GameObject.Find("NavMesh").GetComponent<NavMeshSurface>().BuildNavMesh();
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
        //for (int i = 0; i < EggObjectPool.Count; i++)
        //{
        //    if (EggObjectPool[i] == null)
        //    {
                EggSpawnerInformation newEgg;
                newEgg = Instantiate(m_eggPrefab, new Vector3(original.gameObject.transform.position.x, m_eggPrefab.transform.localScale.y / 2, original.gameObject.transform.position.z), Quaternion.identity, parent).GetComponent<EggSpawnerInformation>();
                EggPool.Add(newEgg);
                EggObjectPool.Add(newEgg.gameObject);
                newEgg.gameObject.name = "Egg n." + EggPool.Count.ToString();
                newEgg.SetPopType(original);
                var eggInfo2 = newEgg.GetComponent<EggSpawnerInformation>();
                eggInfo2.SetOriginalSpeed(original.GetComponent<NavMeshAgent>().speed);
                eggInfo2.SetOriginalMetabolism(original.MetabolismRate);
                eggInfo2.SetOriginalSize(original.Size);
                StartCoroutine(eggInfo2.BirthCountdown());
        //        return;
        //    }
        //}
    }

    private void RespawnFood()
    {
        m_availableBiomass -= FoodPerBamboo;
        for (int i = 0; i < FoodObjectPool.Length; i++)
        {
            if (m_availableBiomass <= 0)
                break;
            if (!FoodObjectPool[i].activeSelf)
            {
                FoodObjectPool[i].SetActive(true);
                FoodObjectPool[i].transform.position = new Vector3(Random.Range(1, GetMaxBoundaries()), 1f, Random.Range(1, GetMaxBoundaries()));
                m_availableBiomass -= FoodPerBamboo;
            }
        }
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
                {
                    pop.GetComponent<NavMeshAgent>().speed += SpeedMutationChange;
                }
                if (shouldDecreaseSpeed)
                {
                    if (original.OriginalSpeed - SpeedMutationChange > 0)
                    {
                        pop.GetComponent<NavMeshAgent>().speed -= SpeedMutationChange;
                    }
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
        //for (int i = 0; i < PopPool.Count; i++)
        //{
        //    if (PopPool[i] == null)
        //    {
                Pop newPop = Instantiate(GetRightPrefab(original), original.gameObject.transform.position, Quaternion.identity, parent).GetComponent<Pop>();
                PopPool.Add(newPop);
                newPop.gameObject.name = "Pop n." + PopPool.Count.ToString();
                PopObjectPool.Add(newPop.gameObject);

                CopyInfoEggOnNewPop(original, newPop);

                if (shouldIncreaseSpeed)
                    newPop.GetComponent<NavMeshAgent>().speed += SpeedMutationChange;
                if (shouldDecreaseSpeed)
                {
                    if (original.OriginalSpeed - SpeedMutationChange > 0)
                        newPop.GetComponent<NavMeshAgent>().speed -= SpeedMutationChange;
                }
                if (shouldIncreaseMetabolism && newPop.MetabolismRate < 100)
                    newPop.MetabolismRate++;
                if (shouldDecreaseMetabolism)
                    newPop.MetabolismRate--;
                if (shouldIncreaseSize && original.OriginalSpeed - SpeedMutationChange > 0)
                {
                    newPop.Size += 0.1f;
                    newPop.GetComponent<NavMeshAgent>().speed = original.OriginalSpeed - SpeedMutationChange;
                }
                if (shouldDecreaseSize && original.OriginalSize > 0.1f)
                {
                    newPop.Size -= 0.1f;
                    newPop.GetComponent<NavMeshAgent>().speed = original.OriginalSpeed + SpeedMutationChange;
                }

                newPop.gameObject.transform.localScale = new Vector3(newPop.Size, newPop.Size, newPop.Size);

                if (newPop is Dove)
                    DoveCount++;
                else
                    HawkCount++;
        //        break;
        //    }
        //}
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
        //for (int i = 0; i < PopPool.Count; i++)
        //{
        //    if (PopPool[i] == null)
        //    {
                Pop newPop = Instantiate(GetOtherPrefab(original), original.gameObject.transform.position, Quaternion.identity, parent).GetComponent<Pop>();
                PopPool.Add(newPop);
                newPop.gameObject.name = "Pop n." + PopPool.Count.ToString();
                PopObjectPool.Add(newPop.gameObject);

                CopyInfoEggOnNewPop(original, newPop);

                newPop.gameObject.transform.localScale = new Vector3(newPop.Size, newPop.Size, newPop.Size);

                if (newPop is Dove)
                    DoveCount++;
                else
                    HawkCount++;
        //        break;
        //    }
        //}
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
