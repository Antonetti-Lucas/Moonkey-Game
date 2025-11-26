using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEngine.UI.Image;

public class playerController : MonoBehaviour
{
    public Transform respawn;
    public Transform playerObj;
    public Transform orientation;

    public Collider hitbox;
    
    public Camera cam;

    private new Rigidbody rigidbody;

    public Animator animator;

    AnimatorStateInfo animStateInfo;
    public float NTime;

    public float mass = 0;

    public float speed = 0;

    public float jump_speed = 0;
    public float jumpCutMultiplier = 0;

    public float rotationSpeed = 0;

    private bool is_jumping = false;

    private float JUMP_BUFFER = 0.15f;
    private float jump_buffer_timer = 0;

    private float COYOTE_TIME = 0.15f;
    private float coyote_timer = 0;

    private float movementX;
    private float movementZ;

    private float target_speed_x;
    private float target_speed_z;

    private float accel_rate_x;
    private float accel_rate_z;

    private float speed_diff_x;
    private float speed_diff_z;

    public static float globalGravity = -9.81f;

    public float gravityScaleModifier = 0;
    private float currGravityScale = 1.0f;
    public float gravityScale = 1.0f;

    public float sprintModifier = 2.0f;
    private float currSprintModifier = 1.0f;

    private bool menuDown;
    public cameraScript cameraScript;
    public GameObject menu;
    public int notPaused = 1;

    //Check if player is on ground
    public int IsGrounded()
    {
        LayerMask groundLayer = LayerMask.GetMask("Ground"); // Always mark Ground layer masks

        // Raycast downwards from the bottom of the character
        if (Physics.SphereCast(transform.position, 0.5f, -transform.up, out RaycastHit hit, 0.6f, groundLayer))
            return 1;
        else
            return 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Set up the rigidbody
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        
        //Disable gravity for custom gravity
        rigidbody.useGravity = false;

        //Disable hitbox
        hitbox.enabled = false;

        //Menu is not up
        menuDown = true;
    }

    //Gravity updates
    void gravity()
    {
        Vector3 gravity = globalGravity * currGravityScale * Vector3.up;
        rigidbody.AddForce(gravity, ForceMode.Acceleration);
    }

    //Movement Input
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 movementVector = context.ReadValue<Vector2>();

        //Get X and Z from the input
        movementZ = movementVector.y;
        movementX = movementVector.x;
    }

    //Sprint Context
    public void OnSprint(InputAction.CallbackContext Context)
    {
        //Button pressed
        //if (Context.started && IsGrounded() == 1)
        //{
        //    animator.SetBool("ShiftRun", true);
        //    currSprintModifier = sprintModifier;
        //}

        //Button released
        //if (Context.canceled)
        //{
        //    animator.SetBool("ShiftRun", false);
        //    currSprintModifier = 1.0f;
        //}
    }

    //Attack Context
    public void OnAttack(InputAction.CallbackContext Context)
    {
        //Button pressed
        if (Context.started)
        {
            animator.SetTrigger("Attacking");
            hitbox.enabled = true;
        }
    }

    public void OnEsc(InputAction.CallbackContext Context)
    {
        //Button pressed
        if (Context.started && menuDown)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Confined;
            UnityEngine.Cursor.visible = true;
            cameraScript.enabled = false;
            menu.SetActive(true);
            notPaused = 0;
        }

        //Button pressed
        if (Context.started && !menuDown)
        {
            MenuDown();
        }
    }

    public void MenuDown()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        cameraScript.enabled = true;
        menu.SetActive(false);
        notPaused = 1;
    }

    //Jump Input Context
    public void OnJump(InputAction.CallbackContext Context)
    {
        //Button pressed
        if(Context.started)
        {
            //Resets the jump timer when the player jumps
            jump_buffer_timer = JUMP_BUFFER;
        }

        //Button released
        if (Context.canceled)
        {
            if (is_jumping == true && rigidbody.linearVelocity.y > 0)
            {
                //the longer you press, the higher you go
                rigidbody.AddForce(Vector3.down * rigidbody.linearVelocity.y * (1 - jumpCutMultiplier), ForceMode.Impulse);
            }
        }

        //Jump endend
        if (Context.performed)
        {
            rigidbody.AddForce(Vector3.down * rigidbody.linearVelocity.y * (1 - jumpCutMultiplier), ForceMode.Impulse);
        }
    }

    //Player movement and jumping
    void movementLogic()
    {
        //If celebrating dont move and look at camera
        if (this.animator.GetCurrentAnimatorStateInfo(0).IsName("Celebrate"))
        {
            playerObj.transform.LookAt(new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z));
            rigidbody.AddForce(-rigidbody.linearVelocity, ForceMode.VelocityChange);
            return;
        }

        // Player Movement --------------------------------------
        //Get movement vector and normalize and make it relative to the world
        Vector3 direction = new Vector3(movementX, 0.0f, movementZ).normalized;
        Vector3 worldDirection = orientation.transform.TransformDirection(direction).normalized;

        //Set the target speed as the direction * the set speed
        target_speed_x = worldDirection.x * speed * currSprintModifier;
        target_speed_z = worldDirection.z * speed * currSprintModifier;

        //Set the acceleration rate for X, based on wether its deceleration or acceleration, or if it's grounded or not, F = mass * acceleration
        if (Mathf.Abs(target_speed_x) > 0.01)
            accel_rate_x = ((mass * 12) / (speed * currSprintModifier)) / Mathf.Pow(5, 1 - IsGrounded());
        else
            accel_rate_x = ((mass * 9) / (speed * currSprintModifier)) / Mathf.Pow(12, 1 - IsGrounded());

        //Set the acceleration rate for Z, based on wether its deceleration or acceleration, or if it's grounded or not, F = mass * acceleration
        if (Mathf.Abs(target_speed_z) > 0.01)
            accel_rate_z = ((mass * 12) / (speed * currSprintModifier)) / Mathf.Pow(5, 1 - IsGrounded());
        else
            accel_rate_z = ((mass * 9) / (speed * currSprintModifier)) / Mathf.Pow(12, 1 - IsGrounded());
        // Player Movement --------------------------------------

        // Handle Jump -----------------------------------------
        if (jump_buffer_timer > 0 && coyote_timer > 0 && is_jumping == false)
        {
            animator.SetBool("IsJumping", true);
            is_jumping = true;
            //If the player has recently pressed jump even if in the air (jump_buffer_timer)
            //and if the player has recently been on the ground (coyote_timer)
            //the player jumps
            rigidbody.AddForce(new Vector3(0f, jump_speed - (rigidbody.linearVelocity.y * rigidbody.mass), 0f), ForceMode.Impulse);
            //The subtraction is to cancel falling if for some reason that is a problem
        }
        else if (is_jumping == true && IsGrounded() == 1)
        {
            //if the player is jumping and touchs the ground they stopped jumping
            animator.SetBool("IsJumping", false);
            is_jumping = false;
        }
        // Handle Jump -----------------------------------------

        // Jump Apex Acceleration -------------------------------
        if (IsGrounded() == 0 && Mathf.Abs(rigidbody.linearVelocity.y) < 0.5)
        {
            accel_rate_x *= 1.2f;  //Player's acceleration when they are at the apex of their jump
            accel_rate_z *= 1.2f;

            target_speed_x *= 1.3f; //Maximum speed that the player can reach when at the apex of their jump.
            target_speed_z *= 1.3f;
        }
        // Jump Apex Acceleration -------------------------------

        //Air Momentum Conservation ----------------------------
        //If falling, velocity is higher than the target speed,
        //target speed is higher than 0.01, zero the accel rate so it doesnt decrease
        if (IsGrounded() == 0 && Mathf.Abs(rigidbody.linearVelocity.x) > Mathf.Abs(target_speed_x) && Mathf.Sign(rigidbody.linearVelocity.x) == Mathf.Sign(target_speed_x) && Mathf.Abs(target_speed_x) > 0.01)
            accel_rate_x = 0;
        if (IsGrounded() == 0 && Mathf.Abs(rigidbody.linearVelocity.z) > Mathf.Abs(target_speed_z) && Mathf.Sign(rigidbody.linearVelocity.z) == Mathf.Sign(target_speed_z) && Mathf.Abs(target_speed_z) > 0.01)
            accel_rate_z = 0;
        //Air Momentum Conservation ----------------------------

        //Jump gravity -----------------------------------------
        if(rigidbody.linearVelocity.y < 0){
            //Higher when falling
            currGravityScale = gravityScale * gravityScaleModifier;
        }else{
            //Normal otherwise
            currGravityScale = gravityScale;
        }
        //Jump gravity -----------------------------------------

        //Set the difference between target speed and current speed
        speed_diff_x = target_speed_x - rigidbody.linearVelocity.x;
        speed_diff_z = target_speed_z - rigidbody.linearVelocity.z;

        //Apply all forces
        Vector3 forces = new Vector3(speed_diff_x * accel_rate_x * notPaused, 0f, speed_diff_z * accel_rate_z * notPaused);
        rigidbody.AddForce(forces);

        //Rotate model to the correct side
       if(worldDirection.x == 0 && worldDirection.z == 0){
            //Player not moving
            animator.SetBool("IsRunning", false);
        }else{
            //PlayerMoving
            animator.SetBool("IsRunning", true);
            Quaternion origin = playerObj.transform.rotation;
            Quaternion target = Quaternion.LookRotation(worldDirection);
            playerObj.transform.rotation = Quaternion.Slerp(origin, target, Time.deltaTime * rotationSpeed);
        }
    }

    // No need for delta time
    void FixedUpdate()
    {
        animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        NTime = animStateInfo.normalizedTime;

        //Disable Hitbox
        if (animStateInfo.IsName("Spin") && NTime > 0.49f && hitbox.enabled == true){
            hitbox.enabled = false;
            //print("CABOU");
        }

        gravity();

        movementLogic();

        if(transform.position.y < -20)
        {
            transform.position = respawn.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Subtracts the jump timer
        jump_buffer_timer -= Time.deltaTime;

        //Resets the Coyote Timer if the player is grounded otherwise subtracts from it
        if (IsGrounded() == 0)
            coyote_timer -= Time.deltaTime;
        else
            coyote_timer = COYOTE_TIME;
    }
}
