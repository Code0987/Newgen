using System;
using Google.GData.Calendar;

namespace Calendar
{
    /// <summary>
    /// Summary description for Calendar.
    /// </summary>
    public class CalendarInfo
    {
        public CalendarData GetFeedOfCalendar(String feedUrl, String user, String pass)
        {
            var myService = new CalendarService("Newgen_Calendar");
            myService.setUserCredentials(user, pass);

            var myQuery = new EventQuery(feedUrl);
            myQuery.StartTime = DateTime.Now;
            myQuery.EndTime = DateTime.Today.AddDays(2);    // we search two days after

            EventFeed calFeed = myService.Query(myQuery);

            // now populate the calendar
            if (calFeed.Entries.Count > 0)
            {
                var entry = (EventEntry)calFeed.Entries[0];
                var result = new CalendarData();

                // Title
                result.Title = entry.Title.Text;

                if (entry.Locations.Count > 0 && !string.IsNullOrEmpty(entry.Locations[0].ValueString))
                    result.Location = entry.Locations[0].ValueString;
                else if (entry.Content != null)
                {
                    result.Description = entry.Content.Content;
                }

                if (entry.Times.Count > 0)
                {
                    result.BeginTime = entry.Times[0].StartTime;
                    result.EndTime = entry.Times[0].EndTime;
                }

                return result;
            }
            return null;
        }
    }
}