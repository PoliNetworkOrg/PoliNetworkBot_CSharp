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

        public AnswerTelegram()
        {
            _currentState = State.WAITING_FOR_ANSWER;
        }

        internal State GetState()
        {
            return _currentState;
        }

        internal void RecordAnswer(string text)
        {
            if (_currentState != State.WAITING_FOR_ANSWER) return;

            _currentState = State.ANSWERED;
            WorkCompleted.Invoke(text);
        }
    }
}