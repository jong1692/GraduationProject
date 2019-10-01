using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance
    {
        get { return instance; }
    }

    private static PlayerInput instance;

    private Vector3 movement;
    private Vector3 camMovement;

    private bool attack;
    private bool pause;
    private bool jump;

    [SerializeField]
    private float attackInputDuration = 0.03f;

    WaitForSeconds attackInputWait;

    public bool IsMoveInput
    {
        get { return !Mathf.Approximately(movement.sqrMagnitude, 0f); }
    }

    public Vector3 Movement
    {
        get { return movement; }
    }

    public Vector3 CamMovement
    {
        get { return camMovement; }
    }

    public bool Attack
    {
        get { return attack; }
    }

    public bool Pause
    {
        get { return pause; }
    }

    public bool Jump
    {
        get { return jump; }
    }

    void Awake()
    {
        attackInputWait = new WaitForSeconds(attackInputDuration);

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            throw new UnityException("Cannot be more than one PlayerInput Script");
        }
    }

    void Update()
    {
        receivePlayerInput();
    }

    private void receivePlayerInput()
    {
        movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        camMovement = new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y"));

        jump = Input.GetButtonDown("Jump");
        pause = Input.GetButtonDown("Pause");

        if (Input.GetButtonDown("Attack"))
        {
            StartCoroutine(attackWait());
        }

    }

    IEnumerator attackWait()
    {
        attack = true;

        yield return attackInputWait;

        attack = false;
    }

}
