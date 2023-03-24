using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace StreamingServices.Youtube
{
    internal class YoutubeJson
    {
        public string authorName { get; set; }
        public string authorPhoto { get; set; }
        public string message { get; set; }
        public string timestamp { get; set; }
        public string id { get; set; }
        public string channelId { get; set; }
        public string purchaseAmount { get; set; }

        [JsonProperty("memberInfo")]
        [JsonConverter(typeof(StringArrayConverter<string>))]
        public IEnumerable<string> memberInfo { get; set; }
        public YoutubeJson()
        {
            this.authorName = string.Empty;
            this.authorPhoto = string.Empty;
            this.message = string.Empty;
            this.timestamp = string.Empty;
            this.id = string.Empty;
            this.channelId = string.Empty;
            this.purchaseAmount = string.Empty;
            this.memberInfo = new List<string>();
        }
        public static YoutubeJson Parse(JObject j) => j.ToObject<YoutubeJson>();
    }
    internal class StringArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<T>));
        }

        public override object ReadJson(
          JsonReader reader,
          Type objectType,
          object existingValue,
          JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
                return token.ToObject<List<T>>();
            return new List<T> { token.ToObject<T>() };
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
