using Microsoft.Win32;
using System;
using System.Windows;

namespace Paranoia {
    class RegDefs {
        private const string notThereMessage = "NothingIsHereAndNothingIsEverything";

        public void InitRegistry() {
            Boolean boolInitNeeded = false;
            String strTestIt = "";
            RegistryKey rkCheck = Registry.CurrentUser.OpenSubKey("Software\\Paranoia\\Settings", true);

            if (rkCheck != null) {
                strTestIt = (String)rkCheck.GetValue("RetroLook", notThereMessage);
                boolInitNeeded = (strTestIt.CompareTo(notThereMessage) == 0);
            } else {
                rkCheck = Registry.CurrentUser.OpenSubKey("Software", true);
                rkCheck = rkCheck.CreateSubKey("Paranoia\\Settings");
                boolInitNeeded = true;
            } // End if (rkCheck != null)

            // If the first one isn't there, add all of them...WRG
            if (boolInitNeeded) {
                // Set default values...WRG
                rkCheck.SetValue("RetroLook", true);
                rkCheck.SetValue("RandomMoves", false);
                rkCheck.SetValue("AllowTalking", true);
                rkCheck.SetValue("UseDefaultVoice", true);
                rkCheck.SetValue("Talkativeness", 90);

                rkCheck.Flush();
            } // End if (strTestIt.CompareTo(notThereMessage) != 0)

            rkCheck.Close();
        } // End private void InitRegistry()

        public Boolean LoadSettings(out Boolean boolRetroLook, out Boolean boolRandomMoves, out Boolean boolAllowTalking, out Boolean boolUseDefaultVoice, out int intTalkativeness) {
            Boolean boolOk = false;

            // I have to do this because of Microsoft bullshit, it should not be needed...WRG
            boolRetroLook = false;
            boolRandomMoves = false;
            boolAllowTalking = false;
            boolUseDefaultVoice = false;
            intTalkativeness = 1;

            try {
                RegistryKey rkLoadFrom = Registry.CurrentUser.OpenSubKey("Software\\Paranoia\\Settings", false);

                boolRetroLook = Convert.ToBoolean(rkLoadFrom.GetValue("RetroLook"));
                boolRandomMoves = Convert.ToBoolean(rkLoadFrom.GetValue("RandomMoves"));
                boolAllowTalking = Convert.ToBoolean(rkLoadFrom.GetValue("AllowTalking"));
                boolUseDefaultVoice = Convert.ToBoolean(rkLoadFrom.GetValue("UseDefaultVoice"));
                intTalkativeness = Convert.ToInt16(rkLoadFrom.GetValue("Talkativeness"));

                rkLoadFrom.Close();

                boolOk = true;

            } catch (Exception e) {
                MessageBox.Show(e.Message);
                //Console.WriteLine(e.Message);
            } // End try

            return boolOk;
        } // End public Boolean LoadSettings(out Boolean boolRetroLook, out Boolean boolRandomMoves, out Boolean boolAllowTalking, out Boolean boolUseDefaultVoice, out int intTalkativeness)

        public Boolean SaveSettings(Boolean boolRetroLook, Boolean boolRandomMoves, Boolean boolAllowTalking, Boolean boolUseDefaultVoice, int intTalkativeness) {
            Boolean boolOk = false;
            try {
                RegistryKey rkSaveTo = Registry.CurrentUser.OpenSubKey("Software\\Paranoia\\Settings", true);

                rkSaveTo.SetValue("RetroLook", boolRetroLook);
                rkSaveTo.SetValue("RandomMoves", boolRandomMoves);
                rkSaveTo.SetValue("AllowTalking", boolAllowTalking);
                rkSaveTo.SetValue("UseDefaultVoice", boolUseDefaultVoice);
                rkSaveTo.SetValue("Talkativeness", intTalkativeness);

                rkSaveTo.Flush();
                rkSaveTo.Close();

                boolOk = true;
            } catch (Exception e) {
                MessageBox.Show(e.Message);
                //Console.WriteLine(e.Message);
            } // End try

            return boolOk;
        } // End public Boolean SaveSettings(Boolean boolRetroLook, Boolean boolRandomMoves, Boolean boolAllowTalking, Boolean boolUseDefaultVoice, int intTalkativeness)

    } // End class RegDefs
} // End namespace Paranoia
