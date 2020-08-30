using System;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class DateTimeSchedule
    {
        private readonly DateTime? _dateTime;
        private readonly bool _schedule;
        public DateTimeSchedule(DateTime? dateTime, bool schedule)
        {
            this._dateTime = dateTime;
            this._schedule = schedule;
        }

        public bool IsInvalid()
        {
            return _schedule && this._dateTime == null;
        }

        public DateTime? GetDate()
        {
            return this._dateTime;
        }
    }
}