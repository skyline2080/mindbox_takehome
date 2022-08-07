using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq;
using Shape_nonconventional;

// ниже набросок чуть менее масштабируемого решения в части добавления новых фигур 
// надеюсь, статический конструктор union типа ниже позволит избежать основных ошибок при добавления новых фигур
// из несомненных плюсов - возможность работать со стэком 

Span<Shape> shapes = stackalloc Shape[] {
    Circle.WithRadius(1f),
    RightTriangle.WithLegs(5f, 4f, 3f)
};

var area_total = 0f;
for (int i = 0; i < shapes.Length; ++i)
    area_total +=
        shapes[i].kind switch {
            Shape.Kind.Circle           => ((Circle)shapes[i]).Area(),
            Shape.Kind.RightTriangle    => ((RightTriangle)shapes[i]).Area(),
            _ => throw new Exception("shape of unknown type")
        };

Console.WriteLine(area_total);

// да и при работе с обычными массивами получаем куда лучшее memory locality и меньшее кол-во cache misses

var other_shapes = new Shape[] {
    Circle.WithRadius(1f),
    RightTriangle.WithLegs(5f, 4f, 3f)
};



// ------------------------------------------------------------------------------------------------------
// вот и сам небольшой framework


namespace Shape_nonconventional {

class ShapeAttribute: Attribute {
    public readonly static string[] required_ops = {"Area"};
}

// union type для всех типов фигур
[StructLayout(LayoutKind.Explicit)]
struct Shape{

    // поскольку у нас тут по сути нарисовался небольшой framework
    // статический конструктор ниже проверяет наличие всех необходимых элементов 
    // надеюсь, это позволит избежать ошибок при добавлении новых фигур

    static Shape() {
        var all_shape_types = 
            Assembly
            .GetAssembly(typeof(Shape_nonconventional.ShapeAttribute))
            ?.GetTypes()
            ?.Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(ShapeAttribute)))
            ?? throw new Exception("presumably should not be here");

        var all_shape_types_have_required_ops =
            all_shape_types
                .All(t => { 
                        var ops_at_hand  = t.GetMembers().Select(t => t.Name).ToHashSet();
                        var required_ops = ShapeAttribute.required_ops.ToHashSet();
                        return required_ops.IsSubsetOf(ops_at_hand);
                    }
                );

        if (! all_shape_types_have_required_ops)
            throw new ArgumentException(
                $"\npls check your shape types {String.Concat(all_shape_types.Select(t => '\n' + t.Name))}" +
                $"\nto include required ops {String.Concat(ShapeAttribute.required_ops.Select(m => '\n' + m))}" +
                $"\nspecified in {typeof(ShapeAttribute).Name}");

        var all_shape_types_have_implicit_conv_method_declared_in_union =
                typeof(Shape)
                .GetMethods()
                .Where(m => m.IsStatic)
                .Select(m => m.ReturnType.Name)
                .ToHashSet()
                .IsSupersetOf(all_shape_types.Select(t => t.Name).ToHashSet());
            
        if (! all_shape_types_have_implicit_conv_method_declared_in_union)
            throw new ArgumentException(
                $"\npls check your {typeof(Shape).Name} union to include implicit type conv" +
                $"\nfor all shape types {String.Concat(all_shape_types.Select(t => '\n' + t.Name))}");


        var all_shapetypes_are_present_in_shapekind_enum = 
            typeof(Shape.Kind)
            .GetMembers()
            .Where(m => m.MemberType == MemberTypes.Field)
            .Select(m => m.Name)
            .ToHashSet().IsSupersetOf(all_shape_types.Select(t => t.Name).ToHashSet());

        if (! all_shapetypes_are_present_in_shapekind_enum)
            throw new ArgumentException(
                $"\npls check your {typeof(Shape.Kind).Name} enum to include" +
                $"\nall shape types {String.Concat(all_shape_types.Select(t => '\n' + t.Name))}");

    }

    public enum Kind: int {
        RightTriangle = 1,
        Circle,
    }

    // в плане memory layout - типичный union type
    [FieldOffset(0)]             internal Kind kind;
    [FieldOffset(sizeof(Kind))]  internal RightTriangle right_triangle;
    [FieldOffset(sizeof(Kind))]  internal Circle circle;

    public Kind ShapeKind => kind;

    public static implicit operator RightTriangle(in Shape shape) =>
        shape.kind == Kind.RightTriangle
        ? shape.right_triangle
        : throw new ArgumentException("Contained shape is not RightTriangle"); 

    public static implicit operator Circle(in Shape shape) =>
        shape.kind == Kind.Circle
        ? shape.circle
        : throw new ArgumentException("Contained shape is not Circle");

}

// минимальная реализация 2 типов фигур

[Shape]
[StructLayout(LayoutKind.Auto)]
struct RightTriangle {
    public static Shape WithLegs(float hypot, float cat1, float cat2) => 
        new Shape {
            kind = Shape.Kind.RightTriangle, 
            right_triangle = new RightTriangle(hypot, cat1, cat2)
        };    

    public readonly float Hypot, Cat1, Cat2;
    public float Area() => 0.5f * Cat1 * Cat2;

    RightTriangle(float hypot, float cat1, float cat2) =>
        (Hypot, Cat1, Cat2) = (hypot, cat1, cat2);   

}

[Shape]
[StructLayout(LayoutKind.Auto)]
struct Circle {
    public static Shape WithRadius(float radius) => 
        new Shape {
            kind = Shape.Kind.Circle,
            circle = new Circle(radius)
        };

    public readonly float Radius;
    public float Area() => MathF.PI * Radius * Radius;

    Circle(float radius) => Radius = radius;

}

}