/*
 *  Read me:
 *      The game engine is not an entity calss which is going to be registered in the EF Core DB.
 *      It is more the brain of the Game domain.
 *      It creates and manages new 
 *
 */

using System;
using System.Collections;
using MediatR;
using Microsoft.AspNetCore.SignalR;
//using our_group.Core.Domain.Game.FakedData;
using our_group.Core.Domain.Game.Dto;
using our_group.Core.Domain.Game.Pipelines;
using our_group.Infrastructure.Hubs;

using our_group.Core.Domain.User.Services; // When implemented
using our_group.LocationDomain.Core.Entities;
using our_group.LocationDomain.Core.Interfaces;
using our_group.LocationDomain.Core.DTOs;
using IUserInfoService = our_group.Core.Domain.User.Services.IUserInfoService;

namespace our_group.Core.Domain.Game;

public class GameEngine
{
    private readonly Dictionary<int, GameRuntime> _activeGames = new();
    private List<Player> _waitingPlayers = new();
    private List<Player>? _playersReadyForGame;
    private readonly IServiceScopeFactory _scopeFactory;
    private IHubContext<GameHub> _hubContext;
    private readonly object _lock = new object();


    private const int MAX_PLAYERS = 2;
    private const int MAX_RANK_DISTANCE = 200;
    private const int NUMBER_OF_ROUNDS_QUICKGAME = 5;
    private const int TIME_LIMIT_QUICKGAME = 30;
    private const int THRESHHOLD_QUICKGAME = 50;

    public GameEngine(IHubContext<GameHub> hubContext, IServiceScopeFactory scopeFactory)
    {
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
    }

    private IUserInfoService CreateUserInfoService()
    {
        var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IUserInfoService>();
    }

    private ILocationService CreateLocationService()
    {
        var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ILocationService>();
    }

    private IMediator CreateMediator()
    {
        var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    // When the player requests to join a quickgame he/she will be added to the queue
    public async Task<bool> JoinQuickGame(int userId)
    {
        Player playerWhoWantsToJoin;

        // Retrieves the player from the Player domain
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var userInfoService = scope.ServiceProvider.GetRequiredService<IUserInfoService>();  // = CreateUserInfoService();
                var playerDto = await userInfoService.GetPlayer(userId);

                Console.WriteLine("Jeg blir kjørt inne i JoinQuickgame. ");

                playerWhoWantsToJoin = new Player(playerDto.UserId, playerDto.UserName, playerDto.PlayerRank); //await userInfoService.GetPlayer(userId);
            }

            // WE HAVE TO USE THIS METHOD HERE FOR THE SCOPED SERVICES. WE CANNOT USE THE HANDY FUNCTIONS..
            playerWhoWantsToJoin.OnRankChanged = async evt =>
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Publish(evt);
            };
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting userinfo: {e.Message}");
            return false;
        }

        // Adds the player to the list of waiting players
        lock (_lock)
        {
            Console.WriteLine("Skal til å legge til PlayerWhoWants to join");
            _waitingPlayers.Add(playerWhoWantsToJoin);
            Console.WriteLine("Spiller som vil være med: " + playerWhoWantsToJoin.PlayerName);
        }

        await this.TryMatchPlayers(); // Method which tries to match the player


        return true;
    }

    public async Task TryMatchPlayers()
    {
        Console.WriteLine("\n\nJEG ER INNE I TRYMATCHPLAYERS\n\n");

        if (_waitingPlayers.Count < MAX_PLAYERS) return; // Not enough players in the queue

        var ordered = _waitingPlayers.OrderBy(p => p.PlayerRank).ToList();
        var matchedPlayers = new List<Player>();

        Console.WriteLine("Skal til å kjøre løkken");
        for (int i = 0; i <= ordered.Count - MAX_PLAYERS; i++)
        {
            Console.WriteLine("Er inne i løkken");
            // We take the amount of players we have specified in our const and try to match them
            var group = ordered.Skip(i).Take(MAX_PLAYERS).ToList();
            int minRank = group.Min(p => p.PlayerRank);
            int maxRank = group.Max(p => p.PlayerRank);

            if (maxRank - minRank <= MAX_RANK_DISTANCE)
            {
                Console.WriteLine("Et quickgame skal bli laget!");


                for (int j = 0; j < group.Count; j++)
                {
                    Console.WriteLine("spiller i matched player: " + group[j].PlayerName);
                }

                await InitializeQuickGame(group);

                matchedPlayers.AddRange(group);
                i += MAX_PLAYERS - 1;
            }
        }

        foreach (var p in matchedPlayers)
        {
            _waitingPlayers.Remove(p);
        }
    }

    public /*async Task<*/GameStatusDto/*>*/ GetQuickGameStatus(int userId)
    {
        // Should probably implement a lock here.

        foreach (KeyValuePair<int, GameRuntime> game in _activeGames)
        {
            if (_activeGames[game.Key].ReturnGameIdBasedOnUserId(userId) == game.Key)
            {

                return new GameStatusDto("True", game.Key);
            }
        }
        return new GameStatusDto("False", -1);
    }

    public async Task InitializeQuickGame(List<Player> players)
    {
        using var scope = _scopeFactory.CreateScope();
        var locationService = scope.ServiceProvider.GetRequiredService<ILocationService>();

        var standardRuleBook = new Rulebook(NUMBER_OF_ROUNDS_QUICKGAME, TIME_LIMIT_QUICKGAME, MAX_PLAYERS, THRESHHOLD_QUICKGAME);
        List<Location> locationsForRounds = new();

        Console.WriteLine("NUMBER OF ROUNDS: " + standardRuleBook.NumberOfRounds);

        for (var i = 0; i < standardRuleBook.NumberOfRounds; i++)
        {

            var locationDto = await RetrieveLocationDto(new("Europe", "Landmark", 1, 5, 1));
            if (locationDto.Id == -1)
            {
                Console.WriteLine("NO LOCATION FOUND. ABORTING");
                return;
            }

            var reviewDtos = await locationService.GetLocationReviewsAsync(locationDto.Id);

            var reviews = new List<Review>();

            foreach (var dto in reviewDtos)
            {
                reviews.Add(new Review(dto.Id, dto.StarRating, dto.Text, dto.AuthorName));
            }

            locationsForRounds.Add(new Location(locationDto, reviews));
        }

        var rounds = CreateRounds(locationsForRounds);
        var newGame = new Game(players, standardRuleBook, rounds);

        try
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var newGameFromDb = await mediator.Send(new InitializeNewGame.Request(newGame));
            var runtime = new GameRuntime(newGameFromDb, _hubContext, this, _scopeFactory);
            _activeGames.Add(newGameFromDb.Id, runtime);

            await runtime.NotifyLobbyUpdate();
        }
        catch (ArgumentNullException e)
        {
            Console.WriteLine($"Error initializing new game: {e.Message}");
        }
    }

    public async Task<LocationDto> RetrieveLocationDto(GameCriteria gameCriteria)
    {
        using var scope = _scopeFactory.CreateScope();

        try
        {
            var locationService = scope.ServiceProvider.GetRequiredService<ILocationService>();
            var locationDto = await locationService.GetRandomLocationForGameAsync(gameCriteria);
            return locationDto;
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine($"Failed to fetch location: {e.Message}");
            return new LocationDto(-1, "", "", "", 0.0, 0.0, "", -1);
        }
    }

    // Method that retrieves a review for a location with star-amount of stars
    public ReviewDto FindReviewWithNStars(int star, IEnumerable<ReviewDto> reviewDtos)
    {
        bool hasTextAttached = false;
        ReviewDto review;
        Random random = new Random();
        int randomIndex = random.Next(reviewDtos.Count() - 1);

        var reviewDtoWithNStars = reviewDtos.Where(dto => dto.StarRating == star).ToList();
        review = reviewDtoWithNStars[randomIndex];

        if (review.Text != null)
        {
            hasTextAttached = true;
        }

        while (!hasTextAttached)
        {
            reviewDtoWithNStars = reviewDtos.Where(dto => dto.StarRating == star).ToList();
            review = reviewDtoWithNStars[randomIndex];
            if (review.Text != null)
            {
                hasTextAttached = true;
            }
            randomIndex = random.Next(reviewDtos.Count() - 1);
        }

        return review;
    }

    // Creates the rounds for a Game and returns a list with these. Maybe make this one private?
    public List<Round> CreateRounds(List<Location> locations)
    {
        List<Round> rounds = new List<Round>();

        for (int i = 0; i < locations.Count; i++)
        {
            Round newRound = new Round(locations[i]);
            rounds.Add(newRound);
        }

        return rounds;
    }

    public async Task GameFinished(int gameId)
    {
        // We retrieve the relevant game from the request. 
        GameRuntime relevantGameRuntime;
        Game relevantGame;

        using var scope = _scopeFactory.CreateScope();

        lock (_lock)
        {
            if (!_activeGames.TryGetValue(gameId, out relevantGameRuntime!))
            {
                return; // Could not found the game, or it has already been removed. 
            }

            relevantGame = relevantGameRuntime.Game;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new StoreGame.Request(relevantGame));

        lock (_lock)
        {
            // After storing the game to the db, we remove the game from the active game
            _activeGames.Remove(gameId);

        }
    }

    public void InitializeCustomGame(Rulebook rulesForCustomGame)
    {
        // If the user starts a custom game. Need the rulebook for the
    }

    public void HandleCanselRequest()
    {
        // If the user doesn't want to join a quickgame after all. 
        // Implement when/if you have the time for it. Not really essential for a functioning product.
    }

    // Method that gets the relevant active game
    public bool TryGetRuntime(int gameId, out GameRuntime runtime)
    {
        // MAYBE IMPLEMENT A LOCK HERE?? I THINK SO YEAH
        return _activeGames.TryGetValue(gameId, out runtime);
    }

    public async Task RegisterGuess(int gameId, double lat, double lng, int userId)
    {
        // Retrieves the active game with the relevant Id
        var gameRuntime = _activeGames[gameId];
        await gameRuntime.ReceiveGuess(lat, lng, userId);
    }
}