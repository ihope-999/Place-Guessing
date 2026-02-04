using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using our_group.Core.Domain.Admin.Dto;
using our_group.LocationDomain.Core.Interfaces;
using our_group.LocationDomain.Infrastructure.Data;

namespace our_group.Pages.Admin
{
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class DashboardModel : PageModel
    {
        private readonly UserContext _userDb;
        private readonly ILocationService _locationService;
        private readonly LocationDbContext _locationDb;

        public PlatformStats Stats { get; private set; } = new PlatformStats();

        public DashboardModel(UserContext userDb, ILocationService locationService, LocationDbContext locationDb)
        {
            _userDb = userDb;
            _locationService = locationService;
            _locationDb = locationDb;
        }

        public async Task OnGetAsync()
        {
            // Users/Admins
            var totalUsers = await _userDb.Users.AsNoTracking().CountAsync();
            var activePlayers = await _userDb.Users.AsNoTracking().CountAsync(u => u.PlayerId > 0);
            var adminUsers = await _userDb.Admins.AsNoTracking().CountAsync();

            // Popular regions from user profiles (FavoriteRegions is comma-separated)
            var regionCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var favorites = await _userDb.Users.AsNoTracking()
                .Select(u => u.FavoriteRegions)
                .ToListAsync();
            foreach (var fav in favorites)
            {
                if (string.IsNullOrWhiteSpace(fav)) continue;
                foreach (var r in fav.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (string.IsNullOrWhiteSpace(r)) continue;
                    regionCounts[r] = regionCounts.GetValueOrDefault(r, 0) + 1;
                }
            }
            var topRegions = regionCounts
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .Take(5)
                .Select(kv => (kv.Key, kv.Value))
                .ToList();

            // Popular categories from locations
            var entities = await _locationDb.Locations.AsNoTracking().ToListAsync();
            var list = entities.Select(e => _locationService.ToLocationDto(e)).ToList();
            var topCategories = list
                .GroupBy(l => l.Category ?? "Unknown")
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Name)
                .Take(5)
                .Select(x => (x.Name, x.Count))
                .ToList();

            // Game metrics are placeholders; real values need persisted game data
            Stats = new PlatformStats
            {
                TotalUsers = totalUsers,
                ActivePlayers = activePlayers,
                AdminUsers = adminUsers,
                TopRegions = topRegions,
                TopCategories = topCategories,
                GamesInProgress = 0,
                GamesCompletedToday = 0,
                GamesCompletedThisWeek = 0,
                GamesCompletedThisMonth = 0,
                AverageGameDuration = null
            };
        }
    }
}
