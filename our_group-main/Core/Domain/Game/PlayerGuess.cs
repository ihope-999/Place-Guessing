namespace our_group.Core.Domain.Game;

public class PlayerGuess
{
    public int Id { get; set; } // For Ef core!!!!
    public int UserId { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }

    public PlayerGuess() { } // For Ef core!!!!

    public PlayerGuess(int userId, double lat, double lng)
    {
        UserId = userId;
        Lat = lat;
        Lng = lng;
    }
}
