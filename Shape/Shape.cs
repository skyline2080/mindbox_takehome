using System;

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

    public float Leg1 { get => _leg1; }
    public float Leg2 { get => _leg2; }
    public float Leg3 { get => _leg3; }
    public virtual float Area {
        get {
            var p = (_leg1 + _leg2 + _leg3) / 2;
            return 
                MathF.Sqrt(
                    (p) * 
                    (p - _leg1) * 
                    (p - _leg2) * 
                    (p - _leg3));
        }
    }

    protected float _leg1, _leg2, _leg3;

    protected Triangle(float leg1, float leg2, float leg3) {
        if (! is_proper_triangle(leg1, leg2, leg3))
            throw new ArgumentException("passed arguments do not represent a proper triangle");
        
        _leg1 = leg1;
        _leg2 = leg2;
        _leg3 = leg3;
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
    public float Hypot { get => _leg1; } 
    public float Cat1  { get => _leg2; } 
    public float Cat2  { get => _leg3; }
    public override float Area => Cat1 * Cat2 / 2;

    internal static new RightTriangle WithLegs(float leg1, float leg2, float leg3) => 
        new RightTriangle(leg1, leg2, leg3);
    
    RightTriangle(float leg1, float leg2, float leg3)
        : base (leg1, leg2, leg3) 
    {
        if (! Triangle.is_right_triangle(leg1, leg2, leg3))
            throw new ArgumentException("passed arguments do not represent a right triangle");

        Span<float> mem = stackalloc float[3] {leg1, leg2, leg3};
        mem.Sort();
        
        _leg1 = mem[2];
        _leg2 = mem[1];
        _leg3 = mem[0];
    
    }
}