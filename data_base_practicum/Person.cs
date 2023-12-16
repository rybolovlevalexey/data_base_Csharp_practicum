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

        public Person() { }
        public Person(string cur_name)
        {
            this.name = cur_name;
        }
    }
}
