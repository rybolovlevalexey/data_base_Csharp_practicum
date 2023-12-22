using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace data_base_practicum
{
    public class Person
    {
        [Key]
        public int Id { get; set; }

        public string? person_id{get; set;}
        public string name { get; set; }
        public List<string> movies_id = new List<string>();

        public List<string> actor_movis_id = new List<string>();
        public List<string> director_movies_id = new List<string>();

        public string? actor_movies_names { get; set; }
        public string? director_movies_names { get; set; }

        public void list_withIDfilms_to_string(Dictionary<string, List<string>> global_films_id_name, 
            List<string> movies_id, bool is_actors)
        {
            StringBuilder result = new StringBuilder();
            Parallel.ForEach(movies_id, mov_id =>
            {
                Parallel.ForEach(global_films_id_name[mov_id], mov_name =>
                {
                    result.Append($"{mov_name} ");
                });
            });
            if (is_actors)
                actor_movies_names = result.ToString();
            else
                director_movies_names = result.ToString();
        }
        public Person() { }
        public Person(string cur_name)
        {
            this.name = cur_name;
        }
    }
}
