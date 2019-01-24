using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hillinworks.TiledImage.Imaging;
using Hillinworks.TiledImage.Imaging.Sources;

namespace Hillinworks.TiledImage.Controls
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public sealed class ImageViewState
    {
        private int _layer;
        private Vector _offset;
        private double _rotation;
        private Matrix _worldToEnvelopMatrix;
        private Matrix _worldToViewMatrix;
        private double _zoomLevel;
        private Rect _envelopRect;

        internal ImageViewState(TiledImageView owner)
        {
            this.Owner = owner;
        }

        private TiledImageView Owner { get; }
        private IImageSource TiledImage => this.Owner.Source;
        public int LODLevel { get; private set; } = -1;
        public Dimensions LODDimensions { get; private set; }

        public double LODToWorldScale { get; private set; }
        public double ViewToWorldScale { get; private set; }
        public double ViewToLODScale { get; private set; }

        public double ViewportWidth => this.Owner.ActualWidth;
        public double ViewportHeight => this.Owner.ActualHeight;

        public int Layer
        {
            get => _layer;
            internal set
            {
                if (_layer == value)
                {
                    return;
                }

                _layer = value;
                this.Owner.TilesManager.UpdateLayer(_layer);
            }
        }

        public double Rotation
        {
            get => _rotation;
            private set
            {
                if (this.UpdatingOwner)
                {
                    return;
                }

                if (_rotation == value)
                {
                    return;
                }

                _rotation = value;

                this.UpdateMatrices();
            }
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            private set
            {
                if (this.UpdatingOwner)
                {
                    return;
                }

                if (_zoomLevel == value)
                {
                    return;
                }

                _zoomLevel = value;

                var newLODLevel = this.TiledImage.LOD.CalculateLODLevel(_zoomLevel);
                if (newLODLevel != this.LODLevel)
                {
                    this.SetLODLevel(newLODLevel);
                }

                this.ViewToWorldScale = this.TiledImage.LOD.MaxZoomLevel / _zoomLevel;
                this.ViewToLODScale = this.ViewToWorldScale / this.LODToWorldScale;

                this.UpdateMatrices();
            }
        }

        /// <summary>
        ///     Current view offset, in view coordinate
        /// </summary>
        public Vector Offset
        {
            get => _offset;
            internal set
            {
                if (this.UpdatingOwner)
                {
                    return;
                }

                if (_offset == value)
                {
                    return;
                }

                _offset = value;

                var matrix = this.WorldToEnvelopMatrix;
                matrix.Translate(-this.Offset.X, -this.Offset.Y);
                this.WorldToViewMatrix = matrix;

                this.OnTransformChanged();
            }
        }

        public Size ContentSize { get; private set; }
        
        public Rect EnvelopRect
        {
            get => _envelopRect;
            private set
            {
                _envelopRect = value;
                this.Owner.ExtentSize = _envelopRect.Size;
            }
        }

        public Matrix EnvelopToWorldMatrix { get; private set; }

        public Matrix WorldToEnvelopMatrix
        {
            get => _worldToEnvelopMatrix;
            private set
            {
                _worldToEnvelopMatrix = value;
                this.EnvelopToWorldMatrix = _worldToEnvelopMatrix.GetInverse();
            }
        }

        public Matrix ViewToWorldMatrix { get; private set; }

        public Matrix WorldToViewMatrix
        {
            get => _worldToViewMatrix;
            private set
            {
                _worldToViewMatrix = value;
                this.ViewToWorldMatrix = _worldToViewMatrix.GetInverse();
            }
        }

        private bool UpdatingOwner { get; set; }

        internal void Initialize()
        {
            this.Zoom(this.TiledImage.LOD.InitialZoomLevel, new Point());

            this.UpdatingOwner = true;
            this.Owner.ZoomLevel = this.ZoomLevel;
            this.Owner.Offset = this.Offset;
            this.Owner.Rotation = this.Rotation;

            this.Layer = this.TiledImage.Dimensions.MinimumLayerIndex;
            this.Owner.Layer = this.Layer;
            this.UpdatingOwner = false;
        }

        /// <summary>
        ///     Perform the specified action while keeping the relationship between the specified point in view space
        ///     and its corresponded point in world space unchanged.
        /// </summary>
        private void FixViewPointInWorld(Point pointInView, Action action)
        {
            var pointInWorldSpace = this.ViewToWorldMatrix.Transform(pointInView);

            action();

            var pointInEnvelopSpace = this.WorldToEnvelopMatrix.Transform(pointInWorldSpace);
            this.Owner.Offset = pointInEnvelopSpace - pointInView;
        }

        internal void Zoom(double zoomLevel, Point origin)
        {
            this.FixViewPointInWorld(origin, () => this.ZoomLevel = zoomLevel);
        }

        internal void Rotate(double rotation, Point origin)
        {
            this.FixViewPointInWorld(origin, () => this.Rotation = rotation);
        }

        internal void OnTransformChanged()
        {
            this.Owner.TilesManager.UpdateTiles();
        }

        private void UpdateMatrices()
        {
            this.ContentSize = new Size(
                this.LODDimensions.ContentWidth / this.ViewToLODScale,
                this.LODDimensions.ContentHeight / this.ViewToLODScale);

            var horizontalMargin = this.LODDimensions.ContentLeft / this.ViewToLODScale;
            var verticalMargin = this.LODDimensions.ContentTop / this.ViewToLODScale;

            var matrix = Matrix.Identity;
            matrix.RotateAt(
                this.Rotation,
                this.ContentSize.Width / 2 + horizontalMargin,
                this.ContentSize.Height / 2 + verticalMargin);

            var envelopBoundaryVertices = matrix.TransformVertices(
                new Rect(new Point(horizontalMargin, verticalMargin), this.ContentSize));

            var envelopLeft = envelopBoundaryVertices.Min(v => v.X);
            var envelopRight = envelopBoundaryVertices.Max(v => v.X);
            var envelopTop = envelopBoundaryVertices.Min(v => v.Y);
            var envelopBottom = envelopBoundaryVertices.Max(v => v.Y);

            this.EnvelopRect = new Rect(
                envelopLeft, 
                envelopTop, 
                envelopRight - envelopLeft,
                envelopBottom - envelopTop);
            
            matrix = Matrix.Identity;

            // first scale world space to content space
            matrix.Scale(1 / this.ViewToWorldScale, 1 / this.ViewToWorldScale);
            // rotate about the center of the content
            matrix.RotateAt(
                this.Rotation,
                this.ContentSize.Width / 2 + horizontalMargin,
                this.ContentSize.Height / 2 + verticalMargin);
            ;
            // zero envelop coordinates
            matrix.Translate(-envelopLeft, -envelopTop);

            this.WorldToEnvelopMatrix = matrix;

            matrix.Translate(-this.Offset.X, -this.Offset.Y);

            this.WorldToViewMatrix = matrix;
        }

        private void SetLODLevel(int lodLevel)
        {
            this.Owner.CaptureGhostImage();
            this.LODLevel = lodLevel;
            this.LODDimensions = this.TiledImage.GetLODDimensions(this.LODLevel);
            this.LODToWorldScale = this.TiledImage.LOD.GetLODToWorldScale(this.LODLevel);
        }
    }
}