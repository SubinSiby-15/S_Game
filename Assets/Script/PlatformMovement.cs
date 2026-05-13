using UnityEngine;
using System.Collections;

public class PlatPlatformMovement : MonoBehaviour
{
    public Transform[] movePoints; // Start & End points
  
    public float speed = 2f;
    private int nextPointIndex = 0;

    private void Update()
    {
        // Move platform between points
        transform.position = Vector2.MoveTowards(transform.position, movePoints[nextPointIndex].position, speed * Time.deltaTime);

        // Change direction when reaching a point
        if (Vector2.Distance(transform.position, movePoints[nextPointIndex].position) < 0.1f)
        {
            nextPointIndex = (nextPointIndex + 1) % movePoints.Length;
            Debug.Log("Target: " + nextPointIndex);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform); //  Attach player to platform
           // transform.SetParent(collision.transform); //  Attach platform to player


          
           
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && gameObject.activeInHierarchy)
        {
            StartCoroutine(DelayedUnparent(collision.transform));
        }
    }

    private IEnumerator DelayedUnparent(Transform player)
    {
        yield return new WaitForEndOfFrame(); // Wait for the current frame to finish
        yield return new WaitForSeconds(1f); // Wait for a short duration


        if (player != null && player.parent == transform)
        {
            player.SetParent(null, true);
        }
    }
}
