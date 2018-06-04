namespace Hillinworks.TiledImage.Imaging
{
    public static class LoadTileStatusExtensions
    {
        public static bool IsAlive(this LoadTileStatus status)
            => status == LoadTileStatus.Loading
            || status == LoadTileStatus.Succeed;
    }

}
