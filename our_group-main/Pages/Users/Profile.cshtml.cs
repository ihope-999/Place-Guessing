using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using MediatR;
using our_group.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using our_group.Core.Domain.User;

namespace our_group.Pages.Users
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private static readonly string[] RegionOptions = new[]
        {
            "worldwide","africa","asia","europe","north-america","south-america","oceania","antarctica"
        };

        private static readonly string[] AllAvatars = new[]
        {
            "🗺️","🏛️","🗽","🗼","🏔️","🏟️","🏝️","🏯","🕌","⛩️","🏙️","🧭","🧳","🚀","🎯"
        };

        private readonly UserContext _db;
        private readonly IMediator _mediator;

        public ProfileModel(UserContext db, IMediator mediator)
        {
            _db = db;
            _mediator = mediator;
        }

        public string? UserName => User.Identity?.Name;
        public string? Email => User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
        public int Rank { get; private set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<string> AvatarChoices { get; private set; } = new();

        public class InputModel
        {
            [Display(Name = "Favorite regions")]
            public List<string> FavoriteRegions { get; set; } = new();

            [Display(Name = "Avatar")]
            public string? Avatar { get; set; }
        }

        public async System.Threading.Tasks.Task OnGet()
        {
            var nameId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nameId)) return;
            if (!int.TryParse(nameId, out var playerId)) return;
            var entity = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.PlayerId == playerId);
            if (entity != null)
            {
                Input.Avatar = entity.Avatar;
                Input.FavoriteRegions = (entity.FavoriteRegions ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
                Rank = entity.Rank;
            }

            AvatarChoices = PickRandomAvatars(Input.Avatar);
        }

        public async System.Threading.Tasks.Task<IActionResult> OnPostAsync()
        {
            var nameId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nameId)) return RedirectToPage("/Users/Login");
            if (!int.TryParse(nameId, out var playerId)) return RedirectToPage("/Users/Login");

            // Validate selected regions
            Input.FavoriteRegions = Input.FavoriteRegions
                .Where(r => RegionOptions.Contains(r, StringComparer.OrdinalIgnoreCase))
                .Select(r => r.ToLowerInvariant())
                .Distinct()
                .ToList();

            // Validate avatar: must be one of known icons; if not provided, pick a random one
            if (string.IsNullOrWhiteSpace(Input.Avatar) || !AllAvatars.Contains(Input.Avatar))
            {
                Input.Avatar = AllAvatars[Random.Shared.Next(AllAvatars.Length)];
            }

            var entity = await _db.Users.FirstOrDefaultAsync(u => u.PlayerId == playerId);
            if (entity == null) return RedirectToPage("/Users/Login");

            entity.Avatar = Input.Avatar;
            entity.FavoriteRegions = string.Join(',', Input.FavoriteRegions);
            await _db.SaveChangesAsync();

            // Publish domain event
            var account = new UserAccount(
                new our_group.Shared.Domain.UserId(entity.Id),
                new our_group.Shared.Domain.UserName(entity.UserName),
                new our_group.Shared.Domain.EmailAddress(entity.Email),
                entity.CreatedAt);
            await _mediator.Publish(new UserProfileUpdated(account));

            TempData["ProfileSaved"] = true;
            return RedirectToPage();
        }

        private List<string> PickRandomAvatars(string? include)
        {
            var list = AllAvatars.ToList();
            var result = new List<string>();
            // Simple shuffle
            foreach (var i in list.OrderBy(_ => Random.Shared.Next()))
            {
                result.Add(i);
                if (result.Count >= 8) break;
            }
            if (!string.IsNullOrEmpty(include) && !result.Contains(include))
            {
                result[0] = include; // ensure current selection is visible
            }
            return result;
        }
    }
}

