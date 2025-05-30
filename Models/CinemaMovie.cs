﻿namespace MovieMart.Models
{
    public class CinemaMovie
    {
        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; } = null!;

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        //todo:Add in Admin And Customer Contrroller
        [Required]
        public DateTime ShowTime { get; set; }
    }
}
