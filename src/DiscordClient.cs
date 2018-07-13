using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace TMDB
{
    public class DiscordClient
    {
        private DiscordSocketClient Client { get; set; }
        private CommandService Commands { get; set; }
        private IServiceProvider Services { get; set; }

        public DiscordClient() 
        {
            Client = new DiscordSocketClient();
            Commands = new CommandService();
            Services = new ServiceCollection()
                .BuildServiceProvider();

            Client.Log += Log;
            Client.MessageReceived += MessageReceived;
            Run();
        }

        public async void Run() 
        {
            string token = "NDY3MDk4NTUzNDI5NTI0NTIw.DilszQ.4Wr0JlWGAaGqqQ4X0KdG0IGCB_U"; // Remember to keep this private!

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();    
            await InstallCommands();
        }

        public async Task InstallCommands()
        {
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        #region Events

        private async Task MessageReceived(SocketMessage msg)
        {
           // Don't process the command if it was a System Message
            var message = msg as SocketUserMessage;
            if(message == null) return;
            
            // Create a number to track where the prefix ends and the command begins
            var argPos = 0;

            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if(!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos))) return;

            // Create a Command Context
            var context = new CommandContext(Client, message);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await Commands.ExecuteAsync(context, argPos);
            if(!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        #endregion
    }
}