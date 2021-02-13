using System.Runtime.CompilerServices;
using System.Threading;

namespace Centrifugo.Client.Helpers
{
    public static class InterlockedEx
    {
        /// <summary>
        /// unsigned equivalent of <see cref="Interlocked.Increment(ref int)"/>
        /// </summary>
        public static uint Increment(ref uint location)
        {
            int incrementedSigned = Interlocked.Increment(ref Unsafe.As<uint, int>(ref location));
            return Unsafe.As<int, uint>(ref incrementedSigned);
        }

        /// <summary>
        /// unsigned equivalent of <see cref="Interlocked.Increment(ref long)"/>
        /// </summary>
        public static ulong Increment(ref ulong location)
        {
            long incrementedSigned = Interlocked.Increment(ref Unsafe.As<ulong, long>(ref location));
            return Unsafe.As<long, ulong>(ref incrementedSigned);
        }
    }
}