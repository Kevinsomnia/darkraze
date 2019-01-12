using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UILabel))]
public class PlayerNameGUI : MonoBehaviour
{
    public string prefix = "VITALS PANEL ";

    private UILabel label;

    void Start()
    {
        label = GetComponent<UILabel>();

        if (Topan.Network.isConnected)
        {
            label.text = prefix + "[FF5040][" + AccountManager.profileData.username.ToUpper() + "][-]";
        }
        else
        {
            label.text = prefix + "[FF5040][INFILTRATOR][-]";
        }
    }
}