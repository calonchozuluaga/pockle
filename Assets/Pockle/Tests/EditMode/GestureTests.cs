using NUnit.Framework;
using Pockle.Core;

namespace Pockle.Tests
{
    public sealed class GestureTests
    {
        [TestCase(PointerTarget.Toy)]
        [TestCase(PointerTarget.Plate)]
        public void FirstSurfaceAndFingerKeepTheGestureUntilRelease(PointerTarget target)
        {
            var gesture = new PointerGesture();
            Assert.That(gesture.TryBegin(0, target), Is.True);
            Assert.That(gesture.TryBegin(0, target == PointerTarget.Toy ? PointerTarget.Plate : PointerTarget.Toy), Is.False);
            Assert.That(gesture.TryBegin(1, PointerTarget.Plate), Is.False);
            Assert.That(gesture.End(1), Is.False);
            Assert.That(gesture.Target, Is.EqualTo(target));
            Assert.That(gesture.End(0), Is.True);
            Assert.That(gesture.IsActive, Is.False);
        }

        [Test]
        public void CancelClearsCaptureAndAllowsANewSurface()
        {
            var gesture = new PointerGesture();
            Assert.That(gesture.TryBegin(-1, PointerTarget.Plate), Is.True);
            gesture.Cancel();
            Assert.That(gesture.PointerId, Is.EqualTo(PointerGesture.NoPointer));
            Assert.That(gesture.TryBegin(3, PointerTarget.Toy), Is.True);
        }

        [Test]
        public void VisibleFaceRayHasCorrectDepthAndRejectsBackFaces()
        {
            var a = new Point3(-1, -1, 0);
            var b = new Point3(0, 1, 0);
            var c = new Point3(1, -1, 0);
            Assert.That(SurfaceRaycast.TryTriangle(new Point3(0, 0, -2), new Point3(0, 0, 1), a, b, c, out float depth), Is.True);
            Assert.That(depth, Is.EqualTo(2f).Within(.00001f));
            Assert.That(SurfaceRaycast.TryTriangle(new Point3(0, 0, 2), new Point3(0, 0, -1), a, b, c, out depth), Is.False);
        }
    }
}
