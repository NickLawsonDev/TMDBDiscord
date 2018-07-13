using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace TMDB
{
    [Group("t")]
    public class TMDBCommand : ModuleBase
    {   
        private APICLient api = new APICLient();

        [Command("movie"), Summary("Searches for a movie in the TMDB Api")]
        public async Task Search([Summary("The name of the movie")]string movie)
        {
            //var token = await api.Client.AuthenticationRequestAutenticationTokenAsync();
            var result = await api.Search(movie);

            await Context.Channel.SendMessageAsync("", false, result);
        }

        [Command("movies"), Summary("Searches for multiple movie in the TMDB Api")]
        public async Task SearchMultiple([Summary("The search term")]string term)
        {
            var results = await api.SearchMultpleMovies(term);

            foreach(var result in results)
            {
                await Context.Channel.SendMessageAsync("", false, result);
            }
        }
    }
}