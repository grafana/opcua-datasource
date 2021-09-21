using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Arrow.Memory;
using Apache.Arrow.Types;
using Grpc.Core.Logging;

namespace Apache.Arrow
{
    public class NanoTimestampArrayBuiler : TimestampArray.Builder {
        private static readonly DateTimeOffset s_epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        protected override long ConvertTo(DateTimeOffset value)
        {
            TimeSpan timeSpan = value - s_epoch;
            long ticks = timeSpan.Ticks;
            return ticks * 100;
            // return base.ConvertTo(value);
        }
    }
       
}
