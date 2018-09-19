using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Hillinworks.TiledImage.Controls
{
    internal abstract partial class CullerBase
    {
        /// <summary>
        ///     Check whether target rect, in world space, intersects with the cull rect
        /// </summary>
        protected static Intersection CheckIntersection(CullContext context, Rect targetRect)
        {
            // This method is a simplified implementation with the knowledge of CullRect being unrotated

            var targetPointsInViewSpace = context.ViewState.WorldToViewMatrix.TransformVertices(targetRect);

            // first check whether any or all vertices of targetRect, transformed to the view space,
            // is contained by the cull rect
            var contained = false;
            var fullyContained = true;
            foreach (var point in targetPointsInViewSpace)
            {
                if (context.CullRect.Contains(point))
                {
                    contained = true;
                    if (!fullyContained)
                    {
                        return Intersection.Intersect;
                    }
                }
                else
                {
                    fullyContained = false;
                }
            }

            if (fullyContained)
            {
                return Intersection.Contain;
            }

            if (contained)
            {
                return Intersection.Intersect;
            }

            // the we go with the SAT (separating axis theorem) checking
            var targetVertices = targetRect.GetVertices();
            foreach (var vertices in new[] { context.WorldCullRectVertices, targetVertices })
            {
                for (var i1 = 0; i1 < vertices.Length; i1++)
                {
                    var i2 = (i1 + 1) % vertices.Length;
                    var p1 = vertices[i1];
                    var p2 = vertices[i2];

                    var normalY = p1.X - p2.X;
                    var normalX = p2.Y - p1.Y;

                    var minA = double.MaxValue;
                    var maxA = double.MinValue;
                    foreach (var p in context.WorldCullRectVertices)
                    {
                        var projected = normalX * p.X + normalY * p.Y;
                        if (projected < minA)
                        {
                            minA = projected;
                        }

                        if (projected > maxA)
                        {
                            maxA = projected;
                        }
                    }

                    var minB = double.MaxValue;
                    var maxB = double.MinValue;
                    foreach (var p in targetVertices)
                    {
                        var projected = normalX * p.X + normalY * p.Y;
                        if (projected < minB)
                        {
                            minB = projected;
                        }

                        if (projected > maxB)
                        {
                            maxB = projected;
                        }
                    }

                    if (maxA < minB || maxB < minA)
                    {
                        return Intersection.NotIntersect;
                    }
                }
            }

            return Intersection.Intersect;

        }
    }
}