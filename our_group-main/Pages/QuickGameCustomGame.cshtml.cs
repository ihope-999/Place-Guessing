using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using our_group.Core.Domain.Game;
using our_group.Core.Domain.Game.Dto;

namespace our_group.Pages;

public class QuickGameCustomGameModel : PageModel
{
    private readonly ILogger<QuickGameCustomGameModel> _logger;
    private GameEngine _gameEngine;
    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public QuickGameCustomGameModel(ILogger<QuickGameCustomGameModel> logger, GameEngine gm)
    {
        _logger = logger;
        _gameEngine = gm;
    }

    public async Task<IActionResult> OnPostJoinQuickGame()
    {
        Console.WriteLine("DOES THIS FUNCTION RUN?");
        var userId = GetUserId();
        Console.WriteLine("------------------------------");
        Console.WriteLine($"User {userId} is joining quick game.");
        Console.WriteLine("------------------------------");
        await _gameEngine.JoinQuickGame(userId);

        return Redirect("WaitingRoom"); // We redirect to the waiting page. 
    }

    public async Task<IActionResult> OnPostJoinCustomGame()
    {
        return Redirect("CustomGame");
    }


    public IActionResult OnGet()
    {
        var nameId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(nameId))
        {
            return RedirectToPage("/Users/Login");
        }

        if (!int.TryParse(nameId, out var playerId))
        {
            return RedirectToPage("/Users/Login");
        }

        return Page();
    }
}