using System;

public interface IMinigame
{
    event Action<bool> OnMinigameEnd; // true = win, false = lose
}