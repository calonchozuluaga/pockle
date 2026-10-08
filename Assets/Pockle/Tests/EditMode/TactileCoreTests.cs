using System;
using NUnit.Framework;
using Pockle.Core;

namespace Pockle.Tests
{
    public sealed class TactileCoreTests
    {
        [TestCase(.65f)]
        [TestCase(1f)]
        [TestCase(2f)]
        public void SpringSettlesAtFifteenFramesPerSecond(float damping)
        {
            var spring = new Spring1D(3.5f, damping);
            spring.Target = 1f;
            for (int frame = 0; frame < 120; frame++)
                spring.Step(1f / 15f);

            Assert.That(spring.Value, Is.EqualTo(1f).Within(.0001f));
            Assert.That(spring.Velocity, Is.EqualTo(0f).Within(.0002f));
        }

        [Test]
        public void SpringConsumesHitchAndResetsVelocity()
        {
            var spring = new Spring1D();
            spring.Target = .8f;
            spring.Step(.04f);
            Assert.That(Math.Abs(spring.Velocity), Is.GreaterThan(.01f));
            spring.Step(20f);
            Assert.That(spring.Value, Is.EqualTo(.8f).Within(.000001f));
            spring.Reset(-.4f);
            Assert.That(spring.Value, Is.EqualTo(-.4f));
            Assert.That(spring.Target, Is.EqualTo(-.4f));
            Assert.That(spring.Velocity, Is.EqualTo(0f));
        }

        [Test]
        public void SquashAndStretchKeepBaseFixedAndChangeBodyInBothAxes()
        {
            Point3 rest = JellyShape.Rest((float)Math.PI * .5f, 0f);
            Point3 squash = JellyShape.Deform(rest, .42f, 0f, 0f, 0f, 0f, 0f);
            Point3 stretch = JellyShape.Deform(rest, 0f, .6f, 0f, 0f, 0f, 0f);
            Assert.That(squash.X, Is.GreaterThan(rest.X));
            Assert.That(squash.Y, Is.LessThan(rest.Y));
            Assert.That(stretch.X, Is.LessThan(rest.X));
            Assert.That(stretch.Y, Is.GreaterThan(rest.Y));

            Point3 anchor = new Point3(.1f, -1f, -.1f);
            Point3 deformed = JellyShape.Deform(anchor, .42f, .6f, 1f, -1f, .5f, -.5f);
            Assert.That(deformed.X, Is.EqualTo(anchor.X));
            Assert.That(deformed.Y, Is.EqualTo(-1f));
            Assert.That(deformed.Z, Is.EqualTo(anchor.Z));
        }

        [Test]
        public void InvalidInputDoesNotPoisonSpringOrShape()
        {
            var spring = new Spring1D(float.NaN, float.PositiveInfinity, float.NaN);
            spring.Target = 1f;
            spring.Target = float.NaN;
            spring.Step(float.PositiveInfinity);
            spring.Step(1f);
            Assert.That(float.IsNaN(spring.Value) || float.IsInfinity(spring.Value), Is.False);
            Assert.That(spring.Target, Is.EqualTo(1f));

            Point3 result = JellyShape.Deform(new Point3(float.NaN, float.PositiveInfinity, float.NaN),
                float.PositiveInfinity, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN);
            Assert.That(float.IsNaN(result.X) || float.IsInfinity(result.X), Is.False);
            Assert.That(float.IsNaN(result.Y) || float.IsInfinity(result.Y), Is.False);
            Assert.That(float.IsNaN(result.Z) || float.IsInfinity(result.Z), Is.False);
        }
    }
}
