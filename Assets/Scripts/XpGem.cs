using UnityEngine;

public class XpGem : MonoBehaviour
{
    [SerializeField] private int xpValue = 5;
    [SerializeField] private float magnetRadius = 3f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float collectDistance = 0.35f;

    private Transform playerTransform;
    private bool isMagnetized = false;

    private void Start()
    {
        GameObject player = GameObject.Find("Foguin");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= magnetRadius)
        {
            isMagnetized = true;
        }

        if (isMagnetized)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                playerTransform.position,
                moveSpeed * Time.deltaTime
            );

            if (distance <= collectDistance)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddXP(xpValue);
                }

                Destroy(gameObject);
            }
        }
    }
}