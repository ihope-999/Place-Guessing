using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.SignalR;
using our_group.Core.Domain.Game;
using our_group.Core.Domain.Game.Dto;
using our_group.Infrastructure.Hubs;
using our_group.LocationDomain.Core.Interfaces;

public class GameRuntime
{
    private readonly Game _game;
    private readonly IHubContext<GameHub> _hub;
    private readonly object _lock = new object();
    private readonly GameEngine _engine;
    private readonly IServiceScopeFactory _scopeFactory;
    private CancellationTokenSource? _roundTimerCts;

    // Data for the results screen
    private List<GameFinalScoreAndRankingDto> _finalScoreAndRankingDto;
    private List<string> _featuredLandmarks;
    private List<GamePointsEarnedPerRoundDto> _gamePointsEarnedPerRoundDto;


    // The hub needs to be registered in the program in the ServiceCollection!
    public GameRuntime(Game game, IHubContext<GameHub> hub, GameEngine engine, IServiceScopeFactory scopeFactory)
    {
        _game = game;
        _hub = hub;
        _engine = engine;
        _scopeFactory = scopeFactory;
    }

    public Game Game => _game;

    public async Task NotifyLobbyUpdate()
    {
        var playerList = _game.Players.Select(p => new { p.UserId/*PlayerId*/, p.PlayerName, p.PlayerRank, p.LobbyStatus }).ToList();

        await _hub.Clients.Group(_game.Id.ToString()).SendAsync("getLobbyData", new { players = playerList, roundCount = _game.Rounds.Count, status = _game.GameStatus.ToString() });

    }

    public async Task<List<PlayerInfoDto>> GetCurrentLobbyState()
    {
        List<PlayerInfoDto> dataList = new List<PlayerInfoDto>();
        var playerList = _game.Players.Select(p => new { p.UserId/*PlayerId*/, p.PlayerName, p.PlayerRank, p.LobbyStatus }).ToList();

        for (int i = 0; i < playerList.Count; i++)
        {
            var data = new PlayerInfoDto(playerList[i].UserId/*PlayerId*/, playerList[i].PlayerName, playerList[i].PlayerRank, playerList[i].LobbyStatus);
            dataList.Add(data);
        }

        return dataList;
    }

    public async Task HandlePlayerReady(int playerId)
    {
        var player = _game.Players.FirstOrDefault(p => p.UserId/*PlayerId*/ == playerId);
        if (player == null)
        {
            return;
        }

        player.LobbyStatus = PlayerLobbyStatus.Ready;

        await _hub.Clients.Group(_game.Id.ToString()).SendAsync("PlayerReadyStatusChanged", new { userId = playerId, lobbyStatus = player.LobbyStatus.ToString() });

        if (_game.Players.All(p => p.LobbyStatus == PlayerLobbyStatus.Ready))
        {
            await StartGameAsync();
        }
    }

    public async Task StartGameAsync()
    {
        await _hub.Clients.Group(_game.Id.ToString()).SendAsync("GameStarting", new { message = "Game is starting!" });
        await RedirectToGame();
        // await StartRound(1);  Replaced with RedirectToGame
    }


    public async Task RedirectToGame()
    {
        // We start the countdown on the client side
        await _hub.Clients.Group(_game.Id.ToString()).SendAsync("StartCountDown", new { });
        // then we wait 10 seconds before redirecting to the game.
        await Task.Delay(TimeSpan.FromSeconds(10));
        await _hub.Clients.Group(_game.Id.ToString()).SendAsync("RedirectToRound", new { gameId = _game.Id });
    }

    public async Task StartRound(int roundNumber, int reviewStar)
    {
        // _roundTimerCts?.Cancel();
        _roundTimerCts = new CancellationTokenSource();

        // CHECK AND SEE IF YOU CAN DO IT LIKE THIS!!
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_game.Rulebook.TimeLimit), _roundTimerCts.Token);
                await HandleRoundEnd();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("\nRound ended before the timer did\n");
            }
        });

        string messageText = "";

        if (reviewStar > 0)
        {
            messageText = "No winner for the " + reviewStar.ToString() + " review.";
        }
        // Should probaby add the countdown timer here.

        var currentRound = _game.Rounds[roundNumber];
        var text = currentRound.Location.Reviews[reviewStar].Text;
        var name = currentRound.Location.Name;

        Console.WriteLine("\nNext round: " + roundNumber + " \n");
        Console.WriteLine("\nNext text: " + text + " \n");
        Console.WriteLine("\nNext name: " + name + " \n");

        // Need to send the information about the points per player thus far in the round 
        var gameScores = _game.GetScoreOfPlayers();

        // We also provide the necessary data for the round 

        await _hub.Clients.Group(_game.Id.ToString())
            .SendAsync("RoundStarted", new
            {
                roundNum = roundNumber,
                roundText = text,
                scores = gameScores,
                locationName = name,
                message = messageText,
                hasGuessed = PlayerHasGuessed(),
                timeLimit = _game.Rulebook.TimeLimit
            });
    }

    public async Task ReceiveGuess(double lat, double lng, int userId)
    {
        // lock (_lock){ Should I lock here?
        _game.RegisterGuess(userId, lat, lng);
        // }

        if (_game.AllPlayersGuessed())
        {
            _roundTimerCts?.Cancel();
            await NotifyRoundResults();
            // Legg til annen funksjon som setter i gang neste runde.
        }

        // We do not have to return anything here return Task.CompletedTask;
    }

    // We get the round results
    public async Task NotifyRoundResults()
    {

        // If CheckWinner returns true, that means we have a winner for the round
        if (await CheckWinner())
        {
            await HandleRoundEnd();
        }
        else
        { // If there was no winner the round will continue with a higher star count and less points for the round
            Console.WriteLine("\nHAa");
            await HandleRoundContinuation(); // Basically this one just moves on to the next review text.
        }
    }

    public async Task<bool> CheckWinner()
    {
        // NB! You have to add a scope service in the GameRuntime to use the ILocationService!
        var currentRoundNumber = _game.CurrentRound;
        var currentRoundObject = _game.GetCurrentRound();
        var RuleBookForGame = _game.Rulebook;
        using var scope = _scopeFactory.CreateScope();
        var locationService = scope.ServiceProvider.GetRequiredService<ILocationService>();

        for (int i = 0; i < currentRoundObject.Answers.Count; i++)
        {
            // Makes ready a GuessValidationDto to transfer to the LocationService
            var newValidationDto = new GuessValidationResult(
                LocationId: currentRoundObject.Location.ExternalLocationId/*Id*/,
                isCorrect: false, // these are actually not necessary here but oh well
                pointsGained: 5,
                threshold: RuleBookForGame.Threshhold,
                distanceKM: 30.00,
                GuessLatitude: currentRoundObject.Answers[i].Lat,
                GuessLongitude: currentRoundObject.Answers[i].Lng,
                Message: "hey"
            );

            // Gets the result if the answer is correct or not from the LocationService
            var answer = await locationService.ValidateGuessAsync(newValidationDto); // Do the scope thing here!

            // We check the answer. If it's correct we add the player to the winner list
            if (answer.isCorrect)
            {
                await _game.RegisterWinner(currentRoundObject.Answers[i].UserId);
            }
        }

        // If there are no winners registered
        if (currentRoundObject.WinnerId.Count == 0)
        {
            return false;
        }

        return true;
    }

    public async Task HandleRoundContinuation()
    {
        // Nobody answered correctly. We then provide a new review with one more star
        var nextReviewStar = _game.NextReview();
        if (nextReviewStar == -1)
        { // If the round has currentReivew = -1, there are no more reviews. 
            await HandleRoundEnd(); // CHECK IF YOU CAN JUST USE THIS HERE FOR THIS PURPOSE!
        }
        else
        {
            await StartRound(_game.CurrentRound, nextReviewStar);
        }

    }

    public async Task HandleRoundEnd()
    {
        // Data which will be provided to the user
        var results = _game.GetWinnerNamesOfCurrentRound();
        var pointsGivenForRound = _game.GetAmountOfPointsForRound();
        Console.WriteLine("\nPOINTS GIVEN FOR ROUND: " + pointsGivenForRound + "\n");
        var location = _game.GetLocationFromGame();

        // Sends the results to the clients that will show the results on their page
        await _hub.Clients.Group(_game.Id.ToString())
            .SendAsync("RoundEnded", new
            {
                roundResults = results,
                roundPoints = pointsGivenForRound,
                locationData = new
                {
                    name = location.Name,
                    country = location.Country
                }
            });

        // Delays by 10 seconds then we progress the game
        await Task.Delay(TimeSpan.FromSeconds(3));
        _game.RoundEnded();  // This function will move the game on.

        // then we check if there are any more rounds left to play
        if (_game.HasMoreRounds())
        {
            var currentReview = _game.Rounds[_game.CurrentRound].CurrentReview;

            // Initiate next round
            await StartRound(_game.CurrentRound, currentReview); // Check if the logic here is sound
        }
        else
        {

            Console.WriteLine("\n\nKOMMER MEG HIT SKAL TIL Å BLI FERDIG MED GAME!\n\n");
            // Start the endgame phase
            await FinishGame(); // TODO: FinishGame is not done. 
        }
    }

    public async Task FinishGame()
    {
        // We finish up the game
        _game.FinishGame(); // TODO: Implement this better

        // Fetches the relevant data to show the players after the game is finished. 
        var finalScoresAndRankings = _game.GetFinalScoresAndRankings(); //
        var featuredLandmarks = _game.GetFeaturedLandmarks();
        var pointsEarnedPerRound = _game.GetPointsEarnedPerRound();

        // We store all of this data which will be accessed by the clients when they connect again with the 
        // results page
        _finalScoreAndRankingDto = finalScoresAndRankings;
        _featuredLandmarks = featuredLandmarks;
        _gamePointsEarnedPerRoundDto = pointsEarnedPerRound;

        // Tell the clients that the game is finished
        await _hub.Clients.Group(_game.Id.ToString()).SendAsync("FinishGame", new { gameId = _game.Id });

        // We wait 30 seconds then tell the gameEngine that the game is done. Then this instance of the gameRuntime will be killed
        await Task.Delay(TimeSpan.FromSeconds(30));

        // Finishes the game
        await _engine.GameFinished(_game.Id);

    }

    public List<GameFinalScoreAndRankingDto> ReturnFinalScoreAndRankings()
    {
        return _finalScoreAndRankingDto;
    }

    public List<string> ReturnFeaturedLandmarks()
    {
        return _featuredLandmarks;
    }

    public List<GamePointsEarnedPerRoundDto> ReturnGamePointsEarnedPerRound()
    {
        return _gamePointsEarnedPerRoundDto;
    }

    public int GetCurrentRoundNumber()
    {
        return _game.CurrentRound;
    }

    public string GetCurrentReviewText()
    {
        var currentRound = _game.CurrentRound;
        var currentReview = _game.Rounds[currentRound].CurrentReview;

        return _game.Rounds[currentRound].Location.Reviews[currentReview].Text;
    }

    public List<int> PlayerHasGuessed()
    {
        return _game.PlayerHasGuessed();
    }


    public int ReturnGameIdBasedOnUserId(int playerId)
    {
        var players = _game.Players;
        Console.WriteLine("NUMBER OF PLAYERS IN GAME: " + players.Count);

        for (int i = 0; i < players.Count; i++)
        {
            Console.WriteLine("CHECKING PLAYER ID: " + players[i]);
            if (players[i].UserId/*PlayerId*/ == playerId)
            {
                return _game.Id;
            }
        }

        return -1;
    }
}
