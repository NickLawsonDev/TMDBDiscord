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
            var result = await api.SearchMovie(movie);

            await Context.Channel.SendMessageAsync("", false, result);
        }

        [Command("movies"), Summary("Searches for multiple movie in the TMDB Api")]
        public async Task SearchMultiple([Summary("The search term")]string term)
        {
            var results = await api.SearchMultipleMovies(term);

            foreach(var result in results)
            {
                await Context.Channel.SendMessageAsync("", false, result);
            }
        }

        [Command("similar"), Summary("Searches for similar movies in the TMDB Api")]
        public async Task Similar([Summary("The search term")]string term)
        {
            var results = await api.Similar(term);

            foreach(var result in results)
            {
                await Context.Channel.SendMessageAsync("", false, result);
            }
        }

        [Command("person"), Summary("Searches for a person in the TMDB Api")]
        public async Task Person([Summary("The search term")]string term)
        {
            var result = await api.SearchPerson(term);

            await Context.Channel.SendMessageAsync("", false, result);
        }

        [Command("people"), Summary("Searches for people in the TMDB Api")]
        public async Task People([Summary("The search term")]string term)
        {
            var results = await api.SearchPeople(term);

            foreach(var result in results)
            {
                await Context.Channel.SendMessageAsync("", false, result);
            }
        }

        [Command("help"), Summary("Help message")]
        public async Task Help()
        {
            var builder = api.Help();

            await Context.Channel.SendMessageAsync("", false, builder);
        }
    }
}