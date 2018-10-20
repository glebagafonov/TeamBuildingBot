using System;

namespace Bot.Infrastructure.Helpers
{
    public static class DateTimeHelper
    {
        public const string TimespanFormat = "h\\:mm";
        public const string DateFormat = "dd.MM.yyyy HH:mm";
        
        public static DateTime Subtract(this DateTime dateTime, TimeSpan time, TimeSpan openingTime, TimeSpan closingTime)
        {
            TimeSpan timeInCurrentDay = (dateTime - time) - (dateTime.Date + openingTime);
            if (timeInCurrentDay.Ticks < 0)

            {
                return Subtract(GetPrevWorkingDay(dateTime.Date, closingTime), new TimeSpan(-timeInCurrentDay.Ticks),  openingTime, closingTime);
            }

            return dateTime - time;
        }
        
        public static DateTime Add(this DateTime dateTime, TimeSpan time, TimeSpan openingTime, TimeSpan closingTime)
        {
            TimeSpan timeInCurrentDay = dateTime.Date + closingTime - dateTime - time;
            if (timeInCurrentDay.Ticks < 0)

            {
                return Add(GetNextWorkingDay(dateTime.Date, openingTime),new TimeSpan(-timeInCurrentDay.Ticks),  openingTime, closingTime);
            }

            return dateTime + time;
        }
        
        private static DateTime GetNextWorkingDay(DateTime date, TimeSpan openingTime)

        {
            return date.AddDays(1) + openingTime;

        }
        
        private static DateTime GetPrevWorkingDay(DateTime date,TimeSpan closingTime)

        {
            return date.AddDays(-1) + closingTime;

        }
    }
}