namespace Aikom.FunctionalAnimation.Utility
{   
    /// <summary>
    /// 3D function vector
    /// </summary>
    public struct Func3
    {
        public Function X;
        public Function Y;
        public Function Z;

        public Func3(Function x, Function y, Function z)
        {
            X = x; Y = y; Z = z;
        }

        public Function this[int axis]
        {
            get
            {
                return axis switch
                {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    _ => throw new System.ArgumentOutOfRangeException(),
                };
            }
        }
    }
}
