using UnityEngine;
using UnityEngine.AI;

using System.Collections; // Nécessaire pour la pause (Coroutine)

public class PatrouilleAgent : MonoBehaviour
{
    public Transform[] buts;
    private NavMeshAgent agent;
    private Animator anim;
    private int dernierButIndex = -1;
    private bool estEnPause = false; // Pour ne pas choisir un but en boucle

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        ChoisirProchainBut();
    }

    void Update()
    {
        if (estEnPause) return; // Si on attend au but, on ne fait rien d'autre

        anim.SetFloat("Vitesse", agent.velocity.magnitude);

        // Vérification de l'arrivée au but 
        if (!agent.pathPending && agent.remainingDistance < 0.7f)
        {
            StartCoroutine(SequenceAtteinteBut());
        }
    }

    IEnumerator SequenceAtteinteBut()
    {
        estEnPause = true;
        agent.isStopped = true; // Arrêt physique
        anim.SetTrigger("AuBut");

        // Attends la durée exacte de ton animation (ex: 2.5 secondes)
        yield return new WaitForSeconds(2.5f);

        agent.isStopped = false; // Relance le mouvement
        estEnPause = false;
        ChoisirProchainBut(); // Change de destination
    }

    void ChoisirProchainBut()
    {
        if (buts.Length == 0) return;
        int nouvelIndex;
        do
        {
            nouvelIndex = Random.Range(0, buts.Length);
        } while (nouvelIndex == dernierButIndex && buts.Length > 1);

        dernierButIndex = nouvelIndex;
        agent.SetDestination(buts[nouvelIndex].position);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZoneRamper") && !anim.GetBool("estSousObstacle"))
        {
            anim.SetBool("estSousObstacle", true);
            agent.speed = 0.5f;

            CapsuleCollider capsule = GetComponent<CapsuleCollider>();
            capsule.height = 0.5f;
            capsule.center = new Vector3(0, 0.25f, 0);
            Debug.Log("Début réel du rampement");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ZoneRamper") && anim.GetBool("estSousObstacle"))
        {
            // On attend que le zombie soit vraiment sorti avant de se relever
            anim.SetBool("estSousObstacle", false);

            CapsuleCollider capsule = GetComponent<CapsuleCollider>();
            capsule.height = 2.0f;
            capsule.center = new Vector3(0, 1.0f, 0);
            Debug.Log("Fin réelle du rampement");
        }
    }
}