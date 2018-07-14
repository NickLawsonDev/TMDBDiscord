using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using TMDbLib.Client;
using TMDbLib.Objects.Authentication;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
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

        public async Task<EmbedBuilder> SearchMovie(string movie)
        {
            var embed = new EmbedBuilder();

            Movie details;
            SearchMovie result;
            try
            {
                result = FindSingleMovie(movie).Result;
                details = await Client.GetMovieAsync(result.Id, MovieMethods.Credits);
            }
            catch (Exception e)
            {
                return new EmbedBuilder().WithTitle("No movie found. Please try a different search term or movies");
            }
            var builder = new EmbedBuilder();

            builder.AddInlineField("Title", result.Title);
            builder.AddInlineField("Release Year", result.ReleaseDate.Value.Year);
            builder.AddInlineField("Director", details.Credits.Crew.Where(w => w.Job == "Director").First().Name);
            builder.AddInlineField("URL", $"https://www.themoviedb.org/movie/{result.Id}-{result.Title.Replace(" ", "-")}language=en-US");
            builder.WithImageUrl($"https://image.tmdb.org/t/p/w92{result.PosterPath}");

            return builder;
        }

        public async Task<List<EmbedBuilder>> SearchMultipleMovies(string searchTerm)
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
                MakeMovieEmbed(5, results, builder, searchTerm, builders);
            }
            else
            {
                MakeMovieEmbed(results.Results.Count, results, builder, searchTerm, builders);
            }

            if (results.Results.Count > 5)
                builder.AddField($"Plus {results.Results.Count - 5} more at ", $"https://www.themoviedb.org/search?language=en-US&query={searchTerm.Replace(" ", "-")}");

            builders.Add(builder);

            return builders;
        }

        public async Task<EmbedBuilder> SearchPerson(string name)
        {
            var embed = new EmbedBuilder();
            SearchContainer<SearchPerson> results = await Client.SearchPersonAsync(name);

            Console.WriteLine($"Got { results.Results.Count } results returned");

            SearchPerson result;
            try
            {
                result = results.Results.Where(w => w.Name.ToLower() == name.ToLower()).First();
            }
            catch (Exception e)
            {
                return new EmbedBuilder().WithTitle("No person found. Please try a different search term or people");
            }
            var builder = new EmbedBuilder();

            var sb = new StringBuilder();
            for(var i = 0; i < result.KnownFor.Count; i++)
            {
                KnownForMovie known = (KnownForMovie)result.KnownFor[i];

                if(i != result.KnownFor.Count-1)
                    sb.Append(known.Title + ", ");
                else
                    sb.Append(known.Title);
            }

            builder.AddInlineField("Name", result.Name);
            builder.AddInlineField("Known For", sb.ToString());
            builder.AddInlineField("URL", $"https://www.themoviedb.org/person/{result.Id}-{result.Name.Replace(" ", "-")}language=en-US");
            builder.WithImageUrl($"https://image.tmdb.org/t/p/w92{result.ProfilePath}");

            return builder;
        }

        public async Task<List<EmbedBuilder>> SearchPeople(string searchTerm)
        {
            var builders = new List<EmbedBuilder>();
            SearchContainer<SearchPerson> results = await Client.SearchPersonAsync(searchTerm);

            Console.WriteLine($"Got { results.Results.Count } results returned");

            if (results.Results.Count == 0)
            {
                builders.Add(new EmbedBuilder() { Title = "No results found. Try a different term" });
                return builders;
            }

            var builder = new EmbedBuilder();

            if (results.Results.Count >= 5)
            {
                MakePeopleEmbed(5, results, builder, searchTerm, builders);
            }
            else
            {
                MakePeopleEmbed(results.Results.Count, results, builder, searchTerm, builders);
            }

            if (results.Results.Count > 5)
                builder.AddField($"Plus {results.Results.Count - 5} more at ", $"https://www.themoviedb.org/search?language=en-US&query={searchTerm.Replace(" ", "-")}");

            builders.Add(builder);

            return builders;
        }

        public async Task<List<EmbedBuilder>> Similar(string searchTerm)
        {
            var builders = new List<EmbedBuilder>();
            var builder = new EmbedBuilder();            

            SearchMovie result;
            SearchContainer<SearchMovie> results = new SearchContainer<SearchMovie>();
            try
            {
                result = FindSingleMovie(searchTerm).Result;
                results = await Client.GetMovieRecommendationsAsync(result.Id);
            }
            catch(Exception e)
            {
                builders.Add(new EmbedBuilder() { Title = "No results found. Try a different term" });
            }

            if (results.Results.Count >= 5)
            {
                MakeMovieEmbed(5, results, builder, searchTerm, builders);
            }
            else
            {
                MakeMovieEmbed(results.Results.Count, results, builder, searchTerm, builders);
            }

            if (results.Results.Count > 5)
                builder.AddField($"Plus {results.Results.Count - 5} more at ", $"https://www.themoviedb.org/search?language=en-US&query={searchTerm.Replace(" ", "-")}");

            builders.Add(builder);

            return builders;
        }

        public EmbedBuilder Help()
        {
            var builder = new EmbedBuilder();
            var footer = new EmbedFooterBuilder()
            {
                Text = "Open source at https://github.com/NickLawsonDev/TMDBDiscord"
                
            };
            builder.WithTitle("Use !t to access the bot");
            builder.AddField("!t movie 'MOVIE NAME'", "Searches for a movie");
            builder.AddField("!t movies 'SEARCH TERM'", "Searches for movies with a similar name to the search term");
            builder.AddField("!t person 'NAME'", "Searches for a person");
            builder.AddField("!t people 'SEARCH TERM'", "Searches for people with a similar name to the search term");
            builder.AddField("!t similar 'MOVIE NAME'", "Searches for movies that are similar to the movie");
            builder.Footer = footer;

            return builder;
        }

        private void MakeMovieEmbed(int count, SearchContainer<SearchMovie> results, EmbedBuilder builder, string searchTerm, List<EmbedBuilder> builders)
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

        private void MakePeopleEmbed(int count, SearchContainer<SearchPerson> results, EmbedBuilder builder, string searchTerm, List<EmbedBuilder> builders)
        {
            for (var i = 0; i < count; i++)
                {
                    try
                    {
                        if (results.Results[i].Name != string.Empty)
                        {
                            builder.AddField("Title", results.Results[i].Name);
                            if(results.Results.Count <= 5)
                                builder.AddField("URL",$"https://www.themoviedb.org/movie/{results.Results[i].Id}-{results.Results[i].Name.Replace(" ", "-")}language=en-US");
                        }

                        builder.Title = $"Search Results for {searchTerm}";
                    }
                    catch (Exception e)
                    {
                        builders.Add(new EmbedBuilder() { Title = $"Title:{results.Results[i].Name}" });
                    }
                }
        }

        private async Task<SearchMovie> FindSingleMovie(string title)
        {
            SearchContainer<SearchMovie> results = await Client.SearchMovieAsync(title);

            Console.WriteLine($"Got { results.Results.Count } results returned");

            SearchMovie result;
            try
            {
                result = results.Results.Where(w => w.Title.ToLower() == title.ToLower()).First();
            }
            catch (Exception e)
            {
                return null;
            }

            return result;
        }
    }
}
