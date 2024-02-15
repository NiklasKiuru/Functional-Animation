namespace Aikom.FunctionalAnimation.Utility
{   
    /// <summary>
    /// 2D function vector
    /// </summary>
    public struct Func2
    {
        public Function X;
        public Function Y;

        public Func2(Function x, Function y)
        {
            X = x; Y = y;
        }

        public Function this[int axis]
        {
            get
            {
                return axis switch
                {
                    0 => X,
                    1 => Y,
                    _ => throw new System.ArgumentOutOfRangeException(),
                };
            }
        }
    }
}