using UnityEngine;
using System;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class MyPlayerInput : NetworkBehaviour
{
    public event Action OnPickUpPressed;
    public event Action OnInteractPressed;

    [Header("Movement")]
    [SerializeField] private InputActionReference _movementReference;
    [SerializeField] private float _smoothTime = 0.1f;

    private Vector2 _rawInput;
    private Vector2 _movementInput;

    public Vector2 MovementInput => _movementInput;

    private void Update()
    {
        if (!IsOwner) return;

        _rawInput = _movementReference.action.ReadValue<Vector2>();
        _movementInput = Vector2.MoveTowards(_movementInput, _rawInput, Time.deltaTime / _smoothTime);

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            OnPickUpPressed?.Invoke();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnInteractPressed?.Invoke();
        }
    }
}
