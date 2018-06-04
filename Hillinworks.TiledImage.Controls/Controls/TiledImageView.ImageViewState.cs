using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hillinworks.TiledImage.Imaging;
using Hillinworks.TiledImage.Utilities;

namespace Hillinworks.TiledImage.Controls
{
	partial class TiledImageView
	{
		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		internal sealed class ImageViewState
		{
			private Vector _offset;
			private double _rotation;
			private double _zoomLevel;
			private int _layer;
			private Matrix _worldToEnvelopMatrix;
			private Matrix _worldToViewMatrix;

			public ImageViewState(TiledImageView owner)
			{
				this.Owner = owner;
			}

			public void Initialize()
			{
				this.Zoom(this.TiledImage.LOD.InitialZoomLevel, new Point());

				this.UpdatingOwner = true;
				this.Owner.ZoomLevel = this.ZoomLevel;
				this.Owner.Offset = this.Offset;
				this.Owner.Rotation = this.Rotation;
				this.UpdatingOwner = false;
			}

			public TiledImageView Owner { get; }
			public Imaging.TiledImageSource TiledImage => this.Owner.Source;
			public int LODLevel { get; private set; } = -1;
			public Dimensions LODDimensions { get; private set; }

			public double LODToWorldScale { get; private set; }
			public double ViewToWorldScale { get; private set; }
			public double ViewToLODScale { get; private set; }

			public int Layer
			{
				get => _layer;
				set
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
				set
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

					this.FixViewPointInWorld(this.Owner.CenterPoint, this.UpdateMatrices);

					this.OnTransformChanged();
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

					var newLODLevel = this.CalculateLODLevel(_zoomLevel);
					if (newLODLevel != this.LODLevel)
					{
						this.SetLODLevel(newLODLevel);
					}

					this.ViewToWorldScale = this.TiledImage.LOD.MaxZoomLevel / _zoomLevel;
					this.ViewToLODScale = this.ViewToWorldScale / this.LODToWorldScale;

					this.UpdateMatrices();
				}
			}

			public Vector Offset
			{
				get => _offset;
				set
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
			public Size EnvelopSize { get; private set; }
			
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

			/// <summary>
			/// Perform the specified action while keeping the relationship between the specified point in view space 
			/// and its corresponded point in world space unchanged.
			/// </summary>
			private void FixViewPointInWorld(Point pointInView, Action action)
			{
				var pointInWorldSpace = this.ViewToWorldMatrix.Transform(pointInView);

				action();

				var pointInEnvelopSpace = this.WorldToEnvelopMatrix.Transform(pointInWorldSpace);
				this.Owner.Offset = pointInEnvelopSpace - pointInView;
			}

			public void Zoom(double zoomLevel, Point origin)
			{
				this.FixViewPointInWorld(origin, () => this.ZoomLevel = zoomLevel);

				this.OnTransformChanged();
			}

			private void OnTransformChanged()
			{
				this.Owner.TilesManager.UpdateTiles();
			}

			private void UpdateMatrices()
			{
				this.ContentSize = new Size(
					this.LODDimensions.Width / this.ViewToLODScale,
					this.LODDimensions.Height / this.ViewToLODScale);

				var matrix = Matrix.Identity;

				matrix.RotateAt(-this.Rotation, this.ContentSize.Width / 2, this.ContentSize.Height / 2);

				var envelopBoundaryVertices = matrix.TransformVertices(
					new Rect(new Point(), this.ContentSize));

				var envelopLeft = envelopBoundaryVertices.Min(v => v.X);
				var envelopRight = envelopBoundaryVertices.Max(v => v.X);
				var envelopTop = envelopBoundaryVertices.Min(v => v.Y);
				var envelopBottom = envelopBoundaryVertices.Max(v => v.Y);
				this.EnvelopSize = new Size(envelopRight - envelopLeft, envelopBottom - envelopTop);

				matrix = Matrix.Identity;

				// first scale world space to content space
				matrix.Scale(1 / this.ViewToWorldScale, 1 / this.ViewToWorldScale);
				// rotate about the center of the content
				matrix.RotateAt(this.Rotation, this.ContentSize.Width / 2, this.ContentSize.Height / 2);
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
				this.LODDimensions = this.TiledImage.Dimensions.AtLODLevel(this.LODLevel);
				this.LODToWorldScale = Math.Pow(2, this.LODLevel);
			}

			private int CalculateLODLevel(double zoomLevel)
			{
				return ((int)Math.Floor(Math.Log(this.TiledImage.LOD.MaxZoomLevel / zoomLevel, 2)))
					.Clamp(0, this.TiledImage.LOD.MaxLODLevel);
			}

			public void SetTransform(Matrix matrix)
			{

				// todo
				throw new NotImplementedException();
			}
		}
	}
}