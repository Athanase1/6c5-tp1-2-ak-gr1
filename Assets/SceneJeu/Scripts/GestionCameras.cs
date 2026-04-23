using UnityEngine;

public class GestionCameras : MonoBehaviour
{
    public GameObject[] cameras; // Mets tes 3 caméras ici
    private int indexActuel = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Désactive la caméra actuelle
            cameras[indexActuel].SetActive(false);

            // Passe à la suivante
            indexActuel = (indexActuel + 1) % cameras.Length;

            // Active la nouvelle
            cameras[indexActuel].SetActive(true);
        }
    }
}