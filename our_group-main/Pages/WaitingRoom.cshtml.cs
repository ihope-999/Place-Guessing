using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using our_group.Core.Domain.Game;
using our_group.Core.Domain.Game.Dto;


[Microsoft.AspNetCore.Authorization.Authorize]
public class WaitingRoomModel : PageModel
{
    private readonly ILogger<WaitingRoomModel> _logger;
    private GameEngine _gameEngine;
    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);


    public WaitingRoomModel(ILogger<WaitingRoomModel> logger, GameEngine gameEngine)
    {
        _logger = logger;
        _gameEngine = gameEngine;
    }

    public /*async Task<*/JsonResult/*>*/ OnGetStatus/*Async*/()
    {
        var userId = GetUserId();
        var status = /*await*/ _gameEngine.GetQuickGameStatus(userId);
        return new JsonResult(status);
    }

}