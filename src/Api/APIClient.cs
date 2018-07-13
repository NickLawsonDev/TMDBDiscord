using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;

namespace TMDB
{
    public class APICLient
    {
        private TMDbClient Client { get; set; }

        public APICLient()
        {
            Client = new TMDbClient("f2ea6889568b9ff1f36d6468c2b1c195");
        }

        public async Task<EmbedBuilder> Search(string movie)
        {
            var embed = new EmbedBuilder();
            SearchContainer<SearchMovie> results = await Client.SearchMovieAsync(movie);

            Console.WriteLine($"Got { results.Results.Count } results returned");

            if(results.Results.Count >= 1)
            {
                var result = results.Results.Where(w => w.Title == movie).First();
                var builder = new EmbedBuilder();

                builder.AddInlineField("Title", result.Title);
                builder.AddInlineField("Release Year", result.ReleaseDate.Value.Year);
                builder.AddInlineField("Avg Popularity", Math.Round(result.Popularity) + "%");
                builder.WithImageUrl($"https://image.tmdb.org/t/p/w500{result.PosterPath}");

                return builder;

            } 
            else 
            {
                return new EmbedBuilder().WithTitle("More then 1 result. Please try a different search term");
            }
        }

        public async Task<List<EmbedBuilder>> SearchMultpleMovies(string searchTerm)
        {
            var builders = new List<EmbedBuilder>();
            SearchContainer<SearchMovie> results = await Client.SearchMovieAsync(searchTerm);

            foreach(var result in results.Results)
            {
                var builder = new EmbedBuilder();

                builder.AddInlineField("Title", result.Title);
                builder.AddInlineField("Release Year", result.ReleaseDate.Value.Year);
                builder.AddInlineField("Avg Popularity", Math.Round(result.Popularity) + "%");
                builder.WithImageUrl($"https://image.tmdb.org/t/p/w500{result.PosterPath}");

                builders.Add(builder);
            }

            return builders;
        }
    }
}
