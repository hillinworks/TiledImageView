using System.Diagnostics;

namespace Hillinworks.TiledImage.Utilities
{
    internal struct ImageCopyMetrics
    {
        private ImageCopyMetrics(int source, int destination, int size)
        {
            this.Source = source;
            this.Destination = destination;
            this.Size = size;
        }

        public int Source { get; }
        public int Destination { get; }
        public int Size { get; }

        public static ImageCopyMetrics Calculate(
            int sourceLowerBound,
            int sourceUpperBound,
            int destinationPosition,
            int destinationSize)
        {
            int source, destination, size;

            if (destinationPosition < sourceLowerBound)
            {
                source = 0;
                destination = sourceLowerBound - destinationPosition;
                size = destinationSize - destination;
            }
            else if (destinationPosition + destinationSize > sourceUpperBound)
            {
                size = sourceUpperBound - destinationPosition;
                destination = 0;
                source = destinationPosition - sourceLowerBound;
            }
            else
            {
                size = destinationSize;
                destination = 0;
                source = destinationPosition - sourceLowerBound;
            }

            var sourceSize = sourceUpperBound - sourceLowerBound;

            if (size > sourceSize)
            {
                size = sourceSize;
            }

            Debug.Assert(size >= 0);

            return new ImageCopyMetrics(source, destination, size);
        }
    }
}