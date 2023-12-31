﻿using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace data_base_practicum
{
    public class Program
    {
        static Dictionary<string, Movie> films = new Dictionary<string, Movie>();  // название: фильм
        static Dictionary<string, List<Movie>> people = new Dictionary<string, List<Movie>>();  // имя учатсника: фильмы
        static Dictionary<string, List<Movie>> tags_dict = new Dictionary<string, List<Movie>>();  // тэг: фильмы
        static string dataset_path = @"C:\Универ\ml-latest\";

        static Dictionary<string, Person>? static_result_people;
        static Dictionary<string, List<string>>? global_films_id_name;

        static Dictionary<string, List<string>> top10_dict = new Dictionary<string, List<string>>();
        static List<MovieTop10> top10_classes = new List<MovieTop10>();


        static void Main(string[] args)
        {
            make_answer_dicts();
            make_top10();
        }

        static void make_top10()
        {
            Console.WriteLine("Saving top10 to BD started");
            using (AppContextTop10 db = new AppContextTop10())
            {
                foreach (var cur_film in films.Keys)
                {
                    List<string> films_top10 = new List<string>();
                    foreach (var film_name in films.Keys.AsParallel())
                    {
                        if (film_name == cur_film)
                            continue;
                        float unionCount = 0;
                        float intersectionCount = 0;

                        foreach (var item in films[cur_film].actors)
                        {
                            if (films[film_name].actors.Contains(item))
                                intersectionCount += 1;
                            unionCount += 1;
                        }
                        foreach (var item in films[film_name].actors)
                        {
                            if (!films[cur_film].actors.Contains(item))
                                unionCount += 1;
                        }

                        foreach (var item in films[cur_film].tags)
                        {
                            if (films[film_name].tags.Contains(item))
                                intersectionCount += 1;
                            unionCount += 1;
                        }
                        foreach (var item in films[film_name].tags)
                        {
                            if (!films[cur_film].tags.Contains(item))
                                unionCount += 1;
                        }

                        foreach (var item in films[cur_film].directors)
                        {
                            if (films[film_name].directors.Contains(item))
                                intersectionCount += 1;
                            unionCount += 1;
                        }
                        foreach (var item in films[film_name].directors)
                        {
                            if (!films[cur_film].directors.Contains(item))
                                unionCount += 1;
                        }

                        if (intersectionCount / unionCount >= 0.7)
                        {
                            films_top10.Add(film_name);
                            if (films_top10.Count >= 10)
                                break;
                        }
                    }
                    top10_dict[cur_film] = films_top10;
                    
                    var top10_ex = new MovieTop10(cur_film);
                    string res_top10 = "";
                    int i = 1;
                    foreach (var val in films_top10)
                    {
                        res_top10 += $"{i}) {val} ";
                        i += 1;
                    }
                    top10_ex.top10_movies = res_top10;

                    var elem = new MovieTop10(cur_film);
                    elem.top10_movies = res_top10;
                    top10_classes.Add(elem);
                }
                db.Top10.BulkInsert(top10_classes, options => options.IncludeGraph = true);
                db.SaveChanges();
            }
            Console.WriteLine("Saving top10 to BD finished");
        }

        static void take_from_bd(string type, string cur_name)
        {
            if (type == "a")
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    var result = db.Movies
                        .Where(x => x.name == cur_name)
                        .FirstOrDefault();
                    if (result != null)
                    {
                        Console.WriteLine($"Film {cur_name} has got rating {result.rating}");
                        Console.WriteLine($"Actors: {result.actors_str}");
                        Console.WriteLine($"Directors: {result.directors_str}");
                    }
                    else
                    {
                        Console.WriteLine("Not found in Data Base");
                    }
                }
            }
            if (type == "b")
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    var result = db.Humans
                        .Where(x => x.name == cur_name)
                        .FirstOrDefault();
                    if (result != null)
                    {
                        Console.WriteLine($"{cur_name}s films: {result.actor_movies_names} and {result.director_movies_names}");
                    }
                    else
                    {
                        Console.WriteLine("Not found in Data Base");
                    }
                }
            }
            if (type == "c")
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    var result = db.Tags
                        .Where(x => x.text == cur_name)
                        .FirstOrDefault();
                    if (result != null)
                    {
                        Console.WriteLine($"Tag {cur_name} films: {result.movies_str}");
                    }
                    else
                    {
                        Console.WriteLine("Not found in Data Base");
                    }
                }
            }
        }
        static void uploading_database_movies()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                Parallel.ForEach(films.Values, mov =>
                {
                    mov.iter_to_actors(mov.actors);
                    mov.iter_to_directors(mov.directors);
                    mov.iter_to_tags(mov.tags);
                    db.Movies.Add(mov);
                });
                Console.WriteLine("Movies saving in process...");
                db.SaveChanges();
            }
            Console.WriteLine("Movies saved correctly");
        }
        static void uploading_database_tags()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                Parallel.ForEach(tags_dict.Keys, key =>
                {
                    Tag tag = new Tag(key);
                    tag.list_to_movies(tags_dict[key]);
                    db.Tags.Add(tag);
                });
                Console.WriteLine("Tags saving in process...");
                db.SaveChanges();
            }
            Console.WriteLine("Tags saved correctly");
        }
        static void uploading_database_persons()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                int i = 0;
                Parallel.ForEach(static_result_people.Values, per =>
                {
                    if (!(per == null || (per.actor_movis_id.Count == 0 && per.director_movies_id.Count == 0)))
                    {
                        per.list_withIDfilms_to_string(global_films_id_name, per.actor_movis_id, true);
                        per.list_withIDfilms_to_string(global_films_id_name, per.director_movies_id, false);
                        db.Humans.Add(per);

                        if (i % 100000 == 0)
                            Console.WriteLine($"{i} persons saved");
                        i += 1;
                        db.SaveChanges();
                    }
                    
                });
                Console.WriteLine("Persons saving in process...");
            }
            Console.WriteLine("Persons saved correctly");
        }
        
        //static void testing_DB()
        //{
        //    using (ApplicationContext db = new ApplicationContext())
        //    {
        //        Movie term = new Movie { name = "Terminator", MovieId = "1" };
        //        term.rating = "6.1";
        //        term.actors = new HashSet<string>() { "James Cameron", "Linda Hamilton", "Arnold Schwarzenegger" };
        //        term.directors = new HashSet<string>() { "James Cameron", "Ryan McDonald", "Ben Hernandez" };
        //        term.actors_str = iter_to_string(term.actors);
        //        term.directors_str = iter_to_string(term.directors);

        //        Movie term1 = new Movie { name = "Terminator", MovieId = "1" };
        //        term1.rating = "6.1";
        //        term1.actors = new HashSet<string>() { "James Cameron", "Linda Hamilton", "Arnold Schwarzenegger" };
        //        term1.directors = new HashSet<string>() { "James Cameron", "Ryan McDonald", "Ben Hernandez" };
        //        term1.actors_str = iter_to_string(term.actors);
        //        term1.directors_str = iter_to_string(term.directors);


        //        Tag tag = new Tag("comedy");
        //        tag.movies_str = list_to_string(new List<Movie>() { new Movie("дедпул"), new Movie("дедпул 2") });

        //        Person per = new Person("reynolds rayan");
        //        per.actor_movies_names = list_to_string(new List<Movie>() { new Movie("дедпул"), new Movie("дедпул 2") });
        //        per.director_movies_names = list_to_string(new List<Movie>() { new Movie("drive") });
                
        //        //db.Humans.Add(per);
        //        //db.Tags.Add(tag);
        //        //db.Movies.Add(term);
        //        //db.Movies.Add(term1);
                
        //        db.SaveChanges();
        //        Console.WriteLine("Test objects saved correctly");
        //    }
        //}
        static void while_true_answer()
        {
            while (true)
            {
                Console.WriteLine("a - фильмы, b - люди, c - тэги");
                string mode = Console.ReadLine();
                switch (mode)
                {
                    case "a":
                        string movie_name = Console.ReadLine();
                        if (!films.ContainsKey(movie_name))
                            Console.WriteLine("Указанный фильм не найден");
                        else
                        {
                            var result = films[movie_name];
                            Console.WriteLine($"Фильм {result.name} с рейтингом {result.rating}");
                            Console.WriteLine($"располагает следующими тэгами: {print_iter(result.tags)}");
                            Console.WriteLine($"и актёрами: {print_iter(result.actors)}");
                            Console.WriteLine($"режиссёры - {print_iter(result.directors)}");
                        }
                        break;
                    case "b":
                        string person_name = Console.ReadLine();
                        if (!people.ContainsKey(person_name))
                        {
                            Console.WriteLine("Указанный человек не найден");
                        }
                        else
                        {
                            int num = 1;
                            Console.WriteLine($"Человек с именем {person_name} участвовал в следующих проектах:");
                            foreach (var cur_film in people[person_name])
                            {
                                Console.Write($"{num}) {cur_film.name}  ");
                                num += 1;
                            }
                            Console.WriteLine();
                        }
                        break;
                    case "c":
                        string tag_name = Console.ReadLine();
                        if (!tags_dict.ContainsKey(tag_name))
                        {
                            Console.WriteLine("Указанный тэг не найден");
                        }
                        else
                        {
                            Console.WriteLine($"Тэг {tag_name} присутствует в следующих фильмах:");
                            int num = 1;
                            foreach (var cur_film in tags_dict[tag_name])
                            {
                                Console.Write($"{num}) " + cur_film.name + "  ");
                                num += 1;
                            }
                            Console.WriteLine();
                        }
                        break;
                }
            }
        }
        
        static void make_answer_dicts()
        {
            Console.WriteLine("Processing of movie data");
            Dictionary<string, List<string>> id_name = new Dictionary<string, List<string>>();  // id фильма: название фильма
            // наполнение словарей films и id_name
            using (StreamReader reader = new StreamReader(dataset_path + "MovieCodes_IMDB.tsv"))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    int zero = line.IndexOf("\t"), one = line.IndexOf("\t", zero + 1),
                        two = line.IndexOf("\t", one + 1), three = line.IndexOf("\t", two + 1), four = line.IndexOf("\t", three + 1);
                    string film_id = line[0..zero], title = line[(one + 1)..two],
                        region = line[(two + 1)..three], language = line[(three + 1)..four];

                    if (region == "RU" || region == "US" || language == "RU" || language == "US")
                    {
                        if (!films.ContainsKey(title))
                        {
                            films[title] = new Movie { name = title, MovieId = film_id };
                        }
                        if (!id_name.ContainsKey(film_id))
                            id_name[film_id] = new List<string>();
                        id_name[film_id].Add(title);
                    }
                }
            }
            global_films_id_name = id_name;
            Console.WriteLine("The initial review of the films is done.");


            // добавление рейтинга во все фильмы
            using (StreamReader reader = new StreamReader(dataset_path + "Ratings_IMDB.tsv"))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    int one = line.IndexOf("\t"), two = line.IndexOf("\t", one + 1);
                    string film_id = line[0..one], rating = line[(one + 1)..two];
                    if (id_name.ContainsKey(film_id))
                    {
                        foreach (var name in id_name[film_id])
                            films[name].rating = rating;
                    }
                }
            }
            Console.WriteLine("Rating done.");


            Dictionary<string, List<string>> result_tags = make_tags(id_name);
            foreach (var film_name in result_tags.Keys.AsParallel())
            {
                films[film_name].tags = result_tags[film_name].ToHashSet<string>();
            }
            Console.WriteLine("Make tags done.");

            // наполнение фильмов их актёрами и режиссёрами
            Dictionary<string, Person> result_people = make_people(id_name);
            static_result_people = result_people;
            foreach (var per in result_people.Values.AsParallel())
            {
                foreach (var movie_id in per.actor_movis_id)
                {
                    if (!id_name.ContainsKey(movie_id))
                        continue;
                    foreach (var movie_name in id_name[movie_id])
                        films[movie_name].actors.Add(per.name);
                }
                foreach (var movie_id in per.director_movies_id)
                {
                    if (!id_name.ContainsKey(movie_id))
                        continue;
                    foreach (var movie_name in id_name[movie_id])
                        films[movie_name].directors.Add(per.name);
                }
            }
            Console.WriteLine("Make people done");

            // финальные приготовления
            // наполнение второго словаря
            foreach (var per in result_people.Values.AsParallel())
            {
                people[per.name] = new List<Movie>();
                foreach (var mov_id in per.movies_id)
                {
                    if (!id_name.ContainsKey(mov_id))
                        continue;
                    foreach (var mov_name in id_name[mov_id])
                        people[per.name].Add(films[mov_name]);
                }
            }
            Console.WriteLine("Dictionary with persons is ready.");

            // наполнение третьего словаря, когда все классы фильмов заполнены
            foreach (var film_name in result_tags.Keys.AsParallel())
            {
                foreach (var tag in result_tags[film_name])
                {
                    if (!tags_dict.ContainsKey(tag))
                        tags_dict[tag] = new List<Movie>();
                    tags_dict[tag].Add(films[film_name]);
                }
            }
            Console.WriteLine("Dictionary with tags is ready.");
        }
        static string print_iter(IEnumerable<string> iterable)
        {
            int i = 0;
            string st = "";
            foreach (var elem in iterable)
            {
                st += $"{i + 1}) {elem}  ";
                i += 1;
            }
            return st;
        }
        static Dictionary<string, List<string>> make_tags(Dictionary<string, List<string>> films_id_name)
        {
            Console.WriteLine("Start make tags.");
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>(); // film name: [all tags]
            Dictionary<string, string> MovLens_IMDB = new Dictionary<string, string>();
            Dictionary<string, string> tag_dict = new Dictionary<string, string>(); // tag_id: tag


            using (StreamReader reader = new StreamReader(dataset_path + "links_IMDB_MovieLens.csv"))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    int one = line.IndexOf(",");
                    MovLens_IMDB[line[0..one]] = "tt" + line[(one + 1)..(line.IndexOf(",", one + 1))];
                }
            }
            Console.WriteLine("Make tags done 1/3.");


            using (StreamReader reader = new StreamReader(dataset_path + "TagCodes_MovieLens.csv"))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    int zero = line.IndexOf(",");
                    tag_dict[line[0..zero]] = line[(zero + 1)..];
                }
            }
            Console.WriteLine("Make tags done 2/3.");

            using (StreamReader reader = new StreamReader(dataset_path + "TagScores_MovieLens.csv"))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    int zero = line.IndexOf(","), one = line.IndexOf(",", zero + 1);
                    string tagid = line[(zero + 1)..one], rel = line[(one + 1)..], MovLens_id = line[0..zero];

                    string IMDB_id = MovLens_IMDB[MovLens_id];
                    int relevants;
                    if (rel.Length < 4)
                        rel += "0";
                    relevants = Convert.ToInt32(rel.Substring(2, 2));
                    if (relevants > 50)
                    {
                        if (!films_id_name.ContainsKey(IMDB_id))
                            continue;
                        foreach (var film_name in films_id_name[IMDB_id])
                        {
                            if (!result.ContainsKey(film_name))
                                result[film_name] = new List<string>();
                            result[film_name].Add(tag_dict[tagid]);
                        }

                    }
                }
            }
            Console.WriteLine("Make tags done 3/3.");

            return result;
        }
        static Dictionary<string, Person> make_people(Dictionary<string, List<string>> films_id_name)
        {
            Console.WriteLine("Start make people.");
            Dictionary<string, Person> persons = new Dictionary<string, Person>();  // id: Person

            using (StreamReader reader = new StreamReader(dataset_path + "ActorsDirectorsNames_IMDB.txt"))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    int zero = line.IndexOf('\t'), one = line.IndexOf('\t', zero + 1), two = line.IndexOf('\t', one + 1),
                        three = line.IndexOf('\t', two + 1), four = line.IndexOf('\t', three + 1);
                    string per_id = line[0..zero], per_name = line[(zero + 1)..one], professions = line[(three + 1)..four];

                    if (professions.Contains("director") || professions.Contains("actor") || professions.Contains("actress"))
                        persons[per_id] = new Person { name = per_name, person_id = per_id };
                }
            }
            Console.WriteLine("Make people 1/2 done.");


            using (StreamReader reader = new StreamReader(dataset_path + "ActorsDirectorsCodes_IMDB.tsv"))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    int zero = line.IndexOf('\t'), one = line.IndexOf('\t', zero + 1), two = line.IndexOf('\t', one + 1),
                        three = line.IndexOf('\t', two + 1);
                    string mov_id = line[..zero], chel_id = line[(one + 1)..two], categ = line[(two + 1)..three];
                    if (!persons.ContainsKey(chel_id) || !films_id_name.ContainsKey(mov_id))
                        continue;
                    if (categ == "director")
                    {
                        persons[chel_id].director_movies_id.Add(mov_id);
                    }
                    else
                    {
                        persons[chel_id].actor_movis_id.Add(mov_id);
                    }
                    persons[chel_id].movies_id.Add(mov_id);
                }
            }
            Console.WriteLine("Make people 2/2 done.");

            return persons;
        }

    }
}