using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common
{
    public class Point2D
    {
        public Point2D()
        {

        }
        public Point2D(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }
        public decimal X { get; set; }
        public decimal Y { get; set; }
    }

    public class Point3D : Point2D
    {
        public Point3D()
        {

        }
        public Point3D(Point2D d, decimal z)
        {
            X = d.X;
            Y = d.Y;
            Z = z;
        }
        public Point3D(decimal x, decimal y, decimal z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public decimal Z { get; set; }
    }

    public class Size2D
    {
        public Size2D()
        {

        }
        public Size2D(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }
        public decimal X { get; set; }
        public decimal Y { get; set; }
    }

    public class Size3D : Size2D
    {
        public Size3D()
        {

        }
        public Size3D(Size2D d, decimal z)
        {
            X = d.X;
            Y = d.Y;
            Z = z;
        }
        public Size3D(decimal x, decimal y, decimal z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public decimal Z { get; set; }
    }

}
