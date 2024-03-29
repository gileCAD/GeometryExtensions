using System;
using Autodesk.AutoCAD.Geometry;
using static System.Math;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides conversions between Euler angles, transformation matrix and Normal / Rotation properties.
    /// </summary>
    public struct EulerAngles
    {
        Matrix3d planeToWorld;

        /// <summary>
        /// Gets the Yaw angle (around Oz axis)
        /// </summary>
        public double Yaw { get; }

        /// <summary>
        /// Gets the Pitch angle (around Oy' axis)
        /// </summary>
        public double Pitch { get; }

        /// <summary>
        /// Gets the Roll angle (around Ox" axis)
        /// </summary>
        public double Roll { get; }

        /// <summary>
        /// Gets the Precession angle (around Oz axis).
        /// </summary>
        public double Precession { get; }

        /// <summary>
        /// Gets the Nutation angle (around Ox' axis).
        /// </summary>
        public double Nutation { get; }

        /// <summary>
        /// Gets the Spin angle (around Oz" axis).
        /// </summary>
        public double Spin { get; }

        /// <summary>
        /// Gets the normal vector of X"Y" plane.
        /// </summary>
        public Vector3d Normal => planeToWorld.CoordinateSystem3d.Zaxis;

        /// <summary>
        /// Gets the proper rotation.
        /// </summary>
        public readonly double Rotation => Spin;

        /// <summary>
        /// Gets the unit scaled transformation matrix.
        /// </summary>
        public Matrix3d Transform { get; }

        /// <summary>
        /// Creates a new Instance of EulerAngles.
        /// </summary>
        /// <param name="transform">Transformation matrix.</param>
        public EulerAngles(Matrix3d transform)
        {
            if (!transform.IsScaledOrtho())
                throw new ArgumentException("Non orthogonal matrix.");

            var cs = transform.CoordinateSystem3d;
            var xAxis = cs.Xaxis.GetNormal();
            var yAxis = cs.Yaxis.GetNormal();
            var zAxis = cs.Zaxis.GetNormal();
            Transform = new Matrix3d(
            [
                xAxis.X, yAxis.X, zAxis.X, 0.0,
                xAxis.Y, yAxis.Y, zAxis.Y, 0.0,
                xAxis.Z, yAxis.Z, zAxis.Z, 0.0,
                0.0, 0.0, 0.0, 1.0
            ]);

            // Z-X'-Z"
            Nutation = Acos(Transform[2, 2]);
            if (Abs(Nutation) < 1e-7)
            {
                Nutation = 0.0;
                Precession = 0.0;
                Spin = Atan2(Transform[1, 0], Transform[1, 1]);
            }
            else
            {
                Precession = Atan2(Transform[0, 2], -Transform[1, 2]);
                Spin = Atan2(Transform[2, 0], Transform[2, 1]);
            }

            // Z-Y'-X"
            Pitch = -Asin(Transform[2, 0]);
            if (Abs(Pitch - PI * 0.5) < 1e-7)
            {
                Pitch = PI * 0.5;
                Yaw = Atan2(Transform[1, 2], Transform[1, 1]);
                Roll = 0.0;
            }
            else if (Abs(Pitch + PI * 0.5) < 1e-7)
            {
                Pitch = -PI * 0.5;
                Yaw = Atan2(-Transform[1, 2], Transform[1, 1]);
                Roll = 0.0;
            }
            else
            {
                Yaw = Atan2(Transform[1, 0], Transform[0, 0]);
                Roll = Atan2(Transform[2, 1], Transform[2, 2]);
            }

            planeToWorld =
                Matrix3d.Rotation(Precession, Vector3d.ZAxis, Point3d.Origin) *
                Matrix3d.Rotation(Nutation, Vector3d.XAxis, Point3d.Origin);
        }

        /// <summary>
        /// Creates a new instance of EulerAngles.
        /// </summary>
        /// <param name="normal">Normal of X'Y' plane.</param>
        /// <param name="rotation">Proper rotation.</param>
        public EulerAngles(Vector3d normal, double rotation)
            : this(Matrix3d.PlaneToWorld(normal) *
                   Matrix3d.Rotation(rotation, Vector3d.ZAxis, Point3d.Origin))
        { }

        /// <summary>
        /// Creates a new instance of EulerAngles using Z-X'-Z" convention.
        /// </summary>
        /// <param name="precession">Precession angle (Z axis).</param>
        /// <param name="nutation">Nutation angle (X' axis).</param>
        /// <param name="rotation">Spin angle (Z" axis).</param>
        /// <returns>New instance of EulerAngles.</returns>
        public static EulerAngles CreateProperEuler(double precession, double nutation, double rotation) =>
            new(
                Matrix3d.Rotation(precession, Vector3d.ZAxis, Point3d.Origin) *
                Matrix3d.Rotation(nutation, Vector3d.XAxis, Point3d.Origin) *
                Matrix3d.Rotation(rotation, Vector3d.ZAxis, Point3d.Origin));

        /// <summary>
        /// Creates a new instance of EulerAngles using Z-Y'-X" convention (Tait-Bryan).
        /// </summary>
        /// <param name="yaw">Yaw angle (Z axis).</param>
        /// <param name="pitch">Pitch angle (Y' axis).</param>
        /// <param name="roll">Roll angle (X" axis).</param>
        /// <returns>New instance of EulerAngles.</returns>
        public static EulerAngles CreateTaitBryan(double yaw, double pitch, double roll) =>
            new(
                Matrix3d.Rotation(yaw, Vector3d.ZAxis, Point3d.Origin) *
                Matrix3d.Rotation(pitch, Vector3d.YAxis, Point3d.Origin) *
                Matrix3d.Rotation(roll, Vector3d.XAxis, Point3d.Origin));

        /// <summary>
        /// Evaluates if the current instance and another are equal using global tolerance.
        /// </summary>
        /// <param name="other">EulerAngles instance.</param>
        /// <returns><c>true</c>, if the two instance are equal ; <c>false</c>, otherwise.</returns>
        public readonly bool IsEqualTo(EulerAngles other) => IsEqualTo(other, Tolerance.Global);

        /// <summary>
        /// Evaluates if the current instance and another are equal using the specified tolerance.
        /// </summary>
        /// <param name="other">EulerAngles intance.</param>
        /// <param name="tolerance">Tolerance.</param>
        /// <returns><c>true</c>, if the two instance are equal ; <c>false</c>, otherwise.</returns>
        public readonly bool IsEqualTo(EulerAngles other, Tolerance tolerance) => Transform.IsEqualTo(other.Transform, tolerance);

        /// <summary>
        /// Evaluates if the current instance and the object are equal.
        /// </summary>
        /// <param name="obj">Object to be compared.</param>
        /// <returns><c>true</c>, if the two objects are equal ; <c>false</c>, otherwise.</returns>
        public override readonly bool Equals(object? obj) =>
            obj is EulerAngles angles && angles.Transform.Equals(Transform);

        /// <summary>
        /// Serves as hashing function for the EulerAngles type.
        /// </summary>
        /// <returns>A hash code for the current instance of EulerAngles.</returns>
        public override readonly int GetHashCode() => Transform.GetHashCode();

        /// <summary>
        /// Evaluates if the two instances of EulerAngles are equal.
        /// </summary>
        /// <param name="a">First instance to compare.</param>
        /// <param name="b">Second instance to compare.</param>
        /// <returns><c>true</c>, if the two instance are equal ; <c>false</c>, otherwise.</returns>
        public static bool operator ==(EulerAngles a, EulerAngles b) => a.Transform == b.Transform;

        /// <summary>
        /// Evaluates if the two instances of EulerAngles are not equal.
        /// </summary>
        /// <param name="a">First instance to compare.</param>
        /// <param name="b">Second instance to compare.</param>
        /// <returns><c>true</c>, if the two instance are not equal ; <c>false</c>, otherwise.</returns>
        public static bool operator !=(EulerAngles a, EulerAngles b) => a.Transform != b.Transform;
    }
}
