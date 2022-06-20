using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.WebSocket;
using Tweetinvi;
using Tweetinvi.Streaming;


namespace CraigBot
{
    class Program
    {
        private DiscordSocketClient discordClient;
        public TwitterClient twitterClient;
        IMessageChannel channel;

        IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

        static void Main(string[]args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public Program()   
        {
            
            //DISCORD STUFF
            discordClient = new DiscordSocketClient();

            discordClient.Log += Log;
            discordClient.Ready += ReadyAsync;
            discordClient.MessageReceived += MessageReceivedAsync;

            //TWITTER STUFF
            twitterClient = new TwitterClient
            (
                config["Settings:TwitterKeys:ConsumerKey"],
                config["Settings:TwitterKeys:ConsumerSecret"],
                config["Settings:TwitterKeys:AccessToken"],
                config["Settings:TwitterKeys:AccessTokenSecret"]
            );
        }

        public async Task MainAsync()
        {
            await discordClient.LoginAsync(TokenType.Bot, config["Settings:DiscordToken"]);
            await discordClient.StartAsync();

            var stream = twitterClient.Streams.CreateFilteredStream();
            
            //var twitterUser = twitterClient.Users.GetUserAsync(config["Settings:Settings:TwitterScreenName"]);

            stream.AddFollow(1288226864457084928);

            stream.MatchingTweetReceived += async(sender, arguments) =>
            {
                if(arguments.Tweet.InReplyToStatusId == null
                && arguments.Tweet.IsRetweet == false
                && arguments.Tweet.CreatedBy.ScreenName == "CraigWeekend")
                {
                    Console.WriteLine("I detected a tweet");
                    await channel.SendMessageAsync
                    ($"https://twitter.com/{arguments.Tweet.CreatedBy.ScreenName}/status/{arguments.Tweet.Id}");
                }
            };

            await stream.StartMatchingAllConditionsAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public Task ReadyAsync()
        {
            Console.WriteLine($"{discordClient.CurrentUser} is connected!");

            //Get Bot Testing channel
            var discordServer = discordClient.GetGuild(ulong.Parse(config["Settings:Botsettings:ServerID"]));
            Console.WriteLine(discordServer);

            channel = discordServer.GetTextChannel(ulong.Parse(config["Settings:Botsettings:ChannelID"])) as IMessageChannel;
            Console.WriteLine(channel);
            //channel.SendMessageAsync("Hello world!");
            
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // The bot should never respond to itself.
            if (message.Author.Id == discordClient.CurrentUser.Id)
                return;

            //respond to the word "weekend" with :eyes:
            if (message.Content.Contains("weekend")||message.Content.Contains("Weekend"))
                await message.AddReactionAsync(new Emoji("\uD83D\uDC40"));
                Console.WriteLine("Reacted to a message containing 'weekend'");
        }
    }
}