using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Paranoia {
    /// <summary>
    /// Interaction logic for ConfigIt.xaml
    /// </summary>
    public partial class ConfigIt : Window {
        private Boolean boolAllowTalking = Properties.Settings.Default.AllowTalking;
        private Boolean boolUseDefaultVoice = Properties.Settings.Default.UseDefaultVoice;
        private Boolean boolRandomMoves = Properties.Settings.Default.RandomMoves;
        private Boolean boolRetroLook = Properties.Settings.Default.RetroLook;
        private int intTalkativeness = Properties.Settings.Default.Talkativeness;

        private SpeechSynthesizer ssTheComputer = new SpeechSynthesizer();

        public ConfigIt() {
            InitializeComponent();
        } // End public ConfigIt()

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            sdFrequency.Value = intTalkativeness;
            cbRetro.IsChecked = boolRetroLook;
            cbTalk.IsChecked = boolAllowTalking;

            if (boolRandomMoves) {
                rbRandom.IsChecked = true;
            } else {
                rbScripted.IsChecked = true;
            } // End if (boolRandomMoves)

            if (boolAllowTalking) {
                sayThis("What is your security clearance citizen?");
            } else {
                MessageBox.Show("Indicate your clearance level.");
            } // End if (boolAllowTalking)

        } // End private void Window_Loaded(object sender, RoutedEventArgs e) 

        private void Window_Unloaded(object sender, RoutedEventArgs e) {
            // Kill the app but first, kill any SpeechSynthesizer process...WRG

            if (ssTheComputer.State != SynthesizerState.Ready) {
                ssTheComputer.SpeakAsyncCancelAll();
                // Pause to make sure SpeechSynthesizer is killed off...WRG
                Thread.Sleep(1000);
            } // End if (ssTheComputer.State != SynthesizerState.Ready)

            ssTheComputer.Dispose();

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
            MessageBox.Show("Report to PLC.  You have been issued a new personal grooming bot, the Mark XV tankbot.  Oh, nevermind, it is homing in on your location in 3...2...");
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
            MessageBox.Show("Please report for termination immediately.  Report all trators.  Have a nice day.");
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
            MessageBox.Show("You have been promoted to Troubleshooter status.  Report to Briefing Room, section R-773.2399.334.A732.p997 within 12.317 minutes, for your orders.  Thank you citizen.");
        } // End private void btnInfrared_Click(object sender, RoutedEventArgs e) 

        private void sayThis(String strThis) {
            if (boolAllowTalking) {
                try {
                    ssTheComputer.SpeakAsync(strThis);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            } // End if (boolAllowTalking)
        } // End private void sayThis(String strThis)

        private void cbRetro_Checked(object sender, RoutedEventArgs e) {
            boolRetroLook = cbRetro.IsChecked.Value;
            Properties.Settings.Default.RetroLook = boolRetroLook;
            Properties.Settings.Default.Save();
        } // End private void cbRetro_Checked(object sender, RoutedEventArgs e)

        private void rbScripted_Checked(object sender, RoutedEventArgs e) {
            if (rbScripted.IsChecked.Value) {
                boolRandomMoves = false;
                Properties.Settings.Default.RandomMoves = boolRandomMoves;
                Properties.Settings.Default.Save();
            } // End if (rbScripted.IsChecked.Value)
        } // End private void rbScripted_Checked(object sender, RoutedEventArgs e)

        private void rbRandom_Checked(object sender, RoutedEventArgs e) {
            if (rbRandom.IsChecked.Value) {
                boolRandomMoves = true;
                Properties.Settings.Default.RandomMoves = boolRandomMoves;
                Properties.Settings.Default.Save();
            } // End if (rbScripted.IsChecked.Value)
        } // End private void rbRandom_Checked(object sender, RoutedEventArgs e)

        private void cbTalk_Checked(object sender, RoutedEventArgs e) {
            boolAllowTalking = cbTalk.IsChecked.Value;
            Properties.Settings.Default.AllowTalking = boolAllowTalking;
            Properties.Settings.Default.Save();
        } // End private void cbTalk_Checked(object sender, RoutedEventArgs e)

        private void sdFrequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            intTalkativeness = (int) sdFrequency.Value;
            Properties.Settings.Default.Talkativeness = intTalkativeness;
            Properties.Settings.Default.Save();
        } // End private void sdFrequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)

    } // End public partial class ConfigIt
} // End namespace Paranoia
