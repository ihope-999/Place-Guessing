namespace our_group.Core.Domain.Game.Dto;

public record GameFinalScoreAndRankingDto(
    int PlayerId,
    string PlayerName,
    int FinalScore,
    int Ranking
);