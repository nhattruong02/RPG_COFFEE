using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] CharacterAnimation anim;
    public Interactable focus;
    // Using LayerMask to ignore NotWalkable
    public LayerMask movementMask;
    Camera cam;
    PlayerMotor motor;
    bool _isMove = true;
    bool _isSitting;
    Interactable interactable;
    Collider collider;
    [SerializeField] bool _isMobile;
    [SerializeField] FixedJoystick _joystick;

    [SerializeField] GameObject _uiMobile;
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] int _speed;
    [SerializeField] GameObject _sitDownUI;
    void Start()
    {
        cam = Camera.main;
        motor = GetComponent<PlayerMotor>();
        anim = GetComponent<CharacterAnimation>();
        collider = GetComponent<Collider>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!_isMobile)
        {
            _uiMobile.SetActive(false);
            if (Input.GetMouseButtonDown(0))
            {
                anim.StandUp(false);
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100))
                {
                    interactable = hit.collider.GetComponent<Interactable>();
                    if (interactable != null)
                    {
                        SetFocus(interactable);
                        motor.MoveToPoint(interactable.interactionTransform.position);
                        StartCoroutine(SitDown());
                    }
                    if (interactable == null)
                    {
                        if (_isSitting)
                        {
                            anim.SitDown(false);
                        }
                        motor.MoveToPoint(hit.point);
                    }
                }

            }
        }
        if (_isMobile)
        {
            _uiMobile.SetActive(true);
            motor.Agent.ResetPath();
            if (_isMove)
            {
                float horizontalMove = _joystick.Horizontal * _speed;
                float verticalMove = _joystick.Vertical * _speed;
                _rigidbody.velocity = new Vector3(horizontalMove, 0, verticalMove);
                if (_joystick.Horizontal != 0 || _joystick.Vertical != 0)
                {
                    anim.StandUp(false);
                    transform.rotation = Quaternion.LookRotation(_rigidbody.velocity);
                    anim.MoveJoyStick(_speed);

                }
                else
                {
                    _rigidbody.velocity = Vector3.zero;
                }
            }
        }
    }



    public void SetFocus(Interactable newFocus)
    {
        if (newFocus != focus)
        {
            if (focus != null)
                focus.OnDefocused();
            focus = newFocus;
        }
        newFocus.OnFocused(transform);
    }

    IEnumerator SitDown()
    {
        yield return new WaitUntil(() => motor.EndOfPath() == true);
        this.collider.enabled = false;
        this.transform.rotation = interactable.interactionTransform.rotation;
        anim.SitDown(true);
        _isSitting = true;
    }

    public void SitDownMobile()
    {
        StartCoroutine(SitDownMobileCoroutine());
    }

    IEnumerator SitDownMobileCoroutine()
    {
        motor.MoveToPoint(interactable.interactionTransform.position);
        yield return new WaitUntil(() => motor.EndOfPath() == true);
        _isMove = false;
        interactable.checkIsEmpty(false);
        this.collider.enabled = false;
        this.transform.rotation = interactable.interactionTransform.rotation;
        anim.SitDown(true);
        _isSitting = true;
        motor.Agent.ResetPath();
    }


    public void StandUp()
    {
        if (_isSitting)
        {
            this.collider.enabled = true;
            interactable.checkIsEmpty(true);
            anim.SitDown(false);
            anim.StandUp(true);
            _isMove = true;
            _isSitting = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isMobile)
        {
            if (other.CompareTag(Common.Chair))
            {
                interactable = other.GetComponent<Interactable>();
                if (interactable != null)
                {
                    _sitDownUI.SetActive(true);
                    SetFocus(interactable);
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        _sitDownUI.SetActive(false);
    }
}
