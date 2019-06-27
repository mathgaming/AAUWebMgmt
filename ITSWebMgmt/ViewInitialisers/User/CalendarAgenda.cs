using ITSWebMgmt.Models;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSWebMgmt.ViewInitialisers.User
{
    public class CalendarAgenda
    {
        public static UserModel Init(UserModel Model)
        {
            var sb = new StringBuilder();
            // Display available meeting times.

            var temp = await Model.userController.getFreeBusyResultsAsync();

            DateTime now = DateTime.Now;
            foreach (AttendeeAvailability availability in temp.AttendeesAvailability)
            {

                foreach (CalendarEvent calendarItem in availability.CalendarEvents)
                {
                    if (calendarItem.FreeBusyStatus != LegacyFreeBusyStatus.Free)
                    {

                        bool isNow = false;
                        if (now > calendarItem.StartTime && calendarItem.EndTime > now)
                        {
                            sb.Append("<b>");
                            isNow = true;
                        }
                        sb.Append(string.Format("{0}-{1}: {2}<br/>", calendarItem.StartTime.ToString("HH:mm"), calendarItem.EndTime.ToString("HH:mm"), calendarItem.FreeBusyStatus));

                        if (isNow)
                        {
                            sb.Append("</b>");
                        }
                    }
                }
            }

            Model.CalAgenda = sb.ToString();

            return Model;
        }
    }
}
