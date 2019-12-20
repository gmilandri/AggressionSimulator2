using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pop : MonoBehaviour
{
    [SerializeField]
    private int m_Energy;
    private NavMeshAgent m_agent;
    public Vector2 MyPos;
    public Vector2 MyDestination;

    private int m_energyTickSpeed;
    private int m_countdownEnergyCheck;

    void Start()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_agent.enabled = true;
        m_agent.speed = Random.Range(8f, 24f);
        GetComponent<Renderer>().material = GameManager.Instance.DoveMaterial;
        MyDestination = Vector2.zero;
        MyPos = new Vector2(transform.position.x, transform.position.z);
        m_Energy = 10;
        m_energyTickSpeed = Random.Range(5,15);
        m_countdownEnergyCheck = m_energyTickSpeed;
        SetDestination();
    }

    public void MyUpdate()
    {
        MyPos = new Vector2(transform.position.x, transform.position.z);

        if (m_distanceFromFood < 0.5f)
        {
            m_Energy += 2;
            EventManager.Instance.OnFoodEaten.Invoke(MyDestination);
        }

        m_countdownEnergyCheck--;
        if (m_countdownEnergyCheck == 0)
        {
            CheckStatus();
            m_Energy--;
            m_countdownEnergyCheck = m_energyTickSpeed;
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
        if (m_Energy <= 0)
            ResetSelf();
        if (m_Energy > 30)
        {
            m_Energy -= 20;
            GameManager.Instance.CreateNewPop(this);
        }
    }

    private void ResetSelf()
    {
        m_energyTickSpeed = Random.Range(5, 15);
        m_countdownEnergyCheck = m_energyTickSpeed;
        m_Energy = 10;
        MyDestination = Vector2.zero;
        m_agent.speed = Random.Range(8f, 24f);
        gameObject.SetActive(false);
    }
}
