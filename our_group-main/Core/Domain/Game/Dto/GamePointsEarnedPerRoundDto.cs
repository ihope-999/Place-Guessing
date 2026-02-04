namespace our_group.Core.Domain.Game.Dto;

public record GamePointsEarnedPerRoundDto(
    int PlayerId,
    List<int> PointsPerRound,
    string PlayerName
    );