namespace our_group.Core.Domain.Game.Dto;

public record PlayerInfoDto(
    int PlayerId,
    string PlayerName,
    int PlayerRank,
    PlayerLobbyStatus LobbyStatus
    ); 