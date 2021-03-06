﻿// The Player script receives input for the Player.

using System.Linq;
using UnityEngine;

[RequireComponent(typeof (PlayerController))]
[RequireComponent(typeof (GunController))]
public class Player : LivingEntity {

    Crosshair crosshair;

    // The PlayerController script handles the movement of the Player
    PlayerController playerController;
    GunController gunController;
    Camera viewCamera;

    float moveSpeed = 5f;
    float lookSpeed = 20f; // turning speed for game controller
    float dashSpeed = 150f;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
    }

    protected override void Start () {
        base.Start();

        UpdateStats(GameManager.instance.playerHealth, GameManager.instance.playerXP);

        crosshair = GameObject.FindGameObjectWithTag("Crosshair").GetComponent<Crosshair>();
        gunController.EquipPrimaryGun(1);
        gunController.EquipSecondaryGun(0);
	}
	
	void Update () {
        if (GameManager.instance.loading)
            return;

        // Move Player based on input from the keyboard
        Vector3 moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        playerController.Move(moveInput.normalized * moveSpeed);

        // Turn Player
        if (Input.GetJoystickNames().Contains("Controller (XBOX 360 For Windows)"))
        {
            // If an XBOX 360 pad is attached, turn the Player according to
            // the movement of the right analog stick.
            Vector3 lookPoint = new Vector3(Input.GetAxisRaw("XBOXRightAnalogH"), 0, Input.GetAxisRaw("XBOXRightAnalogV"));
            Vector3 newPoint = lookPoint.normalized * lookSpeed;
            // 6. move the crosshair to the position of the cursor
            crosshair.Move(newPoint, lookSpeed);
            // 7. turn the Player to face the crosshair
            playerController.LookAt(crosshair.transform.position);
        } else
        {
            // Turn Player to face the direction of the cursor
            // The screen is a 2d world but the game is in a 3d world so we need raycasting to get the
            // accurate position of the mouse cursor in the game world.

            // 1. Cast a ray from the camera through the mouse cursor's position
            Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
            // 2. draw a plane that's perpendicular to the y-axis and passes through a point at the gun height.
            // this would be a plane just above the ground and slicing the player at gun height.
            Plane groundPlane = new Plane(Vector3.up, Vector3.up * 1); // 1 here represents the height of the gun
            float rayDistance;

            // 3. if the ray drawn earlier intersects the plane
            if (groundPlane.Raycast(ray, out rayDistance))
            {
                // 4. get the point where the ray intersects the plane
                Vector3 point = ray.GetPoint(rayDistance);
                // 5. turn the Player to face that point
                playerController.LookAt(point);
                // 6. move the crosshair to the position of the cursor
                crosshair.transform.position = point;
            }
        }

        // Shoot
        if (Input.GetJoystickNames().Contains("Controller (XBOX 360 For Windows)"))
        {
            if (Input.GetAxis("XBOXRightT") > 0)
            {
                gunController.OnTriggerHold();
            } else
            {
                gunController.OnTriggerRelease();
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                gunController.OnTriggerHold();
            }

            if (Input.GetMouseButtonUp(0))
            {
                gunController.OnTriggerRelease();
            }
        }

        // Player dash
        if (Input.GetJoystickNames().Contains("Controller (XBOX 360 For Windows)"))
        {
            if (Input.GetButtonUp("XBOXRightB"))
            {
                playerController.Dash(dashSpeed);
            }
        } else
        {
            if (Input.GetMouseButtonUp(1))
            {
                playerController.Dash(dashSpeed);
            }
        }

        // Switch guns
        if (Input.GetJoystickNames().Contains("Controller (XBOX 360 For Windows)"))
        {
            if (Input.GetButtonUp("XBOXLeftB"))
            {
                gunController.SwitchGuns();
            }
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.F))
            {
                gunController.SwitchGuns();
            }
        }

    }

    // This method allows the player to carry experience and health over
    // levels. The Player's stats are stored when a new level is to be loaded.
    void UpdateStats(float newHP, float newXP)
    {
        health = newHP;
        xp = newXP;
    }

    public float GetHealth()
    {
        return health;
    }

    public float GetXP()
    {
        return xp;
    }
}
