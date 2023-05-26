using System.Collections;
using System.Diagnostics;

namespace TextoIt.API.GoldenRaspberryAwards.Models
{
    public class MoviesModel
    {
        public int year { get; set; }
        
        public string title { get; set; }
        
        public string? studio { get; set; }
        
        public string? producers { get; set; }
        
        public string? winner { get; set; }

        public MoviesModel(int year, string title, string? studio, string? producers, string? winner)
        {
            this.year = year;
            this.title = title;
            this.studio = studio;
            this.producers = producers;
            this.winner = winner;
        }
    }
}
