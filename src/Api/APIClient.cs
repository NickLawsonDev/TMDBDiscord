using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using TMDbLib.Client;
using TMDbLib.Objects.Authentication;
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

            SearchMovie result;
            try
            {
                result = results.Results.Where(w => w.Title.ToLower() == movie.ToLower()).First();
            }
            catch (Exception e)
            {
                return new EmbedBuilder().WithTitle("No movie found. Please try a different search term or searchmultiple");
            }
            var builder = new EmbedBuilder();
            //var rating = await Client.GetMovieAccountStateAsync(result.Id);

            builder.AddInlineField("Title", result.Title);
            builder.AddInlineField("Release Year", result.ReleaseDate.Value.Year);
            builder.AddInlineField("Avg Popularity", Math.Round(result.Popularity) + "%");
            builder.AddInlineField("URL", $"https://www.themoviedb.org/movie/{result.Id}-{result.Title.Replace(" ", "-")}language=en-US");
            builder.WithImageUrl($"https://image.tmdb.org/t/p/w500{result.PosterPath}");

            return builder;
        }

        public async Task<List<EmbedBuilder>> SearchMultpleMovies(string searchTerm)
        {
            var builders = new List<EmbedBuilder>();
            SearchContainer<SearchMovie> results = await Client.SearchMovieAsync(searchTerm);

            Console.WriteLine($"Got { results.Results.Count } results returned");

            if (results.Results.Count == 0)
            {
                builders.Add(new EmbedBuilder() { Title = "No results found. Try a different term" });
                return builders;
            }

            var builder = new EmbedBuilder();

            if (results.Results.Count >= 5)
            {
                MakeMoviesEmbed(5, results, builder, searchTerm, builders);
            }
            else
            {
                MakeMoviesEmbed(results.Results.Count, results, builder, searchTerm, builders);
            }

            if (results.Results.Count > 5)
                builder.AddField($"Plus {results.Results.Count - 5} more at ", $"https://www.themoviedb.org/search?language=en-US&query={searchTerm.Replace(" ", "-")}");

            builders.Add(builder);

            return builders;
        }

        private void MakeMoviesEmbed(int count, SearchContainer<SearchMovie> results, EmbedBuilder builder, string searchTerm, List<EmbedBuilder> builders)
        {
            for (var i = 0; i < count; i++)
                {
                    try
                    {
                        if (results.Results[i].Title != string.Empty)
                        {
                            builder.AddField("Title", results.Results[i].Title);
                            if(results.Results.Count <= 5)
                                builder.AddField("URL",$"https://www.themoviedb.org/movie/{results.Results[i].Id}-{results.Results[i].Title.Replace(" ", "-")}language=en-US");
                        }

                        builder.Title = $"Search Results for {searchTerm}";
                    }
                    catch (Exception e)
                    {
                        builders.Add(new EmbedBuilder() { Title = $"Title:{results.Results[i].Title}" });
                    }
                }
        }
    }
}
