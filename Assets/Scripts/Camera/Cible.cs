using UnityEngine;
using UnityEngine.Events;
#nullable enable

public class Cible : MonoBehaviour
{
    public static Transform? current;
    public static UnityEvent<Transform?> cibleChanged = new UnityEvent<Transform?>();
    public LayerMask layerMask;
    public KeyCode deselectKey;
    private Transform? currentOutlineTransform;
    // used to prevent clicking through UI elements.
    private bool isOverUI = false;
    private void Awake()
    {
        if (cibleChanged == null)
            cibleChanged = new UnityEvent<Transform?>();
    }
    // Update is called once per frame
    void Update()
    {
        if (MenuPrincipal.isActive) return;
        // used to prevent clicking through UI elements.
        isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        // deselect if key is pressed
        if (Input.GetKey(deselectKey))
        {
            current = null;
            cibleChanged.Invoke(null);
        }

        if (isOverUI) return;

        Transform hitTransform = RaycastForCelestialBody();
        if (Input.GetMouseButtonDown(0) && current != hitTransform)
        {
            if (hitTransform != null && hitTransform.gameObject.CompareTag("Axis")) return;
            current = hitTransform;
            cibleChanged.Invoke(current);
            
        }
        UpdateOutline(hitTransform);
    }

    Transform RaycastForCelestialBody()
    {
        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, float.MaxValue, layerMask);
        return hit.transform;
    }
    void UpdateOutline(Transform? hitTransform)
    {
        if (current != null)
        {
            SetOutline(current);
            return;
        }
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            SetOutline(null);
            return;
        }
        SetOutline(hitTransform);
    }
    void SetOutline(Transform? outlineTransform)
    {
        if (currentOutlineTransform != null && (currentOutlineTransform != outlineTransform || outlineTransform == null))
        {
            Destroy(currentOutlineTransform.GetComponent<Outline>());
            currentOutlineTransform = null;
        }

        if (outlineTransform == null || currentOutlineTransform == outlineTransform) return;
        outlineTransform.gameObject.AddComponent<Outline>();
        currentOutlineTransform = outlineTransform;
    }
}
