using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum ElementCommandKind
    {
        Message
    }

    public class ElementCommand
    {
        public string Label { get; set; }

        public ElementCommandKind Kind { get; set; }

        public string MessageTopic { get; set; }
        public string Value { get; set; }
    }
}
