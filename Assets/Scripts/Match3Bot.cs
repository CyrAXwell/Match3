using System.Collections.Generic;
using UnityEngine;

public class Match3Bot : MonoBehaviour
{
    [SerializeField] private Match3 match3;
    [SerializeField] private Match3Visual match3Visual;

    private void Awake()
    {
        match3Visual.StateChanged += OnStateChange;
    }

    private void OnStateChange(Match3Visual.State state)
    {
        switch (state) 
        {
            case Match3Visual.State.WaitingForUser:
                BotDoMove();
                break;
        }
    }

    private void BotDoMove()
    {
        Match3.PossibleMove possibleMove = GetBestPossibleMove();
        
        if (possibleMove != null)
        {
            match3Visual.BotSwap(possibleMove.StartX, possibleMove.StartY, possibleMove.EndX, possibleMove.EndY);
            match3.DoMove();
        }
        else
        {
            Debug.Log("NotPossibleMove");
        }
    }

    public Match3.PossibleMove GetBestPossibleMove()
    {
        Match3.PossibleMove bestMove = null;
        Match3.PossibleMove bestGlassMove = null;

        int maxScore = 0;
        int maxGlassScore = 0;

        for (int x = 0; x < match3.GetLevelSO().Width; x++)
        {
            for (int y = 0; y < match3.GetLevelSO().Height; y++)
            {
                List<Match3.PossibleMove> moves = new List<Match3.PossibleMove>();
                moves.Add(new Match3.PossibleMove(x, y, x + 1, y));
                moves.Add(new Match3.PossibleMove(x, y, x - 1, y));
                moves.Add(new Match3.PossibleMove(x, y, x, y + 1));
                moves.Add(new Match3.PossibleMove(x, y, x, y - 1));

                foreach(Match3.PossibleMove move in moves)
                {
                    if(match3.IsValidPosition(move.EndX, move.EndY))
                    {
                        match3.Swap(move.StartX, move.StartY, move.EndX, move.EndY);

                        int score = match3.GetMatch3LinkScore();
                        if (score > maxScore)
                        {
                            maxScore = score;
                            bestMove = move;
                        }

                        int glassScore = match3.GetMatch3LinkGlassScore();
                        if (glassScore > maxGlassScore)
                        {
                            maxGlassScore = glassScore;
                            bestGlassMove = move;
                        }
                        
                        match3.Swap(move.StartX, move.StartY, move.EndX, move.EndY);

                    }
                }  
            }
        }

        if (match3.GetLevelSO().Target == TargetType.glass && bestGlassMove != null)
            return bestGlassMove;
        else
            return bestMove;
        
    }

}
