using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hawk : Pop
{
    public override void MyUpdate()
    {
        base.MyUpdate();

        if (m_distanceFromFood < 1f)
        {
            //m_Energy.IncreaseEnergy(GameManager.Instance.FoodPerBamboo);
            ResetTarget();
            EventManager.Instance.OnPopEaten.Invoke(MyDestination);
        }

        if (m_Energy.CountdownHasEnded)
        {
            ResetTarget();
            //m_Energy.EnergyTick();
            m_Energy.ResetCountdown();
            CheckStatus();
        }
    }

    public override void SetDestination()
    {
        MyDestination = m_gameManager.ClosestPopDestination(MyPos);
        m_agent.SetDestination(MyDestinationVector3);
    }
}
