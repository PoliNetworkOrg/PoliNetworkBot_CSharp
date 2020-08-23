using System;

namespace PoliNetworkBot_CSharp.Objects
{
    internal class AnswerTelegram
    {
        public enum State
        {
            WaitingForAnswer, Answered
        }

        AnswerTelegram.State current_state = State.WaitingForAnswer;
        private string answer = null;

        public AnswerTelegram()
        {
            this.current_state = State.WaitingForAnswer;
        }

        internal AnswerTelegram.State GetState()
        {
            return this.current_state;
        }

        internal string GetAnswer()
        {
            switch (current_state)
            {
                case State.WaitingForAnswer:
                    return null;
                case State.Answered:
                    return answer;
                default:
                    return null;
            }
        }

        internal void RecordAnswer(string text)
        {
            this.answer = text;
            this.current_state = State.Answered;
        }
    }
}