using UnityEngine;
using UnityEngine.Rendering;

public class Platform : MonoBehaviour
{

    public int Flip=1;
    public float speed = 2f;


    // Update is called once per frame
    void Update()
    {
        


        transform.Translate(Vector3.right * Time.deltaTime * speed * Flip);


    }

   
}
