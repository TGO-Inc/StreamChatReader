using StreamingServices.Chat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace StreamingServices
{
    public class ChatReader
    {
        private readonly List<IReaderBase> ChatReaders;
        public event ChatEventHandler? ChatEvent;
        public event ChatLoadedHandler? ChatLoaded;
        public ChatReader()
        {
            this.ChatReaders = new();
        }
        /// <summary>
        /// Adds a stream of a unknown source (Twitch or Youtube) to the chat callbacks
        /// </summary>
        /// <param name="stream_id"></param>
        public void AddStream(string stream_id, ChatEventHandler? e = null)
        {

        }
        /// <summary>
        /// Adds a Youtube livestream to the chat callbacks
        /// </summary>
        /// <param name="y">Youtube Id</param>
        /// <param name="e">Chat EventHandler</param>
        /// <param name="l">Chat LoadedHandler</param>
        public void AddYoutubeStream(string y, ChatEventHandler? e = null, ChatLoadedHandler? l = null)
        {
            IReaderBase youtube_chat_reader = IReaderBase.NewChatReader(StreamType.Youtube, y);
            youtube_chat_reader.ChatEvent += e ?? ChatEvent;
            youtube_chat_reader.ChatLoaded += l ?? ChatLoaded;
            ChatReaders.Add(youtube_chat_reader);
            youtube_chat_reader.StartReadingChat();
        }

        /// <summary>
        /// Adds a Twitch livestream to the chat callbacks
        /// </summary>
        /// <param name="t"></param>
        /// <param name="e">Chat EventHandler</param>
        /// <param name="l">Chat LoadedHandler</param>
        public void AddTwitchStream(string t, ChatEventHandler? e = null, ChatLoadedHandler? l = null)
        {
            IReaderBase twitch_chat_reader = IReaderBase.NewChatReader(StreamType.Youtube, t);
            twitch_chat_reader.ChatEvent += e ?? ChatEvent;
            twitch_chat_reader.ChatLoaded += l ?? ChatLoaded;
            twitch_chat_reader.StartReadingChat();
            ChatReaders.Add(twitch_chat_reader);
        }
    }
}