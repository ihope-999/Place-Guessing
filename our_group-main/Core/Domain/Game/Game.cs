using our_group.Core.Domain.Game.Dto;
//using our_group.Core.Domain.Game.FakedData;
using our_group.LocationDomain.Core.Interfaces;



namespace our_group.Core.Domain.Game;

public class Game
{
    private List<Player> _players = new();
    private List<Round> _rounds = new();
    private int _currentRound;

    public int Id { get; set; }
    public List<Player> Players
    {
        get => _players;
        set => _players = value;
    }

    public int CurrentRound
    {
        get => _currentRound;
        set => _currentRound = value;
    }

    public List<Round> Rounds
    {
        get => _rounds;
        set => _rounds = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Status GameStatus { get; set; }
    //public int RoundAmount => _rounds.Count;
    public Rulebook Rulebook { get; set; }

    //public List<PlayerReview> PlayerReviewOfGameList;
    //public IReadOnlyCollection<Player> PlayerList => Players;

    public Game() { } // for EF Core

    public Game(IEnumerable<Player> players, Rulebook rulebook, List<Round> rounds)
    {
        _players.AddRange(players);
        GameStatus = Status.Active;
        Rulebook = rulebook;
        _rounds = rounds;
    }

    public void RegisterGuess(int userId, double lat, double lng)
    {
        var currentRound = Rounds[CurrentRound];

        // Adds the user guess to a new record

        currentRound.Answers.Add(new PlayerGuess(userId, lat, lng));
    }

    public bool AllPlayersGuessed()
    {
        var currentRound = Rounds[CurrentRound];
        if (currentRound.Answers.Count == Players.Count)
        {
            return true;
        }

        return false;
    }

    // Function that checks the winner(s) for the current round and returns these 
    // There is a big problem with this function. It uses ILocationService! Not allowed! The GameRunTime must use the
    // ILocationService to check the winner! Then we 

    // TODO: Change this function to Register winner DONE? YES
    // TODO: Keep the registering logic. That one is fine. We only have to pass the userID. DONE

    public async Task/*<bool>*/ RegisterWinner(int userId)
    {
        var currentRound = Rounds[CurrentRound];

        // Goes through all the answers for the current round and check if they were correct or not
        /*  for (int i = 0; i < currentRound.Answers.Count; i++){
              // Makes ready a GuessValidationDto to transfer to the LocationService
              var newValidationDto = new GuessValidationResult(
                  LocationId: currentRound.Location.Id,
                  isCorrect: false, // these are actually not necessary here but oh well
                  pointsGained: 5,
                  threshold: Rulebook.Threshhold,
                  distanceKM: 30.00,
                  GuessLatitude: currentRound.Answers[i].Lat,
                  GuessLongitude: currentRound.Answers[i].Lng,
                  Message: "hey"
                  ); 
                  */

        // Gets the result if the answer is correct or not from the LocationService
        //   var answer = await _locationService.ValidateGuessAsync(newValidationDto);

        // THIS SHOULD STAY HERE. IT WILL USE THE PASSED USER_ID
        // Checks the answer. If it is correct, we add the player to the winner list! 
        //  if (answer.isCorrect){
        // Retrieve the players name LOCK HERE?
        var player = Players.FirstOrDefault(p => p.UserId/*PlayerId*/ == userId)!;

        currentRound.WinnerId.Add(player.UserId/*PlayerId*/);
        currentRound.WinnerName.Add(player.PlayerName);
        //    }
        // }

        // If we have some winners for this round, we return true, if not, false
        //  if (currentRound.WinnerId.Count == 0){
        //      return false; 
        //  }

        // return true; 
    }

    // Do I have to lock this? 
    public Round GetCurrentRound()
    {
        return Rounds[CurrentRound];
    }

    public int NextReview()
    {
        // Change the current review for the current round
        _rounds[_currentRound].ChangeCurrentReview();
        return _rounds[_currentRound].CurrentReview;
    }

    // Gets the current score thus far
    public List<GameScore> GetScoreOfPlayers()
    {
        List<GameScore> data = new List<GameScore>();
        for (int i = 0; i < Players.Count; i++)
        {
            var playerId = _players[i].UserId/*PlayerId*/;
            var playerName = _players[i].PlayerName;
            int playerScore = 0;

            for (int j = 0; j < _currentRound; j++)
            {
                if (_rounds[i].CheckIfPlayerWonRound(playerId))
                {
                    playerScore += _rounds[i].AmountOfPointsGiven;
                }
            }
            data.Add(new GameScore(playerId, playerScore, playerName));
        }

        return data;
    }

    // Function that gets the results of the winner for the current roun 
    public List<int> GetWinnersOfCurrentRound()
    {
        var currentRound = Rounds[_currentRound];
        return currentRound.WinnerId;
    }

    public List<string> GetWinnerNamesOfCurrentRound()
    {
        var currentRound = Rounds[_currentRound];
        return currentRound.WinnerName;
    }

    public int GetAmountOfPointsForRound()
    {
        var startingAmountOfPoints = Rounds[_currentRound].AmountOfPointsGiven;
        var currentReivew = Rounds[_currentRound].CurrentReview;
        return startingAmountOfPoints - currentReivew;
    }

    public void RoundEnded()
    {
        _rounds[_currentRound].RoundEnd();
        _currentRound++;

        // We check if the amount of rounds has exceeded the amount the game should play for
        if (_currentRound >= Rulebook.NumberOfRounds)
        {
            _currentRound = -1;
        }
    }

    public bool HasMoreRounds()
    {
        if (_currentRound == -1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public Location GetLocationFromGame()
    {
        return Rounds[_currentRound].Location;
    }

    public void FinishGame()
    {
        GameStatus = Status.Finished;



    }

    public List<string> GetFeaturedLandmarks()
    {
        List<string> featuredLandmarks = new List<string>();

        // Goes through all the featured landmarks and shows the data
        for (int i = 0; i < Rulebook.NumberOfRounds; i++)
        {
            string landmarkName = Rounds[i].Location.Name;

            featuredLandmarks.Add(landmarkName);
        }
        return featuredLandmarks;
    }

    public List<GameFinalScoreAndRankingDto> GetFinalScoresAndRankings()
    {
        List<GameFinalScoreAndRankingDto> data = new List<GameFinalScoreAndRankingDto>();
        var scores = GetScoreOfPlayers();

        for (int i = 0; i < scores.Count; i++)
        {
            // Retrieves the new rank of every player who played the game
            int newRank = GetNewRankOfPlayer(scores[i].PlayerId, scores[i].Score);


            GameFinalScoreAndRankingDto newDto = new GameFinalScoreAndRankingDto(scores[i].PlayerId,
                                                                                 scores[i].PlayerName,
                                                                                 scores[i].Score,
                                                                                 newRank);
            data.Add(newDto);
        }

        return data;
    }

    // Calculates the new score of a player. For now, it's very primitive: We sum the current rank and the score..
    // NB: THIS FUNCTION ALSO UPDATES THE SCORE OF A PLAYER!!
    public int GetNewRankOfPlayer(int userId, int score)
    {
        var player = Players.FirstOrDefault(p => p.UserId/*PlayerId*/ == userId)!;
        player.UpdateRank(score);

        return player.PlayerRank;
    }

    public List<GamePointsEarnedPerRoundDto> GetPointsEarnedPerRound()
    {
        List<GamePointsEarnedPerRoundDto> data = new List<GamePointsEarnedPerRoundDto>();

        // goes through all players and fetches the points they had per round
        for (int i = 0; i < _players.Count; i++)
        {
            List<int> listWithPoints = new List<int>();
            var playerId = _players[i].UserId/*PlayerId*/;
            var playerName = _players[i].PlayerName;

            for (int j = 0; j < Rulebook.NumberOfRounds; j++)
            {
                if (_rounds[i].WinnerId.Contains(playerId))
                {
                    listWithPoints.Add(_rounds[i].AmountOfPointsGiven);
                }
                else
                {
                    listWithPoints.Add(0); // If the player did not win that round, we add 0 points
                }
            }

            var newPointsEarned = new GamePointsEarnedPerRoundDto(playerId, listWithPoints, playerName);
            data.Add(newPointsEarned);
        }

        return data;
    }


    public List<int> PlayerHasGuessed()
    {
        List<int> temp = new List<int>();

        for (int i = 0; i < _players.Count; i++)
        {
            if (_rounds[_currentRound].PlayerHasGuessed(_players[i].UserId/*PlayerId*/))
            {
                temp.Add(_players[i].UserId/*PlayerId*/);
            }
        }

        return temp;
    }

    // Maybe delete this one it is not used.
    public void StartNextRound(Round round)
    {

    }

    public void EndGame()
    {
        GameStatus = Status.Finished;
    }

}
