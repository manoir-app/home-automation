using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class Size2D
    {
        public decimal Width { get; set; }
        public decimal Length { get; set; }
    }

    public class Size3D : Size2D
    {
        public decimal Height { get; set; }
    }
}
