using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour
{
    public Vector3 followOffset = Vector3.zero;
    public bool followX = true;
    public bool followY = true;
    public bool followZ = true;

    private Transform _targ = null;
    private Transform target
    {
        get
        {
            if (_targ == null && GeneralVariables.player != null)
            {
                _targ = GeneralVariables.player.transform;
            }

            return _targ;
        }
    }

    private Transform tr;

    void Start()
    {
        tr = transform;
    }

    void Update()
    {
        if (target == null)
        {
            return;
        }

        Vector3 finalPos = new Vector3((followX) ? target.position.x : tr.position.x, (followY) ? target.position.y : tr.position.y, (followZ) ? target.position.z : tr.position.z);
        tr.position = finalPos + followOffset;
    }
}