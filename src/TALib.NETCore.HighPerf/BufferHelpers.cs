using System;

namespace TALib.NETCore.HighPerf
{
    public static class BufferHelpers
    {
        public static Span<double> New(int size) =>
            new Memory<double>(new double[size]).Span; // TODO: use memory pool

        public static void Copy(
            ref Span<double> source,
            int sourceIndex,
            ref Span<double> destination,
            int destinationIndex,
            int length)
        {
            var slice = source.Slice(sourceIndex, length);
            if (destination.Length < destinationIndex + length)
            {
                // TODO: use memory pool
                destination = New(destinationIndex + length);
            }
            slice.CopyTo(destination.Slice(destinationIndex, length));
        }
    }
}
