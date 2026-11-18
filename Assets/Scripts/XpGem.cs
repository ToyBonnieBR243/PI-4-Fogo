using UnityEngine;

public class XpGem : MonoBehaviour
{
    [SerializeField] private int xpValue = 5;
    private Transform player;
    private bool isAttracted = false;
    private float attractSpeed = 6f;

    private void Start()
    {
        GameObject p = GameObject.Find("Foguin");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        if (player == null) return;

        // Se o player chegar perto, o XP puxa na direção dele (estilo Vampire Survivors)
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < 3f) // Raio de atração
        {
            isAttracted = true;
        }

        if (isAttracted)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, attractSpeed * Time.deltaTime);

            if (distance < 0.3f)
            {
                GameManager.Instance.AddXP(xpValue);
                Destroy(gameObject);
            }
        }
    }
}