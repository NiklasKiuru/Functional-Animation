namespace Aikom.FunctionalAnimation.Utility
{   
    /// <summary>
    /// 4D function vector
    /// </summary>
    public struct Func4
    {
        public Function X; 
        public Function Y; 
        public Function Z; 
        public Function W;

        public Func4(Function x, Function y, Function z, Function w)
        {
            X = x; Y = y; Z = z; W = w;
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
                    3 => W,
                    _ => throw new System.ArgumentOutOfRangeException(),
                };
            }
        }
    }
}

