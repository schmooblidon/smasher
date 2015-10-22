using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour {

    // VARIABLES

    //floats
    public float maxSpeed = 3;
    public float speed = 50f;

    public float shortHopVelocity = 10f;
    public float fullHopVelocity = 15f;

    public float doubleJumpVelocity = 17f;

    public float doubleJumpMultiplier = 10f;

    public float terminalVelocity = 20f;

    public float initDashVelocity = 10f;

    public float dashAcceleration = 0.3f;

    public float maxWalk = 6f;

    public float walkAcceleration = 0.1f;

    public float initWalkVelocity = 2f;

    public float traction = 0.6f;

    public float groundToAir = 0.9f;

    public float airAcceleration = 0.7f;

    public float airTerminalVelocity = 8f;

    public float grav = 3.2f;

    public float gravAcc = 0.7f;

    // ints
    public int nairLandingLag = 14;
    public int nairLcancelledLag = 7;

    public int upairLandingLag = 16;
    public int upairLcancelledLag = 8;

    public int bairLandingLag = 18;
    public int bairLcancelledLag = 9;

    // arrays for inputs
    public float[] h = new float[] { 0, 0, 0, 0, 0 };
    public float[] v = new float[] { 0, 0, 0, 0, 0 };

    public bool[] sh = new bool[] { false, false, false, false, false, false, false, false, false, false };
    public bool[] j = new bool[] { false, false, false, false, false, false, false, false, false, false };
    public bool[] b = new bool[] { false, false, false, false, false, false, false, false, false, false };


    //booleans
    public string currentState;
    public bool ignorePlatforms = false;

    public bool grounded = false;
    public bool inJumpsquat = false;
    public bool inHitstun = false;
    public bool airdodging = false;
    public bool airdodgeGround = false;
    public bool doubleJumpFlag = true;
    public bool facingRight;
    public bool lcancelled = false;

    public bool ignoreThisGayFunction = false;
    public bool platCollide = false;

    public bool inAerialAction = false;

    public bool alwaysCollide = false;

    public bool disableDashSmashTurn = false;

    // int timers for keeping track of action state frames
    public int frametimer = 0;
    public int airborneTimer = 0;
    public int airdodgeTimer = 0;
    public int jumpsquatTimer = 0;
    public int fullOrShortHop = 0;
    public int dashFrameTimer = 0;
    public int landingTimer = 0;
    public int landingFallSpecialTimer = 0;
    public int runBrakeTimer = 0;
    public int smashTurnTimer = 0;
    public int tiltTurnTimer = 0;
    public int walkTimer = 0;
    public int aerialTimer = 0;
    public int aerialLandingTimer = 0;
    public int crouchDownTimer = 0;
    public int crouchUpTimer = 0;
    public int downSpecialTimer = 0;
    public int downSpecialReleaseTimer = 0;

    public int ignoreGayFunctionTimer = 0;
    //stats
    public int curPercent;
    public int maxPercent = 999;

    //references
    public Rigidbody2D rb2d;
    private Animator anim;
    public PolygonCollider2D ecb0;
    public PolygonCollider2D ecb1;
    public EdgeCollider2D plat;

    public GameObject NairHitbox;
    public GameObject UpairHitbox;
    public GameObject BairHitbox;


    public GameObject LandingDust;
    public GameObject DashingDustLeft;
    public GameObject DashingDustRight;
    public GameObject DoubleJumpDust;
    public GameObject FastFallDust;

    public Text actstate;
    public Text actstate2;
    public Text actstatenum;
    public Text actstatenum2;
    public Text jumpframes;
    public Text LcancelAlert;

    public float gameSpeed = 1f;





    void Start()
    {
        // stuff to do only at the Start

        rb2d = gameObject.GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponent<Animator>();


        curPercent = 0;

        rb2d.gravityScale = grav;
        facingRight = true;

        currentState = "Idle";

        LcancelAlert.text = "";

        Time.timeScale = gameSpeed;
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
       // function to do every frame

        //set the first element of the horizontal and vertical arrays, to the current analog axis inputs. same deal for shoulder, jump and special
        h[0] = Input.GetAxis("Horizontal");
        v[0] = Input.GetAxis("Vertical");

        sh[0] = Input.GetButtonDown("Shoulder");
        j[0] = Input.GetButtonDown("Jump");
        b[0] = Input.GetButtonDown("Special");


        // attempting to limit the ability to dash or smashturn, like in when where you tilt in said direction for at least 2 frames, dash/smashturn in that direction becomes disabled
        if ((!facingRight && (h[0] > 0.05 && h[0] < 0.4) && (h[1] > 0.05 && h[1] < 0.4)) || (facingRight && (h[0] < -0.05 && h[0] > -0.4) && (h[1] < -0.05 && h[1] > -0.4)))
        {
            disableDashSmashTurn = true;
        }
        if (currentState != "Dash" && currentState != "SmashTurn")
        {
            disableDashSmashTurn = false;
        }

        // when airborne, stop ignoring the gay function (makes platforms work). if in any state except airdodge and downspecial (for now), reduce y velocity by gravAcc
        if (!grounded)
        {
            ignoreThisGayFunction = false;
            airborneTimer++;
            if (currentState != "Airdodge" && currentState != "DownSpecial")
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, rb2d.velocity.y - gravAcc);
            }
        }
        else
        {
            airborneTimer = 0;
            // i dont think this does anything atm
            ignorePlatforms = false;
        }


        /*if ((currentState == "Idle" || (currentState == "Landing" && landingTimer > 3)) && (h[0] > 0.4 || h[0] < -0.4) && (h[2] < 0.4 && h[2] > -0.4))
        {
            DashInit();
        }


        if (currentState == "Dash" || currentState == "Walk" || currentState == "Idle" || (currentState == "Landing" && landingTimer > 3))
        {
            SmashTurnInit();
        }*/

        // if shoulder is being pushed, and in the jump/fall/doublejump state, initiate an airdodge
        if (Input.GetButtonDown("Shoulder") && (currentState == "Jump" || currentState == "Fall" || currentState == "DoubleJump" ))
        {
            AirdodgeInit();
        }

        // if jump is being pushed, you arent grounded, you still have a doublejump, and in state jump/fall, initiate double jump
        if (Input.GetButtonDown("Jump") && !grounded && doubleJumpFlag && (currentState == "Jump" || currentState == "Fall"))
        {
            DoubleJumpInit();
        }

        // if current jump push is true and your arent in jumpsquat and your grounded, and in one of those many states. (if in landing have to be after frame 3 at least) initiate jumpsquat
        if (j[0] && !inJumpsquat && grounded)
        {
            if (currentState == "Walk" || currentState == "Idle" || currentState == "SmashTurn" || currentState == "Dash" || currentState == "Crouch" || currentState == "Run" || currentState == "RunBrake" || currentState == "TiltTurn" || (currentState == "Landing" && landingTimer > 3)){
                JumpsquatInit();
            }

        }

        // check if one of those states, and special and down is being pushed. initiate downspecial
        if ((currentState == "Crouch" || currentState == "CrouchDown" || currentState == "CrouchUp" || currentState == "Idle" || currentState == "Dash" || currentState == "Run" || currentState == "Jump" || currentState == "SmashTurn" || currentState == "TiltTurn" || currentState == "Fall" || currentState == "DoubleJump") && b[0] && v[0] < -0.4f)
        {
            DownSpecialInit();
        }

        // finds the currentstate, to see which function(s) should be called
        switch (currentState)
        {
            case "SmashTurn":
                SmashTurn();
                break;
            case "Idle":
                Idle();
                break;
            case "TiltTurn":
                TiltTurn();
                break;
            case "Walk":
                Walk();
                break;
            case "Landing":
                Landing();
                break;
            case "LandingFallSpecial":
                LandingFallSpecial();
                break;
            case "Dash":
                Dash();
                break;
            case "Run":
                Run();
                break;
            case "RunBrake":
                RunBrake();
                break;
            case "Airdodge":
                AirDodge();
                LandingTypeInit();
                break;
            case "Jumpsquat":
                Jumpsquat();
                break;

            case "Fall":
                FastFall();
                AirDrift();
                LandingTypeInit();
                Aerials();
                break;




            case "FallSpecial":
                FastFall();
                AirDrift();
                LandingTypeInit();
                break;

            case "Jump":
            case "DoubleJump":
                AirDrift();
                LandingTypeInit();
                Aerials();
                break;

            case "Nair":
                FastFall();
                Nair();
                AirDrift();
                break;

            case "Upair":
                FastFall();
                Upair();
                AirDrift();
                break;

            case "Bair":
                FastFall();
                Bair();
                AirDrift();
                break;

            case "UpairLanding":
                AerialLanding("Upair", upairLandingLag, upairLcancelledLag);
                break;

            case "NairLanding":
                AerialLanding("Nair", nairLandingLag, nairLcancelledLag);
                break;

            case "BairLanding":
                AerialLanding("Bair", bairLandingLag, bairLcancelledLag);
                break;


            case "CrouchDown":
                CrouchDown();
                break;
            case "Crouch":
                Crouch();
                break;
            case "CrouchUp":
                CrouchUp();
                break;

            case "DownSpecial":
                DownSpecial();
                break;

            case "DownSpecialRelease":
                DownSpecialRelease();
                break;
            default:
                break;
        }

        // sets unity's animator variable grounded to the script grounded variable value
        anim.SetBool("grounded", grounded);

        // if falling faster than terminal velocity, just set new velocity to terminal
        if (rb2d.velocity.y < -terminalVelocity)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, -terminalVelocity);

        }

        // if in one of these state, and y velocity drops below zero, initiate fall
        if (((currentState == "Jump" || currentState == "DoubleJump") && rb2d.velocity.y < 0) || ((currentState == "Walk" || currentState == "Run" || currentState == "RunBrake" || currentState == "Jumpsquat" || currentState == "Idle" || currentState == "LandingFallSpecial" || currentState == "Landing") && !grounded))
        {
            FallInit();

        }


        // flips the sprite to face the right way
        if (!facingRight)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        // setting unity's aniamtor variables to correspond with the script variables
        anim.SetBool("facingRight", facingRight);



        anim.SetFloat("velocityx", rb2d.velocity.x);

        anim.SetFloat("velocityy", rb2d.velocity.y);

        //Debug.Log(currentState);
        //Debug.Log(rb2d.velocity.y);

        // whenever grounded, regain your doublejump
        if (grounded)
        {
            doubleJumpFlag = true;
        }

        // when airborne, within the first 9 frames, have the bottom point set to 0,0 (this is the chars base position). on the 10th frame, raise it higher and squish it.
        if (!grounded)
        {
            if (airborneTimer < 9)
            {
                ecb0.points = new Vector2[] { ecb0.points[0], ecb0.points[1], new Vector2(0, 0), ecb0.points[3] };
            }
            else if (airborneTimer == 10)
            {

                ecb0.points = new Vector2[] { new Vector2(0, 1.6f), new Vector2(0.6f, 1), new Vector2(0, 0.4f), new Vector2(-0.6f, 1) };
            }
        }
        else
        {
            ecb0.points = new Vector2[] { new Vector2(0, 2f), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(-0.5f, 1) };
        }

        // attempting to make sure you collide with plats when in one of these states, but only when you are above the plat. ignoring whether you are holding down.
        if (currentState == "Bair" || currentState == "Upair" || currentState == "Nair" || currentState == "Fair" || currentState == "Dair" || currentState == "Airdodge")
        {
            if (rb2d.velocity.y > 0.00001 && ((ecb0.transform.position.y + ecb0.points[2][1]) < (plat.transform.position.y + 2.0001)))
            {
                // ignore the platform. it does this by checking a box in a collision array. you can find it through edit>projectsettings>physics2D  the player has it's own layer and so does the platform
                Physics2D.IgnoreLayerCollision(8, 9, true);
            }
            else if (rb2d.velocity.y < 0 && ((ecb0.transform.position.y + ecb0.points[2][1]) > (plat.transform.position.y + 2.0001)))
            {
                Physics2D.IgnoreLayerCollision(8, 9, false);
            }

        }/*
        else if ((moving upward OR (ecb bottom IS-LESS-THAN platform height) OR (crouching frame > 3 AND crouching frame < 6 AND holding down) OR (falling && holding down && not grouneded)) AND not ignoring gay function AND not in an aerial action)*/
        else if ((rb2d.velocity.y > 0.00001 || ((ecb0.transform.position.y + ecb0.points[2][1]) < (plat.transform.position.y + 2.0001)) || (crouchDownTimer > 2 && crouchDownTimer < 6 && v[0] < -0.4f) || (rb2d.velocity.y < 0 && v[0] < -0.4f && !grounded)) && !ignoreThisGayFunction && !inAerialAction)
        {
            //ignore platform
            Physics2D.IgnoreLayerCollision(8, 9, true);


        }
        else
        {
            // do not ignore platform
            Physics2D.IgnoreLayerCollision(8, 9, false);

        }

        // sets the current frame ecb (corrected ecb) to the projected ecb's points
        ecb1.points = ecb0.points;

        // moves the input values back one in their respective arrays
        for (int i = 4; i > 0; i--)
        {
            h[i] = h[i - 1];
            v[i] = v[i - 1];
        }

        for (int i = 9; i > 0; i--)
        {
            sh[i] = sh[i - 1];
            j[i] = j[i - 1];
            b[i] = b[i - 1];
        }

        // calling the function that displays the current actionstate
        ActionStateDisplay();


    }

    public void Die()
    {
        //restart
        Application.LoadLevel(Application.loadedLevel);
    }


    // a pretty versatile function, mostly used for traction reductions. takes in the reduction amount, asks if you want to double the value if moving faster than max walk speed, ask whether the velocity can go below zero or above depending on which way you are moving, then asks whether you can be moving backwards with a default value of false
    void ReduceVelocityX(float reduction, bool useDouble, bool belowZero, bool canReverse = false)
    {
        // part of function if you can move backward
        if (canReverse)
        {
            // when velocity is greater than 0
            if (rb2d.velocity.x > 0)
            {
                // if velocity is faster than max walk speed and you want to double the reduction if that is the case, then double the reduction
                if (rb2d.velocity.x > maxWalk && useDouble)
                {
                    reduction = 2 * reduction;
                }
                // new temp velocity with reduction, checking if you can allow it to go below zero, if false then check if 0
                float newH = rb2d.velocity.x - reduction;
                if (!belowZero)
                {
                    if (newH < 0)
                    {
                        newH = 0;
                    }
                }
                // apply new velocity to char
                rb2d.velocity = new Vector2(newH, rb2d.velocity.y);
            }
            else
            {

                if (rb2d.velocity.x < -maxWalk && useDouble)
                {
                    reduction = 2 * reduction;
                }
                float newH = rb2d.velocity.x + reduction;
                if (!belowZero)
                {
                    if (newH > 0)
                    {
                        newH = 0;
                    }
                }
                rb2d.velocity = new Vector2(newH, rb2d.velocity.y);
            }
        }
        else
        {
            // if facing right
            if (transform.localScale.x > 0)
            {
                if (rb2d.velocity.x > maxWalk && useDouble)
                {
                    reduction = 2 * reduction;
                }
                float newH = rb2d.velocity.x - reduction;
                if (!belowZero)
                {
                    if (newH < 0)
                    {
                        newH = 0;
                    }
                }
                rb2d.velocity = new Vector2(newH, rb2d.velocity.y);
            }
            else
            {
                if (rb2d.velocity.x < -maxWalk && useDouble)
                {
                    reduction = 2 * reduction;
                }
                float newH = rb2d.velocity.x + reduction;
                if (!belowZero)
                {
                    if (newH > 0)
                    {
                        newH = 0;
                    }
                }
                rb2d.velocity = new Vector2(newH, rb2d.velocity.y);
            }
        }

    }


    // called when smashturn is the current state
    public void SmashTurn()
    {
        if (smashTurnTimer > 8)
        {
            anim.SetBool("inSmashTurn", false);
            if (facingRight)
            {
                facingRight = false;
            }
            else
            {
                facingRight = true;
            }
            currentState = "Idle";
        }
        else if (smashTurnTimer > 0 && facingRight && h[0] < -0.4)
        {
            rb2d.velocity = new Vector2(-initDashVelocity, rb2d.velocity.y);
            currentState = "Dash";
            anim.SetBool("inSmashTurn", false);
            anim.SetBool("dashing", true);
            dashFrameTimer = 0;
            facingRight = false;
            // creates the dust cloud, and positiones it correctly
            var clone = Instantiate(DashingDustLeft, new Vector3(rb2d.transform.position.x, rb2d.transform.position.y - 0.26f, 0), transform.rotation);
            // destroys the dust gameobject after 0.3 seconds
            Destroy(clone, 0.3f);
        }
        else if (smashTurnTimer > 0 && !facingRight && h[0] > 0.4)
        {
            rb2d.velocity = new Vector2(initDashVelocity, rb2d.velocity.y);
            currentState = "Dash";
            anim.SetBool("inSmashTurn", false);
            anim.SetBool("dashing", true);
            dashFrameTimer = 0;
            facingRight = true;
            var clone = Instantiate(DashingDustRight, new Vector3(rb2d.transform.position.x, rb2d.transform.position.y - 0.26f, 0), transform.rotation);
            Destroy(clone, 0.3f);
        }

        smashTurnTimer++;
    }

    public void Idle()
    {
        if (rb2d.velocity.x != 0)
        {
            ReduceVelocityX(traction, true, false);
        }

        AllowWalkDash();

        if (v[0] < -0.4)
        {
            CrouchDownInit();
        }
    }

    public void WalkInit()
    {
        float newH = 0;
        if (facingRight)
        {
            newH = rb2d.velocity.x + initWalkVelocity;
            if (newH > initWalkVelocity)
            {
                newH = initWalkVelocity;
            }
        }
        else
        {
            newH = rb2d.velocity.x - initWalkVelocity;
            if (newH < -initWalkVelocity)
            {
                newH = -initWalkVelocity;
            }
        }
        rb2d.velocity = new Vector2(newH, rb2d.velocity.y);
        currentState = "Walk";
        anim.SetBool("walking", true);
        walkTimer = 0;
    }

    public void TiltTurnInit()
    {
        anim.SetBool("inTiltTurn", true);
        currentState = "TiltTurn";
        tiltTurnTimer = 0;
    }

    public void CrouchDownInit()
    {
        currentState = "CrouchDown";
        anim.SetBool("crouchDown", true);
        crouchDownTimer = 0;
        ReduceVelocityX(traction, true, false, true);
    }

    public void CrouchDown()
    {
        if (!grounded)
        {
            currentState = "Fall";
            anim.SetBool("crouchDown", false);
        }
        if (crouchDownTimer == 8)
        {
            currentState = "Crouch";
            anim.SetBool("crouchDown", false);
            anim.SetBool("crouching", true);
        }
        else
        {
            ReduceVelocityX(traction, true, false, true);
            crouchDownTimer++;
        }
    }

    public void Crouch()
    {
        if ((facingRight && h[0] < -0.4f) || (!facingRight && h[0] > 0.4f))
        {
            anim.SetBool("crouching", false);
            SmashTurnInit();
        }
        else if ((facingRight && h[0] > 0.4f) || (!facingRight && h[0] < -0.4f))
        {
            anim.SetBool("crouching", false);
            DashInit();
        }
        else if (v[0] > -0.4f)
        {
            currentState = "CrouchUp";
            anim.SetBool("crouching", false);
            anim.SetBool("crouchUp", true);
            crouchUpTimer = 0;
        }
        else
        {
            ReduceVelocityX(traction, true, false, true);
        }
    }

    public void CrouchUp()
    {
        if (crouchDownTimer == 8)
        {
            currentState = "Idle";
            anim.SetBool("crouchUp", false);
        }
        else
        {
            ReduceVelocityX(traction, true, false, true);
        }
    }

    public void TiltTurn()
    {
        if (tiltTurnTimer > 8)
        {
            anim.SetBool("inTiltTurn", false);
            if (facingRight)
            {
                facingRight = false;
            }
            else
            {
                facingRight = true;
            }
            currentState = "Idle";
        }
        else if (tiltTurnTimer > 2 && facingRight && h[0] < -0.4 && h[2] > -0.05)
        {
            rb2d.velocity = new Vector2(-initDashVelocity, rb2d.velocity.y);
            currentState = "Dash";
            anim.SetBool("inTiltTurn", false);
            anim.SetBool("dashing", true);
            var clone = Instantiate(DashingDustLeft, new Vector3(rb2d.transform.position.x, rb2d.transform.position.y - 0.26f, 0), transform.rotation);
            Destroy(clone, 0.3f);
            dashFrameTimer = 0;
            facingRight = false;
        }
        else if (tiltTurnTimer > 2 && !facingRight && h[0] > 0.4 && h[2] < 0.05)
        {
            rb2d.velocity = new Vector2(initDashVelocity, rb2d.velocity.y);
            currentState = "Dash";
            anim.SetBool("inTiltTurn", false);
            anim.SetBool("dashing", true);
            var clone = Instantiate(DashingDustRight, new Vector3(rb2d.transform.position.x, rb2d.transform.position.y - 0.26f, 0), transform.rotation);
            Destroy(clone, 0.3f);

            dashFrameTimer = 0;
            facingRight = true;
        }

        tiltTurnTimer++;
    }

    public void Walk()
    {
        if (walkTimer == 1)
        {
            AllowWalkDash(false);

            /*if ((facingRight && h[0] > 0.4) || (!facingRight && h[0] < -0.4))
            {
                Debug.Log("maybe its this");
                anim.SetBool("walking", false);
                anim.SetBool("dashing", true);

                currentState = "Dash";
                dashFrameTimer = 0;
                if (facingRight)
                {
                    rb2d.velocity = new Vector2(initDashVelocity, rb2d.velocity.y);
                    var clone = Instantiate(DashingDustRight, new Vector3(rb2d.transform.position.x, rb2d.transform.position.y - 0.26f, 0), transform.rotation);
                    Destroy(clone, 0.3f);
                }
                else
                {
                    rb2d.velocity = new Vector2(-initDashVelocity, rb2d.velocity.y);
                    var clone = Instantiate(DashingDustLeft, new Vector3(rb2d.transform.position.x, rb2d.transform.position.y - 0.26f, 0), transform.rotation);
                    Destroy(clone, 0.3f);
                }
            }*/

        }
        if (h[0] > 0.05 || h[0] < -0.05)
        {
            float newH = 0;
            if (facingRight)
            {

                if (rb2d.velocity.x < maxWalk && h[0] > 0.05)
                {

                    newH = rb2d.velocity.x + (h[0] * walkAcceleration);

                }
                else
                {
                    newH = maxWalk;
                }

            }
            else
            {
                if (rb2d.velocity.x > -maxWalk && h[0] < -0.05)
                {
                    newH = rb2d.velocity.x + (h[0] * walkAcceleration);
                }
                else
                {
                    newH = -maxWalk;
                }
            }

            rb2d.velocity = new Vector2(newH, rb2d.velocity.y);

        }
        else
        {
            anim.SetBool("walking", false);
            currentState = "Idle";
        }

        walkTimer++;
    }


    public void LandingTypeInit(bool autocancel = false)
    {
        if (grounded)
        {
            ignoreGayFunctionTimer = 0;
            ignoreThisGayFunction = true;
            rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
            Debug.Log("landingtypeinit is called and grounded");
            ecb1.transform.position = new Vector2(ecb1.transform.position.x, ecb1.transform.position.y + 1);

            if (currentState == "Fall" || autocancel)
            {
                currentState = "Landing";
                landingTimer = 0;
                //LandingDust.enabled = true;

            }
            else if (currentState == "FallSpecial" || currentState == "Airdodge")
            {
                currentState = "LandingFallSpecial";
                landingFallSpecialTimer = 0;
            }
        }
    }

    public void Landing()
    {
        IgnoringIgnoreFunction();
        if (landingTimer == 1)
        {
            LandingFix();

            var clone = Instantiate(LandingDust, new Vector3(rb2d.transform.position.x, rb2d.transform.position.y - 0.26f, 0), transform.rotation);
            Destroy(clone, 0.3f);
        }

        if (landingTimer > 3)
        {
            AllowWalkDash();
        }

        if (landingTimer > 30)
        {

            currentState = "Idle";
        }
        else
        {

            anim.SetBool("autocancel", false);
            ReduceVelocityX(traction, true, false, true);
            rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
            landingTimer++;
        }
    }

    public void LandingFallSpecial()
    {
        IgnoringIgnoreFunction();
        if (landingFallSpecialTimer == 1)
        {
            LandingFix();
            var clone = Instantiate(LandingDust, new Vector3(rb2d.transform.position.x, rb2d.transform.position.y - 0.26f, 0), transform.rotation);
            Destroy(clone, 0.3f);
        }
        if (landingFallSpecialTimer > 8)
        {
            currentState = "Idle";
        }
        else
        {
            anim.SetBool("airdodge", false);
            ReduceVelocityX(traction, true, false, true);
            rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
            landingFallSpecialTimer++;
        }


    }

    public void DashInit()
    {
        if (!disableDashSmashTurn)
        {
            currentState = "Dash";
            anim.SetBool("dashing", true);
            anim.SetBool("walking", false);
            if (facingRight)
            {
                rb2d.velocity = new Vector2(initDashVelocity, rb2d.velocity.y);
                var clone = Instantiate(DashingDustRight, new Vector3(rb2d.transform.position.x, rb2d.transform.position.y - 0.26f, 0), transform.rotation);
                Destroy(clone, 0.3f);
            }
            else
            {
                rb2d.velocity = new Vector2(-initDashVelocity, rb2d.velocity.y);
                var clone = Instantiate(DashingDustLeft, new Vector3(rb2d.transform.position.x, rb2d.transform.position.y - 0.26f, 0), transform.rotation);
                Destroy(clone, 0.3f);
            }
            dashFrameTimer = 0;
        }
    }

    public void Dash()
    {
        float newH = rb2d.velocity.x + (dashAcceleration * (h[0] / 0.7f));

        if (dashFrameTimer < 10)
        {
            if (facingRight && newH > initDashVelocity)
            {
                newH = initDashVelocity;
            }
            else if (!facingRight && newH < -initDashVelocity)
            {
                newH = -initDashVelocity;
            }
            rb2d.velocity = new Vector2(newH, rb2d.velocity.y);
            AllowWalkDash(false, true, true);

            // moonwalk happens when you tilt back for at least 2 frames, as this does not interrupt dash
        }
        else
        {
            if (h[0] > 0.2 || h[0] < -0.2)
            {
                currentState = "Run";
                anim.SetBool("inRun", true);
            }
            else
            {
                currentState = "Idle";
            }
            anim.SetBool("dashing", false);
        }

        dashFrameTimer++;
    }


    public void SmashTurnInit()
    {
        if (!disableDashSmashTurn)
        {
            anim.SetBool("inSmashTurn", true);
            anim.SetBool("dashing", false);
            anim.SetBool("walking", false);
            currentState = "SmashTurn";
            float newH = 0;
            newH = rb2d.velocity.x * 0.25f;
            if (facingRight)
            {
                if (newH < 0)
                {
                    newH = 0;
                }
            }
            else
            {
                if (newH > 0)
                {
                    newH = 0;
                }
            }
            rb2d.velocity = new Vector2(newH, rb2d.velocity.y);
            ReduceVelocityX(traction, true, false);
            smashTurnTimer = 0;
        }
    }

    public void Run()
    {
        if (h[0] > 0.2 && facingRight)
        {
            float newH = rb2d.velocity.x + (0.2f * (h[0] / 0.7f));
            if (newH > initDashVelocity)
            {
                newH = initDashVelocity;
            }
            rb2d.velocity = new Vector2(newH, rb2d.velocity.y);
        }
        else if (h[0] < -0.2 && !facingRight)
        {
            float newH = rb2d.velocity.x + (0.2f * (h[0] / 0.7f));
            if (newH < -initDashVelocity)
            {
                newH = -initDashVelocity;
            }
            rb2d.velocity = new Vector2(newH, rb2d.velocity.y);
        }
        else
        {
            currentState = "RunBrake";
            runBrakeTimer = 0;
            anim.SetBool("inRunBrake", true);
            anim.SetBool("inRun", false);
        }
    }

    public void RunBrake()
    {
        if (runBrakeTimer < 30)
        {
            ReduceVelocityX(traction, true, false);

            runBrakeTimer++;
            if (v[0] < -0.4f && v[1] < -0.4f)
            {
                anim.SetBool("inRunBrake", false);
                CrouchDownInit();
            }
        }
        else
        {
            anim.SetBool("inRunBrake", false);
            currentState = "Idle";
        }
    }

    public void AirDrift()
    {
        float newH = rb2d.velocity.x + (h[0] * airAcceleration);
        if (newH > airTerminalVelocity)
        {
            rb2d.velocity = new Vector2(airTerminalVelocity, rb2d.velocity.y);
        }
        else if (newH < -airTerminalVelocity)
        {
            rb2d.velocity = new Vector2(-airTerminalVelocity, rb2d.velocity.y);
        }
        else
        {
            rb2d.velocity = new Vector2(newH, rb2d.velocity.y);
        }
        if (v[0] < -0.4f)
        {
            ignorePlatforms = true;
        }
        else
        {
            ignorePlatforms = false;
        }
    }

    public void AirDodge()
    {
        if (grounded)
        {
            rb2d.gravityScale = grav;

            currentState = "LandingFallSpecial";
            landingFallSpecialTimer = 0;
            anim.SetBool("airdodge", false);
            rb2d.velocity = new Vector2(rb2d.velocity.x, 0);

        }
        else if (airdodgeTimer < 20)
        {
            float ratio = (rb2d.velocity.x / 2) / (rb2d.velocity.y / 3);
            rb2d.velocity = new Vector2(rb2d.velocity.x * 0.9f, rb2d.velocity.y * 0.9f);



        }
        else if (airdodgeTimer == 20)
        {
            rb2d.velocity = new Vector2(0, 0);
        }
        else
        {
            anim.SetBool("airdodge", false);
            currentState = "FallSpecial";
            rb2d.gravityScale = grav;
        }
        airdodgeTimer++;
    }

    public void AirdodgeInit()
    {
        rb2d.gravityScale = 0;
        currentState = "Airdodge";
        anim.SetBool("airdodge", true);
        anim.SetBool("inJumpsquat", false);
        ignorePlatforms = false;
        float newH = h[0] * 50f / 1.4f;
        float newV = v[0] * 50f / 1.4f;
        rb2d.velocity = new Vector2(newH, newV);
        // moves the bottom of the ecb downward, in an attempt to makes wavelanding better
        ecb0.points = new Vector2[] { new Vector2(0, 1.9f), new Vector2(0.3f, 1), new Vector2(0, 0.1f), new Vector2(-0.3f, 1) };
        airdodgeTimer = 0;
    }

    public void DoubleJumpInit()
    {
        // sets a new horizontal velocity to be proportionate to the input of the analog's horizontal axis
        float newH = h[0] * doubleJumpMultiplier;
        rb2d.velocity = new Vector2(newH, doubleJumpVelocity);
        doubleJumpFlag = false;
        anim.SetBool("doubleJumping", true);
        currentState = "DoubleJump";

        var clone = Instantiate(DoubleJumpDust, new Vector3(rb2d.transform.position.x, rb2d.transform.position.y, 0), transform.rotation);
        Destroy(clone, 0.2f);
    }

    public void JumpsquatInit()
    {
        currentState = "Jumpsquat";
        anim.SetBool("dashing", false);
        anim.SetBool("walking", false);
        anim.SetBool("inRun", false);
        anim.SetBool("inRunBrake", false);
        anim.SetBool("inSmashTurn", false);
        anim.SetBool("inTiltTurn", false);
        inJumpsquat = true;
        jumpsquatTimer = 0;
        fullOrShortHop = 0;
        anim.SetBool("inJumpsquat", inJumpsquat);
    }


    public void Jumpsquat()
    {
        if (jumpsquatTimer > 3)
        {
            inJumpsquat = false;
            anim.SetBool("inJumpsquat", inJumpsquat);
            grounded = false;
            currentState = "Jump";
            float newH = rb2d.velocity.x * groundToAir;
            if (fullOrShortHop < jumpsquatTimer)
            {
                rb2d.velocity = new Vector2(newH, shortHopVelocity);
                //Debug.Log("ShortHop");

            }
            else
            {
                rb2d.velocity = new Vector2(newH, fullHopVelocity);
                //Debug.Log("FullHop");
            }


        }
        else
        {

            ReduceVelocityX(traction, true, false);


            if (Input.GetButton("Jump"))
            {
                fullOrShortHop++;
                //Debug.Log(fullOrShortHop);
            }
            jumpsquatTimer++;
        }
    }


    public void FallInit()
    {
        anim.SetBool("walking", false);
        anim.SetBool("dashing", false);
        anim.SetBool("inJumpsquat", false);
        anim.SetBool("inRun", false);
        anim.SetBool("inRunBrake", false);
        anim.SetBool("doubleJumping", false);
        currentState = "Fall";
    }

    public void FastFall()
    {
        if (v[0] < -0.5 && v[1] > -0.5)
        {

            rb2d.velocity = new Vector2(rb2d.velocity.x, -terminalVelocity);
            var clone = Instantiate(FastFallDust, rb2d.transform.position, transform.rotation);
            Destroy(clone, 0.2f);
        }
    }



    public void Aerials()
    {
        if (Input.GetButtonDown("A") && (h[0] < 0.3 && h[0] > -0.3) && (v[0] < 0.3 && v[0] > -0.3))
        {
            currentState = "Nair";
            anim.SetBool("Nair", true);
            anim.SetBool("doubleJumping", false);
            aerialTimer = 0;
        }

        if (Input.GetButtonDown("A") && (v[0] > 0.5))
        {
            currentState = "Upair";
            anim.SetBool("Upair", true);
            anim.SetBool("doubleJumping", false);
            aerialTimer = 0;
        }

        if (Input.GetButton("A") && ((!facingRight && h[0] > 0.5) || (facingRight && h[0] < -0.5))){
            currentState = "Bair";
            anim.SetBool("Bair", true);
            anim.SetBool("doubleJumping", false);
            aerialTimer = 0;
        }
    }

    public void Nair()
    {
        if (grounded)
        {
            if (aerialTimer > 3 && aerialTimer < 19)
            {
                currentState = "NairLanding";
                NairHitbox.SetActive(false);
                AerialLandingInit("Nair", nairLandingLag, nairLcancelledLag);
            }
            else
            {
                anim.SetBool("autocancel", true);
                anim.SetBool("Nair", false);
                LandingTypeInit(true);

            }

        }
        else if (aerialTimer > 3 && aerialTimer < 8)
        {
            NairHitbox.SetActive(true);

        }
        else if (aerialTimer == 8)
        {
            NairHitbox.SetActive(false);
        }
        else if (aerialTimer > 25)
        {
            currentState = "Fall";
            anim.SetBool("Nair", false);
        }
        aerialTimer++;
    }

    public void Upair()
    {
        if (grounded)
        {
            if (aerialTimer > 4 && aerialTimer < 23)
            {
                currentState = "UpairLanding";
                UpairHitbox.SetActive(false);
                AerialLandingInit("Upair", upairLandingLag, upairLcancelledLag);
            }
            else
            {
                anim.SetBool("autocancel", true);
                anim.SetBool("Upair", false);
                LandingTypeInit(true);
            }

        }
        else if (aerialTimer > 3 && aerialTimer < 8)
        {
            UpairHitbox.SetActive(true);

        }
        else if (aerialTimer == 8)
        {
            UpairHitbox.SetActive(false);
        }
        else if (aerialTimer > 25)
        {
            currentState = "Fall";
            anim.SetBool("Upair", false);
        }
        aerialTimer++;
    }


    public void Bair()
    {
        if (grounded)
        {
            if (aerialTimer > 3 && aerialTimer < 38)
            {
                currentState = "BairLanding";
                BairHitbox.SetActive(false);
                AerialLandingInit("Bair", bairLandingLag, bairLcancelledLag);
            }
            else
            {
                anim.SetBool("autocancel", true);
                anim.SetBool("Bair", false);

                LandingTypeInit(true);

            }

        }
        else if (aerialTimer > 16 && aerialTimer < 20)
        {
            BairHitbox.SetActive(true);

        }
        else if (aerialTimer == 20)
        {
            BairHitbox.SetActive(false);
        }
        else if (aerialTimer > 38)
        {
            currentState = "Fall";
            anim.SetBool("Bair", false);
        }
        aerialTimer++;


    }
    public void AerialLandingInit(string aerial, int landingLag, int lcancelledLag)
    {

        ignoreGayFunctionTimer = 0;
        ignoreThisGayFunction = true;
        rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
        ecb1.transform.position = new Vector2(ecb1.transform.position.x, ecb1.transform.position.y + 1);

        anim.SetBool(aerial, false);
        anim.SetBool(aerial+"landing", true);
        lcancelled = false;
        aerialLandingTimer = 0;
        currentState = aerial+"Landing";
        for (int i = 0;i < 8; i++)
        {
            if (sh[i] == true)
            {
                LcancelAlert.text = "L-cancel!";

                lcancelled = true;

            }
        }
        AerialLanding(aerial, landingLag, lcancelledLag);

    }

    public void AerialLanding(string aerial, int landingLag, int lcancelledLag)
    {
        IgnoringIgnoreFunction();
        if (aerialLandingTimer == 1)
        {
            LandingFix();
            var clone = Instantiate(LandingDust, new Vector3(rb2d.transform.position.x, rb2d.transform.position.y - 0.26f, 0), transform.rotation);
            Destroy(clone, 0.3f);
        }
        if ((lcancelled && aerialLandingTimer >= lcancelledLag) || (!lcancelled && aerialLandingTimer >= landingLag)){
            currentState = "Idle";
            anim.SetBool(aerial+"landing", false);
            LcancelAlert.text = "";
        }
        else
        {
            ReduceVelocityX(traction, true, false, true);
        }
        aerialLandingTimer++;
    }


    public void DownSpecialInit()
    {
        anim.SetBool("downSpecial", true);
        anim.SetBool("crouchDown", false);
        anim.SetBool("crouchUp", false);
        anim.SetBool("crouching", false);
        anim.SetBool("doubleJumping", false);
        anim.SetBool("inTiltTurn", false);
        anim.SetBool("inSmashTurn", false);
        anim.SetBool("inRun", false);
        anim.SetBool("dashing", false);
        anim.SetBool("walking", false);
        currentState = "DownSpecial";
        downSpecialTimer = 0;
        if (!grounded)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
        }
    }

    public void DownSpecial()
    {
        if (j[0])
        {

            anim.SetBool("downSpecial", false);
            if (grounded)
            {
                JumpsquatInit();
            }
            else
            {
                if (doubleJumpFlag)
                {
                    DoubleJumpInit();

                }
            }
        }

        if (downSpecialTimer == 5)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, rb2d.velocity.y - 1.4f);
        }

        if (facingRight && h[0] < -0.4f)
        {

            facingRight = false;
        }
        else if (!facingRight && h[0] > 0.4f)
        {

            facingRight = true;
        }

        if (!Input.GetButton("Special") && downSpecialTimer > 4)
        {

            downSpecialReleaseTimer = 0;
            currentState = "DownSpecialRelease";
            anim.SetBool("downSpecial", false);
        }
        else
        {
            ReduceVelocityX(traction, false, false, true);
            downSpecialTimer++;
        }
    }

    public void DownSpecialRelease()
    {
        Debug.Log("testing");
        if (j[0])
        {

            if (grounded)
            {
                JumpsquatInit();
            }
            else
            {
                if (doubleJumpFlag)
                {
                    DoubleJumpInit();

                }
            }
        }
        if (downSpecialReleaseTimer > 25)
        {
            currentState = "Idle";
        }
        else
        {
            ReduceVelocityX(traction, true, false, true);
            downSpecialReleaseTimer++;
        }
    }

    public void AllowWalkDash(bool allowWalk = true, bool allowDash = true, bool moonWalk = false)
    {
        //Debug.Log(h[0]+" "+h[2]);


        //typical moonwalk = h[0] 0.4, h[1] 0.2, h[2] 0.1, h[3] 0, h[4] -0.2, h[5] -0.4

        // typical dashback = h[0] 0.4 h[1] 0.1 (enter smashturn) h[2] -0.1 h[3] -0.4

        // if h goes below 0.4, only interrupt if h = -0.4 within 2 frames



        //if (!facingRight && h[0] > 0.4 )






        if (h[0] > 0.05)
        {
            if (h[0] > 0.4 && h[3] < 0.05 && allowDash)
            {
                if (facingRight)
                {
                    DashInit();
                }
                else
                {
                    SmashTurnInit();
                }
            }
            else if (allowWalk)
            {
                if (facingRight)
                {
                    WalkInit();
                }
                else
                {
                    TiltTurnInit();

                }
            }
        }
        else if (h[0] < -0.05)
        {
            if (h[0] < -0.4 && h[3] > -0.05 && allowDash)
            {

                if (facingRight)
                {
                    SmashTurnInit();
                }
                else
                {
                    DashInit();
                }
            }
            else if (allowWalk)
            {
                if (facingRight)
                {
                    TiltTurnInit();
                }
                else
                {
                    WalkInit();

                }
            }
        }
    }

    public void LandingFix()
    {
        Debug.Log("landingfix is called");
        rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
        ecb1.transform.position = new Vector2(ecb1.transform.position.x, ecb1.transform.position.y - 1);
        if (platCollide)
        {
            rb2d.transform.position = new Vector2(rb2d.transform.position.x, plat.transform.position.y + plat.points[0][1] + 1f);
        }
        else
        {
            rb2d.transform.position = new Vector2(rb2d.transform.position.x, 1);
        }
    }

    public void IgnoringIgnoreFunction()
    {
        if (ignoreGayFunctionTimer > 3)
        {
            ignoreThisGayFunction = false;
        }
        else
        {
            ignoreGayFunctionTimer++;
        }
    }



    public void ActionStateDisplay()
    {
        actstate.text = currentState;
        actstate2.text = currentState;

        int actnum = 0;

        switch (currentState)
        {
            case "Walk":
                actnum = walkTimer;
                break;
            case "Dash":
                actnum = dashFrameTimer;
                break;
            case "Idle":
                actnum = 0;
                break;
            case "Jumpsquat":
                actnum = jumpsquatTimer;
                break;
            case "SmashTurn":
                actnum = smashTurnTimer;
                break;
            case "TiltTurn":
                actnum = tiltTurnTimer;
                break;
            case "Run":
                actnum = 0;
                break;
            case "RunBrake":
                actnum = runBrakeTimer;
                break;
            case "Jump":
                actnum = 0;
                break;
            case "Fall":
                actnum = 0;
                break;
            case "Landing":
                actnum = landingTimer;
                break;
            case "LandingFallSpecial":
                actnum = landingFallSpecialTimer;
                break;
            case "Airdodge":
                actnum = airdodgeTimer;
                break;
            case "FallSpecial":
                actnum = 0;
                break;
            case "DoubleJump":
                actnum = 0;
                break;
            default:
                break;
        }

        string actnums = actnum.ToString();
        actstatenum.text = actnums;
        actstatenum2.text = actnums;

        string jumpamount = fullOrShortHop.ToString();
        jumpframes.text = jumpamount;
    }


}
