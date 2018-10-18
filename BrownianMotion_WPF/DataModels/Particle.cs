using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using BrownianMotion_WPF.DataModels;
using DevExpress.Mvvm;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;


namespace BrownianMotion_WPF.DataModels
{
    class Particle:ViewModelBase
    {
        #region Properties

        public Timer Timer { get; set; }
        public SolidColorBrush Color { get; set; }
        public Vector Velocity { get; set; }        
        public double Mass { get; set; }
        public double Radius { get; set; }
        public double Size => Radius * 2;
        public int AngleValue { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsSelected { get; set; }

        public double X { get; set; }            

        public double Y { get; set; }
        public double Vx
        {
            get => Velocity.X;
            set => Velocity = new Vector(value, Velocity.Y);
        }
        public double Vy
        {
            get => Velocity.Y;
            set => Velocity=new Vector(Velocity.X,value);
        }

        public double Left => X - Radius;

        public double Top => Y - Radius;

        #endregion

        #region Constructors

        public Particle(double x, double y, double radius, double mass, Vector velocity)
        {
            Velocity=new Vector();            
            X = x;
            Y = y;
            Mass = mass;
            Radius = radius;
            Velocity = velocity;
            Color=new SolidColorBrush(Colors.DarkSlateBlue);
        }
        public Particle(double x, double y, double radius, double mass, SolidColorBrush color, Vector velocity)
        {
            Timer = new Timer();
            Timer.Tick += Timer_Tick;
            Timer.Interval = 100;
            Velocity = new Vector(0,0);            
            X = x+Radius*1.01;
            Y = y+Radius*1.01;
            Mass = mass;
            Radius = radius;
            Velocity = velocity;//= Vector.Normalize(
            Color = color;            
        }

        #endregion

        #region BaseMethods

        public Particle Clone()
        {
            return new Particle(X, Y, Radius, Mass, Color, Velocity);
        }

        public override string ToString()
        {
            var result = $"X: {X}\tY: {Y}\tRadius: {Radius}\tMass: {Mass}\tColor: {Color}\tVx: {Vx}\tVy: {Vy}";
            return result;
        }

        #endregion

        private void Timer_Tick(object sender, EventArgs e)
        {

            AngleValue += 100;
        }
    }
}
