using System;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows;

namespace Paranoia {
    /// <summary>
    /// Interaction logic for ConfigIt.xaml
    /// </summary>
    public partial class ConfigIt : Window {
        public Boolean boolAllowTalking;
        public Boolean boolUseDefaultVoice;
        public Boolean boolRandomMoves;
        public Boolean boolRetroLook;
        public Boolean boolStillOnly;
        public int intTalkativeness;
        private double dblScaleEye;

        private SpeechSynthesizer ssTheComputer = new SpeechSynthesizer();

        public ConfigIt() {
            InitializeComponent();
        } // End public ConfigIt()

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            RegDefs rdSource = new RegDefs();

            if (rdSource.LoadSettings(out boolStillOnly, out boolRetroLook, out boolRandomMoves, out boolAllowTalking, out boolUseDefaultVoice, out intTalkativeness, out dblScaleEye)) {
                sdFrequency.Value = intTalkativeness;
                sdScaleEye.Value = dblScaleEye;
                cbRetro.IsChecked = boolRetroLook;
                cbTalk.IsChecked = boolAllowTalking;

                if (boolStillOnly) {
                    rbStill.IsChecked = true;
                } else {
                    if (boolRandomMoves) {
                        rbRandom.IsChecked = true;
                    } else {
                        rbScripted.IsChecked = true;
                    } // End if (boolRandomMoves)
                } // End if (boolRandomMoves)

                if (boolAllowTalking) {
                    sayThis("What is your security clearance citizen?");
                } else {
                    MessageBox.Show("Indicate your clearance level.");
                } // End if (boolAllowTalking)

            } // End if (rdSource.LoadSettings(out boolRetroLook, out boolRandomMoves, out boolAllowTalking, out boolUseDefaultVoice, out intTalkativeness))
        } // End private void Window_Loaded(object sender, RoutedEventArgs e) 

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            RegDefs rdSource = new RegDefs();

            // Save settings then kill the app but first, kill any SpeechSynthesizer process...WRG
            if (!rdSource.SaveSettings(rbStill.IsChecked.Value, cbRetro.IsChecked.Value, rbRandom.IsChecked.Value, cbTalk.IsChecked.Value, cbDefault.IsChecked.Value, (int)sdFrequency.Value, (double)sdScaleEye.Value)) {
                MessageBox.Show("Failed to save the settings.");
                e.Cancel = true;
            } else {
                if (ssTheComputer.State != SynthesizerState.Ready) {
                    ssTheComputer.SpeakAsyncCancelAll();
                    // Pause to make sure SpeechSynthesizer is killed off...WRG
                    Thread.Sleep(1000);
                } // End if (ssTheComputer.State != SynthesizerState.Ready)

                ssTheComputer.Dispose();
            } // End if (!rdSource.SaveSettings(cbRetro.IsChecked.Value, rbRandom.IsChecked.Value, cbTalk.IsChecked.Value, cbDefault.IsChecked.Value, (int)sdFrequency.Value))

        } // End private void Window_Unloaded(object sender, RoutedEventArgs e)

        private void btnUltraviolet_Click(object sender, RoutedEventArgs e) {
            if (boolAllowTalking) {
                sayThis("Ultraviolet clearance, at your service.");
            } // End if (boolAllowTalking)
            spConfig.Visibility = Visibility.Visible;
        } // End private void btnUltraviolet_Click(object sender, RoutedEventArgs e) 

        private void btnPurple_Click(object sender, RoutedEventArgs e) {
            if (boolAllowTalking) {
                sayThis("Violet clearance.  Well, that is odd.  Wait here a moment.");
            } // End if (boolAllowTalking)
            MessageBox.Show("Report to PLC.  You have been issued a new personal grooming bot, the Mark XV Tankbot.  Oh, never mind, it is homing in on your location in 3...2...");
        } // End private void btnPurple_Click(object sender, RoutedEventArgs e) 

        private void btnIndigo_Click(object sender, RoutedEventArgs e) {
            if (boolAllowTalking) {
                sayThis("Indigo clearance.  Welcome to your new assignment at Internal Security.");
            } // End if (boolAllowTalking)
            MessageBox.Show("This mission will be very safe and lots of fun for all of you.");
        } // End private void btnIndigo_Click(object sender, RoutedEventArgs e) 

        private void btnBlue_Click(object sender, RoutedEventArgs e) {
            if (boolAllowTalking) {
                sayThis("Blue clearance.  Your infraction has been noted.");
            } // End if (boolAllowTalking)
            MessageBox.Show("Please report for termination immediately.  Report all traitors.  Have a nice day.");
        } // End private void btnBlue_Click(object sender, RoutedEventArgs e) 

        private void btnGreen_Click(object sender, RoutedEventArgs e) {
            if (boolAllowTalking) {
                sayThis("Green clearance.  Thank you citizen.");
            } // End if (boolAllowTalking)
            MessageBox.Show("Thank you for volunteering to test new body armor!  Report to R&D at once.");
        } // End private void btnGreen_Click(object sender, RoutedEventArgs e) 

        private void btnYellow_Click(object sender, RoutedEventArgs e) {
            if (boolAllowTalking) {
                sayThis("Yellow clearance.  Report to your local security officer.");
            } // End if (boolAllowTalking)
            MessageBox.Show("I'm sorry.  That information is not available at your security clearance.");
        } // End private void btnYellow_Click(object sender, RoutedEventArgs e) 

        private void btnOrange_Click(object sender, RoutedEventArgs e) {
            if (boolAllowTalking) {
                sayThis("Orange clearance.  Have a nice day.");
            } // End if (boolAllowTalking)
            MessageBox.Show("Ignorance.  Fear.  Fear.  Ignorance.  Dedicate yourself to these principles.");
        } // End private void btnOrange_Click(object sender, RoutedEventArgs e) 

        private void btnRed_Click(object sender, RoutedEventArgs e) {
            if (boolAllowTalking) {
                sayThis("Red clearance.  Remain calm citizen.");
            } // End if (boolAllowTalking)
            MessageBox.Show("Perhaps your next clone will do better.");
        } // End private void btnRed_Click(object sender, RoutedEventArgs e) 

        private void btnInfrared_Click(object sender, RoutedEventArgs e) {
            if (boolAllowTalking) {
                sayThis("Infrared clearance.  Thank you for your cooperation.");
            } // End if (boolAllowTalking)
            MessageBox.Show("You have been promoted to Troubleshooter status.  Report to Briefing Room, section R-773.2399.334.A732.p997a within 12.317 minutes, for your orders.  Thank you citizen.");
        } // End private void btnInfrared_Click(object sender, RoutedEventArgs e) 

        private void cbTalk_Click(object sender, RoutedEventArgs e) {
            sdFrequency.IsEnabled = cbTalk.IsChecked.Value;
        } // End private void cbTalk_Click(object sender, RoutedEventArgs e)

        private void toggleCustomDisplay(object sender, RoutedEventArgs e) {
            if (rbCustom.IsChecked.Value) {
                txtCustomPath.Visibility = Visibility.Visible;
            } else {
                txtCustomPath.Visibility = Visibility.Hidden;
            } // End if (rbCustom.IsChecked.Value)
        } // End private void toggleCustomDisplay(object sender, RoutedEventArgs e)

        private void sayThis(String strThis) {
            if (boolAllowTalking) {
                try {
                    ssTheComputer.SpeakAsync(strThis);
                } catch (Exception e) {
                    MessageBox.Show("SpeakAsync Error: " + e.Message);
                } // End try
            } // End if (boolAllowTalking)
        } // End private void sayThis(String strThis)

    } // End public partial class ConfigIt
} // End namespace Paranoia
