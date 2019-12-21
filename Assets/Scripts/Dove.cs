using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dove : Pop
{
    public override void MyUpdate()
    {
        base.MyUpdate();

        GiveFood();

        if (DistanceFromFood() < 0.5f)
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

    public void GiveFood()
    {
        int RaysToShoot = 30;

        float angle = 0;
        for (int i=0; i<RaysToShoot; i++) 
        {
            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);
            angle += 2 * Mathf.PI / RaysToShoot;
 
            Vector3 dir = new Vector3(transform.position.x + x, transform.position.y + y, 0);
            RaycastHit hit;
            //Debug.DrawLine(transform.position, dir, Color.red);
            if (Physics.Raycast (transform.position, dir, out hit)) 
            {
             //here is how to do your cool stuff ;)
                if (hit.collider.gameObject.tag == "Dove")
                {
                    Dove dove = hit.collider.gameObject.GetComponent<Dove>();
                    if (dove.m_Energy.Energy < m_Energy.Energy)
                    {
                        m_Energy.DecreaseEnergyBy(1);
                        dove.m_Energy.IncreaseEnergy(1);
                    }
                }
            }
        }
    }

}
