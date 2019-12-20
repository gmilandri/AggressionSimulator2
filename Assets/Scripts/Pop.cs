using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pop : MonoBehaviour
{
    //public IBehaviour MyBehaviour;
    private NavMeshAgent m_agent;
    public Vector2 MyPos;
    public Vector2 MyDestination;

    void Start()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_agent.enabled = true;
        //MyBehaviour = new Dove();
        //GetComponent<Renderer>().material = 
        MyDestination = Vector2.zero;
        MyPos = new Vector2(transform.position.x, transform.position.z);
    }

    public void MyUpdate()
    {
        if (Time.frameCount % 10 == 0)
        {
            MyPos = new Vector2(transform.position.x, transform.position.z);
            if (m_distanceFromFood < 1f)
            {
                GameManager.Instance.RespawnFood(MyDestination);
                ResetTarget();
            }
        }

        if (MyDestination == Vector2.zero)
            SetDestination();
    }

    private void SetDestination()
    {
        MyDestination = GameManager.Instance.ClosestDestination(MyPos);
        m_agent.SetDestination(MyDestinationVector3);
        
    }

    private Vector3 MyDestinationVector3 => new Vector3(MyDestination.x, 0f, MyDestination.y);

    private float m_distanceFromFood => Vector2.Distance(MyPos, MyDestination);

    private void ResetTarget()
    {
        MyDestination = Vector2.zero;
        MyPos = new Vector2(transform.position.x, transform.position.z);
    }
}
