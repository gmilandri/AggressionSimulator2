﻿using System.Collections;
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

    public int MetabolismRate = 50;

    public float Size = 1;

    public float EnergySpentPerTick 
    {
        get
        {
            return m_agent.speed / m_gameManager.StartingPopSpeed + Size - 1;
        }
    }


    void Awake()
    {
        m_gameManager = GameManager.Instance;
        m_Energy = new EnergyPop(m_gameManager.StartingFoodPop, Random.Range(11, 15));
        m_agent = GetComponent<NavMeshAgent>();
        m_agent.speed = m_gameManager.StartingPopSpeed;
        m_agent.enabled = true;
    }

    void Start()
    {      
        ResetTarget();
    }

    public virtual void MyUpdate()
    {
        m_Energy.TimeTick();
    }

    public virtual void SetDestination(){}

    public Vector3 MyDestinationVector3 => new Vector3(MyDestination.x, 0f, MyDestination.y);

    public virtual float DistanceFromFood() => Vector2.Distance(MyPos, MyDestination);

    public virtual void ResetTarget() {}

    protected void CheckStatus()
    {
        if (m_Energy.EnergyEnded)
            ResetSelf();
        if (m_Energy.EnergyHigherThan(m_gameManager.StartingFoodPop * 2)) //TESTING
        {
            m_Energy.DecreaseEnergyBy(m_gameManager.StartingFoodPop);
            m_gameManager.SpawnEgg(this);
        }
    }

    public void ResetSelf()
    {
        m_gameManager.AvailableBiomassIncreaseBy(m_Energy.Energy);
        m_Energy.ResetValues(m_gameManager.StartingFoodPop, Random.Range(11, 15));
        MyDestination = Vector2.zero;

        transform.localScale = new Vector3(1, 1, 1);
        Size = 1;

        if (this is Dove)
            m_gameManager.DoveCount--;
        else
            m_gameManager.HawkCount--;
        gameObject.SetActive(false);
    }
}
