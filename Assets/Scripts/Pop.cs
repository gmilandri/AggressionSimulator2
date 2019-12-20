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

    private void Awake()
    {
        m_Energy = new EnergyPop(GameManager.Instance.StartingFoodPop, Random.Range(5, 15));
        m_agent = GetComponent<NavMeshAgent>();
        m_agent.speed = Random.Range(GameManager.Instance.MinPopSpeed, GameManager.Instance.MaxPopSpeed);
        GetComponent<Renderer>().material = GameManager.Instance.DoveMaterial;
    }

    void Start()
    {      
        m_agent.enabled = true;
        MyDestination = Vector2.zero;
        MyPos = new Vector2(transform.position.x, transform.position.z);

        SetDestination();
    }

    public void MyUpdate()
    {
        MyPos = new Vector2(transform.position.x, transform.position.z);

        if (m_distanceFromFood < 0.5f)
        {
            m_Energy.IncreaseEnergy(GameManager.Instance.FoodPerBamboo);
            ResetTarget();
            EventManager.Instance.OnFoodEaten.Invoke(MyDestination);
        }

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
        MyDestination = GameManager.Instance.ClosestDestination(MyPos);
        m_agent.SetDestination(MyDestinationVector3);
    }

    private Vector3 MyDestinationVector3 => new Vector3(MyDestination.x, 0f, MyDestination.y);

    private float m_distanceFromFood => Vector2.Distance(MyPos, MyDestination);

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
        if (m_Energy.EnergyHigherThan(20))
        {
            m_Energy.DecreaseEnergyBy(GameManager.Instance.StartingFoodPop);
            GameManager.Instance.CreateNewPop(this);
        }
    }

    private void ResetSelf()
    {
        m_Energy.ResetValues(GameManager.Instance.StartingFoodPop, Random.Range(5, 15));
        MyDestination = Vector2.zero;
        m_agent.speed = Random.Range(GameManager.Instance.MinPopSpeed, GameManager.Instance.MaxPopSpeed);
        gameObject.SetActive(false);
    }
}
