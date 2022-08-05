using Xunit;
using Shape;
using System;

namespace Shape.Tests;

public class UnitTest1
{
    [Fact]
    public void TestNegativeArgsThrow() {
        Assert.Throws<ArgumentException>(() => Circle.WithRadius(-10f));
        Assert.Throws<ArgumentException>(() => Triangle.WithLegs(-10f, 3f, 4f));
    }

    [Fact]
    public void TestTriangles() {
        Triangle
            tr1 = Triangle.WithLegs(3f, 4f, 5f),
            tr2 = Triangle.WithLegs(2f, 3f, 4f);

        Assert.True(tr1 is RightTriangle);
        Assert.True(tr2 is Triangle);
        Assert.True(tr1.Area - 6f < Single.Epsilon);
        Assert.True(tr2.Area - 2.9047375096555625f < Single.Epsilon);
    }

    [Fact]
    public void TestCircle() {
        Assert.True(Circle.WithRadius(3).Area - 29.608813203268074f < Single.Epsilon);        
    }
}