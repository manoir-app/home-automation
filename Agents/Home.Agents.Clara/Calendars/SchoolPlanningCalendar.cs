using Home.Common.Model;
using Home.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Home.Agents.Clara.Calendars
{
    public static class SchoolPlanningCalendar
    {
        internal static Location _meshLocation = null;

        public static List<TodoItem> GetNextScheduledItems(DateTimeOffset maxDate)
        {
            List<TodoItem> todoItems = new List<TodoItem>();

            //var allintegs = AgentHelper.GetMyIntegrations("clara", true);
            //var school = (from z in allintegs where z.Id.Equals("SchoolPlanning") select z).FirstOrDefault();
            //if (school != null)
            //{
            if (_meshLocation == null)
                _meshLocation = AgentHelper.GetLocalMeshLocation("clara");
            if (!string.IsNullOrEmpty(_meshLocation.Country))
            {
                IEnumerable<TodoItem> fromSch = null;
                switch (_meshLocation.Country.ToUpperInvariant())
                {
                    case "FRA":
                    case "FRANCE":
                    case "FR":
                        fromSch = new SchoolPlanning.FranceSchoolPlanning().GetNextScheduledItems(maxDate);
                        break;
                    default:
                        Console.WriteLine($"Country {_meshLocation.Country} has no school planning module");
                        break;
                }
                if (fromSch != null)
                    todoItems.AddRange(fromSch);
            }
            //}
            return todoItems;
        }
    }
}
