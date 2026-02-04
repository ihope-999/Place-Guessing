using Microsoft.AspNetCore.SignalR;
using our_group.Core.Domain.Game;

namespace our_group.Infrastructure.Hubs;

public class GameHub : Hub
{

    private readonly GameEngine _engine;

    public GameHub(GameEngine engine)
    {
        _engine = engine;
    }

    public async Task JoinGame(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        await Clients.Group(gameId).SendAsync("PlayerJoined", Context.ConnectionId);
    }

    public async Task PlayerReady(string gameId, int playerId)
    {
        if (!_engine.TryGetRuntime(int.Parse(gameId), out var runtime))
        {
            await Clients.Caller.SendAsync("Error", "Game Not Found");
            return;
        }

        await runtime.HandlePlayerReady(playerId);
    }

    // I actually do not think we need a lock here, since we just accesses the List with gameruntimes. I think it's fine
    public async Task SendGuess(string gameId, string lat, string lng, string userId)
    {
        // We pass the guess to the GameRuntime
        await _engine.RegisterGuess(int.Parse(gameId), double.Parse(lat), double.Parse(lng), int.Parse(userId));
    }

    public async Task GetCurrentGameState(string gameId)
    {
        // We find the relevant gameRuntime and then returns the state
        if (_engine.TryGetRuntime(int.Parse(gameId), out GameRuntime runtime))
        {

            Console.WriteLine("\nLocation name og review! Round count: " + runtime.Game.Rounds.Count + "\n");
            for (int i = 0; i < runtime.Game.Rounds.Count; i++)
            {
                Console.WriteLine(runtime.Game.Rounds[i].Location.Name);
                Console.WriteLine("\n");
                Console.WriteLine(runtime.Game.Rounds[i].Location.Reviews[0].Text);
            }

            await Clients.Caller.SendAsync("RoundStarted", new
            {
                roundNum = runtime.GetCurrentRoundNumber(),
                roundText = runtime.GetCurrentReviewText(),
                message = "",
                hasGuessed = runtime.PlayerHasGuessed(),
                timeLimit = runtime.Game.Rulebook.TimeLimit
            });
        }
    }

    public async Task GetResults(string gameId)
    {
        if (_engine.TryGetRuntime(int.Parse(gameId), out GameRuntime runtime))
        {
            var finalScoresAndRankings = runtime.ReturnFinalScoreAndRankings();
            var featuredLandmarks = runtime.ReturnFeaturedLandmarks();
            var gamePointsEarnedPerRound = runtime.ReturnGamePointsEarnedPerRound();

            await Clients.Caller.SendAsync("GetResults", new
            {
                finalScoresAndRankingsJS = finalScoresAndRankings,
                featuredLandmarksJS = featuredLandmarks,
                gamePointsEarnedPerRoundJS = gamePointsEarnedPerRound
            });
        }
    }

    public async Task GetCurrentLobbyState(string gameId)
    {
        if (_engine.TryGetRuntime(int.Parse(gameId), out GameRuntime runtime))
        {
            var relevantData = await runtime.GetCurrentLobbyState();

            await Clients.Caller.SendAsync("getLobbyData", new
            {
                players = relevantData,
                roundCount = runtime.Game.Rounds.Count,
                status = runtime.Game.GameStatus.ToString()
            });
        }
    }

    public async Task ShowResults(string gameId)
    {
        if (_engine.TryGetRuntime(int.Parse(gameId), out GameRuntime runtime))
        {
            var finalScoresAndRankings = runtime.ReturnFinalScoreAndRankings();
            var featuredLandmarks = runtime.ReturnFeaturedLandmarks();
            var gamePointsEarnedPerRound = runtime.ReturnGamePointsEarnedPerRound();

            await Clients.Caller.SendAsync("GetResults", new
            {
                finalScoresAndRankingsJs = finalScoresAndRankings,
                featuredLandmarksJs = featuredLandmarks,
                gamePointsEarnedPerRoundJs = gamePointsEarnedPerRound
            });
        }
    }
}