using UnityEngine;
using Aikom.FunctionalAnimation;
using System.Collections.Generic;
using Unity.Mathematics;
using Aikom.FunctionalAnimation.Extensions;

public class CallbackHandleTest : MonoBehaviour
{
    [SerializeField] private int _amount;

    private List<IInterpolatorHandle<float>> _handles = new();
    private IInterpolatorHandle<float3> _vectorHandle;
    private int _targetIndex = 0;
    float _val = 0;

    private void Start()
    {
        for(int i = 0; i < _amount; i++)
        {
            var handle = EF.Create(0, 1, 3, Function.Linear)
                .OnUpdate(this, (v) => _val += v)
                .OnComplete(this, Test);

            _handles.Add(handle);
        }
        var pausePos = Vector3.zero;
        var newObj = new GameObject("Test");
        _vectorHandle = newObj.transform.TransitionLoop(TransformProperty.Position, Function.EaseOutExp, 3, Vector3.one)
            .OnComplete(this, (v) => { 
                var newVec = new Vector3(v.x, transform.position.y, v.z);
                transform.position = newVec;
            });
            
    }

    private void Test(float t)
    {
        //Debug.Log(t);
    }

    private unsafe void Update()
    {   
        if(Input.GetMouseButtonDown(0))
        {   
            _handles[_targetIndex].Complete();
            _targetIndex++;
            _targetIndex = Mathf.Clamp(_targetIndex, 0, _amount);
        }
            
    }

}
