using System;

namespace TALib.NETCore.HighPerf
{
    public static class BufferHelpers
    {
        public static Span<decimal> New(int size) =>
            new Memory<decimal>(new decimal[size]).Span; // TODO: use memory pool

        public static void Copy(
            ref Span<decimal> source,
            int sourceIndex,
            ref Span<decimal> destination,
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
