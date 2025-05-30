using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4th_Semester_Final_Project
{
    internal class TmdbPopularResponse
    {
        public int page { get; set; }
        public int total_pages { get; set; }
        public List<Movie> results { get; set; }
    }
}
