using UnityEngine;

public class MenuPrincipal : MonoBehaviour
{
    public static bool isActive = false;


    [SerializeField] private UpdateMenu menuPrincipalButton;
    private GameObject cover;
    private void Awake()
    {
        cover = transform.Find("Cover").gameObject;
    }
    private void Start()
    {
        if (menuPrincipalButton != null)
        {
            menuPrincipalButton.stateChanged.AddListener(OnMenuPrincipalStateChanged);
        }
    }
    private void OnMenuPrincipalStateChanged(bool isActive)
    {
        MenuPrincipal.isActive = isActive;
       cover.SetActive(isActive);
        // Vous pouvez également ajouter d'autres actions ici si nécessaire
    }
}
