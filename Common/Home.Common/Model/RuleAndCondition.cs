using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum ConditionKind
    {
        Or,                     // 0
        And,                    // 1
        EntityCheck,            // 2
        UserCheck,              // 3
        DeviceCheck,            // 4
        RoomPropertyCheck,      // 5
        MeshPropertyCheck,      // 6
        SceneCheck,             // 7
    }

    public class Condition
    {
        public ConditionKind Kind { get; set; }

        public Condition[] SubConditions { get; set; } = new Condition[0];

        public string ElementId { get; set; }
        public string Operator { get; set; }

        public string PropertyName { get; set; }

        public string[] InValues { get; set; }

        public string Value { get; set; }

        public TimeSpan? MinDuration { get; set; }
        public TimeSpan? MaxDuration { get; set; }



        public override string ToString()
        {
            Normalize();
        
            StringBuilder blr = new StringBuilder();
            ToStringBuilder(blr, 0);
            return blr.ToString();
        }

        public void Normalize()
        {
            if (string.IsNullOrEmpty(Operator))
                Operator = "==";
            if ("=".Equals(Operator))
                Operator = "==";
            if ("<>".Equals(Operator))
                Operator = "!=";

            if (Operator.Equals("!=") && Value!=null 
                && bool.TryParse(Value, out bool booleanVal))
            {
                Operator = "==";
                Value = (!booleanVal).ToString().ToLowerInvariant();
            }

        }

        private void ToStringBuilder(StringBuilder blr, int decal)
        {
            blr.Append(new string(' ', decal));

            switch (Kind)
            {
                case ConditionKind.Or:
                    blr.AppendLine("OR (");
                    if (SubConditions != null)
                    {
                        for (int i = 0; i < SubConditions.Length; i++)
                        {
                            if (i > 0)
                                blr.AppendLine();
                            SubConditions[i].ToStringBuilder(blr, decal + 2);
                        }
                    }
                    blr.AppendLine();
                    blr.Append(new string(' ', decal));
                    blr.AppendLine(")");
                    break;
                case ConditionKind.And:
                    blr.AppendLine("AND (");
                    if (SubConditions != null)
                    {
                        for (int i = 0; i < SubConditions.Length; i++)
                        {
                            if (i > 0)
                                blr.AppendLine();
                            SubConditions[i].ToStringBuilder(blr, decal + 2);
                        }
                    }
                    blr.AppendLine();
                    blr.Append(new string(' ', decal));
                    blr.AppendLine(")");
                    break;
                case ConditionKind.DeviceCheck:
                    blr.Append("Device(");
                    blr.Append(ElementId);
                    blr.Append(")");
                    AddOperatorAndValues(blr);
                    break;
                case ConditionKind.UserCheck:
                    blr.Append("User(");
                    blr.Append(ElementId);
                    blr.Append(")");
                    AddOperatorAndValues(blr);
                    break;
                case ConditionKind.RoomPropertyCheck:
                    blr.Append("Room(");
                    blr.Append(ElementId);
                    blr.Append(")");
                    AddOperatorAndValues(blr);
                    break;
                case ConditionKind.MeshPropertyCheck:
                    blr.Append("Mesh(");
                    blr.Append(ElementId == null ? "local" : ElementId);
                    blr.Append(")");
                    AddOperatorAndValues(blr);
                    break;
                case ConditionKind.SceneCheck:
                    blr.Append("Scene(");
                    blr.Append(ElementId == null ? "??" : ElementId);
                    blr.Append(")");
                    AddOperatorAndValues(blr);
                    break;
            }
        }

        private void AddOperatorAndValues(StringBuilder blr)
        {
            blr.Append(".");
            blr.Append(PropertyName);
            if(Operator!=null)
                blr.Append(Operator);
            else
                blr.Append(" ");
            if (InValues != null && InValues.Length > 0)
            {
                blr.Append("(");
                for (int i = 0; i < InValues.Length; i++)
                {
                    if (i > 0)
                        blr.Append(",");
                    blr.Append(InValues[i]);
                }
                blr.Append(")");
            }
            else
            {
                blr.Append(Value == null ? "-null-" : Value);
            }


        }
    }
}
