using System;
using System.Collections.Generic;

namespace our_group.Core.Domain.Admin.Dto
{
    public class PlatformStats
    {
        // User/Admin domain
        public int TotalUsers { get; set; }
        public int ActivePlayers { get; set; }
        public int AdminUsers { get; set; }
        public List<(string Name, int Count)> TopRegions { get; set; } = new();
        public List<(string Name, int Count)> TopCategories { get; set; } = new();

        // Game domain (placeholders until game persistence exists)
        public int GamesInProgress { get; set; }
        public int GamesCompletedToday { get; set; }
        public int GamesCompletedThisWeek { get; set; }
        public int GamesCompletedThisMonth { get; set; }
        public TimeSpan? AverageGameDuration { get; set; }
    }
}

