using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dove : Pop
{
    public override void MyUpdate()
    {
        base.MyUpdate();

        if (m_distanceFromFood < 0.5f)
        {
            m_Energy.IncreaseEnergy(GameManager.Instance.FoodPerBamboo);
            ResetTarget();
            EventManager.Instance.OnFoodEaten.Invoke(MyDestination);
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
        MyDestination = m_gameManager.ClosestFoodDestination(MyPos);
        m_agent.SetDestination(MyDestinationVector3);
    }
}
