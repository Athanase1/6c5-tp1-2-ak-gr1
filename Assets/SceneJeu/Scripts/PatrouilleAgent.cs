using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class PatrouilleAgent : MonoBehaviour
{
    [Header("Configuration Patrouille")]
    public Transform[] buts;
    [Tooltip("Distance à partir de laquelle le zombie commence à marcher")]
    public float distanceSeuilMarche = 4.0f;
    public float vitesseMarche = 1.5f;
    public float vitesseCourse = 3.5f;

    [Header("Configuration Zones NavMesh")]
    public string nomDeLaZoneRamper = "ZoneRamper";
    public float vitesseRamper = 1.0f;

    private NavMeshAgent agent;
    private Animator anim;
    private int dernierButIndex = -1;
    private bool estEnPause = false;
    private int zoneRamperIndex;
    private bool estEnTrainDeRamper = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        // Récupération de l'ID de la zone
        zoneRamperIndex = NavMesh.GetAreaFromName(nomDeLaZoneRamper);
        if (zoneRamperIndex == -1) Debug.LogError("Zone '" + nomDeLaZoneRamper + "' introuvable !");

        ChoisirProchainBut();
    }

    void Update()
    {
        // Sécurité : Si l'agent est en pause ou n'est pas sur le NavMesh, on arrête
        if (estEnPause || !agent.isOnNavMesh) return;

        // 1. MISE À JOUR ANIMATION (Vitesse effective / Velocity)
        if (anim != null)
        {
            // On utilise la magnitude du vecteur vitesse réelle
            anim.SetFloat("Vitesse", agent.velocity.magnitude);
        }

        // 2. GESTION DYNAMIQUE DE LA VITESSE MAX (Proche vs Loin)
        GererVitesseSelonDistance();

        // 3. DÉTECTION ZONE SPÉCIALE (Rampement)
        DetecterZoneRobuste();

        // 4. LOGIQUE DE PATROUILLE
        if (!agent.pathPending && agent.remainingDistance < 0.7f)
        {
            StartCoroutine(SequenceAtteinteBut());
        }
    }

    void GererVitesseSelonDistance()
    {
        // Si on rampe, on ne touche pas à la vitesse, le mode rampement est prioritaire
        if (estEnTrainDeRamper) return;

        // On ajuste la vitesse maximale (agent.speed) selon la distance du but
        if (agent.remainingDistance <= distanceSeuilMarche)
        {
            agent.speed = vitesseMarche;
        }
        else
        {
            agent.speed = vitesseCourse;
        }
    }

    void DetecterZoneRobuste()
    {
        NavMeshHit hit;
        // On check sous les pieds de l'agent
        if (NavMesh.SamplePosition(transform.position, out hit, 0.6f, NavMesh.AllAreas))
        {
            bool zoneDetectee = (hit.mask & (1 << zoneRamperIndex)) != 0;

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
            agent.baseOffset = 0.45f; // On soulève l'agent pour qu'il soit sur le cube
            ModifierDimensionsAgent(0.5f, 0.25f);
            Debug.Log("<color=purple>--- MODE RAMPEMENT ---</color>");
        }
        else
        {
            // Quand on sort, on relance immédiatement le calcul de vitesse selon distance
            GererVitesseSelonDistance();
            agent.baseOffset = 0.0f;
            ModifierDimensionsAgent(2.0f, 1.0f);
            Debug.Log("<color=green>--- FIN RAMPEMENT ---</color>");
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