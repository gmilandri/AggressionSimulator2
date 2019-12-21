using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pop : MonoBehaviour
{
    private NavMeshAgent m_agent;
    public Vector2 MyPos;
    public Vector2 MyDestination;

    public EnergyPop m_Energy;

    GameManager m_gameManager;

    private void Awake()
    {
        m_gameManager = GameManager.Instance;
        m_Energy = new EnergyPop(m_gameManager.StartingFoodPop, Random.Range(5, 15));
        m_agent = GetComponent<NavMeshAgent>();
        m_agent.speed = Random.Range(m_gameManager.MinPopSpeed, m_gameManager.MaxPopSpeed);
        //GetComponent<Renderer>().material = m_gameManager.DoveMaterial;
    }

    void Start()
    {      
        m_agent.enabled = true;
        MyDestination = Vector2.zero;
        MyPos = new Vector2(transform.position.x, transform.position.z);

        SetDestination();
    }

    public virtual void MyUpdate()
    {
        MyPos = new Vector2(transform.position.x, transform.position.z);

        m_Energy.TimeTick();
        if (m_Energy.CountdownHasEnded)
        {           
            m_Energy.EnergyTick();
            m_Energy.ResetCountdown();
            CheckStatus();
        }

    }

    public void SetDestination()
    {
        MyDestination = m_gameManager.ClosestDestination(MyPos);
        m_agent.SetDestination(MyDestinationVector3);
    }

    private Vector3 MyDestinationVector3 => new Vector3(MyDestination.x, 0f, MyDestination.y);

    protected float m_distanceFromFood => Vector2.Distance(MyPos, MyDestination);

    public void ResetTarget()
    {
        MyDestination = Vector2.zero;
        MyPos = new Vector2(transform.position.x, transform.position.z);
        SetDestination();
    }

    private void CheckStatus()
    {
        if (m_Energy.EnergyEnded)
            ResetSelf();
        if (m_Energy.EnergyHigherThan(m_gameManager.StartingFoodPop * 2))
        {
            m_Energy.DecreaseEnergyBy(m_gameManager.StartingFoodPop);
            m_gameManager.CreateNewPop(this);
        }
    }

    private void ResetSelf()
    {
        m_gameManager.AvailableBiomassIncreaseBy(m_Energy.Energy);
        m_Energy.ResetValues(m_gameManager.StartingFoodPop, Random.Range(5, 15));
        MyDestination = Vector2.zero;
        m_agent.speed = Random.Range(m_gameManager.MinPopSpeed, m_gameManager.MaxPopSpeed);
        gameObject.SetActive(false);
    }
}
