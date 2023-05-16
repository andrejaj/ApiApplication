using System.Text.Json.Serialization;
using System;
using ProtoBuf;

namespace ApiApplication.API.DTO
{
    [ProtoContract]
    public class Show
    {
        [ProtoMember(1)]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [ProtoMember(3)]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [ProtoMember(2)]
        [JsonPropertyName("rank")]
        public string Rank { get; set; }

        [ProtoMember(4)]
        [JsonPropertyName("fullTitle")]
        public string FullTitle { get; set; }

        [ProtoMember(5)]
        [JsonPropertyName("year")]
        public string Year { get; set; }

        [ProtoMember(7)]
        [JsonPropertyName("crew")]
        public string Crew { get; set; }

        [ProtoMember(6)]
        [JsonPropertyName("image")]
        public string Image { get; set; }

        [ProtoMember(8)]
        [JsonPropertyName("imDbRating")]
        public string ImDbRating { get; set; }

        [ProtoMember(9)]
        [JsonPropertyName("imDbRatingCount")]
        public string ImDbRatingCount { get; set; }
    }
}