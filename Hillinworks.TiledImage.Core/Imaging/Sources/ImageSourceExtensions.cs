namespace Hillinworks.TiledImage.Imaging.Sources
{
    public static class ImageSourceExtensions
    {
        public static Dimensions GetLODDimensions(this IImageSource imageSource, int lodLevel)
        {
            return imageSource.Dimensions.AtLODLevel(imageSource.LOD, lodLevel);
        }
    }
}