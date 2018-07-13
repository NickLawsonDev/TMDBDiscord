using System;
using System.Threading.Tasks;
using TMDB;

namespace TMDBDiscord
{
    class Program
    {
        private DiscordClient Discord { get;set; }

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
		{
            Discord = new DiscordClient();

            Discord.Run();
            
            // Block this task until the program is closed.
            await Task.Delay(-1);
		}
    }
}
