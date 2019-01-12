using UnityEngine;
using System.Collections;

public class ConsoleControl : MonoBehaviour
{
    public DeveloperConsole cachedDC;

    void Start()
    {
        DeveloperConsole.ManualInit(cachedDC);
    }

    void Update()
    {
        GameObject selObj = UICamera.selectedObject;
        if ((selObj == null || (selObj != null && (!selObj.GetComponent<UIInput>() || selObj.transform.root.GetComponent<DeveloperConsole>()))) && (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.F12)))
        {
            DeveloperConsole.ToggleConsole();
        }
    }
}