using Microsoft.EntityFrameworkCore;

namespace our_group.Core.Domain.Game;

[Owned]
public class Rulebook
{
    private int _numberOfRounds;
    private int _timeLimit;
    private int _numberOfPlayers;
    private int _threshhold;

    //public int NumberOfRounds{ get; set; }
    public int NumberOfRounds
    {
        get => _numberOfRounds;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Number of rounds must be positive.");

            _numberOfRounds = value;
        }
    }
    public int TimeLimit
    {
        get => _timeLimit;
        set => _timeLimit = value;
    }

    public int NumberOfPlayers { get; set; }

    public int Threshhold
    {
        get => _threshhold;
        set => _threshhold = value;
    }

    public Rulebook(int numberOfRounds, int timeLimit, int numberOfPlayers, int threshhold)
    {
        _numberOfPlayers = numberOfPlayers;
        _timeLimit = timeLimit;
        _numberOfPlayers = numberOfPlayers;
        _threshhold = threshhold;
        _numberOfRounds = numberOfRounds;
    }
}