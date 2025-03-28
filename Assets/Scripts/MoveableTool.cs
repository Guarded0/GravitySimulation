using UnityEngine;

public class MoveTool : MonoBehaviour
{
    public GameObject axisGizmoPrefab;
    private GameObject axisGizmoInstance;
    
    private Transform selectedObject;
    private Camera mainCamera;
    private bool isBeingDragged = false;

    private enum Axis { None, X, Y, Z }
    private Axis currentAxis = Axis.None;

    private Plane movementPlane;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleSelection();

        if (isBeingDragged && selectedObject != null)
        {
            Vector3 newMousePosition = GetMouseWorldPosition();
            selectedObject.position = ConstrainToAxis(newMousePosition);
        }

        if (Input.GetMouseButtonUp(0)) // Release object or axis
        {
            isBeingDragged = false;
        }
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Celestial Body"))
                {
                    selectedObject = hit.transform;
                    SpawnGizmo();
                }
                else if (hit.transform.CompareTag("Axis")) 
                {
                    SelectAxis(hit.transform);
                }
                else 
                {
                    DeselectObject();
                }
            }
            else 
            {
                DeselectObject();
            }
        }
    }

    private void SpawnGizmo()
    {
        // Destroy old gizmo if it exists
        if (axisGizmoInstance != null)
            Destroy(axisGizmoInstance);

        if (selectedObject == null)
            return;

        // Instantiate the gizmo at the selected object's exact world position
        axisGizmoInstance = Instantiate(axisGizmoPrefab, selectedObject.position, Quaternion.identity);

        axisGizmoInstance.transform.SetParent(selectedObject, true);
    }

    private void SelectAxis(Transform axisTransform) {
        isBeingDragged = true; // Start moving object

        // Determine which axis was selected
        if (axisTransform.name.Equals("Arrow_X")) { 
            currentAxis = Axis.X;
            movementPlane = new Plane(Vector3.up, axisTransform.position);
        } else if (axisTransform.name.Equals("Arrow_Y")) {
            currentAxis = Axis.Y;
            movementPlane = new Plane(Vector3.forward, axisTransform.position);
        } else if (axisTransform.name.Equals("Arrow_Z")) {
            currentAxis = Axis.Z;
            movementPlane = new Plane(Vector3.up, axisTransform.position);
        }
    }

    private void DeselectObject()
    {
        selectedObject = null;
        DestroyGizmo();
    }

    private void DestroyGizmo()
    {
        if (axisGizmoInstance != null)
        {
            Destroy(axisGizmoInstance);
            axisGizmoInstance = null;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (movementPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        return selectedObject != null ? selectedObject.position : Vector3.zero;
    }

    private Vector3 ConstrainToAxis(Vector3 position)
    {
        if (selectedObject == null) return position;

        Vector3 objectPosition = selectedObject.position;

        switch (currentAxis)
        {
            case Axis.X:
                return new Vector3(position.x, objectPosition.y, objectPosition.z);
            case Axis.Y:
                return new Vector3(objectPosition.x, position.y, objectPosition.z);
            case Axis.Z:
                return new Vector3(objectPosition.x, objectPosition.y, position.z);
            default:
                return objectPosition;
        }
    }
}

