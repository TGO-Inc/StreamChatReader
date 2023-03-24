using StreamingServices.Chat;
using StreamingServices.Youtube;

namespace StreamingServices
{
    internal interface IReaderBase
    {
        StreamType StreamType { get; init; }
        string StreamId { get; init; }
        event ChatEventHandler? ChatEvent;
        event ChatLoadedHandler? ChatLoaded;
        public static IReaderBase NewChatReader(StreamType StreamType, string StreamId)
        {
            if(StreamType == StreamType.Youtube)
            {
                return new YoutubeChatReader(StreamType, StreamId);
            }
            return new YoutubeChatReader(StreamType, StreamId);
        }
        void StartReadingChat();
        void StopReadingChat();
        void PauseReadingChat();
        void ResumeReadingChat();
    }
}
