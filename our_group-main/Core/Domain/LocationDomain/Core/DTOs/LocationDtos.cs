
using our_group.LocationDomain.Core.Entities;

public record LocationDto(
    
    int Id,
    string Name,
    string? Country,
    string? City,
    double Latitude,
    double Longitude,
    string Category,
    int Difficulty
    
);

public record ReviewDto (
    
    int Id,
    int StarRating,
    string? Text,
    string? AuthorName,
    DateTime LastCachedAt

    
    );

public record LocationDetailsDto(
    int Id,
    string? Name,
    string? Address,
    Coordinate Coordinates,
    string Region,
    string Country,
    string City,
    string Category,
    int Difficulty,
    double AverageRating,
    DateTime LastCached,
    List<ReviewDto> Reviews

    
    );


public record GameCriteria(
    string? Region,
    string? Category,
    int MinDifficulty,
    int MaxDifficulty,
    int Count = 1
    
    
    );

public record GuessValidationResult(
    int LocationId,
    bool isCorrect,
    double GuessLatitude,
    double GuessLongitude,
    double threshold,
    double distanceKM,
    int pointsGained,
    string? Message
    
    );