using System;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Centrifugo.Client.Helpers
{
    public static class NullTaskResult
    {
        public static Task<ByteString> Instance { get; } = Task.FromResult(ByteString.Empty);

        public static Task<ByteString> NotConnected { get; } = Task.FromException<ByteString>(new Exception("Not connected"));
    }
}