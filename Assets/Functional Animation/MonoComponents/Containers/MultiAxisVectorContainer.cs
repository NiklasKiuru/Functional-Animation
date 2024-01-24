using System;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class MultiAxisVectorContainer : VectorContainerBase
    {
        [SerializeField] private FloatContainer _x;
        [SerializeField] private FloatContainer _y;
        [SerializeField] private FloatContainer _z;

        internal override Vector3 IncrimentValue(float time, Vector3 startval, Vector3 endVal)
        {
            for(int i = 0; i < 3; i++)
            {
                if (Axis[i])
                {
                    switch(i)
                    {
                        case 0:
                            startval.x = _x.IncrimentValue(time, startval.x, endVal.x);
                            break;
                        case 1:
                            startval.y = _y.IncrimentValue(time, startval.y, endVal.y);
                            break;
                        case 2:
                            startval.z = _z.IncrimentValue(time, startval.z, endVal.z);
                            break;
                    }
                }
            }
            return startval;
        }

        protected override Func<float, float> GenerateEasingFunction()
        {
            return EditorFunctions.Funcs[Function.Linear];
        }

        protected override void OnInitialize(Vector3 start)
        {
            //base.OnInitialize(start);
            //_x = new FloatContainer(start.x, Target.x, Duration, null, );
            //_y = new FloatContainer();
            //_z = new FloatContainer();
        }
    }

}
