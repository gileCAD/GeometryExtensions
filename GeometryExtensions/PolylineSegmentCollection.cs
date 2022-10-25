﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;

namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Describes a PolylineSegment collection.
    /// </summary>
    public class PolylineSegmentCollection : List<PolylineSegment>
    {
        #region Properties

        /// <summary>
        /// Gets the start point of the first segment.
        /// </summary>
        public Point2d StartPoint => this[0].StartPoint;

        /// <summary>
        /// Gets the end point of the last segment.
        /// </summary>
        public Point2d EndPoint => this[Count - 1].EndPoint;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new empty instance of PolylineSegmentCollection.
        /// </summary>
        public PolylineSegmentCollection() { }

        /// <summary>
        /// Creates a new instance of PolylineSegmentCollection.
        /// </summary>
        /// <param name="segments">A sequence of PolylineSegment.</param>
        public PolylineSegmentCollection(IEnumerable<PolylineSegment> segments)
        {
            AddRange(segments);
        }

        /// <summary>
        /// Creates a new instance of PolylineSegmentCollection.
        /// </summary>
        /// <param name="segments">A PolylineSegment array.</param>
        public PolylineSegmentCollection(params PolylineSegment[] segments)
        {
            AddRange(segments);
        }

        /// <summary>
        /// Creates a new instance of PolylineSegmentCollection.
        /// </summary>
        /// <param name="pline">An instance of Polyline.</param>
        public PolylineSegmentCollection(Polyline pline)
        {
            addPolyline(pline);
        }

        private void addPolyline(Polyline pline)
        {
            int n = pline.NumberOfVertices - 1;
            for (int i = 0; i < n; i++)
            {
                Add(new PolylineSegment(
                    pline.GetPoint2dAt(i),
                    pline.GetPoint2dAt(i + 1),
                    pline.GetBulgeAt(i),
                    pline.GetStartWidthAt(i),
                    pline.GetEndWidthAt(i)));
            }
            if (pline.Closed)
            {
                Add(new PolylineSegment(
                    pline.GetPoint2dAt(n),
                    pline.GetPoint2dAt(0),
                    pline.GetBulgeAt(n),
                    pline.GetStartWidthAt(n),
                    pline.GetEndWidthAt(n)));
            }
        }

        /// <summary>
        /// Creates a new instance of PolylineSegmentCollection.
        /// </summary>
        /// <param name="pline">An instance of Polyline2d.</param>
        public PolylineSegmentCollection(Polyline2d pline)
        {
            Vertex2d[] vertices = pline.GetVertices().ToArray();
            int n = vertices.Length - 1;
            for (int i = 0; i < n; i++)
            {
                Vertex2d vertex = vertices[i];
                Add(new PolylineSegment(
                    vertex.Position.Convert2d(),
                    vertices[i + 1].Position.Convert2d(),
                    vertex.Bulge,
                    vertex.StartWidth,
                    vertex.EndWidth));
            }
            if (pline.Closed)
            {
                Vertex2d vertex = vertices[n];
                Add(new PolylineSegment(
                    vertex.Position.Convert2d(),
                    vertices[0].Position.Convert2d(),
                    vertex.Bulge,
                    vertex.StartWidth,
                    vertex.EndWidth));
            }
        }

        /// <summary>
        /// Creates a new instance of PolylineSegmentCollection.
        /// </summary>
        /// <param name="circle">An instance of Circle.</param>
        public PolylineSegmentCollection(Circle circle)
        {
            Plane plane = new Plane(Point3d.Origin, circle.Normal);
            Point2d cen = circle.Center.Convert2d(plane);
            Vector2d vec = new Vector2d(circle.Radius, 0.0);
            Add(new PolylineSegment(cen + vec, cen - vec, 1.0));
            Add(new PolylineSegment(cen - vec, cen + vec, 1.0));
        }

        /// <summary>
        /// Creates a new instance of PolylineSegmentCollection.
        /// </summary>
        /// <param name="ellipse">An instance of Ellipse.</param>
        public PolylineSegmentCollection(Ellipse ellipse)
        {
            // PolylineSegmentCollection figurant l'ellipse fermée
            double pi = Math.PI;
            Plane plane = new Plane(Point3d.Origin, ellipse.Normal);
            Point3d cen3d = ellipse.Center;
            Point3d pt3d0 = cen3d + ellipse.MajorAxis;
            Point3d pt3d4 = cen3d + ellipse.MinorAxis;
            Point3d pt3d2 = ellipse.GetPointAtParameter(pi / 4.0);
            Point2d cen = cen3d.Convert2d(plane);
            Point2d pt0 = pt3d0.Convert2d(plane);
            Point2d pt2 = pt3d2.Convert2d(plane);
            Point2d pt4 = pt3d4.Convert2d(plane);
            Line2d line01 = new Line2d(pt0, (pt4 - cen).GetNormal() + (pt2 - pt0).GetNormal());
            Line2d line21 = new Line2d(pt2, (pt0 - pt4).GetNormal() + (pt0 - pt2).GetNormal());
            Line2d line23 = new Line2d(pt2, (pt4 - pt0).GetNormal() + (pt4 - pt2).GetNormal());
            Line2d line43 = new Line2d(pt4, (pt0 - cen).GetNormal() + (pt2 - pt4).GetNormal());
            Line2d majAx = new Line2d(cen, pt0);
            Line2d minAx = new Line2d(cen, pt4);
            Point2d pt1 = line01.IntersectWith(line21)[0];
            Point2d pt3 = line23.IntersectWith(line43)[0];
            Point2d pt5 = pt3.TransformBy(Matrix2d.Mirroring(minAx));
            Point2d pt6 = pt2.TransformBy(Matrix2d.Mirroring(minAx));
            Point2d pt7 = pt1.TransformBy(Matrix2d.Mirroring(minAx));
            Point2d pt8 = pt0.TransformBy(Matrix2d.Mirroring(minAx));
            Point2d pt9 = pt7.TransformBy(Matrix2d.Mirroring(majAx));
            Point2d pt10 = pt6.TransformBy(Matrix2d.Mirroring(majAx));
            Point2d pt11 = pt5.TransformBy(Matrix2d.Mirroring(majAx));
            Point2d pt12 = pt4.TransformBy(Matrix2d.Mirroring(majAx));
            Point2d pt13 = pt3.TransformBy(Matrix2d.Mirroring(majAx));
            Point2d pt14 = pt2.TransformBy(Matrix2d.Mirroring(majAx));
            Point2d pt15 = pt1.TransformBy(Matrix2d.Mirroring(majAx));
            double bulge1 = Math.Tan((pt4 - cen).GetAngleTo(pt1 - pt0) / 2.0);
            double bulge2 = Math.Tan((pt1 - pt2).GetAngleTo(pt0 - pt4) / 2.0);
            double bulge3 = Math.Tan((pt4 - pt0).GetAngleTo(pt3 - pt2) / 2.0);
            double bulge4 = Math.Tan((pt3 - pt4).GetAngleTo(pt0 - cen) / 2.0);
            Add(new PolylineSegment(pt0, pt1, bulge1));
            Add(new PolylineSegment(pt1, pt2, bulge2));
            Add(new PolylineSegment(pt2, pt3, bulge3));
            Add(new PolylineSegment(pt3, pt4, bulge4));
            Add(new PolylineSegment(pt4, pt5, bulge4));
            Add(new PolylineSegment(pt5, pt6, bulge3));
            Add(new PolylineSegment(pt6, pt7, bulge2));
            Add(new PolylineSegment(pt7, pt8, bulge1));
            Add(new PolylineSegment(pt8, pt9, bulge1));
            Add(new PolylineSegment(pt9, pt10, bulge2));
            Add(new PolylineSegment(pt10, pt11, bulge3));
            Add(new PolylineSegment(pt11, pt12, bulge4));
            Add(new PolylineSegment(pt12, pt13, bulge4));
            Add(new PolylineSegment(pt13, pt14, bulge3));
            Add(new PolylineSegment(pt14, pt15, bulge2));
            Add(new PolylineSegment(pt15, pt0, bulge1));

            // if elliptical arc:
            if (!ellipse.Closed)
            {
                double startParam, endParam;
                Point2d startPoint = ellipse.StartPoint.Convert2d(plane);
                Point2d endPoint = ellipse.EndPoint.Convert2d(plane);

                int startIndex = GetClosestSegmentIndexTo(startPoint);
                startPoint = this[startIndex].ToCurve2d().GetClosestPointTo(startPoint).Point;
                if (startPoint.IsEqualTo(this[startIndex].EndPoint))
                {
                    if (startIndex == 15)
                        startIndex = 0;
                    else
                        startIndex++;
                    startParam = 0.0;
                }
                else
                {
                    startParam = this[startIndex].GetParameterOf(startPoint);
                }

                int endIndex = GetClosestSegmentIndexTo(endPoint);
                endPoint = this[endIndex].ToCurve2d().GetClosestPointTo(endPoint).Point;
                if (endPoint.IsEqualTo(this[endIndex].StartPoint))
                {
                    if (endIndex == 0)
                        endIndex = 15;
                    else
                        endIndex--;
                    endParam = 1.0;
                }
                else
                {
                    endParam = this[endIndex].GetParameterOf(endPoint);
                }

                if (startParam != 0.0)
                {
                    this[startIndex].StartPoint = startPoint;
                    this[startIndex].Bulge = this[startIndex].Bulge * (1.0 - startParam);
                }

                if (endParam != 1.0) //(endParam != 0.0)
                {
                    this[endIndex].EndPoint = endPoint;
                    this[endIndex].Bulge = this[endIndex].Bulge * endParam;
                }

                if (startIndex == endIndex)
                {
                    PolylineSegment segment = this[startIndex];
                    Clear();
                    Add(segment);
                }

                else if (startIndex < endIndex)
                {
                    RemoveRange(endIndex + 1, 15 - endIndex);
                    RemoveRange(0, startIndex);
                }
                else
                {
                    AddRange(GetRange(0, endIndex + 1));
                    RemoveRange(0, startIndex);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the index of the closest segment to the specified point.
        /// </summary>
        /// <param name="pt">The 2d point from which the distances are calculated</param>
        /// <returns>The index of the segment in the colllection.</returns>
        public int GetClosestSegmentIndexTo(Point2d pt)
        {
            int result = 0;
            double dist = this[0].ToCurve2d().GetDistanceTo(pt);
            for (int i = 1; i < Count; i++)
            {
                double tmpDist = this[i].ToCurve2d().GetDistanceTo(pt);
                if (tmpDist < dist)
                {
                    result = i;
                    dist = tmpDist;
                }
            }
            return result;
        }

        /// <summary>
        /// Joins the contiguous segments into one or more instance of PolylineSegmentCollection.
        /// Points are compared using Tolerance.Global.
        /// </summary>
        /// <returns>A list of instances of PolylineSegmentCollection.</returns>
        public List<PolylineSegmentCollection> Join()
        {
            return Join(Tolerance.Global);
        }

        /// <summary>
        /// Joins the contiguous segments into one or more instance of PolylineSegmentCollection.
        /// Points are compared using the specified Tolerance.
        /// </summary>
        /// <param name="tol">The tolerance to be used in points equality comparison.</param>
        /// <returns>A list of instances of PolylineSegmentCollection.</returns>
        public List<PolylineSegmentCollection> Join(Tolerance tol)
        {
            List<PolylineSegmentCollection> result = new List<PolylineSegmentCollection>();
            PolylineSegmentCollection clone = new PolylineSegmentCollection(this);
            while (clone.Count > 0)
            {
                PolylineSegmentCollection newCol = new PolylineSegmentCollection();
                PolylineSegment seg = clone[0];
                newCol.Add(seg);
                Point2d start = seg.StartPoint;
                Point2d end = seg.EndPoint;
                clone.RemoveAt(0);
                while (true)
                {
                    int i = clone.FindIndex(s => s.StartPoint.IsEqualTo(end, tol));
                    if (i >= 0)
                    {
                        seg = clone[i];
                        newCol.Add(seg);
                        end = seg.EndPoint;
                        clone.RemoveAt(i);
                        continue;
                    }
                    i = clone.FindIndex(s => s.EndPoint.IsEqualTo(end, tol));
                    if (i >= 0)
                    {
                        seg = clone[i];
                        seg.Inverse();
                        newCol.Add(seg);
                        end = seg.EndPoint;
                        clone.RemoveAt(i);
                        continue;
                    }
                    i = clone.FindIndex(s => s.EndPoint.IsEqualTo(start, tol));
                    if (i >= 0)
                    {
                        seg = clone[i];
                        newCol.Insert(0, seg);
                        start = seg.StartPoint;
                        clone.RemoveAt(i);
                        continue;
                    }
                    i = clone.FindIndex(s => s.StartPoint.IsEqualTo(start, tol));
                    if (i >= 0)
                    {
                        seg = clone[i];
                        seg.Inverse();
                        newCol.Insert(0, seg);
                        start = seg.StartPoint;
                        clone.RemoveAt(i);
                        continue;
                    }
                    break;
                }
                result.Add(newCol);
            }
            return result;
        }

        /// <summary>
        /// Reverse the order of the collection and of the segments it contains.
        /// </summary>
        public void Inverse()
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].Inverse();
            }
            Reverse();
        }

        /// <summary>
        /// Creates a new instance of Polyline.
        /// </summary>
        /// <returns>A new instance of Polyline.</returns>
        public Polyline ToPolyline()
        {
            Polyline pline = new Polyline();
            for (int i = 0; i < Count; i++)
            {
                PolylineSegment seg = this[i];
                pline.AddVertexAt(i, seg.StartPoint, seg.Bulge, seg.StartWidth, seg.EndWidth);
            }
            int j = Count;
            pline.AddVertexAt(j, this[j - 1].EndPoint, 0.0, this[j - 1].EndWidth, this[0].StartWidth);
            if (pline.GetPoint2dAt(0).IsEqualTo(pline.GetPoint2dAt(j)))
            {
                pline.RemoveVertexAt(j);
                pline.Closed = true;
            }
            return pline;
        }

        /// <summary>
        /// Add a single polyline
        /// </summary>
        /// <param name="polyline"></param>
        public void Add(Polyline polyline)
        {
            addPolyline(polyline);
        }

        /// <summary>
        /// Add a collection of polylines.
        /// </summary>
        /// <param name="polylines"></param>
        public void AddRange(IEnumerable<Polyline> polylines)
        {
            foreach (Polyline polyline in polylines)
            {
                addPolyline(polyline);
            }
        }

        #endregion
    }
}
