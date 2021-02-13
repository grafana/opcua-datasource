namespace Centrifugo.Client.Events
{
    public class ErrorEvent
    {
        public uint Code { get; }

        public string Message { get; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public ErrorEvent(uint code, string message)
        {
            Code = code;
            Message = message;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Code + " - " + Message;
        }
    }
}