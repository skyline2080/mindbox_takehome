namespace Shape;

// решил использовать float, т.к. double предоставляет избыточную в данном случае точность

public interface IArea {
    float Area {get; }
}

public sealed class Circle: IArea {
    public static Circle WithRadius(float radius) => new Circle(radius);
    public float Radius {get; }
    public float Area => MathF.PI * Radius * Radius;

    Circle(float radius) => 
        Radius = 
            radius > 0 
            ? radius 
            : throw new ArgumentException("radius cannot be negatibve or zero");

}

public class Triangle: IArea {
    public static Triangle WithLegs(float leg1, float leg2, float leg3) =>
        is_proper_triangle(leg1, leg2, leg3)
        ? is_right_triangle(leg1, leg2, leg3)
            ? RightTriangle.WithLegs (leg1, leg2, leg3)
            : new Triangle (leg1, leg2, leg3) 
        : throw new ArgumentException("passed arguments do not represent a proper triangle");

    public float Leg1 {get; }
    public float Leg2 {get; }
    public float Leg3 {get; }
    public virtual float Area {
        get {
            var p = (Leg1 + Leg2 + Leg3) / 2;
            return 
                MathF.Sqrt(
                    (p) * 
                    (p - Leg1) * 
                    (p - Leg2) * 
                    (p - Leg3));
        }
    }

    protected Triangle(float leg1, float leg2, float leg3) {
        if (! is_proper_triangle(leg1, leg2, leg3))
            throw new ArgumentException("passed arguments do not represent a proper triangle");
        
        Leg1 = leg1;
        Leg2 = leg2;
        Leg3 = leg3;
    }

    protected static bool is_proper_triangle(float leg1, float leg2, float leg3) {
        return
            (leg1 > 0) &&
            (leg2 > 0) &&
            (leg3 > 0) && 
            (leg1 < leg2 + leg3) &&
            (leg2 < leg1 + leg3) &&
            (leg3 < leg1 + leg2);
    }


    protected static bool is_right_triangle(float leg1, float leg2, float leg3) {
        return 
            is_proper_triangle(leg1, leg2, leg3)
            ?   MathF.Abs(leg1*leg1 - leg2*leg2 - leg3*leg3) < Single.Epsilon ||
                MathF.Abs(leg2*leg2 - leg1*leg1 - leg3*leg3) < Single.Epsilon ||
                MathF.Abs(leg3*leg3 - leg1*leg1 - leg2*leg2) < Single.Epsilon
            :   throw new ArgumentException("passed arguments do not represent a proper triangle");        
    }
}

public sealed class RightTriangle: Triangle, IArea {
    public static new RightTriangle WithLegs(float leg1, float leg2, float leg3) => 
        new RightTriangle(leg1, leg2, leg3);
    
    public float Hypot {get; } 
    public float Cat1  {get; } 
    public float Cat2  {get; }
    public override float Area => Cat1 * Cat2 / 2;

    RightTriangle(float leg1, float leg2, float leg3)
        : base (leg1, leg2, leg3) {
        
        if (! Triangle.is_right_triangle(leg1, leg2, leg3))
            throw new ArgumentException("passed arguments do not represent a right triangle");

        Span<float> mem = stackalloc float[3] {leg1, leg2, leg3};
        mem.Sort();
        
        Hypot = mem[2];
        Cat1  = mem[1];
        Cat2  = mem[0];
    
    }
}