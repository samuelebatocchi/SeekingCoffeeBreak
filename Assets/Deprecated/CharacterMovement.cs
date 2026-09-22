using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterMovement : MonoBehaviour
{

    public float jump ; // è un valore che imposto dall'inspector. Viene moltiplicato per l'input della forza. Valori più alti -> salti più alti
    private Rigidbody2D rb; //reference al character 2D
    private bool isGrounded; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); //reference all'awake del gioco
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
        }
    }

    //CHECK se il personaggio è già in aria non può saltare nuovamente
    private void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.CompareTag("Ground")) //Ground è la tag per il Floor
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if(other.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    //Respawn se si tocca l'ostacolo
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("ObjectTestTrigger"))
        {
            SceneManager.LoadScene(0); //IN FUTURO da vedere la scena con il build index. Al momento l'inizio del gioco ha indice 0. In futuro cercare di passarla in modo variabile
        } 
    }

}
