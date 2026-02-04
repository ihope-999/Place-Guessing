namespace our_group.Core.Domain.User.DTOs; 

public record PlayerInfoDto(
    int UserId,
    string UserName,
    int PlayerRank
    );