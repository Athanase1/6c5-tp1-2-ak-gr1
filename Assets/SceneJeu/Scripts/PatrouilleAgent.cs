using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class PatrouilleAgent : MonoBehaviour
{
    [Header("Configuration Patrouille")]
    public Transform[] buts;

    [Header("Configuration Zones NavMesh")]
    public string nomDeLaZoneRamper = "ZoneRamper";
    public float vitesseRamper = 1.0f;
    public float vitesseCourse = 3.5f;

    private NavMeshAgent agent;
    private Animator anim;
    private int dernierButIndex = -1;
    private bool estEnPause = false;
    private int zoneRamperIndex;

    // Pour éviter que l'animation clignote
    private bool estEnTrainDeRamper = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        zoneRamperIndex = NavMesh.GetAreaFromName(nomDeLaZoneRamper);
        if (zoneRamperIndex == -1) Debug.LogError("Zone introuvable !");

        ChoisirProchainBut();
    }

    void Update()
    {
        if (estEnPause || !agent.isOnNavMesh) return;

        if (anim != null) anim.SetFloat("Vitesse", agent.velocity.magnitude);

        DetecterZoneRobuste();

        if (!agent.pathPending && agent.remainingDistance < 0.7f)
        {
            StartCoroutine(SequenceAtteinteBut());
        }
    }

    void DetecterZoneRobuste()
    {
        NavMeshHit hit;

        // ASTUCE : On utilise agent.transform.position directement au lieu de SamplePathPosition
        // pour éviter les erreurs quand l'agent change son propre offset vertical.
        if (NavMesh.SamplePosition(transform.position, out hit, 0.6f, NavMesh.AllAreas))
        {
            bool zoneDetectee = (hit.mask & (1 << zoneRamperIndex)) != 0;

            // On ne change l'état que si c'est nécessaire (évite les bugs d'Animator)
            if (zoneDetectee && !estEnTrainDeRamper)
            {
                AppliquerChangementEtat(true);
            }
            else if (!zoneDetectee && estEnTrainDeRamper)
            {
                AppliquerChangementEtat(false);
            }
        }
    }

    void AppliquerChangementEtat(bool ramper)
    {
        estEnTrainDeRamper = ramper;

        if (anim != null) anim.SetBool("estSousObstacle", ramper);

        if (ramper)
        {
            agent.speed = vitesseRamper;
            // On lève le modèle visuel pour qu'il soit SUR le cube et pas dedans
            agent.baseOffset = 0.45f;
            ModifierDimensionsAgent(0.5f, 0.25f);
            Debug.Log("<color=purple>--- DÉBUT RAMPEMENT ---</color>");
        }
        else
        {
            agent.speed = vitesseCourse;
            agent.baseOffset = 0.0f;
            ModifierDimensionsAgent(2.0f, 1.0f);
            Debug.Log("<color=green>--- RETOUR COURSE ---</color>");
        }
    }

    void ModifierDimensionsAgent(float height, float centerY)
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            capsule.height = height;
            capsule.center = new Vector3(0, centerY, 0);
        }
        agent.height = height;
    }

    IEnumerator SequenceAtteinteBut()
    {
        estEnPause = true;
        agent.isStopped = true;
        if (anim != null) anim.SetTrigger("AuBut");
        yield return new WaitForSeconds(2.5f);
        agent.isStopped = false;
        estEnPause = false;
        ChoisirProchainBut();
    }

    void ChoisirProchainBut()
    {
        if (buts == null || buts.Length == 0) return;
        int nouvelIndex;
        do
        {
            nouvelIndex = Random.Range(0, buts.Length);
        } while (nouvelIndex == dernierButIndex && buts.Length > 1);
        dernierButIndex = nouvelIndex;
        agent.SetDestination(buts[nouvelIndex].position);
    }
}