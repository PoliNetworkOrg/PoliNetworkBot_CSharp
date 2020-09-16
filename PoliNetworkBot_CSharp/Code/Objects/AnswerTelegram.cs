#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class AnswerTelegram
    {
        public enum State
        {
            WAITING_FOR_ANSWER,
            ANSWERED
        }

        private State _currentState;
        internal Action<object> WorkCompleted;
        private bool _answeredProcessed;

        public AnswerTelegram()
        {
            Reset();
        }

        internal State GetState()
        {
            return _currentState;
        }

        internal void RecordAnswer(string text)
        {
            if (_currentState != State.WAITING_FOR_ANSWER) return;

            _currentState = State.ANSWERED;
            if (_answeredProcessed == false)
            {
                WorkCompleted.Invoke(text);
                _answeredProcessed = true;
            }
        }

        internal void Reset()
        {
            _currentState = State.WAITING_FOR_ANSWER;
            _answeredProcessed = false;
            WorkCompleted = null;
        }

        internal void SetState(State state)
        {
            this._currentState = state;
        }

        internal void SetAnswerProcessed(bool v)
        {
            this._answeredProcessed = v;
        }

        internal bool GetAlreadyProcessedAnswer()
        {
            return this._answeredProcessed;
        }
    }
}