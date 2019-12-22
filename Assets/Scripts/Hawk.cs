using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hawk : Pop
{
    int m_indexTarget = -1;

    public override void MyUpdate()
    {
        base.MyUpdate();

        ResetTarget();
        SelectPrey();

        if (!HasPossibleTarget)
            ResetSelf();
        else
            m_agent.SetDestination(m_gameManager.PopObjectPool[m_indexTarget].transform.position);

        if (DistanceFromFood() < 1.5f)
        {
            m_Energy.IncreaseEnergy(GameManager.Instance.PopPool[m_indexTarget].m_Energy.Energy - GameManager.Instance.MinimumPopEnergy);
            GameManager.Instance.PopPool[m_indexTarget].m_Energy.ResetEnergy();

            EventManager.Instance.OnPopEaten.Invoke(m_indexTarget);
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

    public override float DistanceFromFood()
    {
       return Vector3.Distance(transform.position, m_gameManager.PopObjectPool[m_indexTarget].transform.position);
    }

    public override void ResetTarget() => m_indexTarget = -1;

    private bool HasPossibleTarget => m_indexTarget == -1 ? false : true;

    private void SelectPrey() => m_indexTarget = m_gameManager.ClosestPopDestination(transform.position);


}
