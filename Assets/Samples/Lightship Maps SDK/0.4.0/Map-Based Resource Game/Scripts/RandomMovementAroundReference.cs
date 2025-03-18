using UnityEngine;

public class RandomMovementAroundReference : MonoBehaviour
{
    // Référence au point autour duquel le modèle se déplace (par exemple, la sphere)
    public Transform centerPoint;
    // Référence à la caméra vers laquelle le modèle doit se tourner
    public Transform cameraTarget;

    // Rayon de la sphère dans laquelle le modèle se déplace autour de la référence
    public float radius = 2.0f;
    // Vitesse de déplacement vers la position cible
    public float moveSpeed = 1.0f;
    
    // Position cible actuelle
    private Vector3 targetPosition;

    void Start()
    {
        if (centerPoint == null)
        {
            Debug.LogError("Aucune référence (centerPoint) n'est assignée !");
            enabled = false;
            return;
        }
        if (cameraTarget == null)
        {
            Debug.LogError("Aucune référence (cameraTarget) n'est assignée !");
            enabled = false;
            return;
        }
        SetNewTargetPosition();
    }

    void Update()
    {
        // Déplacer le modèle vers la position cible
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        // Si le modèle atteint la position cible, en définir une nouvelle
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetNewTargetPosition();
        }

        // Orienter le modèle pour qu'il soit exactement face à la caméra
        transform.LookAt(cameraTarget, Vector3.up);
    }

    void SetNewTargetPosition()
    {
        // Générer une direction aléatoire en 3D
        Vector3 randomDir = Random.insideUnitSphere.normalized;
        // Définir la nouvelle position cible autour de la référence centrale (sphere)
        targetPosition = centerPoint.position + randomDir * radius;
    }
}