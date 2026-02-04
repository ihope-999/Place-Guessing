using System.ComponentModel.DataAnnotations.Schema;
using our_group.Core.Domain.Game.Events;

namespace our_group.Core.Domain.Game;

/*
 * Important readme:
 * Id is the real PK in the EF Core setting for player.
 * PlayerId is a reference to the connected User's id.
 */


public class Player
{
    private int _userId/*_playerId*/;
    private string _playerName;
    private int _playerRank;
    private PlayerLobbyStatus _lobbyStatus;

    public Player() { } // for ef core

    public Player(int userId/*playerId*/, string playerName, int playerRank)
    {
        _userId/*_playerId*/ = userId/*playerId*/;
        _playerName = playerName;
        _playerRank = playerRank;
        _lobbyStatus = PlayerLobbyStatus.NotReady;
    }
    
    public int Id{ get; set; } // Id for ef core
    
    public int UserId /*PlayerId*/
    {
        get => _userId/*_playerId*/;
        set => _userId/*_playerId*/ = value;
    }

    public Game Game { get; set; }

    public int GameId { get; set; } // for ef core

    public int PlayerRank
    {
        get => _playerRank;
        set
        {
            if (_playerRank == value)
                return;
            int oldRank = _playerRank;
            _playerRank = value;

            OnRankChanged?.Invoke(new PlayerRankChanged(UserId/*PlayerId*/, oldRank, value));
        }
    }

    public string PlayerName
    {
        get => _playerName;
        set => _playerName = value ?? throw new ArgumentNullException(nameof(value));
    }

    public PlayerLobbyStatus LobbyStatus
    {
        get => _lobbyStatus;
        set => _lobbyStatus = value;
    }

    [NotMapped]
    public Action<PlayerRankChanged>? OnRankChanged { get; set; } // Simplest way to add the event

    public void UpdateRank(int score)
    {
        int newRank = _playerRank + score;
        PlayerRank = newRank;
    }

}