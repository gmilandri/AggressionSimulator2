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
    }
}
