namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class AnswerTelegram
    {
        public enum State
        {
            WaitingForAnswer, Answered
        }

        private AnswerTelegram.State current_state = State.WaitingForAnswer;
        internal System.Action<object> WorkCompleted;

        public AnswerTelegram()
        {
            this.current_state = State.WaitingForAnswer;
        }

        internal AnswerTelegram.State GetState()
        {
            return this.current_state;
        }

        internal void RecordAnswer(string text)
        {
            if (this.current_state == State.WaitingForAnswer)
            {
                this.current_state = State.Answered;
                this.WorkCompleted.Invoke(text);
            }
        }
    }
}