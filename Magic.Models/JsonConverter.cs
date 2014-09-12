using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Magic.Models
{
    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        /// <summary>
        /// Create an instance of objectType, based properties in the JSON object.
        /// </summary>
        /// <param name="objectType">Type of object expected</param>
        /// <param name="jObject">Contents of JSON object that will be deserialized</param>
        protected abstract T Create(Type objectType, JObject jObject);
        
        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Serializing to JSON was not needed and has not yet been implemented.");
        }
    }

    public class CardConverter : JsonCreationConverter<Card>
    {
        protected override Card Create(Type objectType, JObject jObject)
        {
            if (((string)jObject["type"]).Contains("Creature"))
            {
                return new CreatureCard();
            }
            if (((string)jObject["type"]).Contains("Planeswalker"))
            {
                return new PlaneswalkerCard();
            }
            return new Card();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            System.Diagnostics.Debug.WriteLine(jObject);

            // Create target object based on JObject
            var card = Create(objectType, jObject);

            var properties = jObject.Properties();

            System.Diagnostics.Debug.WriteLine(properties);

            //// Populate C# object with according JObject data.
            //var deserializedTweet = new Tweet()
            //{
            //    Id = (string)jsonObject.SelectToken("id_str"),
            //    Published = ((string)jsonObject.SelectToken("created_at")).ParseTwitterTime(),
            //    Updated = ((string)jsonObject.SelectToken("updated_at") ?? (string)jsonObject.SelectToken("created_at")).ParseTwitterTime(),
            //    Text = (string)jsonObject.SelectToken("text"),
            //    Statistics = new TweetStatistics()
            //    {
            //        RetweetsCount = (int)jsonObject.SelectToken("retweet_count"),
            //        FavouritesCount = (int)jsonObject.SelectToken("favorite_count")
            //    },
            //    UserId = (string)jsonObject.SelectToken("user").SelectToken("id_str")
            //};

            //if (Tweet.IncludeUserDetails)
            //{
            //    deserializedTweet.User = new TwitterUser()
            //    {
            //        Id = (string)jsonObject.SelectToken("user").SelectToken("id_str"),
            //        Name = (string)jsonObject.SelectToken("user").SelectToken("name"),
            //        ScreenName = "@" + (string)jsonObject.SelectToken("user").SelectToken("screen_name")
            //    };
            //    deserializedTweet.User.Link = "https://www.twitter.com/" + deserializedTweet.User.ScreenName;
            //    // Add a User object associated with the tweet.
            //    //var jsonSubObject = jsonObject.SelectToken("user");

            //    //deserializedTweet.User = new TwitterUser()
            //    //{
            //    //    Id = (string)jsonSubObject.SelectToken("id_str"),
            //    //    Name = (string)jsonSubObject.SelectToken("name"),
            //    //    ScreenName = "@" + (string)jsonSubObject.SelectToken("screen_name"),
            //    //    Description = (string)jsonSubObject.SelectToken("description"),
            //    //    Joined = ((string)jsonSubObject.SelectToken("created_at")).ParseTwitterTime(),
            //    //    //Url = new Link((string) jsonSubObject.SelectToken("entities").SelectToken("url").SelectToken("urls").First.SelectToken("url")),
            //    //    Statistics = new UserStatistics()
            //    //    {
            //    //        FollowersCount = (int)jsonSubObject.SelectToken("followers_count"),
            //    //        FriendsCount = (int)jsonSubObject.SelectToken("friends_count"),
            //    //        ListedCount = (int)jsonSubObject.SelectToken("listed_count"),
            //    //        TweetStatusesCount = (int)jsonSubObject.SelectToken("statuses_count")
            //    //    },
            //    //    Profile = new UserProfile()
            //    //    {
            //    //        ImageUrl = ((string)jsonSubObject.SelectToken("profile_image_url")).Replace("_normal", ""),
            //    //        ImageInTweetUrl = (string)jsonSubObject.SelectToken("profile_image_url"),
            //    //        BackgroundImageUrl = (string)jsonSubObject.SelectToken("profile_background_image_url"),
            //    //        BackgroundColor = (string)jsonSubObject.SelectToken("profile_background_color"),
            //    //        LinkColor = (string)jsonSubObject.SelectToken("profile_link_color"),
            //    //        TextColor = (string)jsonSubObject.SelectToken("profile_text_color"),
            //    //        SidebarFillColor = (string)jsonSubObject.SelectToken("profile_sidebar_fill_color"),
            //    //        SidebarBorderColor = (string)jsonSubObject.SelectToken("profile_sidebar_border_color")
            //    //    }
            //    //};
            //}
            return card;
        }
    }
}