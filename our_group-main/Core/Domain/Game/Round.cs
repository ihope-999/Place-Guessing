using System.ComponentModel.Design;

namespace our_group.Core.Domain.Game;

public class Round{
    private int _id;
    private Location? _location;
    private Status _status;
    private List<string>? _winnerName;
    private List<int>? _winnerId;
    private List<PlayerGuess>? _answers;
    private int _amountOfPointsGiven;
    private int _currentReview;
    
    public Round(){} // for ef core
    
    public Round(Location location){
        _location = location;
        _status = Status.Active;
        _amountOfPointsGiven = 5;
        _currentReview = 0;   // We start with the 1 star review. 
        
        // Every round starts with the answers and winners unknown.
        _winnerName = new List<string>();
        _winnerId = new List<int>();
        _answers = new List<PlayerGuess>();
    }

    public int Id{ // for ef core
        get => _id;
        set => _id = value;
    }

    public Location Location{
        get => _location;
        set => _location = value ?? throw new ArgumentNullException(nameof(value));
    }

    public List<PlayerGuess> Answers{
        get => _answers;
        set => _answers = value ?? throw new ArgumentNullException(nameof(value));
    }

    public List<int> WinnerId{
        get => _winnerId;
        set => _winnerId = value ?? throw new ArgumentNullException(nameof(value));
    }

    public List<string> WinnerName{
        get => _winnerName;
        set => _winnerName = value ?? throw new ArgumentNullException(nameof(value));
    }

    public int AmountOfPointsGiven{
        get => _amountOfPointsGiven;
        set => _amountOfPointsGiven = value;
    }

    public Status Status{
        get => _status;
        set => _status = value;
    }

    public int CurrentReview{
        get => _currentReview;
        set => _currentReview = value;
    }

    public bool CheckIfPlayerWonRound(int playerId){
        bool playerFound = false;
        for (int i = 0; i < _winnerId.Count; i++){
            if (_winnerId[i] == playerId){
                
                playerFound = true; 
            }
        }
        return playerFound; 
    }

    public void ChangeCurrentReview(){
        // We also null out the answer list, if not no player could answer again!
        _answers.Clear();
        _winnerId.Clear(); // JUST IN CASE. DELETE IF BUGS
        _winnerName.Clear(); // SAME AS ABOVE
        _currentReview++;
        if (_currentReview >= 5){
            _currentReview = -1;
        }
    }

    public bool PlayerHasGuessed(int playerId){
        bool exist = _answers.Any(a => a.UserId == playerId);
        return exist; 
    }

    public void RoundEnd(){
        _status = Status.Finished; // MAYBE BE TURNED INTO AN EVENT?
    }
}