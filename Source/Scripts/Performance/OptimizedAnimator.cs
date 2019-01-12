using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OptimizedAnimator
{
    public Animator myAnimator;
    public Dictionary<string, object> keys = new Dictionary<string, object>();

    public OptimizedAnimator(Animator an)
    {
        myAnimator = an;
    }

    public void SetFloat(string key, float value)
    {
        if (!keys.ContainsKey(key))
        {
            keys[key] = value;
            myAnimator.SetFloat(key, value);
            return;
        }

        if ((float)keys[key] != value)
        {
            keys[key] = value;
            myAnimator.SetFloat(key, value);
        }

    }

    public void SetBool(string key, bool value)
    {

        if (!keys.ContainsKey(key))
        {
            keys[key] = value;
            myAnimator.SetBool(key, value);
            return;
        }

        if ((bool)keys[key] != value)
        {
            keys[key] = value;
            myAnimator.SetBool(key, value);
        }

    }

    public void SetInteger(string key, int value)
    {

        if (!keys.ContainsKey(key))
        {
            keys[key] = value;
            myAnimator.SetInteger(key, value);
            return;
        }

        if ((int)keys[key] != value)
        {
            keys[key] = value;
            myAnimator.SetInteger(key, value);
        }
    }
}