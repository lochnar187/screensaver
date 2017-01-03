using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Paranoia {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window {
        private Boolean boolAllowTalking = Properties.Settings.Default.AllowTalking;
        private Boolean boolUseDefaultVoice = Properties.Settings.Default.UseDefaultVoice;
        private Boolean boolRandomMoves = Properties.Settings.Default.RandomMoves;
        private Boolean boolRetroLook = Properties.Settings.Default.RetroLook;
        private Boolean boolAnimationInProgress = false;
        private int intAnimationControl = 0;
        private int intTalkativeness = Properties.Settings.Default.Talkativeness;
        private int intCenterX = 0;
        private int intCenterY = 0;
        private double dblScreenWidth;
        private double dblScreenHeight;
        private SpeechSynthesizer ssTheComputer = new SpeechSynthesizer();
        private DispatcherTimer dtPuppetMaster = new DispatcherTimer();
        private Stopwatch swTimeSinceLastSpeech = new Stopwatch();

        public MainWindow() {
            InitializeComponent();
        } // End public MainWindow()

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            double dblImageTop, dblImageLeft;

            dblScreenWidth = this.MainGrid.RenderSize.Width;
            dblScreenHeight = this.MainGrid.RenderSize.Height;

            // Look at this more, I don't like it...WRG
            dblImageTop = (dblScreenHeight / 2) - 672; // -172 + 1024 -172 = 680
            dblImageLeft = (dblScreenWidth / 2) - 672; // 128 + 1024 + 128 = 1280

            // Center the eye images on the screen...WRG
            Canvas.SetLeft(iris, dblImageLeft);
            Canvas.SetTop(iris, dblImageTop);
            Canvas.SetLeft(pupil, dblImageLeft);
            Canvas.SetTop(pupil, dblImageTop);
            Canvas.SetLeft(shine, dblImageLeft);
            Canvas.SetTop(shine, dblImageTop);

            // Start retro animation...WRG
            animateScanlines();

            // No mouse pointer for screen savers...WRG
            Mouse.OverrideCursor = Cursors.None;

            // Start the animation control clock...WRG
            dtPuppetMaster.Tick += dispatcherTimer_Tick;
            dtPuppetMaster.Interval = new TimeSpan(0, 0, 1);
            dtPuppetMaster.Start();

        } // End private void Window_Loaded(object sender, RoutedEventArgs e)

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            killIt();
        } // End private void Window_MouseDown(object sender, MouseButtonEventArgs e)

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            killIt();
        } // End private void Window_KeyDown(object sender, KeyEventArgs e)

        private void Window_TouchDown(object sender, TouchEventArgs e) {
            killIt();
        } // End private void Window_TouchDown(object sender, TouchEventArgs e)

        private void Window_StylusDown(object sender, StylusDownEventArgs e) {
            killIt();
        } // End private void Window_StylusDown(object sender, StylusDownEventArgs e)

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            // On each tick run a little animation if no other animation is running...WRG
            if (!boolAnimationInProgress) {
                if (boolRandomMoves) {
                    RandomMoves();
                } else {
                    ScriptedMoves();
                } // End if (boolRandomMoves)
            } // End  if (!boolAnimationInProgress)
        } // End private void dispatcherTimer_Tick(object sender, EventArgs e)

        private void movementAnimation_Completed(object sender, EventArgs e) {
            boolAnimationInProgress = false;
            //Console.WriteLine("movementAnimation_Completed, next is case #" + intAnimationControl.ToString());
        } // End private void myanim_Completed(object sender, EventArgs e)

        private void RandomMoves() {
            // Move the center point of the eye to randomish locations...WRG
            Random ranGenerator = new Random();
            int intRangeMinY = (int) ((dblScreenHeight / 2) / 10) + 50, // Keep the eye near the bottom of the screen for that iconic Paranoia look...WRG
                intRangeMaxY = (int) ((((dblScreenHeight / 2) - dblScreenHeight) + 50) * (-1)), 
                intNewCenterY = 0,
                intRangeMinX = (int) ((dblScreenWidth / 2) - dblScreenWidth) + 50,
                intRangeMaxX = (int) (dblScreenWidth / 2) + 50,
                intNewCenterX = 0,
                intNextAnimationType = ranGenerator.Next(1, 21); // Upper bound is exclusive so, this gives an int between 1 and 20...WRG

            switch (intNextAnimationType) {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    // Do nothing, pause.  Add random saying here...WRG
                    saySomethingButNotTooMuch();
                    intAnimationControl = -1;
                    break;
                case 7:
                case 8:
                    // Move only on the Y axis...WRG
                    intNewCenterY = ranGenerator.Next(intRangeMinY, intRangeMaxY);
                    animateMoveTo(0, (intCenterY * -1) + intNewCenterY, 100 * (intNextAnimationType-6));
                    break;
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                    // Move only on the X axis...WRG
                    intNewCenterX = ranGenerator.Next(intRangeMinX, intRangeMaxX);
                    animateMoveTo((intCenterX * -1) + intNewCenterX, 0, (int)((deltaUnits(intCenterX, intNewCenterX) * intNextAnimationType) / 3));
                    break;
                case 15:
                case 16:
                case 17:
                    // Move on both the X and Y axis slowly...WRG
                    intNewCenterX = ranGenerator.Next(intRangeMinX, intRangeMaxX);
                    intNewCenterY = ranGenerator.Next(intRangeMinY, intRangeMaxY);
                    animateMoveTo((intCenterX * -1) + intNewCenterX, (intCenterY * -1) + intNewCenterY, (int)((deltaUnits(intCenterX, intNewCenterX) * intNextAnimationType) / 3));
                    break;
                case 18:
                case 19:
                    // Move on both the X and Y axis quickly...WRG
                    intNewCenterX = ranGenerator.Next(intRangeMinX, intRangeMaxX);
                    intNewCenterY = ranGenerator.Next(intRangeMinY, intRangeMaxY);
                    animateMoveTo((intCenterX * -1) + intNewCenterX, (intCenterY * -1) + intNewCenterY, (int)(deltaUnits(intCenterX, intNewCenterX) / 7));
                    break;
                default:
                    // Center and greet...WRG
                    animateMoveTo((intCenterX * -1), (intCenterY * -1), 100);
                    saySomethingButNotTooMuch(1);
                    animateDilate(0.9);
                    break;
            } // End switch (intAnimationControl)
            intAnimationControl++;
            boolAnimationInProgress = (intAnimationControl > 0);

        } // End private void RandomMoves()

        private void ScriptedMoves() {
            switch (intAnimationControl) {
                case 0:
                    saySomethingButNotTooMuch(1);
                    animateDilate(0.9);
                    break;
                case 1:
                    animateMoveTo(-450, 300, 1000);
                    break;
                case 2:
                    animateMoveTo(900, 0, 3500);
                    break;
                case 3:
                    saySomethingButNotTooMuch(2);
                    animateMoveTo(-450, 0, 2500);
                    break;
                case 4:
                    animateMoveTo(-150, 0, 1250);
                    break;
                case 5:
                    saySomethingButNotTooMuch(3);
                    animateMoveTo(-200, -100, 2250);
                    break;
                case 6:
                    animateMoveTo(350, -200, 1250);
                    break;
                default:
                    intAnimationControl = -1;
                    break;
            } // End switch (intAnimationControl)
            intAnimationControl++;
            boolAnimationInProgress = (intAnimationControl > 0);
        } // End private void ScriptedMoves()

        private int deltaUnits(int intFrom, int intTo) {
            int intValue = 0;

            if (intFrom >= intTo) {
                intValue = intFrom - intTo;
            } else {
                intValue = intTo - intFrom;
            } // End if (intFrom >= intTo)

            return intValue;
        } // End private int deltaUnits(int intFrom, int intTo)

        private void saySomethingButNotTooMuch(int intIndex = 0) {
            // Don't get too talkative, limit it by time...WRG
            if ((swTimeSinceLastSpeech.IsRunning && (swTimeSinceLastSpeech.ElapsedMilliseconds > (intTalkativeness * 1000))) || (! swTimeSinceLastSpeech.IsRunning)) {
                if (intIndex == 0) {
                    Random ranGenerator = new Random();
                    intIndex = ranGenerator.Next(1, 12);
                } // End if (intIndex == 0)

                switch (intIndex) {
                    case 1:
                        sayThis("Hello citizen!");
                        break;
                    case 2:
                        sayThis("Happiness is mandatory.");
                        break;
                    case 3:
                        sayThis("Are you happy?");
                        break;
                    case 4:
                        sayThis("At your service.");
                        break;
                    case 5:
                        sayThis("Remain calm citizen.");
                        break;
                    case 6:
                        sayThis("Thank you for your cooperation.");
                        break;
                    case 7:
                        sayThis("Just say no, to secret societies.");
                        break;
                    case 8:
                        sayThis("Report to your local security officer.");
                        break;
                    case 9:
                        sayThis("Report all trators.  Have a nice day.");
                        break;
                    case 10:
                        sayThis("Your infraction has been noted.");
                        break;
                    default:
                        sayThis("Please disregard this message.");
                        break;
                } // End switch (intIndex)

            } // End if ((swTimeSinceLastSpeech.IsRunning && (swTimeSinceLastSpeech.ElapsedMilliseconds > (intTalkativeness * 1000))) || (! swTimeSinceLastSpeech.IsRunning))
        } // End private void saySomethingButNotTooMuch(int intIndex = 0)

        private void sayThis(String strThis) {
            if (boolAllowTalking) {
                try {
                    swTimeSinceLastSpeech.Restart();
                    ssTheComputer.SpeakAsync(strThis);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            } // End if (boolAllowTalking)
        } // End private void sayThis(String strThis)

        private void killIt() {
            // Kill the app but first, kill any SpeechSynthesizer process...WRG

            if (ssTheComputer.State != SynthesizerState.Ready) {
                ssTheComputer.SpeakAsyncCancelAll();
                // Pause to make sure SpeechSynthesizer is killed off...WRG
                Thread.Sleep(1000); 
            } // End if (ssTheComputer.State != SynthesizerState.Ready)

            ssTheComputer.Dispose();
            Application.Current.Shutdown();
        } // End private void killIt()

        private void animateScanlines() {
            // Scanline effect for the retro CRT look...WRG
            if (boolRetroLook) {
                Storyboard sbMoves = new Storyboard();

                TranslateTransform trans = new TranslateTransform(100,100);
                scanlines.RenderTransformOrigin = new Point(0, 0);
                scanlines.RenderTransform = trans;

                DoubleAnimation daYAxis = new DoubleAnimation();
                daYAxis.Duration = new Duration(TimeSpan.FromMilliseconds(10));
                daYAxis.To = 5;
                daYAxis.From = 0;
                daYAxis.AutoReverse = true;
                daYAxis.RepeatBehavior = RepeatBehavior.Forever;

                sbMoves.Children.Add(daYAxis);

                Storyboard.SetTargetProperty(daYAxis, new PropertyPath("RenderTransform.Y"));
                Storyboard.SetTarget(daYAxis, scanlines);

                sbMoves.Begin();
            } else {
                scanlines.Visibility = Visibility.Hidden;
            } // End if (boolRetroLook)
        } // End private void animateScanlines()

        private void animateDilate(double dblFactor) {
            // Dilate the pupil by dblFactor.  I suggest dblFactor be between 0.8 and 1.2 for best results...WRG
            Storyboard sbMoves = new Storyboard();

            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            pupil.RenderTransformOrigin = new Point(0.5, 0.5);
            pupil.RenderTransform = scale;

            DoubleAnimationUsingKeyFrames dakfScaleX = new DoubleAnimationUsingKeyFrames();
            dakfScaleX.Duration = new Duration(TimeSpan.FromMilliseconds(5000));
            dakfScaleX.KeyFrames.Add(new LinearDoubleKeyFrame(dblFactor, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
            dakfScaleX.KeyFrames.Add(new LinearDoubleKeyFrame(dblFactor, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(4500))));
            dakfScaleX.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(5000))));
            dakfScaleX.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Children.Add(dakfScaleX);

            Storyboard.SetTargetProperty(dakfScaleX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(dakfScaleX, pupil);

            DoubleAnimationUsingKeyFrames dakfScaleY = new DoubleAnimationUsingKeyFrames();
            dakfScaleY.Duration = new Duration(TimeSpan.FromMilliseconds(5000));
            dakfScaleY.KeyFrames.Add(new LinearDoubleKeyFrame(dblFactor, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
            dakfScaleY.KeyFrames.Add(new LinearDoubleKeyFrame(dblFactor, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(4500))));
            dakfScaleY.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(5000))));
            dakfScaleY.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Children.Add(dakfScaleY);

            Storyboard.SetTargetProperty(dakfScaleY, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(dakfScaleY, pupil);

            sbMoves.Begin();
        } // End private void animateDilate(double dblFactor)

        private void animateMoveTo(int intPosX, int intPosY, int intDuration) {
            // Move the eye to intPosX, intPosY location in intDuration milliseconds...WRG
            Storyboard sbMoves = new Storyboard();

            TranslateTransform trans = new TranslateTransform();
            eye.RenderTransformOrigin = new Point(0, 0);
            eye.RenderTransform = trans;

            DoubleAnimation daXAxis = new DoubleAnimation();
            daXAxis.Duration = new Duration(TimeSpan.FromMilliseconds(intDuration));
            daXAxis.To = intCenterX + intPosX;
            daXAxis.From = intCenterX;
            daXAxis.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Children.Add(daXAxis);

            Storyboard.SetTargetProperty(daXAxis, new PropertyPath("RenderTransform.X"));
            Storyboard.SetTarget(daXAxis, eye);

            DoubleAnimation daYAxis = new DoubleAnimation();
            daYAxis.Duration = new Duration(TimeSpan.FromMilliseconds(intDuration));
            daYAxis.To = intCenterY + intPosY;
            daYAxis.From = intCenterY;
            daYAxis.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Children.Add(daYAxis);

            Storyboard.SetTargetProperty(daYAxis, new PropertyPath("RenderTransform.Y"));
            Storyboard.SetTarget(daYAxis, eye);

            sbMoves.Begin();
            intCenterX += intPosX;
            intCenterY += intPosY;

        } // End private void animateMoveTo(int intPosX, int intPosY, int intDuration)

        /*************************************************************************************************
            Old Testing Functions follow, not for use...WRG
         *************************************************************************************************/

        //ssTheComputer.SpeakAsync("Hello citizen!  Your friend the Computer is here.  Happiness is mandatory.  Are you happy?");
        /*

        foreach (var voice in ssTheComputer.GetInstalledVoices()) {
            Console.WriteLine(voice.VoiceInfo.Description);
        }

        String strSSML =    "<?xml version=\"1.0\"?>" +
                                    "< speak version = \"1.0\" " +
                                    "        xmlns = \"http://www.w3.org/2001/10/synthesis\"" +
                                    "        xmlns:xsi = \"http://www.w3.org/2001/XMLSchema-instance\"" +
                                    "        xsi:schemaLocation = \"http://www.w3.org/2001/10/synthesis" +
                                    "           http://www.w3.org/TR/speech-synthesis/synthesis.xsd\"" +
                                    "        xml:lang = \"en-US\" >" +
                                    "             <voice gender = \"male\" > Mary had a little lamb,</ voice >" +
                                    "                  < !--now request a different female child's voice -->" +
                                    "                  <voice gender = \"female\" variant = \"2\" >" +
                                    "                     Its fleece was white as snow." +
                                    "                  </ voice >" +
                                    "                     < !--processor - specific voice selection -->" +
                                    "                       < voice name = \"Mike\" > I want to be like Mike.</ voice >" +
                                    "</ speak >";
        String s = "<speak version=\"1.0\" ";
        s += "xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-US\">";
        s += "Hello citizen!  Your friend the Computer is here.  Happiness is mandatory.  Are you happy?";

        //s += "<prosody contour=\"(0 %, +20Hz)(10 %, +30 %)(40 %, +10Hz)\">good morning</ prosody > ";

        //s += "<s>";
        //s += "Your order for <prosody pitch = \" + 1st\" rate = \"-10%\" volume = \"90\"> 8 books and 1 reading lamp </prosody>";
        //s += "will be shipped tomorrow.";
        //s += "</s>";

        s += "<mark name=\"anything\"/>";
        s += "<mark name=\"END\"/>";
        s += "</speak>";
        */

        /*
        DoubleAnimation daScroll = new DoubleAnimation();
        TransformGroup tgMoveEye = new TransformGroup();
        TranslateTransform ttStartFrom = new TranslateTransform();

        tgMoveEye.Children.Add(ttStartFrom);

        eye.RenderTransform = tgMoveEye;

        daScroll.To = 200;
        daScroll.From = 0;
        daScroll.Duration = new Duration(TimeSpan.FromSeconds(2));
        daScroll.AutoReverse = true;
        daScroll.Completed += new EventHandler(movementAnimation_Completed);

        ttStartFrom.BeginAnimation(TranslateTransform.XProperty, daScroll);
        daScroll.To = -100;
        ttStartFrom.BeginAnimation(TranslateTransform.YProperty, daScroll);
        */
        /*
        private void animationSet00() {
            Storyboard sbMoves = new Storyboard();

            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            pupil.RenderTransformOrigin = new Point(0.5, 0.5);
            pupil.RenderTransform = scale;

            DoubleAnimation daScaleX = new DoubleAnimation();
            daScaleX.Duration = TimeSpan.FromMilliseconds(250);
            daScaleX.From = 1;
            daScaleX.To = 1.05;
            daScaleX.AutoReverse = true;
            daScaleX.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Children.Add(daScaleX);

            Storyboard.SetTargetProperty(daScaleX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(daScaleX, pupil);

            DoubleAnimation daScaleY = new DoubleAnimation();
            daScaleY.Duration = TimeSpan.FromMilliseconds(250);
            daScaleY.From = 1;
            daScaleY.To = 1.05;
            daScaleY.AutoReverse = true;
            daScaleY.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Children.Add(daScaleY);

            Storyboard.SetTargetProperty(daScaleY, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(daScaleY, pupil);

            sbMoves.Begin();
        } // End private void animationSet00()

        private void animationSet01() {
            Storyboard sbMoves = new Storyboard();

            TranslateTransform trans = new TranslateTransform();
            eye.RenderTransformOrigin = new Point(0, 0);
            eye.RenderTransform = trans;

            DoubleAnimationUsingKeyFrames dakfXAxis = new DoubleAnimationUsingKeyFrames();
            dakfXAxis.Duration = new Duration(TimeSpan.FromSeconds(10));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(-100, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(-400, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(5))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(450, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(10))));
            dakfXAxis.AutoReverse = true;
            dakfXAxis.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Children.Add(dakfXAxis);

            Storyboard.SetTargetProperty(dakfXAxis, new PropertyPath("RenderTransform.X"));
            Storyboard.SetTarget(dakfXAxis, eye);

            DoubleAnimationUsingKeyFrames dakfYAxis = new DoubleAnimationUsingKeyFrames();
            dakfYAxis.Duration = new Duration(TimeSpan.FromSeconds(10));
            dakfYAxis.KeyFrames.Add(new LinearDoubleKeyFrame(100, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))));
            dakfYAxis.KeyFrames.Add(new LinearDoubleKeyFrame(200, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(5))));
            dakfYAxis.KeyFrames.Add(new LinearDoubleKeyFrame(225, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(10))));
            dakfYAxis.AutoReverse = true;
            dakfYAxis.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Children.Add(dakfYAxis);

            Storyboard.SetTargetProperty(dakfYAxis, new PropertyPath("RenderTransform.Y"));
            Storyboard.SetTarget(dakfYAxis, eye);

            sbMoves.Begin();

        } // End private void animationSet01()

        private void animationSet02() {
            Storyboard sbMoves = new Storyboard();

            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            pupil.RenderTransformOrigin = new Point(0.5, 0.5);
            pupil.RenderTransform = scale;

            DoubleAnimation daScaleX = new DoubleAnimation();
            daScaleX.Duration = TimeSpan.FromMilliseconds(250);
            daScaleX.From = 1;
            daScaleX.To = 0.95;
            daScaleX.AutoReverse = true;
            daScaleX.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Children.Add(daScaleX);

            Storyboard.SetTargetProperty(daScaleX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(daScaleX, pupil);

            DoubleAnimation daScaleY = new DoubleAnimation();
            daScaleY.Duration = TimeSpan.FromMilliseconds(250);
            daScaleY.From = 1;
            daScaleY.To = 0.95;
            daScaleY.AutoReverse = true;
            daScaleY.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Children.Add(daScaleY);

            Storyboard.SetTargetProperty(daScaleY, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(daScaleY, pupil);

            sbMoves.Begin();
        } // End private void animationSet02()

        private void animationSet03() {
            Storyboard sbMoves = new Storyboard();

            TranslateTransform trans = new TranslateTransform();
            eye.RenderTransformOrigin = new Point(0, 0);
            eye.RenderTransform = trans;

            DoubleAnimationUsingKeyFrames dakfXAxis = new DoubleAnimationUsingKeyFrames();
            dakfXAxis.Duration = new Duration(TimeSpan.FromSeconds(10));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(100, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(400, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(5))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(-450, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(10))));
            dakfXAxis.AutoReverse = true;
            dakfXAxis.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Children.Add(dakfXAxis);

            Storyboard.SetTargetProperty(dakfXAxis, new PropertyPath("RenderTransform.X"));
            Storyboard.SetTarget(dakfXAxis, eye);

            DoubleAnimationUsingKeyFrames dakfYAxis = new DoubleAnimationUsingKeyFrames();
            dakfYAxis.Duration = new Duration(TimeSpan.FromSeconds(10));
            dakfYAxis.KeyFrames.Add(new LinearDoubleKeyFrame(100, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))));
            dakfYAxis.KeyFrames.Add(new LinearDoubleKeyFrame(200, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(5))));
            dakfYAxis.KeyFrames.Add(new LinearDoubleKeyFrame(-225, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(10))));
            dakfYAxis.AutoReverse = true;
            dakfYAxis.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Children.Add(dakfYAxis);

            Storyboard.SetTargetProperty(dakfYAxis, new PropertyPath("RenderTransform.Y"));
            Storyboard.SetTarget(dakfYAxis, eye);

            sbMoves.Begin();
        } // End private void animationSet03()

        private void animationSet04() {
            // Doesn't work...WRG
            TransformGroup tgMoveEye = new TransformGroup();
            DoubleAnimation daScroll = new DoubleAnimation();
            TranslateTransform ttStartFrom = new TranslateTransform(0, 0);
            ScaleTransform stStartFrom = new ScaleTransform(0.75, 0.75, 10, 10);
            DoubleAnimationUsingKeyFrames dakfXAxis = new DoubleAnimationUsingKeyFrames();
            DoubleAnimationUsingKeyFrames dakfYAxis = new DoubleAnimationUsingKeyFrames();
            DoubleKeyFrameCollection dkfcKeyFrames = new DoubleKeyFrameCollection();
            Storyboard sbMoves = new Storyboard();

            tgMoveEye.Children.Add(ttStartFrom);
            //tgMoveEye.Children.Add(stStartFrom);

            dkfcKeyFrames.Add(new LinearDoubleKeyFrame(100, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))));
            dkfcKeyFrames.Add(new LinearDoubleKeyFrame(200, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4))));
            dkfcKeyFrames.Add(new LinearDoubleKeyFrame(300, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(6))));
            dkfcKeyFrames.Add(new LinearDoubleKeyFrame(400, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(8))));
            dkfcKeyFrames.Add(new LinearDoubleKeyFrame(450, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(10))));
            dakfXAxis.Duration = new Duration(TimeSpan.FromSeconds(10));
            dakfXAxis.AutoReverse = true;
            Storyboard.SetTargetName(dakfXAxis, eye.Name);
            Storyboard.SetTargetProperty(dakfXAxis, new PropertyPath(TranslateTransform.XProperty));
            dakfXAxis.KeyFrames = dkfcKeyFrames;
            sbMoves.Children.Add(dakfXAxis);

            dkfcKeyFrames.Clear();

            dkfcKeyFrames.Add(new LinearDoubleKeyFrame(100, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(5))));
            dkfcKeyFrames.Add(new LinearDoubleKeyFrame(200, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(7))));
            dkfcKeyFrames.Add(new LinearDoubleKeyFrame(225, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(10))));
            dakfYAxis.Duration = new Duration(TimeSpan.FromSeconds(10));
            dakfYAxis.AutoReverse = true;
            Storyboard.SetTargetName(dakfXAxis, eye.Name);
            Storyboard.SetTargetProperty(dakfYAxis, new PropertyPath(TranslateTransform.YProperty));
            dakfYAxis.KeyFrames = dkfcKeyFrames;
            sbMoves.Children.Add(dakfYAxis);

            sbMoves.Completed += new EventHandler(movementAnimation_Completed);

            eye.BeginStoryboard(sbMoves);
        } // End private void animationSet04()

        private void animationSet05() {
            // Doesn't work...WRG
            DoubleAnimationUsingKeyFrames dakfXAxis = new DoubleAnimationUsingKeyFrames();
            DoubleAnimationUsingKeyFrames dakfYAxis = new DoubleAnimationUsingKeyFrames();
            TransformGroup tgMoveEye = new TransformGroup();
            TranslateTransform ttStartFrom = new TranslateTransform();

            tgMoveEye.Children.Add(ttStartFrom);

            eye.RenderTransform = tgMoveEye;

            dakfXAxis.Duration = new Duration(TimeSpan.FromSeconds(10));
            dakfXAxis.AutoReverse = true;
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(100, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(200, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(300, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(6))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(400, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(8))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(450, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(10))));

            dakfYAxis.Duration = new Duration(TimeSpan.FromSeconds(10));
            dakfYAxis.AutoReverse = true;
            dakfYAxis.KeyFrames.Add(new LinearDoubleKeyFrame(100, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(5))));
            dakfYAxis.KeyFrames.Add(new LinearDoubleKeyFrame(200, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(7))));
            dakfYAxis.KeyFrames.Add(new LinearDoubleKeyFrame(225, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(10))));

            dakfYAxis.Completed += new EventHandler(movementAnimation_Completed);

            //ttStartFrom.BeginAnimation(TranslateTransform.XProperty, dakfXAxis);
            //ttStartFrom.BeginAnimation(TranslateTransform.YProperty, dakfYAxis);
            eye.BeginAnimation(TranslateTransform.XProperty, dakfYAxis);
            eye.BeginAnimation(TranslateTransform.YProperty, dakfYAxis);
        } // End private void animationSet05()
        */

    } // End public partial class MainWindow 

} // End namespace Paranoia