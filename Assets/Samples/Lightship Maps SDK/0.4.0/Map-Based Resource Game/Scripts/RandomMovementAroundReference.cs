using UnityEngine;

public class RandomMovementAroundReference : MonoBehaviour
{
    public Transform centerPoint;
    public Transform cameraTarget;

    public float radius = 2.0f;
    public float moveSpeed = 1.0f;
    
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
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetNewTargetPosition();
        }

        transform.LookAt(cameraTarget, Vector3.up);
    }

    void SetNewTargetPosition()
    {
        Vector3 randomDir = Random.insideUnitSphere.normalized;
        targetPosition = centerPoint.position + randomDir * radius;
    }
}