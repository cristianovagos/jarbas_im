using System;
using System.Collections.Generic;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System.IO;
namespace gestureModality
{
    public sealed class GestureDetector : IDisposable
    {
        /// <summary> Path to the gesture database that was trained with VGB </summary>
        private readonly string gestureDatabase = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, @"Database\Gestures_db.gbd");

        /// <summary> Name of the discrete gesture in the database for detecting when the user is holding the maximum left turn position </summary>
        private readonly string Kill_gesture = "Kill";
        /// <summary> Name of the discrete gesture in the database for detecting when the user is holding the maximum right turn position </summary>
        private readonly string Hands_Air_gesture = "Hands_air";
        /// <summary> Name of the discrete gesture in the database for detecting when the user is holding the maximum right turn position </summary>
        private readonly string Headphones_gesture = "HeadPhones";
        /// <summary> Name of the discrete gesture in the database for detecting when the user is holding the maximum right turn position </summary>
        private readonly string Jarbas_init_Right_gesture = "Jarbas_init_Right";
        /// <summary> Name of the discrete gesture in the database for detecting when the user is holding the maximum right turn position </summary>
        private readonly string Play_Pause_Left_gesture = "Play_Pause_Left";
        /// <summary> Name of the discrete gesture in the database for detecting when the user is holding the maximum right turn position </summary>
        private readonly string Swipe_Left_gesture = "Swipe_Left";
        /// <summary> Name of the discrete gesture in the database for detecting when the user is holding the maximum right turn position </summary>
        private readonly string Swipe_Right_gesture = "Swipe_Right";
        /*
                /// <summary> Name of the discrete gesture in the database for detecting when the user is holding the wheel straight </summary>
                private readonly string steerStraightGestureName = "SteerStraight";

                /// <summary> Name of the discrete gesture in the database for detecting when the user is actively turning the wheel to the left </summary>
                private readonly string steerLeftGestureName = "Steer_Left";

                /// <summary> Name of the discrete gesture in the database for detecting when the user is actively turning the wheel to the right </summary>
                private readonly string steerRightGestureName = "Steer_Right";

                /// <summary> Name of the discrete gesture in the database for detecting when the user is returning the wheel to the straight position after turning left </summary>
                private readonly string returnRightGestureName = "Return_Left";

                /// <summary> Name of the discrete gesture in the database for detecting when the user is returning the wheel to the straight position after turning right </summary>
                private readonly string returnLeftGestureName = "Return_Right";

                /// <summary> Name of the continuous gesture in the database which tracks the steering progress </summary>
                private readonly string steerProgressGestureName = "SteerProgress";
                */
        /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        /// <summary>
        /// Initializes a new instance of the GestureDetector class along with the gesture frame source and reader
        /// </summary>
        /// <param name="kinectSensor">Active sensor to initialize the VisualGestureBuilderFrameSource object with</param>
        /// <param name="gestureResultView">GestureResultView object to store gesture results of a single body to</param>
        public GestureDetector(KinectSensor kinectSensor, GestureResultView gestureResultView)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }

            if (gestureResultView == null)
            {
                throw new ArgumentNullException("gestureResultView");
            }

            this.GestureResultView = gestureResultView;
            //this.ClosedHandState = false;

            // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);

            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.IsPaused = true;
            }

            // load all gestures from the gesture database
            using (var database = new VisualGestureBuilderDatabase(this.gestureDatabase))
            {
                this.vgbFrameSource.AddGestures(database.AvailableGestures);
            }

            // disable the set of gestures which determine the 'keep straight' behavior, we will use hand state instead
           /* foreach (var gesture in this.vgbFrameSource.Gestures)
            {
                if (gesture.Name.Equals(this.Kill_gesture) || gesture.Name.Equals(this.Pray_gesture))
                {
                    this.vgbFrameSource.SetIsEnabled(gesture, false);
                }
            }*/
        }

        /// <summary> 
        /// Gets the GestureResultView object which stores the detector results for display in the UI 
        /// </summary>
        public GestureResultView GestureResultView { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the body associated with the detector has at least one hand closed
        /// </summary>
        //public bool ClosedHandState { get; set; }

        /// <summary>
        /// Gets or sets the body tracking ID associated with the current detector
        /// The tracking ID can change whenever a body comes in/out of scope
        /// </summary>
        public ulong TrackingId
        {
            get
            {
                return this.vgbFrameSource.TrackingId;
            }

            set
            {
                if (this.vgbFrameSource.TrackingId != value)
                {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the detector is currently paused
        /// If the body tracking ID associated with the detector is not valid, then the detector should be paused
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return this.vgbFrameReader.IsPaused;
            }

            set
            {
                if (this.vgbFrameReader.IsPaused != value)
                {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        /// <summary>
        /// Retrieves the latest gesture detection results from the sensor
        /// </summary>
        public void UpdateGestureData()
        {
            using (var frame = this.vgbFrameReader.CalculateAndAcquireLatestFrame())
            {
                if (frame != null)
                {
                    // get all discrete and continuous gesture results that arrived with the latest frame
                    var discreteResults = frame.DiscreteGestureResults;
                    //var continuousResults = frame.ContinuousGestureResults;

                    if (discreteResults != null)
                    {
                        float kill = this.GestureResultView.Kill;
                        float hands_air = this.GestureResultView.Hands_air;
                        float swipe_left = this.GestureResultView.Swipe_Left;
                        float swipe_right = this.GestureResultView.Swipe_Right;
                        float jarbas_init = this.GestureResultView.Jarbas_Init;
                        float headphones= this.GestureResultView.Headphones;
                        float play_pause = this.GestureResultView.Play_Pause;


                        foreach (var gesture in this.vgbFrameSource.Gestures)
                        {
                            if (gesture.GestureType == GestureType.Discrete)
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    if (gesture.Name.Equals(this.Kill_gesture))
                                    {
                                        kill = result.Confidence;
                                    }
                                    else if (gesture.Name.Equals(this.Hands_Air_gesture))
                                    {
                                        hands_air = result.Confidence;
                                    }
                                    else if (gesture.Name.Equals(this.Swipe_Left_gesture))
                                    {
                                        swipe_left = result.Confidence;
                                    }
                                    else if (gesture.Name.Equals(this.Swipe_Right_gesture))
                                    {
                                        swipe_right = result.Confidence;
                                    }
                                    else if (gesture.Name.Equals(this.Play_Pause_Left_gesture))
                                    {
                                        play_pause = result.Confidence;
                                    }
                                    else if (gesture.Name.Equals(this.Jarbas_init_Right_gesture))
                                    {
                                        jarbas_init = result.Confidence;
                                    }
                                    else if (gesture.Name.Equals(this.Headphones_gesture))
                                    {
                                        headphones = result.Confidence;
                                    }

                                }


                                /*  if (continuousResults != null)
                                  {
                                      if (gesture.Name.Equals(this.steerProgressGestureName) && gesture.GestureType == GestureType.Continuous)
                                      {
                                          ContinuousGestureResult result = null;
                                          continuousResults.TryGetValue(gesture, out result);

                                          if (result != null)
                                          {
                                              steerProgress = result.Progress;
                                          }
                                      }
                                  }*/


                                // use handstate to determine if the user is holding the steering wheel
                                // note: we could use a combination of the 'SteerStraight' 'Return_Left' and 'Return_Right' gestures here,
                                // but in this case, handstate is easier to detect and does essentially the same thing
                                /* keepStraight = this.ClosedHandState;

                                 // if either the 'Steer_Left' or 'MaxTurn_Left' gesture is detected, then we want to turn the ship left
                                 if (steerLeft || maxTurnLeft)
                                 {
                                     steerLeft = true;
                                     keepStraight = false;
                                 }

                                 // if either the 'Steer_Right' or 'MaxTurn_Right' gesture is detected, then we want to turn the ship right
                                 if (steerRight || maxTurnRight)
                                 {
                                     steerRight = true;
                                     keepStraight = false;
                                 }

                                 // clamp the progress value between 0 and 1
                                 if (steerProgress < 0)
                                 {
                                     steerProgress = 0;
                                 }
                                 else if (steerProgress > 1)
                                 {
                                     steerProgress = 1;
                                 }

                                 // Continuous gestures will always report a value while the body is tracked. 
                                 // We need to provide context to this value by mapping it to one or more discrete gestures.
                                 // For this sample, we will ignore the progress value whenever the user is not performing any of the discrete gestures.
                                 if (!steerLeft && !steerRight && !keepStraight)
                                 {
                                     steerProgress = -1;
                                 }*/

                                // update the UI with the latest gesture detection results
                                this.GestureResultView.UpdateGestureResult(true, kill, headphones,hands_air,swipe_right,swipe_left,play_pause,jarbas_init);
                            }
                        }
                    }
                }
            }
        }
    

        /// <summary>
        /// Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
        /// </summary>
        public void Dispose()
        {
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.Dispose();
                this.vgbFrameReader = null;
            }

            if (this.vgbFrameSource != null)
            {
                this.vgbFrameSource.Dispose();
                this.vgbFrameSource = null;
            }
        }
    }
}
