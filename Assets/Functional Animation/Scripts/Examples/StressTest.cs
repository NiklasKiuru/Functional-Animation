using Aikom.FunctionalAnimation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animator = Aikom.FunctionalAnimation.Animator;

public class StressTest : MonoBehaviour
{
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private int _count = 1000;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private Vector3 _startPos = Vector3.zero;
    [SerializeField] private TransformAnimation _anim;

    private void Start()
    {   
        int rowSize = Mathf.CeilToInt(Mathf.Sqrt(_count));
        var list = new List<Transform>();

        for (int i = 0; i < rowSize; i++)
        {   
            float z = _startPos.z + i * _radius;
            for (int j = 0; j < rowSize; j++)
            {
                float x = _startPos.x + j * _radius;
                var copy = Instantiate(_cubePrefab, new Vector3(x, 2, z), Quaternion.identity);
                copy.transform.SetParent(null);
                list.Add(copy.transform);
            }
        }

        Animator.CreateTransformGroup("StressTest", _anim, 16, list);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var pos = ray.origin + ray.direction * 10;
            pos.z = 0;
            var copy = Instantiate(_cubePrefab, pos, Quaternion.identity);
            Animator.AddToTransformGroup(copy.transform, "StressTest");
        }

        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hitInfo);
            if (hitInfo.collider != null)
                Animator.RemoveFromTransformGroup(hitInfo.collider.transform, "StressTest");
        }
    }
}
