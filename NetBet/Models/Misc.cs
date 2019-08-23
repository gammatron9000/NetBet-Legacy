using System;

namespace NetBet.Models
{

    public enum Result
    {
        TBD,
        Win,
        Lose
    }
    public enum WinMethod
    {
        NA,
        UD,
        SD,
        TKO,
        SUB,
        DQ,
        Any
    }

    public enum Round
    {
        NA = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Any = 6
    }
}