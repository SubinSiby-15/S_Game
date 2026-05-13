using UnityEngine;

public class coin : MonoBehaviour
{
    public int coinValue = 1;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Coin Collected!"); // Log a message to the console when the coin is collected for debugging purposes.)

           AudioManger.instance.PlayCoinSound(); // Play the coin collection sound effect using the AudioManger singleton instance.

            Destroy(gameObject); // Destroy the coin GameObject after it has been collected to prevent it from being collected again.

        }
    }


}
