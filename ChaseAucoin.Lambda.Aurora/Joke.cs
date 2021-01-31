using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChaseAucoin.Aurora
{
    public class Joke
    {
        [Key]
        public int Id { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
