using UnityEngine;

public class MolandakController : MonoBehaviour 
{
    private Animator animator;
    
    [Header("Contrôle du mouvement")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float minWalkTime = 2f;
    [SerializeField] private float maxWalkTime = 4f;
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;
    [SerializeField] private float moveRadius = 3f;
    
    private float moveTimer = 0f;
    private bool isWalking = false;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        
        // Commencer par un état immobile
        moveTimer = Random.Range(minIdleTime, maxIdleTime);
        isWalking = false;
        
        if (animator != null)
            animator.SetBool("isWalking", false);
        else
            Debug.LogWarning("Animator non trouvé sur " + gameObject.name);
    }
    
    void Update()
    {
        moveTimer -= Time.deltaTime;
        
        if (moveTimer <= 0f)
        {
            // Changer d'état
            isWalking = !isWalking;
            
            if (isWalking)
            {
                // Définir une nouvelle destination aléatoire dans un rayon
                Vector2 randomDirection = Random.insideUnitCircle * moveRadius;
                targetPosition = startPosition + new Vector3(randomDirection.x, 0, randomDirection.y);
                
                // Calculer la rotation vers la cible
                Vector3 direction = targetPosition - transform.position;
                if (direction != Vector3.zero)
                {
                    targetRotation = Quaternion.LookRotation(direction);
                }
                
                // Nouveau délai pour marcher
                moveTimer = Random.Range(minWalkTime, maxWalkTime);
            }
            else
            {
                // Nouveau délai pour rester immobile
                moveTimer = Random.Range(minIdleTime, maxIdleTime);
            }
            
            // Mettre à jour l'animation
            if (animator != null)
                animator.SetBool("isWalking", isWalking);
        }
        
        // Si en mouvement, déplacer et tourner le monstre
        if (isWalking)
        {
            // Rotation fluide vers la cible
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
            
            // Déplacement vers la cible
            transform.position = Vector3.MoveTowards(
                transform.position, 
                targetPosition, 
                moveSpeed * Time.deltaTime
            );
            
            // Si on est arrivé à destination, on peut s'arrêter plus tôt
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isWalking = false;
                moveTimer = Random.Range(minIdleTime, maxIdleTime);
                
                if (animator != null)
                    animator.SetBool("isWalking", false);
            }
        }
    }
    
    // Fonction utile pour visualiser le rayon de déplacement dans l'éditeur
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, moveRadius);
    }
}