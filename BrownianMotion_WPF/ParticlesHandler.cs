using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using BrownianMotion_WPF.DataModels;

namespace BrownianMotion_WPF
{
    static class ParticlesHandler
    {        
        public static int Width { get; set; } = 1000;
        public static int Height { get; set; } = 1000;


        public static void Move(Particle particle)
        {
            particle.X += particle.Velocity.X;
            particle.Y += particle.Velocity.Y;
        }

        private static double GetDistance(Particle a, Particle b)
        {

            double x = Math.Pow((a.X - b.X), 2);
            double y = Math.Pow((a.Y - b.Y), 2);

            var result = Math.Sqrt(x + y);
            return result;
        }

        public static void CheckCollisionWithBorder(Particle a)
        {
            if (a.X > Width - a.Radius)
            {

                if (a.Velocity.X >= 0)
                {
                    a.X = Width - a.Radius;
                    a.Velocity = new Vector(-a.Velocity.X, a.Velocity.Y);
                    Task.Factory.StartNew(() => ChangeColor(a));
                }
            }
            else if (a.X < a.Radius)
            {
                if (a.Velocity.X <= 0)
                {
                    a.Velocity = new Vector(-a.Velocity.X, a.Velocity.Y);
                    a.X = a.Radius;
                    Task.Factory.StartNew(() => ChangeColor(a));
                }

            }
            else if (a.Y > Height - a.Radius)
            {
                if (a.Velocity.Y >= 0)
                {
                    a.Y = Height - a.Radius;
                    a.Velocity = new Vector(a.Velocity.X, -a.Velocity.Y);
                    Task.Factory.StartNew(() => ChangeColor(a));
                }
            }
            else if (a.Y < a.Radius)
            {
   
                if (a.Velocity.Y <= 0)
                {
                    a.Y = a.Radius;
                    a.Velocity = new Vector(a.Velocity.X, -a.Velocity.Y);
                    Task.Factory.StartNew(() => ChangeColor(a));
                }
            }
        }

        public static IEnumerable<Particle> LoadFromFile(string fileName)
        {
            var particles = new ObservableCollection<Particle>();

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
                            (SolidColorBrush)(new BrushConverter().ConvertFrom(args[5])),
                            new Vector(Convert.ToSingle(args[6]), Convert.ToSingle(args[7])));

                        particles.Add(particle);
                    }
                }
            }
            catch (Exception e)
            {

            }
            return particles;
        }

        public static void SaveInXmlFile(IEnumerable<Particle> particles, string path)
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

            foreach (var circle in particles)
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

        public static IEnumerable<Particle> LoadFromXmlFile(string filepatch)
        {
            var particles = new List<Particle>();
            var doc = new XmlDocument();

            doc.Load(filepatch);

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

            return particles;
        }

        private static void AddChildNode(string childName, string childText, XmlElement parentNode, XmlDocument doc)
        {
            var child = doc.CreateElement(childName);
            child.InnerText = childText;
            parentNode.AppendChild(child);
        }

        public static void SaveInFile(IEnumerable<Particle> particles, string fileName)
        {
            using (var writer = new StreamWriter(new FileStream(fileName, FileMode.OpenOrCreate)))
            {
                var strToWrite = "Width: " + Width + "; Height: " +
                                 Height + "\r\n";
                foreach (var particle in particles)
                {
                    strToWrite += particle + "\r\n";
                }

                writer.Write(strToWrite);
            }
        }

        private static List<string> ConvertArgs(List<string> args)
        {
            var convertedArgs = new List<string>();
            foreach (var arg in args)
            {
                var settingValue = arg.Substring(arg.IndexOf(':') + 1).Trim();
                convertedArgs.Add(settingValue);
            }

            return convertedArgs;
        }

        public static bool CheckCollision(Particle a, Particle b, int idA, int idB, bool isLogsOn = false)
        {
            {
                if (GetDistance(a, b) - a.Radius - b.Radius <= 0)
                {
                    var collisionResult = "===============================================================================================================" +
                                             "\r\n" +
                                             $"Particle {idA} has been collided with Particle {idB}\r\n"+
                                             $"Particle {idA}->> Vx: {a.Vx} Vy: {a.Vy} Mass: {a.Mass} Coordinates[{a.X}, {a.Y}]" +
                                             "\r\n" +
                                             $"Particle {idB}->> Vx: {b.Vx} Vy: {b.Vy} Mass: {b.Mass} Coordinates[{b.X}, {b.Y}]" +
                                             "\r\n" +
                                             $"====>>>" + "\r\n";

                    CollisionProc(a, b);

                    collisionResult += $"Particle {idA} has been collided with Particle {idB}\r\n"+
                                       $"Particle {idA}->> Vx: {a.Vx} Vy: {a.Vy} Mass: {a.Mass} Coordinates[{a.X}, {a.Y}]" +
                                       "\r\n" +
                                       $"Particle {idB}->> Vx: {b.Vx} Vy: {b.Vy} Mass: {b.Mass} Coordinates[{b.X}, {b.Y}]" +
                                       "\r\n" +
                                       $"====>>>" + "\r\n";

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

        private static void ChangeColor(Particle a, Particle b = null)
        {
            if(a.IsEnabled || b!=null && b.IsEnabled)
                return;

            a.IsEnabled = true;
            if (b != null) b.IsEnabled = true;
            
            Thread.Sleep(150);
            a.IsEnabled = false;
            if (b != null) b.IsEnabled = false;
        }

        private static void CollisionProc(Particle circle1, Particle circle2)
        {
            Point d = ClosestPointOnLine( circle1.X,  circle1.Y,
                 circle1.X +  circle1.Velocity.X,  circle1.Y +  circle1.Velocity.Y,
                 circle2.X,  circle2.Y);
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

                circle1.Velocity = new Vector( vx1,  vy1);
                circle2.Velocity = new Vector( VX2,  VY2);

                circle1.X = circle1.X + vx1;
                circle1.Y = circle1.Y + vy1;
                circle2.X = circle2.X + VX2;
                circle2.Y = circle2.Y + VY2;
            }
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
                cx =  ((A1 * C1 - B1 * C2) / det);
                cy =  ((A1 * C2 - -B1 * C1) / det);
            }
            else
            {
                cx = x0;
                cy = y0;
            }

            return new Point(cx, cy);
        }

        #region Stuff
        //public static void checkCollision(Particle circle1, Particle circle2)
        //{
        //    Vector a = new Vector( circle1.X,  circle1.Y);
        //    Vector b = new Vector( circle2.X,  circle2.Y);
        //    // Get distances between the balls components
        //    Vector distanceVect = a - b;

        //    // Calculate magnitude of the vector separating the balls
        //    double distanceVectMag = distanceVect.magnitude;

        //    // Minimum distance before they are touching
        //    double minDistance =  (circle1.Radius + circle2.Radius);

        //    if (distanceVectMag < minDistance)
        //    {
        //        double distanceCorrection = (minDistance - distanceVectMag) / 2;
        //        Vector d = distanceVect;
        //        Vector correctionVector = d.normalized * (distanceCorrection);

        //        //other.position.add(correctionVector);
        //        //position.sub(correctionVector);

        //        // get angle of distanceVect


        //        double theta =  Math.Atan2(distanceVect.x, distanceVect.y);
        //        // precalculate trig values
        //        double sine =  Math.Sin(theta);
        //        double cosine =  Math.Cos(theta);

        //        /* bTemp will hold rotated ball positions. You 
        //         just need to worry about bTemp[1] position*/
        //        Vector[] bTemp =
        //        {
        //            new Vector(), new Vector()
        //        };

        //        /* this ball's position is relative to the other
        //         so you can use the vector between them (bVect) as the 
        //         reference point in the rotation expressions.
        //         bTemp[0].position.x and bTemp[0].position.y will initialize
        //         automatically to 0.0, which is what you want
        //         since b[1] will rotate around b[0] */
        //        bTemp[1].x = cosine * distanceVect.x + sine * distanceVect.y;
        //        bTemp[1].y = cosine * distanceVect.y - sine * distanceVect.x;

        //        // rotate Temporary velocities
        //        Vector[] vTemp =
        //        {
        //            new Vector(), new Vector()
        //        };

        //        vTemp[0].x = cosine * circle1.Velocity.X + sine * circle1.Velocity.Y;
        //        vTemp[0].y = cosine * circle1.Velocity.Y - sine * circle1.Velocity.X;
        //        vTemp[1].x = cosine * circle2.Velocity.X + sine * circle2.Velocity.Y;
        //        vTemp[1].y = cosine * circle2.Velocity.Y - sine * circle2.Velocity.X;

        //        /* Now that velocities are rotated, you can use 1D
        //         conservation of momentum equations to calculate 
        //         the final velocity along the x-axis. */
        //        Vector[] vFinal =
        //        {
        //            new Vector(), new Vector()
        //        };

        //        // final rotated velocity for b[0]
        //        vFinal[0].x =
        //             (((circle1.Mass - circle2.Mass) * vTemp[0].x + 2 * circle2.Mass * vTemp[1].x) /
        //                     (circle1.Mass + circle2.Mass));
        //        vFinal[0].y = vTemp[0].y;

        //        // final rotated velocity for b[0]
        //        vFinal[1].x =
        //             (((circle2.Mass - circle1.Mass) * vTemp[1].x + 2 * circle1.Mass * vTemp[0].x) /
        //                     (circle1.Mass + circle2.Mass));
        //        vFinal[1].y = vTemp[1].y;

        //        // hack to avoid clumping
        //        bTemp[0].x += vFinal[0].x;
        //        bTemp[1].x += vFinal[1].x;

        //        /* Rotate ball positions and velocities back
        //         Reverse signs in trig expressions to rotate 
        //         in the opposite direction */
        //        // rotate balls
        //        Vector[] bFinal =
        //        {
        //            new Vector(), new Vector()
        //        };

        //        bFinal[0].x = cosine * bTemp[0].x - sine * bTemp[0].y;
        //        bFinal[0].y = cosine * bTemp[0].y + sine * bTemp[0].x;
        //        bFinal[1].x = cosine * bTemp[1].x - sine * bTemp[1].y;
        //        bFinal[1].y = cosine * bTemp[1].y + sine * bTemp[1].x;

        //        // update balls to screen position
        //        //circle2.X = circle1.Y + bFinal[1].x;
        //        //circle2.Y = circle1.Y + bFinal[1].y;

        //        //position.add(bFinal[0]);

        //        // update velocities
        //        circle1.Velocity = new Vector(cosine * vFinal[0].x - sine * vFinal[0].y,
        //            cosine * vFinal[0].y + sine * vFinal[0].x);


        //        circle2.Velocity = new Vector(cosine * vFinal[1].x - sine * vFinal[1].y,
        //            cosine * vFinal[1].y + sine * vFinal[1].x);
        //        circle1.Move();
        //        circle2.Move();
        //    }
        //}
        #endregion
    }
}

