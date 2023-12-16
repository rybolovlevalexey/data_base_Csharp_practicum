using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace data_base_practicum
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }
        
        public string MovieId { get; set; }
        public string name { get; set; }
        public string? rating { get; set; }
        
        public HashSet<string> tags = new HashSet<string>();
        public HashSet<string> actors = new HashSet<string>();
        public HashSet<string> directors = new HashSet<string>();

        public string? tags_str { get; set; }
        public string? actors_str { get; set; }
        public string? directors_str { get; set; }


        public Movie(string cur_name)
        {
            name = cur_name;
        }
        public Movie()
        {
        }
    }
}
