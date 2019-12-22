using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dove : Pop
{
    public override void MyUpdate()
    {
        base.MyUpdate();

        MyPos = new Vector2(transform.position.x, transform.position.z);

        if (m_gameManager.DoveGenerosity)
            GiveFood();

        if (DistanceFromFood() < 1f)
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

    public override void ResetTarget()
    {
        MyDestination = Vector2.zero;
        MyPos = new Vector2(transform.position.x, transform.position.z);
        SetDestination();
    }

    public void GiveFood()
    {
        int RaysToShoot = 6;

        float angle = 0;
        for (int i=0; i<RaysToShoot; i++) 
        {
            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);
            angle += 2 * Mathf.PI / RaysToShoot;
 
            Vector3 dir = new Vector3(transform.position.x + x, transform.position.y + y, 0);
            RaycastHit hit;

            if (Physics.Raycast (transform.position, dir, out hit)) 
            {
                if (hit.collider.gameObject.CompareTag("Dove"))
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
