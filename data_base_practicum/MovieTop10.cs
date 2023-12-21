using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace data_base_practicum
{
    public class MovieTop10
    {
        [Key]
        public int Id { get; set; }

        public string name { get; set; }
        public string top10_movies { get; set; }

        public MovieTop10(string cur_name)
        {
            this.name = cur_name;
        }
        public MovieTop10()
        {
        }
    }
}
