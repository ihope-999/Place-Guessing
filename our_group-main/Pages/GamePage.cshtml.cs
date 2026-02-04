using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using our_group.Core.Domain.Game;
using our_group.Infrastructure.Hubs;
using our_group.LocationDomain.Core.DTOs;

public class GamePageModel : PageModel{
    private readonly IHubContext<GameHub> _hubContext;
    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value); // Check up on this!
    public int userId{ get; set; } 

    
    public string GoogleKey{ get; }

    public GamePageModel(IOptions<GoogleMapsSettings> googleSettings){
        GoogleKey = googleSettings.Value.ApiKey;
    }

    public void OnGet(){
        userId = GetUserId(); 
    }
}