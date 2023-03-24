using System;
using Newtonsoft.Json;
using StreamingServices.Youtube;
using Newtonsoft.Json.Converters;
using StreamingServices.Chat.Money;
using Newtonsoft.Json.Linq;

namespace StreamingServices.Chat
{
    public delegate void ChatEventHandler(ChatEventArgs e);
    public class ChatEventArgs : EventArgs
    {
        public string Message { get;  init; }
        public string ChatId { get;  init; }
        public ChatAuthor Author { get; init; }
        public double DontationAmount { get; init; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DonationType DontationType { get; init; }
        public ChatEventArgs(string m, string cid, ChatAuthor a, int da, DonationType dt, bool isMemeber, int memeberLevel)
        {
            this.Message = m;
            this.ChatId = cid;
            this.Author = a;
            this.DontationAmount = da;
            this.DontationType = dt;
        }
        internal ChatEventArgs(YoutubeJson youtube)
        {
            this.Message = youtube.message;
            this.ChatId = youtube.id;

            this.Author = new(
                youtube.authorName,
                youtube.channelId,
                youtube.memberInfo);

            if (youtube.purchaseAmount.Length > 2)
            {
                double amount = MoneyConverter.Convert(youtube.purchaseAmount);
                this.DontationAmount = amount;
                this.DontationType = DonationType.USD;
            }
        }
        internal ChatEventArgs(JObject json) : this(YoutubeJson.Parse(json)) { }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}