using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace data_base_practicum
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        public string text { get; set; }
        public string? movies_str { get; set; }

        public Tag() { }
        public Tag(string tag_name) { text = tag_name; }
    }
}
