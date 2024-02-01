using Aikom.FunctionalAnimation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StressTest : MonoBehaviour
{
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private int _count = 1000;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private Vector3 _startPos = Vector3.zero;

    // **TEST**
    [SerializeField] private GraphData _graphData;
    [SerializeField] private ScriptableTransformAnimator _animator;

    private void Start()
    {   
        int rowSize = Mathf.CeilToInt(Mathf.Sqrt(_count));

        for (int i = 0; i < rowSize; i++)
        {   
            float y = _startPos.y + i * _radius;
            for (int j = 0; j < rowSize; j++)
            {
                float x = _startPos.x + j * _radius;
                var copy = Instantiate(_cubePrefab, new Vector3(x, y, 0), Quaternion.identity);
                copy.transform.SetParent(null);
                _animator.AddGroupTarget(copy.transform);
            }
        }

        _animator.Play("Cool Cube");
    }
}
