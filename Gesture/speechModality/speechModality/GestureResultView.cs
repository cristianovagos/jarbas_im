using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;


namespace gestureModality
{
    public sealed class GestureResultView : Prism.Mvvm.BindableBase
    {

        ///usar float para passar nivel de confiança!!
        /// <summary> True, if the user is attempting to turn left (either 'Steer_Left' or 'MaxTurn_Left' is detected) </summary>
        private float kill = 0;

        /// <summary> True, if the user is attempting to turn right (either 'Steer_Right' or 'MaxTurn_Right' is detected) </summary>
        private float hands_air = 0;

        private float headphones=0;

        private float jarbas_init = 0;

        private float play_pause = 0;

        private float swipe_left = 0;

        private float swipe_right = 0;
        //MORE GESTURES 
        /// <summary> True, if the user is holding the wheel, but not turning it (Closed hands detected) </summary>
       // private bool keepStraight = false;

        /// <summary> Current progress value reported by the continuous 'SteerProgress' gesture </summary>
        //private float steerProgress = 0.0f;

        /// <summary> True, if the body is currently being tracked </summary>
        private bool isTracked = false;

       
        /// <summary>
        /// Initializes a new instance of the GestureResultView class and sets initial property values
        /// </summary>
        /// <param name="isTracked">True, if the body is currently tracked</param>
        /// <param name="left">True, if the 'Steer_Left' gesture is currently detected</param>
        /// <param name="right">True, if the 'Steer_Right' gesture is currently detected</param>
        /// <param name="straight">True, if the 'SteerStraight' gesture is currently detected</param>
        /// <param name="progress">Progress value of the 'SteerProgress' gesture</param>
        /// <param name="space">SpaceView object in UI which should be updated with latest gesture result data</param>
        public GestureResultView(bool isTracked, float kill, float pray)
        {
            

            this.IsTracked = isTracked;
            this.kill = kill;
            this.hands_air = hands_air;
            this.headphones = headphones;
            this.jarbas_init = jarbas_init;
            this.play_pause = play_pause;
            this.swipe_left = swipe_left;
            this.swipe_right = swipe_right;


    }

        /// <summary> 
        /// Gets a value indicating whether or not the body associated with the gesture detector is currently being tracked 
        /// </summary>
        public bool IsTracked
        {
            get
            {
                return this.isTracked;
            }

            private set
            {
                this.SetProperty(ref this.isTracked, value);
            }
        }

        /// <summary> 
        /// Gets a value indicating whether the user is attempting to turn the ship left 
        /// </summary>
        public float Kill
        {
            get
            {
                return this.kill;
            }

            private set
            {
                this.SetProperty(ref this.kill, value);
            }
        }

        /// <summary> 
        /// Gets a value indicating whether the user is attempting to turn the ship right 
        /// </summary>
        public float Hands_air
        {
            get
            {
                return this.hands_air;
            }

            private set
            {
                this.SetProperty(ref this.hands_air, value);
            }
        }
        public float Swipe_Right
        {
            get
            {
                return this.swipe_right;
            }

            private set
            {
                this.SetProperty(ref this.swipe_right, value);
            }
        }
        public float Swipe_Left
        {
            get
            {
                return this.swipe_left;
            }

            private set
            {
                this.SetProperty(ref this.swipe_left, value);
            }
        }
        public float Play_Pause
        {
            get
            {
                return this.play_pause;
            }

            private set
            {
                this.SetProperty(ref this.play_pause, value);
            }
        }
        public float Jarbas_Init
        {
            get
            {
                return this.jarbas_init;
            }

            private set
            {
                this.SetProperty(ref this.jarbas_init, value);
            }
        }
        public float Headphones
        {
            get
            {
                return this.headphones;
            }

            private set
            {
                this.SetProperty(ref this.headphones, value);
            }
        }
            

        /// <summary> 
        /// Gets a value indicating whether the user is trying to keep the ship straight
        /// </summary>
       /* public bool KeepStraight
        {
            get
            {
                return this.keepStraight;
            }

            private set
            {
                this.SetProperty(ref this.keepStraight, value);
            }
        }*/

        /// <summary> 
        /// Gets a value indicating the progress associated with the 'SteerProgress' gesture for the tracked body 
        /// </summary>
      /*  public float SteerProgress
        {
            get
            {
                return this.steerProgress;
            }

            private set
            {
                this.SetProperty(ref this.steerProgress, value);
            }
        }*/

        /// <summary>
        /// Updates gesture detection result values for display in the UI
        /// </summary>
        /// <param name="isBodyTrackingIdValid">True, if the body associated with the GestureResultView object is still being tracked</param>
        /// <param name="left">True, if detection results indicate that the user is attempting to turn the ship left</param>
        /// <param name="right">True, if detection results indicate that the user is attempting to turn the ship right</param>
        /// <param name="straight">True, if detection results indicate that the user is attempting to keep the ship straight</param>
        /// <param name="progress">The current progress value of the 'SteerProgress' continuous gesture</param>
        public void UpdateGestureResult(bool isBodyTrackingIdValid, float kill, float headphones, float hands_air, float swipe_right, float swipe_left, float play_pause, float jarbas_init)
        {
            this.IsTracked = isBodyTrackingIdValid;

            if (!this.isTracked)
            {
                this.kill = 0;
                this.swipe_left = 0;
                this.swipe_right = 0;
                this.jarbas_init = 0;
                this.headphones = 0;
                this.hands_air = 0;
                this.play_pause = 0;
            }
            else
            {
                this.kill = kill;
                this.swipe_left = swipe_left;
                this.swipe_right = swipe_right;
                this.jarbas_init = jarbas_init;
                this.headphones = headphones;
                this.hands_air = hands_air;
                this.play_pause = play_pause;

            }


        }
    }
}
