using UnityEngine;
using UnityEngine.InputSystem; // new input system namespace

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public Rigidbody2D rb;

    private Vector2 movementInput;

    // Reference to generated input actions
    private PlayerInputActions inputActions;

    void Awake()
    {
        //initializes the input system
        inputActions = new PlayerInputActions();
        //turns on the input system
        inputActions.Player.Enable();
    }

    //Used to disable the input system when the script/GameObject is disabled
    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void FixedUpdate()//Difference between Update and FixedUpdate?
    {
        //waits for input
        //.x results in a singular number output, meaning this needs to be stored in a float
        //and not a Vector2
        float moveX = inputActions.Player.Move.ReadValue<Vector2>().x;
        //stores the current velocity in a separate variable so that we can modify only the x value
        Vector2 velocity = rb.linearVelocity;
        //sets the horizontal speed of the body
        velocity.x = moveX * speed;

        //moves body based on input using only the horizontal component
        rb.linearVelocity = velocity;
    }

}
