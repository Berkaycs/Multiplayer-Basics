using UnityEngine;
using Unity.Netcode;

public class CameraFollow : NetworkBehaviour
{
    private Camera _camera;
    private Vector3 _offsetFromPlayer;
    private Vector3 _originPosition;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _camera = Camera.main;
            _offsetFromPlayer = transform.position - _camera.transform.position;
            _originPosition = _camera.transform.position;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && _camera != null)
        {
            _camera.transform.position = _originPosition;
        }
    }

    private void LateUpdate()
    {
        if (IsOwner && _camera != null)
        {
            _camera.transform.position = transform.position - _offsetFromPlayer;
        }
    }
}
