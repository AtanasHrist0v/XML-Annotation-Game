using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Annotate : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private LayerMask annotationsLayer;
    [SerializeField]
    private LayerMask objectsLayer;

    [SerializeField]
    private float pickupRange = 2f;
    private GameObject currentLookedAtObject = null;
    private GameObject originalParent = null;
    private Camera playerCamera = null;

    [Header("Debug")]
    public bool logTargetName = true;

    private void Awake()
    {
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
            Debug.LogError("No Main Camera found! Assign a camera reference.");
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        CastRay();
    }

    private void CastRay()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // TODO: don't always create and delete same vector

        if (currentLookedAtObject == null)
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, pickupRange, annotationsLayer))
            {
                currentLookedAtObject = hitInfo.collider.gameObject;
                originalParent = currentLookedAtObject.transform.parent.gameObject;

                currentLookedAtObject.transform.SetParent(playerCamera.transform);
                currentLookedAtObject.transform.localPosition = new Vector3(0.3f, -0.15f, 0.4f);
                currentLookedAtObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                currentLookedAtObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

                if (logTargetName)
                {
                    Debug.Log($"Looking at: {currentLookedAtObject.name}");
                    Debug.Log($"With parent: {originalParent.name}");
                }
            }
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, pickupRange, objectsLayer))
            {
                gameManager.AddAttempt();
                if (hitInfo.collider.gameObject != originalParent)
                {
                    Debug.Log("Wrong object!");
                    return;
                }

                Vector3 surfaceNormal = hitInfo.normal;
                Vector3 forward = Vector3.ProjectOnPlane(playerCamera.transform.position - hitInfo.point, surfaceNormal).normalized;

                currentLookedAtObject.transform.rotation = Quaternion.LookRotation(forward, surfaceNormal);
                currentLookedAtObject.transform.Rotate(-90f, 0f, 0f, Space.Self);

                if (hitInfo.point.y < playerCamera.transform.position.y)
                {
                    Debug.Log("make upright");
                    //currentLookedAtObject.transform.RotateAround(hitInfo.point, surfaceNormal, 180);
                    currentLookedAtObject.transform.Rotate(0f, 180f, 0f, Space.Self);
                }

                currentLookedAtObject.transform.position = hitInfo.point + surfaceNormal * 0.01f;
                currentLookedAtObject.transform.SetParent(hitInfo.collider.gameObject.transform);
                currentLookedAtObject.GetComponent<BoxCollider>().enabled = false;

                currentLookedAtObject = null;
                originalParent = null;

                gameManager.AddPoint();
            }
        }
    }

    private void PickupAnnotation()
    {

    }
}
