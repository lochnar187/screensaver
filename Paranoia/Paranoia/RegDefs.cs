using Microsoft.Win32;
using System;
using System.Windows;

namespace Paranoia {
    class RegDefs {
        private const string NOT_THERE_MSG = "NothingIsHereAndNothingIsEverything";
        private const string RK_STILL_ONLY = "StillOnly";
        private const string RK_RETRO_LOOK = "RetroLook";
        private const string RK_RND_MOVEMT = "RandomMoves";
        private const string RK_AW_TALKING = "AllowTalking";
        private const string RK_DEFAULT_VC = "UseDefaultVoice";
        private const string RK_TALK_RATES = "Talkativeness";
        private const string RK_SCALE_EYES = "ScaleEye";
        private const string RK_BAS_ROOT_K = "Software";
        private const string RK_APP_ROOT_K = "Paranoia\\Settings";
        private const string RK_FUL_ROOT_K = RK_BAS_ROOT_K+"\\"+RK_APP_ROOT_K;

        public void InitRegistry() {
            Boolean boolInitNeeded = false;
            String strTestIt = "";
            RegistryKey rkCheck = Registry.CurrentUser.OpenSubKey(RK_FUL_ROOT_K, true);

            if (rkCheck != null) {
                // If any new RegKeys are added make one of them the test.  In that way, reinstalls 
                // add all keys again...WRG
                strTestIt = (String)rkCheck.GetValue(RK_STILL_ONLY, NOT_THERE_MSG);
                boolInitNeeded = (strTestIt.CompareTo(NOT_THERE_MSG) == 0);
            } else {
                rkCheck = Registry.CurrentUser.OpenSubKey(RK_BAS_ROOT_K, true);
                rkCheck = rkCheck.CreateSubKey(RK_APP_ROOT_K);
                boolInitNeeded = true;
            } // End if (rkCheck != null)

            // If the first one isn't there, add all of them.  Yes, that overwrites everything making 
            // this action a factory default reset...WRG
            if (boolInitNeeded) {
                // Set default values...WRG
                rkCheck.SetValue(RK_STILL_ONLY, false);
                rkCheck.SetValue(RK_RETRO_LOOK, true);
                rkCheck.SetValue(RK_RND_MOVEMT, false);
                rkCheck.SetValue(RK_AW_TALKING, true);
                rkCheck.SetValue(RK_DEFAULT_VC, true);
                rkCheck.SetValue(RK_TALK_RATES, 90);
                rkCheck.SetValue(RK_SCALE_EYES, 100);

                rkCheck.Flush();
            } // End if (boolInitNeeded)

            rkCheck.Close();
        } // End private void InitRegistry()

        public void DeleteRegistry() {
            RegistryKey rkRoot = Registry.CurrentUser.OpenSubKey(RK_BAS_ROOT_K, true);

            try {
                rkRoot.DeleteSubKey(RK_APP_ROOT_K);
            } catch (Exception e) {
                MessageBox.Show("DeleteRegistry Error: " + e.Message);
            } // End try

        } // End public void DeleteRegistry()

        public Boolean LoadSettings(out Boolean boolStillOnly, out Boolean boolRetroLook, out Boolean boolRandomMoves, out Boolean boolAllowTalking, out Boolean boolUseDefaultVoice, out int intTalkativeness, out double dblScaleEye) {
            Boolean boolOk = false;

            // I have to do this because of Microsoft bullshit, it should not be needed...WRG
            boolStillOnly = false;
            boolRetroLook = false;
            boolRandomMoves = false;
            boolAllowTalking = false;
            boolUseDefaultVoice = false;
            intTalkativeness = 1;
            dblScaleEye = 100;

            try {
                RegistryKey rkLoadFrom = Registry.CurrentUser.OpenSubKey(RK_FUL_ROOT_K, false);

                boolStillOnly = Convert.ToBoolean(rkLoadFrom.GetValue(RK_STILL_ONLY));
                boolRetroLook = Convert.ToBoolean(rkLoadFrom.GetValue(RK_RETRO_LOOK));
                boolRandomMoves = Convert.ToBoolean(rkLoadFrom.GetValue(RK_RND_MOVEMT));
                boolAllowTalking = Convert.ToBoolean(rkLoadFrom.GetValue(RK_AW_TALKING));
                boolUseDefaultVoice = Convert.ToBoolean(rkLoadFrom.GetValue(RK_DEFAULT_VC));
                intTalkativeness = Convert.ToInt16(rkLoadFrom.GetValue(RK_TALK_RATES));
                dblScaleEye = Convert.ToDouble(rkLoadFrom.GetValue(RK_SCALE_EYES));

                rkLoadFrom.Close();

                boolOk = true;

            } catch (Exception e) {
                MessageBox.Show("LoadSettings Error: " + e.Message);
                //Console.WriteLine(e.Message);
            } // End try

            return boolOk;
        } // End public Boolean LoadSettings(out Boolean boolStillOnly, out Boolean boolRetroLook, out Boolean boolRandomMoves, out Boolean boolAllowTalking, out Boolean boolUseDefaultVoice, out int intTalkativeness, out double dblScaleEye)

        public Boolean SaveSettings(Boolean boolStillOnly, Boolean boolRetroLook, Boolean boolRandomMoves, Boolean boolAllowTalking, Boolean boolUseDefaultVoice, int intTalkativeness, double dblScaleEye) {
            Boolean boolOk = false;
            try {
                RegistryKey rkSaveTo = Registry.CurrentUser.OpenSubKey(RK_FUL_ROOT_K, true);

                rkSaveTo.SetValue(RK_STILL_ONLY, boolStillOnly);
                rkSaveTo.SetValue(RK_RETRO_LOOK, boolRetroLook);
                rkSaveTo.SetValue(RK_RND_MOVEMT, boolRandomMoves);
                rkSaveTo.SetValue(RK_AW_TALKING, boolAllowTalking);
                rkSaveTo.SetValue(RK_DEFAULT_VC, boolUseDefaultVoice);
                rkSaveTo.SetValue(RK_TALK_RATES, intTalkativeness);
                rkSaveTo.SetValue(RK_SCALE_EYES, dblScaleEye);

                rkSaveTo.Flush();
                rkSaveTo.Close();

                boolOk = true;
            } catch (Exception e) {
                MessageBox.Show("SaveSettings Error: " + e.Message);
                //Console.WriteLine(e.Message);
            } // End try

            return boolOk;
        } // End public Boolean SaveSettings(Boolean boolStillOnly, Boolean boolRetroLook, Boolean boolRandomMoves, Boolean boolAllowTalking, Boolean boolUseDefaultVoice, int intTalkativeness, double dblScaleEye)

    } // End class RegDefs
} // End namespace Paranoia
