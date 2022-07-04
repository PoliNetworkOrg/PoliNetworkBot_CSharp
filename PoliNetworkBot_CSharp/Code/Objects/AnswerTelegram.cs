#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class AnswerTelegram
{
    public enum State
    {
        WAITING_FOR_ANSWER,
        ANSWERED
    }

    private bool _answeredProcessed;

    private State _currentState;
    internal Action<object?>? WorkCompleted;

    public AnswerTelegram()
    {
        Reset();
    }

    internal State GetState()
    {
        return _currentState;
    }

    internal void RecordAnswer(string? text)
    {
        if (_currentState != State.WAITING_FOR_ANSWER) return;

        _currentState = State.ANSWERED;
        if (_answeredProcessed) return;
        WorkCompleted?.Invoke(text);
        _answeredProcessed = true;
    }

    internal void Reset()
    {
        _currentState = State.WAITING_FOR_ANSWER;
        _answeredProcessed = false;
        WorkCompleted = null;
    }

    internal void SetState(State state)
    {
        _currentState = state;
    }

    internal void SetAnswerProcessed(bool v)
    {
        _answeredProcessed = v;
    }

    internal bool GetAlreadyProcessedAnswer()
    {
        return _answeredProcessed;
    }
}