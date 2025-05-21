using NUnit.Framework.Constraints;
using UnityEngine;

public class MoveTool : MonoBehaviour
{
    public float scaleMultiplier;
    public GameObject axisGizmoPrefab;
    private GameObject axisGizmoInstance;

    private bool isBeingDragged = false;

    private enum Axis { None, X, Y, Z }
    private Axis currentAxis = Axis.None;

    private Plane movementPlane;

    public LayerMask rayMask;
    public bool isEnabled = false;


    void Update()
    {
        if (Cible.current != null)
        {
            SpawnGizmo();
        }
        else
        {
            DestroyGizmo();
        }
        if (!MenuPrincipal.isActive && Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }
        if (isBeingDragged && Cible.current != null)
        {
            Vector3 newMousePosition = GetMouseWorldPosition();
            Cible.current.position = ConstrainToAxis(newMousePosition);
        }

        if (axisGizmoInstance != null)
        {
            axisGizmoInstance.transform.position = Cible.current.position;

            float distance = Vector3.Distance(axisGizmoInstance.transform.position, Camera.main.transform.position);
            Vector3 arrowSize = Vector3.one * distance * scaleMultiplier;
            axisGizmoInstance.transform.Find("ScaleX").localScale = arrowSize;
            axisGizmoInstance.transform.Find("ScaleY").localScale = arrowSize;
            axisGizmoInstance.transform.Find("ScaleZ").localScale = arrowSize;


            float planetRadius = Cible.current.GetComponent<CelestialBody>().planetSettings.radius * 1.1f;
            axisGizmoInstance.transform.Find("ScaleX").position = Cible.current.position + new Vector3(planetRadius, 0, 0);
            axisGizmoInstance.transform.Find("ScaleY").position = Cible.current.position + new Vector3(0, planetRadius, 0);
            axisGizmoInstance.transform.Find("ScaleZ").position = Cible.current.position + new Vector3(0, 0, planetRadius);
            
        }

        if (Input.GetMouseButtonUp(0)) // Release object or axis
        {
            isBeingDragged = false;
            MouvementCamera.softUnlock = false;
        }
    }

    private void HandleSelection()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, float.MaxValue, rayMask.value))
        {
            if (hit.transform.CompareTag("Axis"))
            {
                SelectAxis(hit.transform);
            }
        }
    }

    private void SpawnGizmo()
    {
        // Dont touch anything
        if (axisGizmoInstance != null)
        {
            return;
        }

        // Instantiate the gizmo at the selected object's exact world position
        axisGizmoInstance = Instantiate(axisGizmoPrefab, Cible.current.position, Quaternion.identity);
    }

    private void SelectAxis(Transform axisTransform)
    {
        isBeingDragged = true; // Start moving object
        MouvementCamera.softUnlock = true;
        // Determine which axis was selected
        if (axisTransform.name.Equals("Arrow_X"))
        {
            currentAxis = Axis.X;
            movementPlane = new Plane(Vector3.forward, axisTransform.position);
        }
        else if (axisTransform.name.Equals("Arrow_Y"))
        {
            currentAxis = Axis.Y;
            movementPlane = new Plane(Vector3.forward, axisTransform.position);
        }
        else if (axisTransform.name.Equals("Arrow_Z"))
        {
            currentAxis = Axis.Z;
            movementPlane = new Plane(Vector3.up, axisTransform.position);
        }
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (movementPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        return Cible.current != null ? Cible.current.position : Vector3.zero;
    }

    private Vector3 ConstrainToAxis(Vector3 position)
    {
        if (Cible.current == null) return position;

        Vector3 objectPosition = Cible.current.position;
        float offset = (axisGizmoInstance.transform.Find("ScaleX").Find("Arrow_X").Find("Tip").position - objectPosition).magnitude;
        switch (currentAxis)
        {
            case Axis.X:
                return new Vector3(position.x - offset, objectPosition.y, objectPosition.z);
            case Axis.Y:
                return new Vector3(objectPosition.x, position.y - offset, objectPosition.z);
            case Axis.Z:
                return new Vector3(objectPosition.x, objectPosition.y, position.z - offset);
            default:
                return objectPosition;
        }
    }
}

