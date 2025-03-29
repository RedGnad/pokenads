using System.Collections;
using UnityEngine;

public class JumpingMovement : MonoBehaviour
{
    [SerializeField] private float jumpDuration = 1f;
    [SerializeField] private float jumpInterval = 3f;
    [SerializeField] private float maxJumpHeight = 2f;
    [SerializeField] private float maxHorizontalDistance = 3f;

    private Vector3 startingPosition;
    private bool isJumping = false;

    void Start()
    {
        startingPosition = transform.position;
        StartCoroutine(JumpLoop());
    }

    IEnumerator JumpLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(jumpInterval);
            if (!isJumping)
            {
                Vector2 randomOffset = Random.insideUnitCircle * maxHorizontalDistance;
                Vector3 targetHorizontal = startingPosition + new Vector3(randomOffset.x, 0f, randomOffset.y);
                yield return StartCoroutine(PerformJump(targetHorizontal));
            }
        }
    }

    IEnumerator PerformJump(Vector3 targetHorizontal)
    {
        isJumping = true;
        Vector3 startPos = transform.position;
        float t = 0f;
        Vector2 horizontalStart = new Vector2(startPos.x, startPos.z);
        Vector2 horizontalTarget = new Vector2(targetHorizontal.x, targetHorizontal.z);
        
        while (t < jumpDuration)
        {
            t += Time.deltaTime;
            float factor = Mathf.Clamp01(t / jumpDuration);
            Vector2 horizontalPos = Vector2.Lerp(horizontalStart, horizontalTarget, factor);
            float height = 4f * maxJumpHeight * factor * (1f - factor);
            transform.position = new Vector3(horizontalPos.x, startPos.y + height, horizontalPos.y);
            yield return null;
        }
        transform.position = new Vector3(targetHorizontal.x, startPos.y, targetHorizontal.z);
        isJumping = false;
    }
}