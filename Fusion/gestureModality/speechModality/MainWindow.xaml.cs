using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using mmisharp;
namespace gestureModality
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        private bool reset = false;
        
        /// <summary> Active Kinect sensor </summary>
        private KinectSensor kinectSensor = null;

        
        /// <summary> Array for the bodies (Kinect can track up to 6 people simultaneously) </summary>
        private Body[] bodies = null;

        /// <summary>  Index of the active body (first tracked person in the body array) </summary>
        private int activeBodyIndex = 0;

        /// <summary> Reader for body frames </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary> Current kinect status text to display </summary>
        private string statusText = null;

        /// <summary> KinectBodyView object which handles drawing the active body to a view box in the UI </summary>
        private KinectBodyView kinectBodyView = null;

        /// <summary> Gesture detector which will be tied to the active body (closest skeleton to the sensor) </summary>
        private GestureDetector gestureDetector = null;

        /// <summary> GestureResultView for displaying gesture results associated with the tracked person in the UI </summary>
        private GestureResultView gestureResultView = null;

        /// <summary> Timer for updating Kinect frames and space images at 60 fps </summary>
        private DispatcherTimer dispatcherTimer = null;

        private LifeCycleEvents lce;
        private MmiCommunication mmic;
        private bool inciou;

        public MainWindow()
        {
            lce = new LifeCycleEvents("TOUCH", "FUSION", "touch-1", "touch", "command");
            mmic = new MmiCommunication("localhost", 9876, "User1", "TOUCH");
            mmic.Send(lce.NewContextRequest());
            InitializeComponent();

            // only one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            // open the sensor
            this.kinectSensor.Open();

            // set the initial status text
            this.UpdateKinectStatusText();

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // initialize the BodyViewer object for displaying tracked bodies in the UI
            this.kinectBodyView = new KinectBodyView(this.kinectSensor);

            // initialize the GestureDetector object
            this.gestureResultView = new GestureResultView(false, 0, 0);
            this.gestureDetector = new GestureDetector(this.kinectSensor, this.gestureResultView);

            // set data context objects for display in UI
            this.DataContext = this;
            this.kinectBodyViewbox.DataContext = this.kinectBodyView;
            
            //this.gestureResultGrid.DataContext = this.gestureResultView;

        }

        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            private set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Disposes all unmanaged resources for the class
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Disposes the GestureDetector object
        /// </summary>
        /// <param name="disposing">True if Dispose was called directly, false if the GC handles the disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.gestureDetector != null)
                {
                    this.gestureDetector.Dispose();
                    this.gestureDetector = null;
                }
            }
        }


        /// <summary>
        /// Starts the dispatcher timer to check for new Kinect frames and update objects in space @60fps
        /// Note: We are using a dispatcher timer to demonstrate usage of the VGB polling APIs,
        /// please see the 'DiscreteGestureBasics-WPF' sample for event notification.
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            // set the UI to render at 60fps
            CompositionTarget.Rendering += this.DispatcherTimer_Tick;

            // set the game timer to run at 60fps
            this.dispatcherTimer = new DispatcherTimer();
            this.dispatcherTimer.Tick += this.DispatcherTimer_Tick;
            this.dispatcherTimer.Interval = TimeSpan.FromSeconds(1 / 60);
            this.dispatcherTimer.Start();
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            CompositionTarget.Rendering -= this.DispatcherTimer_Tick;

            if (this.dispatcherTimer != null)
            {
                this.dispatcherTimer.Stop();
                this.dispatcherTimer.Tick -= this.DispatcherTimer_Tick;
            }

            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.gestureDetector != null)
            {
                // The GestureDetector contains disposable members (VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader)
                this.gestureDetector.Dispose();
                this.gestureDetector = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// <summary>
        /// Gets the first body in the bodies array that is currently tracked by the Kinect sensor
        /// </summary>
        /// <returns>Index of first tracked body, or -1 if no body is tracked</returns>
        private int GetActiveBodyIndex()
        {
            int activeBodyIndex = -1;
            int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;

            for (int i = 0; i < maxBodies; ++i)
            {
                // find the first tracked body and verify it has hands tracking enabled (by default, Kinect will only track handstate for 2 people)
                if (this.bodies[i].IsTracked && (this.bodies[i].HandRightState != HandState.NotTracked || this.bodies[i].HandLeftState != HandState.NotTracked))
                {
                    activeBodyIndex = i;
                    break;
                }
            }

            return activeBodyIndex;
        }

        /// <summary>
        /// Retrieves the latest body frame data from the sensor and updates the associated gesture detector object
        /// </summary>
        private void UpdateKinectFrameData()
        {
            bool dataReceived = false;

            using (var bodyFrame = this.bodyFrameReader.AcquireLatestFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        // creates an array of 6 bodies, which is the max number of bodies that Kinect can track simultaneously
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);

                    if (!this.bodies[this.activeBodyIndex].IsTracked)
                    {
                        // we lost tracking of the active body, so update to the first tracked body in the array
                        int bodyIndex = this.GetActiveBodyIndex();

                        if (bodyIndex > 0)
                        {
                            this.activeBodyIndex = bodyIndex;
                        }
                    }

                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                Body activeBody = this.bodies[this.activeBodyIndex];

                // visualize the new body data
                this.kinectBodyView.UpdateBodyData(activeBody);

                // visualize the new gesture data
                if (activeBody.TrackingId != this.gestureDetector.TrackingId)
                {
                    // if the tracking ID changed, update the detector with the new value
                    this.gestureDetector.TrackingId = activeBody.TrackingId;
                }

                if (this.gestureDetector.TrackingId == 0)
                {
                    // the active body is not tracked, pause the detector and update the UI
                    this.gestureDetector.IsPaused = true;
                    //this.gestureDetector.ClosedHandState = false;
                    this.gestureResultView.UpdateGestureResult(false, 0, 0,0,0,0,0,0);
                }
                else
                {
                    // the active body is tracked, unpause the detector
                    this.gestureDetector.IsPaused = false;
                    // get the latest gesture frame from the sensor and updates the UI with the results
                    this.gestureDetector.UpdateGestureData();
                    this.GJson();
                }
            }
        }
        /// <summary>
        /// Updates the StatusText with the latest sensor state information
        /// </summary>
        private void UpdateKinectStatusText()
        {
            // reset the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies UI that a property has changed
        /// </summary>
        /// <param name="propertyName">Name of property that has changed</param> 
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.UpdateKinectStatusText();
            this.UpdateKinectFrameData();


        }
        private void GJson()
        {
            string gesture = "{ \"recognized\": ";
            if (reset == false)
            {
                if (this.gestureResultView.Headphones < 0.2 &&
                        this.gestureResultView.Hands_air < 0.3 &&
                        this.gestureResultView.Kill < 0.2 &&
                        this.gestureResultView.Play_Pause < 0.2 &&
                        this.gestureResultView.Swipe_Left < 0.2 &&
                        this.gestureResultView.Swipe_Right < 0.2 &&
                        this.gestureResultView.Jarbas_Init < 0.2)
                {
                    reset = true;
                    this.stackpanelBox.Background = new SolidColorBrush(Colors.Red);
                    this.stacklabel.Text = "";
                }
            }
            else
            {
                if (this.gestureResultView.Hands_air > 0.8)
                {
                    gesture += "[\"" + "Hands_air\"] }";
                    gesture.Substring(0, gesture.Length - 2);

                    this.stackpanelBox.Background = new SolidColorBrush(Colors.Green);
                    this.stacklabel.Text = "MODO LAZER";

                    var exNot = lce.ExtensionNotification("", "", this.gestureResultView.Hands_air, gesture);
                    mmic.Send(exNot);
                    reset = false;

                    Console.WriteLine(gesture);
                }
                else if (this.gestureResultView.Kill > 0.5)
                {
                    gesture += "[\"" + "Kill\"] }";
                    gesture.Substring(0, gesture.Length - 2);

                    this.stackpanelBox.Background = new SolidColorBrush(Colors.Green);
                    this.stacklabel.Text = "MODO TRABALHO";

                    var exNot = lce.ExtensionNotification("", "", this.gestureResultView.Kill, gesture);
                    mmic.Send(exNot);
                    reset = false;

                    Console.WriteLine(gesture);
                }
                else if (this.gestureResultView.Headphones > 0.5)
                {
                    gesture += "[\"" + "Headphones\"] }";
                    gesture.Substring(0, gesture.Length - 2);

                    

                    var exNot = lce.ExtensionNotification("", "", this.gestureResultView.Headphones, gesture);
                    mmic.Send(exNot);
                    reset = false;

                    Console.WriteLine(gesture);
                }
                else if (this.gestureResultView.Play_Pause > 0.5)
                {
                    gesture += "[\"" + "Play_Pause\"] }";
                    gesture.Substring(0, gesture.Length - 2);

                    this.stackpanelBox.Background = new SolidColorBrush(Colors.Green);
                    this.stacklabel.Text = "SCREENSHOT";

                    var exNot = lce.ExtensionNotification("", "", this.gestureResultView.Play_Pause, gesture);
                    mmic.Send(exNot);
                    reset = false;

                    Console.WriteLine(gesture);
                }
                else if (this.gestureResultView.Swipe_Left > 0.5)
                {
                    gesture += "[\"" + "Swipe_Left\"] }";
                    gesture.Substring(0, gesture.Length - 2);

                    this.stackpanelBox.Background = new SolidColorBrush(Colors.Green);
                    this.stacklabel.Text = "SWIPE ESQUERDA";

                    var exNot = lce.ExtensionNotification("", "", this.gestureResultView.Swipe_Left, gesture);
                    mmic.Send(exNot);
                    reset = false;

                    Console.WriteLine(gesture);
                }
                else if (this.gestureResultView.Swipe_Right > 0.4)
                {
                    gesture += "[\"" + "Swipe_Right\"] }";
                    gesture.Substring(0, gesture.Length - 2);

                    this.stackpanelBox.Background = new SolidColorBrush(Colors.Green);
                    this.stacklabel.Text = "SWIPE DIREITA";

                    var exNot = lce.ExtensionNotification("", "", this.gestureResultView.Swipe_Right, gesture);
                    mmic.Send(exNot);
                    reset = false;

                    Console.WriteLine(gesture);
                }
                else if (this.gestureResultView.Jarbas_Init > 0.5)
                {
                    gesture += "[\"" + "Jarbas_init\"] }";
                    gesture.Substring(0, gesture.Length - 2);

                    //this.stackpanelBox.Background = new SolidColorBrush(Colors.Green);
                    //this.stacklabel.Text = "TERMINAR SESSÃO";

                    var exNot = lce.ExtensionNotification("", "", this.gestureResultView.Jarbas_Init, gesture);
                    mmic.Send(exNot);
                    reset = false;

                    Console.WriteLine(gesture);
                }

                /*if (this.gestureResultView.Headphones > 0.5 ||
                        this.gestureResultView.Hands_air > 0.7 ||
                        this.gestureResultView.Kill > 0.5 ||
                        this.gestureResultView.Play_Pause > 0.5 ||
                        this.gestureResultView.Swipe_Left > 0.5 ||
                        this.gestureResultView.Swipe_Right > 0.4 ||
                        this.gestureResultView.Jarbas_Init > 0.5)
                {
                    gesture += "\"" + "Kill" + "\":\"" + this.gestureResultView.Kill + "\", ";
                    gesture += "\"" + "Hands_air" + "\":\"" + this.gestureResultView.Hands_air + "\", ";
                    gesture += "\"" + "Headphones" + "\":\"" + this.gestureResultView.Headphones + "\", ";
                    gesture += "\"" + "Swipe_Right" + "\":\"" + this.gestureResultView.Swipe_Right + "\", ";
                    gesture += "\"" + "Swipe_Left" + "\":\"" + this.gestureResultView.Swipe_Left + "\", ";
                    gesture += "\"" + "Play_Pause" + "\":\"" + this.gestureResultView.Play_Pause + "\", ";
                    gesture += "\"" + "Jarbas_init" + "\":\"" + this.gestureResultView.Jarbas_Init + "\" ";

                    gesture.Substring(0, gesture.Length - 2);
                    
                    var exNot = lce.ExtensionNotification("", "", 0.0f, gesture);
                    Console.WriteLine("Kill" + this.gestureResultView.Kill);
                    Console.WriteLine("Play_Pause" + this.gestureResultView.Play_Pause);
                    Console.WriteLine("Headphones" + this.gestureResultView.Headphones);
                    Console.WriteLine("Hands_air" + this.gestureResultView.Hands_air);
                    Console.WriteLine("Swipe_Left" + this.gestureResultView.Swipe_Left);
                    Console.WriteLine("Swipe_right" + this.gestureResultView.Swipe_Right);
                    Console.WriteLine("Jarbas_init" + this.gestureResultView.Jarbas_Init);
                    mmic.Send(exNot);
                    reset = false;
                }*/

            }

        }

    }

}
