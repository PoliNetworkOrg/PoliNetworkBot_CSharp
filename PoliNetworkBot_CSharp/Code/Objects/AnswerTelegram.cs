#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class AnswerTelegram
    {
        public enum State
        {
            WaitingForAnswer,
            Answered
        }

        private State current_state = State.WaitingForAnswer;
        internal Action<object> WorkCompleted;

        public AnswerTelegram()
        {
            current_state = State.WaitingForAnswer;
        }

        internal State GetState()
        {
            return current_state;
        }

        internal void RecordAnswer(string text)
        {
            if (current_state == State.WaitingForAnswer)
            {
                current_state = State.Answered;
                WorkCompleted.Invoke(text);
            }
        }
    }
}