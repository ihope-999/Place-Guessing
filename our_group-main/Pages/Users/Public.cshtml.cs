/*

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using our_group.Core.Domain.User;
using our_group.Infrastructure.Data;

namespace our_group.Pages.Users
{
    public class PublicModel : PageModel
    {
        private readonly UserContext _db;

        public PublicModel(UserContext db)
        {
            _db = db;
        }

        [FromRoute]
        public string Username { get; set; } = string.Empty;

        // Display fields
        public string? Avatar { get; private set; }
        public int Rank { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public bool IsOwnProfile { get; private set; }
        public bool IsFollowing { get; private set; }
        public int FollowerCount { get; private set; }
        public int FollowingCount { get; private set; }

        private string? _targetId;
        private Guid _targetGuid;
        private Guid? _viewerGuid;

        public async Task<IActionResult> OnGetAsync(string username)
        {
            var ok = await LoadAsync(username);
            if (!ok) return NotFound();
            return Page();
        }

        
        public async Task<IActionResult> OnPostFollowAsync(string username)
        {
            var ok = await LoadAsync(username);
            if (!ok) return NotFound();
            if (_viewerGuid == null || IsOwnProfile)
                return RedirectToPage(new { username });

            if (!await _db.Follows.AnyAsync(f => f.FollowerId == _viewerGuid.Value && f.FolloweeId == _targetGuid))
            {
                _db.Follows.Add(new Follow { FollowerId = _viewerGuid.Value, FolloweeId = _targetGuid });
                await _db.SaveChangesAsync();
            }
            return RedirectToPage(new { username });
        }
        

        
        public async Task<IActionResult> OnPostUnfollowAsync(string username)
        {
            var ok = await LoadAsync(username);
            if (!ok) return NotFound();
            if (_viewerGuid == null || IsOwnProfile)
                return RedirectToPage(new { username });

            var rel = await _db.Follows.FirstOrDefaultAsync(f => f.FollowerId == _viewerGuid.Value && f.FolloweeId == _targetGuid);
            if (rel != null)
            {
                _db.Follows.Remove(rel);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage(new { username });
        }
        

        
        private async Task<bool> LoadAsync(string username)
        {
            Username = username;
            var norm = username.ToLowerInvariant();
            var target = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.NormalizedUserName == norm);
            if (target == null) return false;
            _targetId = target.Id;
            if (!Guid.TryParse(_targetId, out _targetGuid))
            {
                // Data invariant violation: User.Id should be GUID string
                return false;
            }

            Avatar = target.Avatar;
            Rank = target.Rank;
            JoinedAt = target.CreatedAt;

            // Resolve viewer from cookie auth: ClaimTypes.NameIdentifier is PlayerId (int)
            var nameId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(nameId, out var playerId))
            {
                var viewer = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.PlayerId == playerId);
                if (viewer != null && Guid.TryParse(viewer.Id, out var vg))
                {
                    _viewerGuid = vg;
                    IsOwnProfile = viewer.Id == _targetId;
                    if (!IsOwnProfile)
                    {
                        IsFollowing = await _db.Follows.AsNoTracking().AnyAsync(f => f.FollowerId == vg && f.FolloweeId == _targetGuid);
                    }
                }
            }

            FollowerCount = await _db.Follows.AsNoTracking().CountAsync(f => f.FolloweeId == _targetGuid);
            FollowingCount = await _db.Follows.AsNoTracking().CountAsync(f => f.FollowerId == _targetGuid);

            return true;
        }
        
    }

}

*/