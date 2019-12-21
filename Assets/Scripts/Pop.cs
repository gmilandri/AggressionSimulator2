using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pop : MonoBehaviour
{
    public NavMeshAgent m_agent;
    public Vector2 MyPos;
    public Vector2 MyDestination;

    public EnergyPop m_Energy;

    public GameManager m_gameManager;

    void Awake()
    {
        m_gameManager = GameManager.Instance;
        m_Energy = new EnergyPop(m_gameManager.StartingFoodPop, Random.Range(5, 15));
        m_agent = GetComponent<NavMeshAgent>();
        m_agent.speed = Random.Range(m_gameManager.MinPopSpeed, m_gameManager.MaxPopSpeed);
        m_agent.enabled = true;
    }

    void Start()
    {      
        ResetTarget();
    }

    public virtual void MyUpdate()
    {
        MyPos = new Vector2(transform.position.x, transform.position.z);

        m_Energy.TimeTick();
    }

    public virtual void SetDestination(){}

    public Vector3 MyDestinationVector3 => new Vector3(MyDestination.x, 0f, MyDestination.y);

    public virtual float DistanceFromFood() => Vector2.Distance(MyPos, MyDestination);

    public virtual void ResetTarget()
    {
        MyDestination = Vector2.zero;
        MyPos = new Vector2(transform.position.x, transform.position.z);
        SetDestination();
    }

    protected void CheckStatus()
    {
        if (m_Energy.EnergyEnded)
            ResetSelf();
        if (m_Energy.EnergyHigherThan(m_gameManager.StartingFoodPop * 2)) //TESTING
        {
            m_Energy.DecreaseEnergyBy(m_gameManager.StartingFoodPop);
            m_gameManager.CreateNewPop(this);
        }
    }

    public void ResetSelf()
    {
        m_gameManager.AvailableBiomassIncreaseBy(m_Energy.Energy);
        m_Energy.ResetValues(m_gameManager.StartingFoodPop, Random.Range(5, 15));
        MyDestination = Vector2.zero;
        m_agent.speed = Random.Range(m_gameManager.MinPopSpeed, m_gameManager.MaxPopSpeed);
        gameObject.SetActive(false);
    }
}
