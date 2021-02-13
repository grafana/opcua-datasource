using Protocol;

namespace Centrifugo.Client.Helpers
{
    public static class CommandExtensions
    {
        public static bool IsAwaitable(this Command cmd)
        {
            return cmd.Method != MethodType.Send;
        }
    }
}