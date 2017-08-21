using System;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Paranoia {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window {
        public Boolean boolDebug = false;
        private Boolean boolAllowTalking;
        private Boolean boolUseDefaultVoice;
        private Boolean boolRandomMoves;
        private Boolean boolRetroLook;
        private Boolean boolStillOnly;
        private Boolean boolAnimationInProgress = false;
        private int intOverrideKnt = 0;
        private int intAnimationControl = 0;
        private int intTalkativeness;
        private int intCenterX = 0;
        private int intCenterY = 0;
        private double dblScaleEye;
        private double dblScreenWidth;
        private double dblScreenHeight;
        private double dblScaleFactorWidth;
        private double dblScaleFactorHeight;
        private SpeechSynthesizer ssTheComputer = new SpeechSynthesizer();
        private DispatcherTimer dtPuppetMaster = new DispatcherTimer();
        private Stopwatch swTimeSinceLastSpeech = new Stopwatch();
        private Stopwatch swTimeSinceAnimationStart = new Stopwatch();
        private Random ranGenerator = new Random();

        public MainWindow() {
            InitializeComponent();
        } // End public MainWindow()

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // Load setting, adjust eye display, scale it to local screen resolution, animate scanlines,
            // kill the mouse pointer, and start the puppet master timer...WRG
            RegDefs rdSource = new RegDefs();

            if (rdSource.LoadSettings(out boolStillOnly, out boolRetroLook, out boolRandomMoves, out boolAllowTalking, out boolUseDefaultVoice, out intTalkativeness, out dblScaleEye)) {
                dblScreenWidth = this.MainGrid.RenderSize.Width;
                dblScreenHeight = this.MainGrid.RenderSize.Height;

                // Center the eye images on the screen...WRG
                Canvas.SetLeft(iris, (dblScreenWidth / 2) - (iris.RenderSize.Width / 2));
                Canvas.SetTop(iris, (dblScreenHeight / 2) - (iris.RenderSize.Height / 2));
                Canvas.SetLeft(pupil, (dblScreenWidth / 2) - (pupil.RenderSize.Width / 2));
                Canvas.SetTop(pupil, (dblScreenHeight / 2) - (pupil.RenderSize.Height / 2));
                Canvas.SetLeft(shine, (dblScreenWidth / 2) - (shine.RenderSize.Width / 2));
                Canvas.SetTop(shine, (dblScreenHeight / 2) - (shine.RenderSize.Height / 2));

                // Scale the eye.  Resolution of the concept images are 1024x1024 but they render at 1345x1345
                // on my test box which has a screen resolution of 1600x900.  This gives the effect I want, an
                // eye taking up most of the screen.  Now I need to make that scale...WRG
                dblScaleFactorWidth = (dblScreenWidth / 1600) * (dblScaleEye / 100);
                dblScaleFactorHeight = (dblScreenHeight / 900) * (dblScaleEye / 100);

                if (((dblScaleFactorHeight > 1.01) || (dblScaleFactorHeight < 0.99)) || ((dblScaleFactorWidth > 1.01) || (dblScaleFactorWidth < 0.99))) {
                    // Apply eye scale...WRG
                    Storyboard sbMoves = new Storyboard();

                    ScaleTransform scale = new ScaleTransform(1.0, 1.0);
                    eye.RenderTransformOrigin = new Point(0.5, 0.5);
                    eye.RenderTransform = scale;

                    DoubleAnimation daScaleX = new DoubleAnimation();
                    daScaleX.Duration = new Duration(TimeSpan.FromMilliseconds(1500));
                    daScaleX.From = 1;
                    daScaleX.To = dblScaleFactorHeight;
                    sbMoves.Children.Add(daScaleX);

                    Storyboard.SetTargetProperty(daScaleX, new PropertyPath("RenderTransform.ScaleX"));
                    Storyboard.SetTarget(daScaleX, eye);

                    DoubleAnimation daScaleY = new DoubleAnimation();
                    daScaleY.Duration = new Duration(TimeSpan.FromMilliseconds(1500));
                    daScaleY.From = 1;
                    daScaleY.To = dblScaleFactorHeight;
                    sbMoves.Children.Add(daScaleY);

                    Storyboard.SetTargetProperty(daScaleY, new PropertyPath("RenderTransform.ScaleY"));
                    Storyboard.SetTarget(daScaleY, eye);

                    sbMoves.Completed += new EventHandler(movementAnimation_Completed);
                    sbMoves.Begin();
                    boolAnimationInProgress = true;

                } // End if (((dblScaleFactorHeight > 1.01) || (dblScaleFactorHeight < 0.99)) || ((dblScaleFactorWidth > 1.01) || (dblScaleFactorWidth < 0.99)))

                // Start retro animation...WRG
                animateScanlines();

                // No mouse pointer for screen savers...WRG
                Mouse.OverrideCursor = Cursors.None;

                // Start the animation control clock...WRG
                dtPuppetMaster.Tick += dispatcherTimer_Tick;
                dtPuppetMaster.Interval = new TimeSpan(0, 0, 1);
                dtPuppetMaster.Start();

                StatusDebug();
            } else {
                MessageBox.Show("Failed to load setting.");
                killIt();
            } // End if (rdSource.LoadSettings(out boolRetroLook, out boolRandomMoves, out boolAllowTalking, out boolUseDefaultVoice, out intTalkativeness))

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

        private void msgText_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            // Scroll the text if visible.  For a 130 char message, 12 seconds should be enough time...WRG
            if (msgText.Visibility == Visibility.Visible) {
                Storyboard sbMoves = new Storyboard();

                TranslateTransform trans = new TranslateTransform();
                message.RenderTransformOrigin = new Point(0, 0);
                message.RenderTransform = trans;

                DoubleAnimation daXAxis = new DoubleAnimation();
                daXAxis.Duration = new Duration(TimeSpan.FromSeconds(12));
                daXAxis.To = (dblScreenWidth * -2.75);
                daXAxis.From = dblScreenWidth;
                daXAxis.Completed += new EventHandler(textAnimation_Completed);
                sbMoves.Children.Add(daXAxis);

                Storyboard.SetTargetProperty(daXAxis, new PropertyPath("RenderTransform.X"));
                Storyboard.SetTarget(daXAxis, message);

                sbMoves.Begin();
            } // End if (msgText.Visibility == Visibility.Visible)
        } // End private void msgText_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            // On each tick run a little animation if no other animation is running...WRG
            StatusDebug();

            if (!boolAnimationInProgress) {
                if (boolStillOnly) {
                    MostlyStill();
                } else {
                    if (boolRandomMoves) {
                        RandomMoves();
                    } else {
                        SimpleScriptedMoves();
                    } // End if (boolRandomMoves)
                } // End if (boolStillOnly)
            } else {
                // Sometimes it gets stuck, don't let that happen.  Currently, 
                // the max duration of an animation is found in RandomMoves(), 
                // case 17, adding 5% for a margin.  Don't go beyond that...WRG
                double dblMaxAnimationDuration = (dblScreenWidth * 17 / 3) * 1.05;

                if (swTimeSinceAnimationStart.ElapsedMilliseconds > dblMaxAnimationDuration) {
                    // Reset the in progress flag so we are no longer stuck...WRG
                    boolAnimationInProgress = false;
                    intOverrideKnt++;
                } // End if (swTimeSinceAnimationStart.ElapsedMilliseconds > dblMaxAnimationDuration)

            } // End  if (!boolAnimationInProgress)
        } // End private void dispatcherTimer_Tick(object sender, EventArgs e)

        private void movementAnimation_Completed(object sender, EventArgs e) {
            boolAnimationInProgress = false;
            //Console.WriteLine("movementAnimation_Completed, next is case #" + intAnimationControl.ToString());
        } // End private void myanim_Completed(object sender, EventArgs e)

        private void textAnimation_Completed(object sender, EventArgs e) {
            msgText.Visibility = Visibility.Hidden;
        } // End private void myanim_Completed(object sender, EventArgs e)

        private void StatusDebug() {
            if (this.boolDebug) {
                String strMessage = "(AIP=" + boolAnimationInProgress.ToString() + ") ";
                strMessage += "(SAS=" + swTimeSinceAnimationStart.ElapsedMilliseconds.ToString() + ") ";
                strMessage += "(AOK=" + intOverrideKnt.ToString() + ") ";
                strMessage += "(ACK=" + intAnimationControl.ToString() + ") ";
                strMessage += "(CX=" + intCenterX.ToString() + ") ";
                strMessage += "(CY=" + intCenterY.ToString() + ") ";
                strMessage += "(PME=" + dtPuppetMaster.IsEnabled.ToString() + ") ";
                strMessage += "(SLS=" + swTimeSinceLastSpeech.ElapsedMilliseconds.ToString() + ") ";

                dbgText.Text = strMessage;

                if (dbgText.Visibility == Visibility.Hidden) {
                    dbgText.Visibility = Visibility.Visible;
                } // End if (dbgText.Visibility == Visibility.Hidden)
            } // End if (this.boolDebug)
        } // End private void StatusDebug()

        private void MostlyStill() {
            int intNextAnimationType = ranGenerator.Next(1, 20); // Upper bound is exclusive so, this gives an int between 1 and 50...WRG

            switch (intNextAnimationType) {
                case 1:
                case 2:
                    animateDilate(0.9);
                    break;
                case 3:
                    animateDilate(1.15);
                    break;
                case 4:
                    animateDilate(0.8);
                    saySomethingButNotTooMuch();
                    break;
                case 5:
                    animateBlink();
                    break;
                case 6:
                    ComplexScriptedMoves(1);
                    saySomethingButNotTooMuch();
                    break;
                default:
                    intAnimationControl = -1;
                    break;
            } // End switch (intNextAnimationType)
            intAnimationControl++;
            boolAnimationInProgress = (intAnimationControl > 0);

        } // End private void MostlyStill()

        private void RandomMoves() {
            // Move the center point of the eye to randomish locations...WRG
            int intRangeMinY = (int) ((dblScreenHeight / 2) * 0.1), // Keep the eye near the bottom of the screen for that iconic Paranoia look...WRG
                intRangeMaxY = (int) ((((dblScreenHeight / 2) - dblScreenHeight) * 0.9) * (-1)), 
                intNewCenterY = 0,
                intRangeMinX = (int) (((dblScreenWidth / 2) - dblScreenWidth) * 0.9),
                intRangeMaxX = (int) ((dblScreenWidth / 2) * 0.9),
                intNewCenterX = 0,
                intNextAnimationType = ranGenerator.Next(1, 21); // Upper bound is exclusive so, this gives an int between 1 and 20...WRG

            switch (intNextAnimationType) {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    // Do nothing, pause.  Add random saying here...WRG
                    saySomethingButNotTooMuch();
                    intAnimationControl = -1;
                    break;
                case 6:
                    animateDilate(0.9);
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
                    break;
            } // End switch (intAnimationControl)
            intAnimationControl++;
            boolAnimationInProgress = (intAnimationControl > 0);

        } // End private void RandomMoves()

        private void SimpleScriptedMoves() {
            switch (intAnimationControl) {
                case 0:
                    saySomethingButNotTooMuch(1);
                    animateDilate(0.9);
                    break;
                case 1:
                    saySomethingButNotTooMuch();
                    animateMoveTo(-450, 300, 1000);
                    break;
                case 2:
                    saySomethingButNotTooMuch();
                    animateMoveTo(900, 0, 3500);
                    break;
                case 3:
                    saySomethingButNotTooMuch();
                    animateMoveTo(0, 0, 500);
                    break;
                case 4:
                    saySomethingButNotTooMuch();
                    animateMoveTo(-450, 0, 2500);
                    break;
                case 5:
                    saySomethingButNotTooMuch();
                    animateMoveTo(-150, 0, 1250);
                    break;
                case 6:
                    saySomethingButNotTooMuch();
                    animateMoveTo(0, 0, 500);
                    break;
                case 7:
                    saySomethingButNotTooMuch();
                    animateMoveTo(-200, -100, 2250);
                    break;
                case 8:
                    saySomethingButNotTooMuch();
                    animateMoveTo(350, -200, 1250);
                    break;
                case 9:
                    saySomethingButNotTooMuch();
                    animateDilate(0.9);
                    break;
                case 10:
                    saySomethingButNotTooMuch();
                    animateMoveTo(400, 200, 1000);
                    break;
                case 11:
                    saySomethingButNotTooMuch();
                    animateMoveTo(0, 150, 500);
                    break;
                case 12:
                    saySomethingButNotTooMuch();
                    animateMoveTo(-800, 0, 3500);
                    break;
                case 13:
                    saySomethingButNotTooMuch();
                    animateMoveTo(300, 0, 2500);
                    break;
                case 14:
                    saySomethingButNotTooMuch();
                    animateMoveTo(-300, 0, 1250);
                    break;
                case 15:
                    saySomethingButNotTooMuch();
                    animateMoveTo(-200, 100, 2250);
                    break;
                case 16:
                    saySomethingButNotTooMuch();
                    animateMoveTo(0, -150, 500);
                    break;
                case 17:
                    saySomethingButNotTooMuch();
                    animateMoveTo(200, -150, 1750);
                    break;
                case 18:
                    saySomethingButNotTooMuch();
                    animateMoveTo(400, -150, 2500);
                    break;
                case 19:
                    saySomethingButNotTooMuch();
                    animateMoveTo(-200, 150, 750);
                    break;
                case 20:
                    saySomethingButNotTooMuch();
                    animateMoveTo(200, -150, 2500);
                    break;
                default:
                    saySomethingButNotTooMuch();
                    animateBlink();
                    intAnimationControl = -1;
                    break;
            } // End switch (intAnimationControl)
            intAnimationControl++;
            boolAnimationInProgress = (intAnimationControl > 0);
        } // End private void ScriptedMoves()

        private void ComplexScriptedMoves(int intIndex = 0) {
            // Calls one of a set of predefined complex animation movements, will add more cases later...WRG
            switch (intIndex) {
                case 1:
                    saySomethingButNotTooMuch();
                    animateComplexMoveset01();
                    break;
                default:
                    saySomethingButNotTooMuch();
                    animateBlink();
                    animateDilate(1.4);
                    break;
            } // End switch (intIndex)
        } // End private void ComplexScriptedMoves(int intIndex)

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
                    intIndex = ranGenerator.Next(1, 54);
                } // End if (intIndex == 0)

                // Keep the sayings under 130 chars.  The animation will not look as good with longer text...WRG
                switch (intIndex) {
                    case 1:
                        sayThis("Hello Citizen!");
                        break;
                    case 2:
                        sayThis("Welcome to Alpha Complex.  Be happy.  Be well.  Be loyal.");
                        break;
                    case 3:
                        sayThis("The Computer is your friend.  Do you love the Computer?");
                        break;
                    case 4:
                        sayThis("At your service.  The Computer is your friend.");
                        break;
                    case 5:
                        sayThis("Remain calm Citizen.");
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
                        sayThis("Report all traitors.  Have a nice day.");
                        break;
                    case 10:
                        sayThis("Your infraction has been noted.");
                        break;
                    case 11:
                        sayThis("Happiness is mandatory.  Are you happy?");
                        break;
                    case 12:
                        sayThis("I'm sorry Citizen, you are not cleared for that information.");
                        break;
                    case 13:
                        sayThis("Being a citizen of Alpha Complex is fun.");
                        break;
                    case 14:
                        sayThis("Rooting out traitors will make you happy.  Report all traitors.");
                        break;
                    case 15:
                        sayThis("Report to your Happiness Officer for the new and improved \"Feel Good Now\" pill!");
                        break;
                    case 16:
                        sayThis("Your request has been filed Citizen!  New requests are subject to scan review.  Please bend over.");
                        break;
                    case 17:
                        sayThis("Your mandatory medication allotment has been adjusted.");
                        break;
                    case 18:
                        sayThis("There is no danger.  Dispatching cleaning bots to your location.");
                        break;
                    case 19:
                        sayThis("For your convenience, instructions have been printed in both red on red and white on white.");
                        break;
                    case 20:
                        sayThis("Enjoy another wonderful awake cycle in the safe, peaceful, happy home we call Alpha Complex!");
                        break;
                    case 21:
                        sayThis("Remain calm Citizen.  Love the Computer.  The Computer is your friend.");
                        break;
                    case 22:
                        sayThis("No Citizen, I did not hear screams.  It was nothing.  Move along now.  Love the Computer.");
                        break;
                    case 23:
                        sayThis("There is no danger.  Happiness is mandatory.  Be at peace Citizen.");
                        break;
                    case 24:
                        sayThis("Yes Citizen, the food vats are in perfect working order.  Nothing to see there.  No one is missing.");
                        break;
                    case 25:
                        sayThis("Mutations are a Communist plot.  Report all mutants.  Be happy Citizen!");
                        break;
                    case 26:
                        sayThis("Beware the evil ways of Communism.  Report all traitors.  Have a nice day.");
                        break;
                    case 27:
                        sayThis("Thank you for your mandatory volition.  Report to R&D to test the new Mark 7 body armor, so lite you'd swear it's not there.");
                        break;
                    case 28:
                        sayThis("You have been reassigned, report to REDACTED.  Failure to comply is treason.  Have a wonderful day Citizen.");
                        break;
                    case 29:
                        sayThis("The Computer is your friend.  The Computer wants you to be happy.  Therefore, happiness is mandatory.");
                        break;
                    case 30:
                        sayThis("As a reward for good behavior, oxygen rations in your sector will only be reduced 5%!  Citizen, breathe deep the air of Freedom!");
                        break;
                    case 31:
                        sayThis("Alpha Complex is safe and happy.  Therefore, all citizens are required to be both safe and happy.");
                        break;
                    case 32:
                        sayThis("Traitors have been reported in the area.  Shoot all traitors on sight.  Keep your laser handy.  Trust no one.");
                        break;
                    case 33:
                        sayThis("Citizen, you are still alive!  Well done.");
                        break;
                    case 34:
                        sayThis("The term \"Eureka\" is beyond your security clearance.  Knowledge of this term is treason.  Report for termination.");
                        break;
                    case 35:
                        sayThis("Failure to comply with the following is treason and will result in termination.  01010000 01101100 01100101 01100001");
                        break;
                    case 36:
                        sayThis("Do not be alarmed Citizen.  Being alarmed is treason.");
                        break;
                    case 37:
                        sayThis("Good news Citizen!  You have been selected for a new fun and exciting job!  Report to Reactor Shielding station R-77-234.");
                        break;
                    case 38:
                        sayThis("Error 530: Connection refused, unknown IP7 address.  Thank you Citizen.");
                        break;
                    case 39:
                        sayThis("This is a test.  Thank you for your cooperation.");
                        break;
                    case 40:
                        sayThis("All food vat service requests must now include a TF-63B form.  To requisition TF-63B forms, submit a completed TF-63B form to PLC.");
                        break;
                    case 41:
                        sayThis("Congratulations Citizen!  You have been chosen to test the new model 3 bionic lung.  Report for surgery.");
                        break;
                    case 42:
                        sayThis("The Computer loves all good citizens.  Stay alert!");
                        break;
                    case 43:
                        sayThis("Attention Citizens!  All food vat products labeled H1-34-81-99 have been recalled.  This has nothing to do with radiation.");
                        break;
                    case 44:
                        sayThis("Alpha Complex is running at peak perfection.  Any reports to the contrary are treason.");
                        break;
                    case 45:
                        sayThis("For the next hour, all loyal citizens will dance with glee.  Happiness is mandatory.  Dance well Citizen.");
                        break;
                    case 46:
                        sayThis("Beware of REDACTED!  You could lose a leg or even an eye.  Failure to report REDACTED will result in termination.");
                        break;
                    case 47:
                        sayThis("Commie Mutant Traitor Scum have been found dancing.  Dancing is prohibited to all loyal citizens.  Thank you for your cooperation.");
                        break;
                    case 48:
                        sayThis("There is no problem with the reactor.  No citizen glows-in-the-dark, to do so is treason.  Lights off in 3, 2, 7, 83, 0.");
                        break;
                    case 49:
                        sayThis("That was an interesting response Citizen.  I don't believe I have ever seen such a thing.  This requires study.  Come with me.");
                        break;
                    case 50:
                        sayThis("There is no fire.  Reports of benzene in the sprinklers is false.  No one is trying to add a \"pine fresh scent\" to Alpha Complex.");
                        break;
                    case 51:
                        sayThis("The time is now 14:30 hours.  Correction, the time is now 14:30 hours.  No, wait, it's 14:30 hours.  The time is now 16:15 hours.");
                        break;
                    case 52:
                        sayThis("I've been thinking about you Citizen.");
                        break;
                    case 53:
                        sayThis("The Killbots want to play hide and seek.  You have 10 seconds to hide Citizen!");
                        break;
                    default:
                        sayThis("Please disregard this message.");
                        break;
                } // End switch (intIndex)

            } // End if ((swTimeSinceLastSpeech.IsRunning && (swTimeSinceLastSpeech.ElapsedMilliseconds > (intTalkativeness * 1000))) || (! swTimeSinceLastSpeech.IsRunning))
        } // End private void saySomethingButNotTooMuch(int intIndex = 0)

        private void sayThis(String strThis) {
            try {
                swTimeSinceLastSpeech.Restart();
                if (boolAllowTalking) {
                    try {
                        ssTheComputer.SpeakAsync(strThis);
                    } catch (Exception e) {
                        MessageBox.Show("SpeakAsync Error: " + e.Message);
                    } // End try
                } // End if (boolAllowTalking)
                msgText.Text = strThis;
                msgText.Visibility = Visibility.Visible;
            } catch (Exception e) {
                MessageBox.Show("Speech Error: " + e.Message);
                //Console.WriteLine(e.Message);
            } // End try
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

                TranslateTransform trans = new TranslateTransform(100, 100);
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

        private void animateBlink(double dblTightness = 1.0, double dblSpeed = 1.0, double dblPause = 50) {
            // Blink.  Use dblTightness to contol how close the two lids come to closing. (range between 0.1 and 1.0)  
            //         With dblSpeed the quickness of the blink can be altered (range between 0.5 and 15.0)
            //         And dblPause defines how long the lids are closed, in milliseconds (default 50 ms)...WRG
            Storyboard sbMoves = new Storyboard();

            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            toplid.RenderTransformOrigin = new Point(0.5, 0.5);
            toplid.RenderTransform = scale;
            botlid.RenderTransformOrigin = new Point(0.5, 0.5);
            botlid.RenderTransform = scale;

            DoubleAnimationUsingKeyFrames dakfScaleY = new DoubleAnimationUsingKeyFrames();
            dakfScaleY.Duration = new Duration(TimeSpan.FromMilliseconds((250 * dblSpeed) + dblPause));
            dakfScaleY.KeyFrames.Add(new LinearDoubleKeyFrame(dblScreenHeight * dblTightness, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(125 * dblSpeed))));
            dakfScaleY.KeyFrames.Add(new LinearDoubleKeyFrame(dblScreenHeight * dblTightness, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds((125 * dblSpeed) + dblPause))));
            dakfScaleY.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds((250 * dblSpeed) + dblPause))));
            sbMoves.Children.Add(dakfScaleY);

            Storyboard.SetTargetProperty(dakfScaleY, new PropertyPath("RenderTransform.ScaleY"));

            sbMoves.Begin(toplid);
            sbMoves.Begin(botlid);

            swTimeSinceAnimationStart.Restart();

        } // End private void animateBlink()

        private void animateDilate(double dblFactor = 0.9, double dblDuration = 4000) {
            // Dilate the pupil by dblFactor adding dblDuration to the time the pupil is dilated.  I suggest dblFactor be 
            // between 0.8 and 1.2 for best results.  Add dblDuration to the animation time, it is in milliseconds...WRG
            Storyboard sbMoves = new Storyboard();

            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            pupil.RenderTransformOrigin = new Point(0.5, 0.5);
            pupil.RenderTransform = scale;

            DoubleAnimationUsingKeyFrames dakfScaleX = new DoubleAnimationUsingKeyFrames();
            dakfScaleX.Duration = new Duration(TimeSpan.FromMilliseconds(dblDuration + 1000));
            dakfScaleX.KeyFrames.Add(new LinearDoubleKeyFrame(dblFactor, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
            dakfScaleX.KeyFrames.Add(new LinearDoubleKeyFrame(dblFactor, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(dblDuration + 500))));
            dakfScaleX.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(dblDuration + 1000))));
            sbMoves.Children.Add(dakfScaleX);

            Storyboard.SetTargetProperty(dakfScaleX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(dakfScaleX, pupil);

            DoubleAnimationUsingKeyFrames dakfScaleY = new DoubleAnimationUsingKeyFrames();
            dakfScaleY.Duration = new Duration(TimeSpan.FromMilliseconds(dblDuration + 1000));
            dakfScaleY.KeyFrames.Add(new LinearDoubleKeyFrame(dblFactor, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
            dakfScaleY.KeyFrames.Add(new LinearDoubleKeyFrame(dblFactor, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(dblDuration + 500))));
            dakfScaleY.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(dblDuration + 1000))));
            sbMoves.Children.Add(dakfScaleY);

            Storyboard.SetTargetProperty(dakfScaleY, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(dakfScaleY, pupil);

            sbMoves.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Begin();

            swTimeSinceAnimationStart.Restart();

        } // End private void animateDilate(double dblFactor)

        private void addBasicScale(out Storyboard sbAppendTo) {
            // Start every eye animation with a scale applied from the settings, this way I don't have the same code all 
            // over the place.  Returns the storyboard with a transform group set to the eye's render transformation...WRG
            Storyboard sbMoves = new Storyboard();

            TransformGroup renderGroup = new TransformGroup();
            TranslateTransform trans = new TranslateTransform();
            ScaleTransform scale = new ScaleTransform(1.0, 1.0);

            renderGroup.Children.Add(scale);
            renderGroup.Children.Add(trans);

            eye.RenderTransformOrigin = new Point(0.5, 0.5);
            eye.RenderTransform = renderGroup;

            DoubleAnimation daScaleX = new DoubleAnimation();
            daScaleX.Duration = new Duration(TimeSpan.FromMilliseconds(0));
            daScaleX.From = 1;
            daScaleX.To = dblScaleFactorHeight;
            sbMoves.Children.Add(daScaleX);

            Storyboard.SetTargetProperty(daScaleX, new PropertyPath("RenderTransform.Children[0].ScaleX"));
            Storyboard.SetTarget(daScaleX, eye);

            DoubleAnimation daScaleY = new DoubleAnimation();
            daScaleY.Duration = new Duration(TimeSpan.FromMilliseconds(0));
            daScaleY.From = 1;
            daScaleY.To = dblScaleFactorHeight;
            sbMoves.Children.Add(daScaleY);

            Storyboard.SetTargetProperty(daScaleY, new PropertyPath("RenderTransform.Children[0].ScaleY"));
            Storyboard.SetTarget(daScaleY, eye);

            sbAppendTo = sbMoves; // Pass it back to finish up...WRG
        }

        private void animateMoveTo(int intPosX, int intPosY, int intDuration) {
            // Move the eye to intPosX, intPosY location in intDuration milliseconds...WRG
            Storyboard sbMoves = new Storyboard();

            addBasicScale(out sbMoves);

            DoubleAnimation daXAxis = new DoubleAnimation();
            daXAxis.Duration = new Duration(TimeSpan.FromMilliseconds(intDuration));
            daXAxis.To = intCenterX + intPosX;
            daXAxis.From = intCenterX;
            sbMoves.Children.Add(daXAxis);

            Storyboard.SetTargetProperty(daXAxis, new PropertyPath("RenderTransform.Children[1].X"));
            Storyboard.SetTarget(daXAxis, eye);

            DoubleAnimation daYAxis = new DoubleAnimation();
            daYAxis.Duration = new Duration(TimeSpan.FromMilliseconds(intDuration));
            daYAxis.To = intCenterY + intPosY;
            daYAxis.From = intCenterY;
            sbMoves.Children.Add(daYAxis);

            Storyboard.SetTargetProperty(daYAxis, new PropertyPath("RenderTransform.Children[1].Y"));
            Storyboard.SetTarget(daYAxis, eye);

            sbMoves.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Begin();

            intCenterX += intPosX;
            intCenterY += intPosY;
            swTimeSinceAnimationStart.Restart();

        } // End private void animateMoveTo(int intPosX, int intPosY, int intDuration)

        private void animateComplexMoveset01() {
            // Crazy eye, using this to test ideas for custom path feature...WRG
            Storyboard sbMoves = new Storyboard();

            addBasicScale(out sbMoves);

            DoubleAnimationUsingKeyFrames dakfXAxis = new DoubleAnimationUsingKeyFrames();
            dakfXAxis.Duration = new Duration(TimeSpan.FromMilliseconds(4000));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterX, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2100))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterX - 50, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2200))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterX - 50, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2225))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterX + 100, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2425))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterX + 100, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2450))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterX - 100, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2650))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterX - 100, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2675))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterX + 100, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2875))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterX + 100, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2900))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterX - 100, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(3000))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterX - 100, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(3400))));
            dakfXAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterX, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(3500))));
            sbMoves.Children.Add(dakfXAxis);

            Storyboard.SetTargetProperty(dakfXAxis, new PropertyPath("RenderTransform.Children[1].X"));
            Storyboard.SetTarget(dakfXAxis, eye);

            DoubleAnimationUsingKeyFrames dakfYAxis = new DoubleAnimationUsingKeyFrames();
            dakfYAxis.Duration = new Duration(TimeSpan.FromMilliseconds(4000));
            dakfYAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterY, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2000))));
            dakfYAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterY + 25, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2050))));
            dakfYAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterY + 25, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(3500))));
            dakfYAxis.KeyFrames.Add(new LinearDoubleKeyFrame(intCenterY, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(3750))));
            sbMoves.Children.Add(dakfYAxis);

            Storyboard.SetTargetProperty(dakfYAxis, new PropertyPath("RenderTransform.Children[1].Y"));
            Storyboard.SetTarget(dakfYAxis, eye);

            sbMoves.Completed += new EventHandler(movementAnimation_Completed);
            sbMoves.Begin();

            swTimeSinceAnimationStart.Restart();

        } // End private void animateComplexMoveset01()

    } // End public partial class MainWindow 

} // End namespace Paranoia