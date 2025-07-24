using System.Collections;
using UnityEngine;
public class Arrow : MonoBehaviour
{
    public Transform player;
    public Transform targetB;
    public Vector3 offsetFromPlayer = new Vector3(0, 0.01f, 0); // sedikit di atas lantai
    public bool rotateOnlyOnY = true;

    void Update()
    {
        // Ikuti posisi player (dengan offset di lantai)
        transform.position = new Vector3(player.position.x, 0f, player.position.z) + offsetFromPlayer;

        // Arahkan ke target B
        Vector3 direction = targetB.position - transform.position;
        if (rotateOnlyOnY) direction.y = 0f;

        if (direction != Vector3.zero)
        {
           Quaternion targetRotation = Quaternion.LookRotation(direction);
transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);

        }
    }
}
