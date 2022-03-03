#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class DateTimeSchedule
    {
        private readonly DateTime? _dateTime;
        private readonly bool _schedule;

        public DateTimeSchedule(DateTime? dateTime, bool schedule)
        {
            _dateTime = dateTime;
            _schedule = schedule;
        }

        public bool IsInvalid()
        {
            return _schedule && _dateTime == null;
        }

        public DateTime? GetDate()
        {
            return _dateTime;
        }
    }
}