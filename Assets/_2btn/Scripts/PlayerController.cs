using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public enum SIDE {
        TOP,
        LEFT,
        RIGHT,
        BOTTOM
    };

    //Side the player is currently at
    public SIDE currentSide = SIDE.BOTTOM;

    private GameManager _gameManager;
    private CameraController cam;
    private Renderer rend;
    private Rigidbody rb;
    private AudioSource moveSound;

    private Vector3 currentPos; //Current player position
    private Vector3 RotationEdge; //Edge cube will rotate around.

    private float rotationSpeed = 1000;
    private float laneWideness = 1; //Wideness of the lanes
    private float angle = 0;

    public bool wallClimb = false, waitGameOver = false;

    void Start() {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        cam = Camera.main.GetComponent<CameraController>();
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        moveSound = GetComponent<AudioSource>();

        RotationEdge = rend.bounds.min;
        currentPos = rb.position;
    }

    // Update is called once per frame
    void Update() {
        ControlPlayer();
    }

    void ControlPlayer() {
        if (!_gameManager.GameOver) {
            if (Input.GetButtonDown("Left")) {
                FindCurrentSide();
                if (transform.position.x == currentPos.x) {
                    moveSound.Play();
                    MovePlayer(-laneWideness);
                    StartCoroutine("RotateLeft");
                }
            }

            if (Input.GetButtonDown("Right")) {
                FindCurrentSide();
                if (transform.position.x == currentPos.x) {
                    moveSound.Play();
                    MovePlayer(laneWideness);
                    StartCoroutine("RotateRight");
                }
            }
        } else {
            // Wait half second before input to avoid players spam pressing to a new scene
            StartCoroutine(GameOverDelay());
            if (waitGameOver) {
                if (Input.GetButtonDown("Left")) {
                    _gameManager.RestartGame();
                }

                if (Input.GetButtonDown("Right")) {
                    _gameManager.ViewHighScores();
                }
            }
        }
    }

    //Change side based on input, changes side to left <-> right depening on button pressed
    void ChangeSide(SIDE left, SIDE right) {
        if (Input.GetButtonDown("Left")) {
            currentSide = left;
        }
        if (Input.GetButtonDown("Right")) {
            currentSide = right;
        }

        if (!wallClimb) {
            wallClimb = true;
        }

        cam.Rotate();
    }

    //If player is in specific position check for input and change side.
    void FindCurrentSide() {
        if (currentPos.x == 6 && currentPos.y == -6) {
            ChangeSide(SIDE.BOTTOM, SIDE.RIGHT);
        }
        if (currentPos.x == -6 && currentPos.y == -6) {
            ChangeSide(SIDE.LEFT, SIDE.BOTTOM);
        }
        if (currentPos.x == 6 && currentPos.y == 6) {
            ChangeSide(SIDE.RIGHT, SIDE.TOP);
        }
        if (currentPos.x == -6 && currentPos.y == 6) {
            ChangeSide(SIDE.TOP, SIDE.LEFT);
        }
    }

    //Move player based on side
    void MovePlayer(float moveDistance) {

        //Reset when full angle.
        if (angle == 360 || angle == -360) { angle = 0; }

        //Changes position based on side.
        switch (currentSide) {
            case SIDE.BOTTOM:
                currentPos.x += moveDistance;
                break;
            case SIDE.TOP:
                currentPos.x -= moveDistance;
                break;
            case SIDE.LEFT:
                currentPos.y -= moveDistance;
                break;
            case SIDE.RIGHT:
                currentPos.y += moveDistance;
                break;
        }
    }

    IEnumerator GameOverDelay() {
        yield return new WaitForSeconds(0.5f);
        waitGameOver = true;
    }

    //fix and clean this shit
    IEnumerator RotateLeft() {
        //Set angle of rotation
        angle += 90;

        //Set rotation edge.
        switch (currentSide) {
            case (SIDE.BOTTOM):
                RotationEdge = rend.bounds.min;
                break;
            case (SIDE.LEFT):
                RotationEdge = rend.bounds.min;
                RotationEdge.y += 1;
                break;
            case (SIDE.RIGHT):
                RotationEdge = rend.bounds.max;
                RotationEdge.y -= 1;
                break;
            case (SIDE.TOP):
                RotationEdge = rend.bounds.max;
                break;
        }

        //Rotate cube.
        while (DefineDirection()) {
            transform.RotateAround(RotationEdge, Vector3.forward, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        //Set final position and rotation.
        transform.position = currentPos;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    IEnumerator RotateRight() {
        //Set angle of rotation
        angle -= 90;

        //Set rotation edge.
        switch (currentSide) {
            case (SIDE.BOTTOM):
                RotationEdge = rend.bounds.min;
                RotationEdge.x += 1;
                break;
            case (SIDE.LEFT):
                RotationEdge = rend.bounds.min;
                break;
            case (SIDE.RIGHT):
                RotationEdge = rend.bounds.max;
                break;
            case (SIDE.TOP):
                RotationEdge = rend.bounds.max;
                RotationEdge.x -= 1;
                break;
        }

        //Rotate cube.
        while (DefineDirection()) {
            transform.RotateAround(RotationEdge, Vector3.forward, -rotationSpeed * Time.deltaTime);
            yield return null;
        }

        //Set final position and rotation.
        transform.position = currentPos;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    //Used for rotation.
    bool DefineDirection() {
        if (transform.position.x > currentPos.x + 0.1) {
            return true;
        } else if (transform.position.y < currentPos.y - 0.1) {
            return true;
        } else if (transform.position.y > currentPos.y + 0.1) {
            return true;
        } else if (transform.position.x < currentPos.x - 0.1) {
            return true;
        } else {
            return false;
        }
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "Obstacle" && !_gameManager.GameOver) {
            _gameManager.EndGame();
        }
    }
}
