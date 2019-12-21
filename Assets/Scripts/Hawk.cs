using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hawk : Pop
{
    int m_indexTarget = -1;

    public override void MyUpdate()
    {
        base.MyUpdate();

        if (NoTarget)
        {
            m_indexTarget = m_gameManager.ClosestPopDestination(transform.position);
            if (m_indexTarget == -1)
            {
                ResetSelf();
            }
            else
                m_agent.SetDestination(m_gameManager.PopObjectPool[m_indexTarget].transform.position);
        }

        if (!NoTarget)
        {
            if (!m_gameManager.PopObjectPool[m_indexTarget].activeSelf)
                ResetTarget();
            else
                m_agent.SetDestination(m_gameManager.PopObjectPool[m_indexTarget].transform.position);
        }

        if (!NoTarget && DistanceFromFood() < 1f)
        {
            m_Energy.IncreaseEnergy(GameManager.Instance.PopPool[m_indexTarget].m_Energy.Energy - GameManager.Instance.MinimumPopEnergy);
            GameManager.Instance.PopPool[m_indexTarget].m_Energy.ResetEnergy();

            EventManager.Instance.OnPopEaten.Invoke(m_indexTarget);
            ResetTarget();
        }

        if (m_Energy.CountdownHasEnded)
        {
            m_Energy.EnergyTick();
            m_Energy.ResetCountdown();
            CheckStatus();
        }
    }

    public override void SetDestination()
    {
        m_agent.SetDestination(MyDestinationVector3);
    }

    private bool NoTarget => m_indexTarget == -1 ? true : false;
    public override float DistanceFromFood()
    {
       return Vector3.Distance(transform.position, m_gameManager.PopObjectPool[m_indexTarget].transform.position);
    }

    public override void ResetTarget()
    {
        m_indexTarget = -1;
    }
}
