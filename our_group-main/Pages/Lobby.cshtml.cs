using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using our_group.Core.Domain.Game;

public class LobbyModel : PageModel{
    public string GameId{ get; private set; }
    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value); // Check up on this!
    public int userId{ get; set; } 


    public void OnGet(string gameId){
        GameId = gameId; 
        userId = GetUserId(); 

    }
}