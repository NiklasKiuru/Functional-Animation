using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NativeMultiAnimationTest : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    [SerializeField] private int _count;
    private void Start()
    {
        var posX = 0f;
        for(int i = 0; i < _count; i++)
        {
            Instantiate(_target, new Vector3(posX, 0, 0), Quaternion.identity);
            posX += 1;
        }
    }
}
