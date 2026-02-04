namespace our_group.Core.Domain.Game.Dto;

public record GameScore(
    int PlayerId,
    int Score,
    string PlayerName
    );