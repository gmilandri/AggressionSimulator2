using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dove : Pop {

    GameObject m_previousTarget;
    GameObject m_target;

    bool dangerClose;

    public override void MyUpdate()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward) * VisionDistance;
        Debug.DrawRay(transform.position, forward, Color.red);

        m_Energy.TimeTick();

        Debug.Log(gameObject.name + " is determining his target.");
        DetermineTarget();

        if (HasSafeTarget() && TargetHasChanged)
        {
            Debug.Log(gameObject.name + " has found a new target and set his destination to such target.");
            m_agent.SetDestination(m_target.transform.position);
        }

        if (HasSafeTarget() && DistanceFromFood() < 1f)
        {
            Debug.Log(gameObject.name + " is close to target and ate it.");
            float totalEnergyFromEatenFood = GameManager.Instance.FoodPerBamboo;
            float totalEnergyToDove = totalEnergyFromEatenFood / 100 * MetabolismRate;
            m_Energy.IncreaseEnergyBy(totalEnergyToDove);
            m_gameManager.AvailableBiomassIncreaseBy(totalEnergyFromEatenFood - totalEnergyToDove);

            DespawnFoodAndResetMyTarget();
        }

        if (!HasSafeTarget() && !m_agent.hasPath)
        {
            dangerClose = false;
            TargetRandomSpot();
        }

        /// RAYCAST IN FRONT OF POP
        /// IF RAY HITS FOOD
        /// SET IT AS TARGET AND START MOVING TOWARDS IT
        /// IF TARGET IS CLOSE, EAT IT!

        if (m_Energy.CountdownHasEnded)
        {
            m_Energy.EnergyTick(EnergySpentPerTick);
            m_Energy.ResetCountdown();
            CheckStatus();
        }
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

            if (Physics.Raycast (transform.position, dir, out hit, maxDistance: 3f)) 
            {
                if (hit.collider.gameObject.CompareTag("Dove"))
                {
                    Dove dove = hit.collider.gameObject.GetComponent<Dove>();
                    if (dove.m_Energy.Energy < m_Energy.Energy)
                    {
                        m_Energy.DecreaseEnergyBy(1);
                        dove.m_Energy.IncreaseEnergyBy(1);
                    }
                }
            }
        }
    }

    private GameObject ClosestTarget ()
    {
        //if (m_gameManager.DoveGenerosity)
        //    GiveFood();

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance: VisionDistance))
        {
            if (hit.collider.gameObject.CompareTag("Hawk"))
            {
                dangerClose = true;
                Debug.Log(gameObject.name + " has an hawk in front of him!");
            }
            if (hit.collider.gameObject.CompareTag("Food"))
            {
                return hit.collider.gameObject;
            }          
        }
        return null;
    }

    private void DetermineTarget()
    {
        m_previousTarget = m_target;
        m_target = ClosestTarget();
    }

    private bool HasSafeTarget()
    {
        if (m_target == null)
            return false;
        if (!m_target.activeSelf)
            return false;
        if (dangerClose)
            return false;
        return true;
    }

    private bool TargetHasChanged => m_previousTarget == m_target ? false : true;

    public override float DistanceFromFood() => Vector3.Distance(transform.position, m_target.transform.position);

    private void DespawnFoodAndResetMyTarget()
    {
        Debug.Log("Food was despawned.");
        m_target.SetActive(false);
        ResetTarget();
    }

    public void ResetTarget()
    {
        Debug.Log(gameObject.name + " target has been reset.");
        m_previousTarget = m_target;
        m_target = null;
        m_agent.ResetPath();
    }

    private void TargetRandomSpot()
    {
        Debug.Log(gameObject.name + " is targeting a random spot.");

        bool foundSpot = false;
        Vector3 tempDest = Vector3.zero;

        while (!foundSpot)
        {
            var randomX = Random.Range(-5f, 5f);
            var randomZ = Random.Range(-5f, 5f);
            tempDest = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
            foundSpot = m_gameManager.IsDestinationOnFloor(tempDest);
        }
        m_agent.SetDestination(tempDest);
    }

}
