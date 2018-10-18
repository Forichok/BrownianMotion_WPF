using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Xml;
using BrownianMotion_WPF.DataModels;
using DevExpress.Mvvm;

namespace BrownianMotion_WPF
{
    class ParticlesViewModel:ViewModelBase
    {
        #region Properties

        public int Width { get; set; } = 1000;
        public int Height { get; set; } = 1000;
        public ObservableCollection<Particle> Particles { get; set; }
        public double AngleValue { get; set; }

        #endregion

        #region Constructors

        public ParticlesViewModel()
        {
            Particles=new ObservableCollection<Particle>();
        }

        #endregion

        #region Standard Methods

        public void Remove(Particle particle)
        {
            Particles.Remove(particle);
        }

        public void Add(Particle particle)
        {
            if (particle.Radius > 0)
                Particles.Add(particle);
        }

        public void Clear()
        {
            Particles.Clear();
        }

        #endregion

        #region Math And Movement Methods

        public void MoveParticles()
        {
            foreach (var particleA in Particles)
            {
                Move(particleA);

                CheckCollisionWithBorder(particleA);

                foreach (var particleB in Particles)
                {
                    if (particleA == particleB) continue;

                    if (CheckCollision(particleA, particleB))
                    {
                        RaisePropertyChanged("Particles");
                    }
                }
            }
        }

        private void Move(Particle particle)
        {
            particle.X += particle.Velocity.X;
            particle.Y += particle.Velocity.Y;
            AngleValue = (AngleValue + 0.1) % 360;
        }

        public void DragMove(MouseDragArgs args)
        {
            var e = args.e as DragDeltaEventArgs;

            Particle particle = (Particle)((FrameworkElement)args?.sender)?.DataContext;


            if (particle.Left + e.HorizontalChange > 0 && particle.Left + e.HorizontalChange < Width - particle.Radius * 2)
                particle.X += e.HorizontalChange;

            if (particle.Top + e.VerticalChange > 0 && particle.Top + e.VerticalChange < Height - particle.Radius * 2)
                particle.Y += e.VerticalChange;
        }

        private void CheckCollisionWithBorder(Particle a)
        {
            if (a.X > Width - a.Radius)
            {

                if (a.Velocity.X > 0)
                {
                    a.X = Width - a.Radius - 0.001;
                    a.Velocity = new Vector(-a.Velocity.X, a.Velocity.Y);
                    Task.Factory.StartNew(() => ChangeColor(a));
                }
            }
            else if (a.X < a.Radius)
            {
                if (a.Velocity.X < 0)
                {
                    a.Velocity = new Vector(-a.Velocity.X, a.Velocity.Y);
                    a.X = a.Radius + 0.001;
                    Task.Factory.StartNew(() => ChangeColor(a));
                }

            }
            else if (a.Y > Height - a.Radius)
            {
                if (a.Velocity.Y > 0)
                {
                    a.Y = Height - a.Radius - 0.001;
                    a.Velocity = new Vector(a.Velocity.X, -a.Velocity.Y);
                    Task.Factory.StartNew(() => ChangeColor(a));
                }
            }
            else if (a.Y < a.Radius)
            {

                if (a.Velocity.Y < 0)
                {
                    a.Y = a.Radius + 0.001;
                    a.Velocity = new Vector(a.Velocity.X, -a.Velocity.Y);
                    Task.Factory.StartNew(() => ChangeColor(a));
                }
            }
        }

        private bool CheckCollision(Particle a, Particle b, bool isLogsOn = false)
        {
            {
                if (GetDistance(a, b) - a.Radius - b.Radius <= 0)
                {
                    var idA = Particles.IndexOf(a) + 1;
                    var idB = Particles.IndexOf(b) + 1;
                    var collisionResult = "===============================================================================================================\r\n" +
                                             $"Particle {idA} has been collided with Particle {idB} \r\n" +
                                             $"Particle {idA}->> Vx: {a.Vx} Vy: {a.Vy} Mass: {a.Mass} Coordinates[{a.X}, {a.Y}]\r\n" +
                                             $"Particle {idB}->> Vx: {b.Vx} Vy: {b.Vy} Mass: {b.Mass} Coordinates[{b.X}, {b.Y}]\r\n" +
                                             $"====>>>\r\n";

                    CollisionProc(a, b);

                    collisionResult += $"Particle {idA} has been collided with Particle {idB} \r\n" +
                                       $"Particle {idA}->> Vx: {a.Vx} Vy: {a.Vy} Mass: {a.Mass} Coordinates[{a.X}, {a.Y}]\r\n" +
                                       $"Particle {idB}->> Vx: {b.Vx} Vy: {b.Vy} Mass: {b.Mass} Coordinates[{b.X}, {b.Y}]\r\n" +
                                       $"====>>>\r\n";

                    Task.Factory.StartNew(() => ChangeColor(a, b));


                    if (isLogsOn)
                    {
                        using (var writer =
                            new StreamWriter(new FileStream(Directory.GetCurrentDirectory() + "\\logs.txt",
                                FileMode.Append)))
                        {
                            writer.Write(collisionResult);
                        }
                    }
                    return true;
                }
                return false;
            }
        }

        private void CollisionProc(Particle circle1, Particle circle2)
        {
            Point d = ClosestPointOnLine(circle1.X, circle1.Y,
                 circle1.X + circle1.Velocity.X, circle1.Y + circle1.Velocity.Y,
                 circle2.X, circle2.Y);
            var closestdistsq = Math.Pow(circle2.X - d.X, 2) + Math.Pow((circle2.Y - d.Y), 2);
            if (closestdistsq <= Math.Pow(circle1.Radius + circle2.Radius, 2))
            {
                var backdist = Math.Sqrt(Math.Pow(circle1.Radius + circle2.Radius, 2) - closestdistsq);
                var movementVectorLength = Math.Sqrt(Math.Pow(circle1.Velocity.X, 2) + Math.Pow(circle1.Velocity.Y, 2));
                if (movementVectorLength == 0) return;
                var cX = d.X - backdist * (circle1.Velocity.X / movementVectorLength);
                var cY = d.Y - backdist * (circle1.Velocity.Y / movementVectorLength);

                var collisiondist = Math.Sqrt(Math.Pow(circle2.X - cX, 2) + Math.Pow(circle2.Y - cY, 2));
                var nX = (circle2.X - cX) / collisiondist;
                var nY = (circle2.Y - cY) / collisiondist;
                var p = 2 * (circle1.Velocity.X * nX + circle1.Velocity.Y * nY) / (circle1.Mass + circle2.Mass);

                var vx1 = circle1.Velocity.X - p * circle2.Mass * nX * 1;
                var vy1 = circle1.Velocity.Y - p * circle2.Mass * nY * 1;
                var VX2 = circle2.Velocity.X + p * circle1.Mass * nX * 1;
                var VY2 = circle2.Velocity.Y + p * circle1.Mass * nY * 1;

                circle1.Velocity = new Vector(vx1, vy1);
                circle2.Velocity = new Vector(VX2, VY2);

                circle1.X = circle1.X + vx1;
                circle1.Y = circle1.Y + vy1;
                circle2.X = circle2.X + VX2;
                circle2.Y = circle2.Y + VY2;
            }
        }

        private void ChangeColor(Particle a, Particle b = null)
        {
            if (a.IsEnabled || b != null && b.IsEnabled)
                return;

            a.IsEnabled = true;
            if (b != null) b.IsEnabled = true;

            Thread.Sleep(150);
            a.IsEnabled = false;
            if (b != null) b.IsEnabled = false;
        }

        #endregion
        
        #region Working with Files

        public IEnumerable<Particle> LoadFromFile(string fileName)
        {
            var particles = new List<Particle>();

            try
            {
                using (var reader = new StreamReader(new FileStream(fileName, FileMode.Open)))
                {
                    var str = reader.ReadToEnd();
                    List<string> parsedStrings = str.Trim().Split('\n').ToList();

                    var CanvasSize = parsedStrings[0].Split(';');

                    Width = Convert.ToInt32(CanvasSize[0].Split(':')[1].Trim());
                    Height = Convert.ToInt32(CanvasSize[1].Split(':')[1].Trim());

                    parsedStrings.RemoveAt(0);

                    foreach (var particleSettings in parsedStrings)
                    {
                        if (particleSettings.Trim().Length == 0)
                            continue;

                        var args = ConvertArgs(particleSettings.Split('\t').ToList());

                        var particle = new Particle(
                            Convert.ToDouble(args[0]),
                            Convert.ToDouble(args[1]),
                            Convert.ToDouble(args[2]),
                            Convert.ToDouble(args[3]),
                            new Vector(Convert.ToSingle(args[5]), Convert.ToSingle(args[6])));

                        particles.Add(particle);
                    }
                }
            }
            catch (Exception e)
            {

            }
            Particles=new ObservableCollection<Particle>(particles);
            return particles;
        }

        public void SaveInFile(string path)
        {
            using (var writer = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate)))
            {
                var strToWrite = "Width: " + Width + "; Height: " +
                                 Height + "\r\n";
                foreach (var particle in Particles)
                {
                    strToWrite += particle + "\r\n";
                }

                writer.Write(strToWrite);
            }
        }

        public void SaveInXmlFile(string path)
        {
            var doc = new XmlDocument();

            var xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            doc.AppendChild(xmlDeclaration);

            var root = doc.CreateElement("collection");

            var weightAttr = doc.CreateAttribute("Weidth");
            weightAttr.InnerText = Width.ToString();
            root.Attributes.Append(weightAttr);

            var heightAttr = doc.CreateAttribute("Height");
            heightAttr.InnerText = Height.ToString();
            root.Attributes.Append(heightAttr);

            foreach (var circle in Particles)
            {
                var circleNode = doc.CreateElement("Circle");

                AddChildNode("Radius", circle.Radius.ToString(), circleNode, doc);
                AddChildNode("Mass", circle.Mass.ToString(), circleNode, doc);
                AddChildNode("VX", circle.Velocity.X.ToString(), circleNode, doc);
                AddChildNode("VY", circle.Velocity.Y.ToString(), circleNode, doc);
                AddChildNode("X", circle.X.ToString(), circleNode, doc);
                AddChildNode("Y", circle.Y.ToString(), circleNode, doc);

                root.AppendChild(circleNode);
            }

            doc.AppendChild(root);

            doc.Save(path);
        }

        public IEnumerable<Particle> LoadFromXmlFile(string path)
        {
            var particles = new List<Particle>();
            var doc = new XmlDocument();

            doc.Load(path);

            var root = doc.DocumentElement;

            if (root == null)
            {
                throw new FileLoadException("incorrect file structure");
            }

            var weidth = Convert.ToInt32(root.Attributes["Weidth"].InnerText);
            var height = Convert.ToInt32(root.Attributes["Height"].InnerText);

            Height = height;
            Width = weidth;

            foreach (var child in root.ChildNodes)
            {
                if (child is XmlNode node)
                {
                    var radius = Convert.ToInt32(node["Radius"].InnerText);
                    var mass = Convert.ToInt32(node["Mass"].InnerText);
                    var vx = Convert.ToDouble(node["VX"].InnerText);
                    var vy = Convert.ToDouble(node["VY"].InnerText);
                    var x = Convert.ToDouble(node["X"].InnerText);
                    var y = Convert.ToDouble(node["Y"].InnerText);

                    var loadedFigure = new Particle(x, y, radius, mass, new Vector(vx, vy));
                    particles.Add(loadedFigure);
                }
            }

            Particles = new ObservableCollection<Particle>(particles);
            return particles;
        }

   

        private void AddChildNode(string childName, string childText, XmlElement parentNode, XmlDocument doc)
        {
            var child = doc.CreateElement(childName);
            child.InnerText = childText;
            parentNode.AppendChild(child);
        }

        #endregion

        #region Helper methods

        private List<string> ConvertArgs(List<string> args)
        {
            var convertedArgs = new List<string>();
            foreach (var arg in args)
            {
                var settingValue = arg.Substring(arg.IndexOf(':') + 1).Trim();
                convertedArgs.Add(settingValue);
            }

            return convertedArgs;
        }
        private static Point ClosestPointOnLine(double lx1, double ly1, double lx2, double ly2, double x0, double y0)
        {
            var A1 = ly2 - ly1;
            var B1 = lx1 - lx2;
            var C1 = (ly2 - ly1) * lx1 + (lx1 - lx2) * ly1;
            var C2 = -B1 * x0 + A1 * y0;
            var det = A1 * A1 - -B1 * B1;
            double cx = 0;
            double cy = 0;
            if (det != 0)
            {
                cx = ((A1 * C1 - B1 * C2) / det);
                cy = ((A1 * C2 - -B1 * C1) / det);
            }
            else
            {
                cx = x0;
                cy = y0;
            }

            return new Point(cx, cy);
        }

        private static double GetDistance(Particle a, Particle b)
        {

            double x = Math.Pow((a.X - b.X), 2);
            double y = Math.Pow((a.Y - b.Y), 2);

            var result = Math.Sqrt(x + y);
            return result;
        }

        #endregion
    }
}
