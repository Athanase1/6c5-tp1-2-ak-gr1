using UnityEngine;

public class MouvementObstacle : MonoBehaviour
{
    public float distance = 5f; // Distance du trajet
    public float vitesse = 2f;  // Vitesse du mouvement
    private Vector3 positionDepart;

    void Start()
    {
        positionDepart = transform.position;
    }

    void Update()
    {
        // Calcule le décalage avec une fonction Mathf.PingPong
        float deplacement = Mathf.PingPong(Time.time * vitesse, distance);
        transform.position = positionDepart + new Vector3(deplacement, 0, 0);
    }
}