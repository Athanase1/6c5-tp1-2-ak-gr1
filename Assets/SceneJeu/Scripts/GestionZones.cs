using UnityEngine;
using UnityEngine.AI;

public class GestionZones : MonoBehaviour
{
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        NavMeshHit hit;

        // On check la zone sous les pieds de la capsule
        if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            // SI ZONE RALENTISSEUR (Index 3)
            if (hit.mask == (1 << 3))
            {
                agent.speed = 1.5f; // On ralentit
            }
            // SINON SI ZONE ACCÉLÉRATEUR (Index 4)
            else if (hit.mask == (1 << 4))
            {
                agent.speed = 8.0f; // On fonce !
            }
            // SINON (Zone normale)
            else
            {
                agent.speed = 3.5f; // Vitesse de base
            }
        }
    }
}