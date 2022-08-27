using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Home.Common.FileFormats
{
    public static class VCalendarFormat
    {
        public static string FromEventList(List<TodoItem> items)
        {
            StringBuilder blr = new StringBuilder();

            blr.Append("BEGIN:VCALENDAR");
            blr.Append("\r\n");
            // on doit forcément être en CRLF
            // d'après le validator de https://icalendar.org/validator.html#l1

            blr.Append("VERSION:2.0");
            blr.Append("\r\n");
            blr.Append("PRODID:-//manoir.app");
            blr.Append("\r\n");

            foreach (TodoItem item in items)
            {
                blr.Append("BEGIN:VEVENT");
                blr.Append("\r\n");
                var dtStart = item.DueDate.GetValueOrDefault().ToUniversalTime();
                blr.Append("DTSTAMP:");
                blr.Append(DateTimeOffset.Now.ToUniversalTime().ToString("yyyyMMddTHHmmss") + "Z");
                blr.Append("\r\n");

                blr.Append("UID:");
                blr.Append(item.Id);
                blr.Append("\r\n");


                if (!item.Duration.HasValue)
                {
                    blr.Append("DTSTART;VALUE=DATE:");
                    blr.Append(dtStart.ToString("yyyyMMdd"));
                    blr.Append("\r\n");
                    blr.Append("DTEND;VALUE=DATE:");
                    blr.Append(dtStart.AddDays(1).ToString("yyyyMMdd"));
                    blr.Append("\r\n");
                    blr.Append("X-MICROSOFT-CDO-ALLDAYEVENT:TRUE");
                    blr.Append("\r\n");
                    blr.Append("X-MICROSOFT-CDO-BUSYSTATUS:FREE");
                    blr.Append("\r\n");
                }
                else
                {
                    blr.Append("DTSTART:");
                    blr.Append(dtStart.ToString("yyyyMMddTHHmmss") + "Z");
                    blr.Append("\r\n");
                    blr.Append("DURATION:");
                    blr.Append(XmlConvert.ToString(item.Duration.Value));
                    blr.Append("\r\n");
                }

                blr.Append("STATUS:CONFIRMED");
                blr.Append("\r\n");


                blr.Append("SUMMARY:");
                blr.Append(item.Label);
                blr.Append("\r\n");
                blr.Append("END:VEVENT");
                blr.Append("\r\n");
            }

            blr.Append("END:VCALENDAR");
            blr.Append("\r\n");

            return blr.ToString();
        }

    }
}
