using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float speed = 7.0f;
    public float mouseSpeed = 10.0f;
    float vertical;

    public float camMoveSpeed = 7.0f;

    public Camera playerCam;

    Vector2 mouseDirection;

    void Update() {

        MovePlayer();
        MoveCam();

    }

    void MovePlayer() {

        if (Input.GetKeyDown(KeyCode.Space)) vertical += 1;
        if (Input.GetKeyUp(KeyCode.Space)) vertical -= 1;
        if (Input.GetKeyDown(KeyCode.LeftShift)) vertical -= 1;
        if (Input.GetKeyUp(KeyCode.LeftShift)) vertical += 1;

        float verticalMovement = vertical;

        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), verticalMovement, Input.GetAxisRaw("Vertical")).normalized;

        transform.Translate(direction * speed * Time.deltaTime);

    }

    void MoveCam() {

        Vector2 mouseMovement = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        mouseDirection += mouseMovement;

        if (mouseDirection.y > 18) mouseDirection.y = 18;
        if (mouseDirection.y < -18) mouseDirection.y = -18;

        playerCam.transform.localRotation = Quaternion.AngleAxis(-mouseDirection.y * mouseSpeed, Vector3.right);
        transform.localRotation = Quaternion.AngleAxis(mouseDirection.x * mouseSpeed, Vector3.up);

    }

}
