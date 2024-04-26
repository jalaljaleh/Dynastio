using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Dynastio.Bot.Database
{
    public enum AdsType
    {
        MessageContent = 0,
        InlineEmbedDescription,
        Buttons,
        UserDirectMessageContent
    }
    public class Advertise
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ulong User { get; set; }

        public string Label { get; set; }
        public string Url { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
        public AdsType Type { get; set; }
        public int Count { get; set; }
        public int DisplayCount { get; set; }

        public string Emoji { get; set; }

        public string GetEmbedLink()
        {
            return $" [{Label}]({Url}) ";
        }
        public bool HasEmoji()
        {
            return !string.IsNullOrEmpty(this.Emoji);
        }
    }
}
