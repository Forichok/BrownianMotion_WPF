using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using BrownianMotion_WPF.DataModels;
using DevExpress.Mvvm;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace BrownianMotion_WPF.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        #region Properties

        private int _SpeedCoef = 50;
        public ParticlesViewModel ParticlesVM { get; set; }
        public Particle SelectedParticle { get; set; }

        public Particle NewParticle { get; set; }     

        public Timer Timer { get; set; }
        public bool IsLogsOn { get; set; }
        public bool IsReadyToAddNew { get; set; }
        public int SpeedCoef
        {
            get => _SpeedCoef;
            set
            {
                _SpeedCoef = value;
                Timer.Interval =  65-value;
            }
        }
         
        #endregion

        #region Constructors

        public MainViewModel()
        {
            ParticlesVM=new ParticlesViewModel();
            NewParticle = new Particle(0, 0, 0, 0, new Vector(0, 0));
            SelectedParticle = new Particle(0, 0, 0, 0, new Vector(0,0));
            Timer = new Timer();
            Timer.Tick += Timer_Tick;
            Timer.Interval = 10;
        }

        #endregion

        #region Commands
        public ICommand DragDeltaCommand
        {
            get
            {
                return new DelegateCommand<MouseDragArgs>(args =>
                {
                    ParticlesVM.DragMove(args);
                });
            }
        }

        public ICommand StartStopCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Timer.Enabled = !Timer.Enabled;
                    RaisePropertyChanged("Timer");
                });
            }
        }

        public ICommand AddNewCommand
        {
            get { return new DelegateCommand<object>(obj =>
            {
                ParticlesVM.Add(NewParticle.Clone());
            }); }
        }

        public ICommand ResetCommand
        {
            get { return new DelegateCommand<object>(obj =>
            {
                ParticlesVM.Clear();
                SelectedParticle = new Particle(0, 0, 0, 0, new SolidColorBrush(Colors.Black),new Vector(0,0));
            }); }
        }

        public ICommand DeleteCommand
        {
            get { return new DelegateCommand<object>(obj =>
            {
                ParticlesVM.Remove(SelectedParticle);
                SelectedParticle = new Particle(0, 0, 0f, 0, new SolidColorBrush(Colors.Black), new Vector(0, 0));
            }); }
        }

        public ICommand SelectCommand
        {
            get
            {
                return new DelegateCommand<MouseButtonEventArgs>(obj =>
                {
                    IsReadyToAddNew = false;
                    SelectedParticle.IsSelected = false;
                    try
                    {
                        var b = obj.OriginalSource as Ellipse;
                        SelectedParticle = b.DataContext as Particle;
                        SelectedParticle.IsSelected = true;
                    }
                    catch (Exception e)
                    {

                    }
                });
            }
        }

        public ICommand ExitCommand
        {
            get { return new DelegateCommand(() => { Application.Current.Shutdown(); }); }
        }

        public ICommand SaveProjectCommand
        {
            get
            {
                return new DelegateCommand<string>(str =>
                {
                        var fbd = new SaveFileDialog
                        {
                            Title = @"Select File To Save",
                            Filter = @"txt files (*.txt)|*.txt|XML files (*.xml)|*.xml|All files (*.*)|*.*",
                            InitialDirectory = Directory.GetCurrentDirectory()
                        };
                        var result = fbd.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            var tmpTimerState = Timer.Enabled;
                            Timer.Enabled = false;
                            if (fbd.FileName.EndsWith(".xml"))
                            {
                                ParticlesVM.SaveInXmlFile(fbd.FileName);
                            }

                            if (fbd.FileName.EndsWith(".txt"))
                            {
                                ParticlesVM.SaveInFile(fbd.FileName);
                            }

                            Timer.Enabled = tmpTimerState;
                        }
                });
            }
        }

        public ICommand OpenProjectCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var fbd = new OpenFileDialog { Filter = @"txt files (*.txt)|*.txt|XML files (*.xml)|*.xml|All files (*.*)|*.*"};
                    fbd.InitialDirectory = Directory.GetCurrentDirectory();

                    var result = fbd.ShowDialog();
                    if (result != null && !result.Value) return;

                    Timer.Enabled = false;

                    if (fbd.FileName.EndsWith(".xml"))
                    {
                        ParticlesVM.LoadFromXmlFile(fbd.FileName);
                    }

                    if (fbd.FileName.EndsWith(".txt"))
                    {
                        ParticlesVM.LoadFromFile(fbd.FileName);
                    }
                    RaisePropertyChanged("Timer");
                });
            }
        }

        #endregion
 
        private void Timer_Tick(object sender, EventArgs e)
        {
            ParticlesVM.MoveParticles();
        }        
    }
}