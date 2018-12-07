#if DEBUG
#define DEBUG_PRINT_CULLING_PROCESS
#endif
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows;
using slf4net;
using WpfPoint = System.Windows.Point;

namespace Hillinworks.TiledImage.Controls.Overlays
{
    partial class OverlayCuller<T>
    {
        private class QuadTreeCuller : CullerBase
        {
            // ReSharper disable once StaticMemberInGenericType
            private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(QuadTreeCuller));

#if DEBUG_PRINT_CULLING_PROCESS
            private static readonly bool _enablePrintCullingProcess = false;
#endif

            public QuadTreeCuller(PointQuadTree<QuadTreeItem> quadTree)
            {
                this.QuadTree = quadTree;
            }

            private PointQuadTree<QuadTreeItem> QuadTree { get; }

            private static WpfPoint ToWpfPoint(PointF point)
            {
                return new WpfPoint(point.X, point.Y);
            }

            private static Rect ToRect(RectangleF rect)
            {
                return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
            }

            public IEnumerable<T> Cull(ImageViewState viewState)
            {
                var elements = new List<T>();

                if (this.QuadTree.RootNode != null)
                {
                    var cullRect = new Rect(0, 0, viewState.ViewportWidth, viewState.ViewportHeight);
                    var cullContext = new CullContext(cullRect, viewState);
                    CullRecursive(elements, cullContext, this.QuadTree.RootNode);
                }

                return elements;
            }

#if DEBUG_PRINT_CULLING_PROCESS
            [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
#endif
            private static void CullRecursive(
                List<T> elements,
                CullContext context,
                PointQuadTree<QuadTreeItem>.Node node)
            {
                var intersection =
                    CheckIntersection(context, ToRect(node.Bounds));

                if (intersection == Intersection.Contain)
                {
#if DEBUG_PRINT_CULLING_PROCESS

                    if (_enablePrintCullingProcess)
                    {
                        Logger.Debug(
                            $"{node.DebugDisplayName} fully contained by view, adding itself and all descendants");

                        if (node.Element != null)
                        {
                            Logger.Debug($"\tADDED {node.DebugDisplayName} {node.Element.Element}");
                        }

                        foreach (var descendant in node.EnumerateDescendantNodes())
                        {
                            if (descendant.Element != null)
                            {
                                Logger.Debug($"\tADDED {descendant.DebugDisplayName} {descendant.Element.Element}");
                            }
                        }
                    }

#endif
                    if (node.Element != null)
                    {
                        elements.Add(node.Element.Element);
                    }

                    elements.AddRange(node.EnumerateDescendants().Where(i => i.Element != null).Select(i => i.Element));
                }
                else if (intersection == Intersection.Intersect)
                {
                    var viewPosition = context.ViewState.WorldToViewMatrix.Transform(ToWpfPoint(node.Point));
                    if (context.CullRect.Contains(viewPosition) && node.Element != null)
                    {
#if DEBUG_PRINT_CULLING_PROCESS
                        if (_enablePrintCullingProcess)
                        {
                            Logger.Debug(
                                $"{node.DebugDisplayName} intersects with view, and its Point is contained by view, adding");
                            Logger.Debug($"\tADDED {node.DebugDisplayName} {node.Element.Element}");
                        }
#endif
                        elements.Add(node.Element.Element);
                    }

                    foreach (var subnode in node.Subnodes)
                    {
                        CullRecursive(elements, context, subnode);
                    }
                }
#if DEBUG_PRINT_CULLING_PROCESS
                else
                {
                    if (_enablePrintCullingProcess)
                    {
                        Logger.Debug($"{node.DebugDisplayName} does not intersect with view, ruled out");
                        if (node.Element != null)
                        {
                            Logger.Debug($"\tRULE-OUT {node.DebugDisplayName} {node.Element.Element}");
                        }

                        foreach (var descendant in node.EnumerateDescendantNodes())
                        {
                            if (descendant.Element != null)
                            {
                                Logger.Debug($"\tRULE-OUT {descendant.DebugDisplayName} {descendant.Element.Element}");
                            }
                        }
                    }
                }
#endif
            }
        }
    }
}