using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.WebSocket;


namespace CraigBot
{
    class Program
    {
        private DiscordSocketClient discordClient;

        //private DiscordSocketConfig discordConfig;



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
            var discordConfig = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            discordClient = new DiscordSocketClient(discordConfig);

            discordClient.Log += Log;
            discordClient.Ready += ReadyAsync;
            discordClient.MessageReceived += MessageReceivedAsync;
        }

        public async Task MainAsync()
        {
            await discordClient.LoginAsync(TokenType.Bot, config["Settings:DiscordToken"]);
            await discordClient.StartAsync();

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

            //var following = await twitterClient.GetInfoTweetStreamAsync();
            
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            var eyesEmoji = new Emoji("\uD83D\uDC40");
            // The bot should never respond to itself.
            if (message.Author.Id == discordClient.CurrentUser.Id)
                return;

            //respond to the word "weekend" with :eyes:
            if (message.Content.ToLower().Contains("weekend"))
                //var emoji = new Emoji("\uD83D\uDC40");
                //await message.Channel.SendMessageAsync("hello?");
                await message.AddReactionAsync(eyesEmoji);
                Console.WriteLine("Reacted to a message containing 'weekend': " + message.Author.Id);
        }
    }
}