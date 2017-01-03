using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Windows.Media;
using Extras;
using Application = System.Windows.Application;

/*************************************************************************************************
    Dec 30, 2016

    A Paranoia themed screensaver.  A huge fucking eye moves around the screen and our friend
    The Computer gives words of hope and love to all citizens of Alpha Complex...WRG 

    Supports the standard three screensaver arguments, "/s" "/c" and "/p x".  If none are passed
    it will us "/s" as the default...WRG

 *************************************************************************************************/

namespace Paranoia {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        private HwndSource winWPFContent;

        private void AppStart(object sender, StartupEventArgs e) {
            if (e.Args.Length == 0 || e.Args[0].ToLower().StartsWith("/s")) {
                foreach (Screen currentScreen in Screen.AllScreens) {
                    if (currentScreen != Screen.PrimaryScreen) {
                        BlankIt currentWindow = new BlankIt();
                        currentWindow.Top = currentScreen.WorkingArea.Top;
                        currentWindow.Left = currentScreen.WorkingArea.Left;
                        currentWindow.Width = currentScreen.WorkingArea.Width;
                        currentWindow.Height = currentScreen.WorkingArea.Height;
                        currentWindow.Show();
                    } else {
                        MainWindow theWindow = new MainWindow();
                        theWindow.Top = currentScreen.WorkingArea.Top;
                        theWindow.Left = currentScreen.WorkingArea.Left;
                        theWindow.Width = currentScreen.WorkingArea.Width;
                        theWindow.Height = currentScreen.WorkingArea.Height;
                        theWindow.Show();
                    } // End if (s != Screen.PrimaryScreen)
                } // End foreach (Screen s in Screen.AllScreens)
            } else if (e.Args[0].ToLower().StartsWith("/p")) {
                MainWindow theWindow = new MainWindow();
                Int32 previewHandle = Convert.ToInt32(e.Args[1]);
                IntPtr pPreviewHnd = new IntPtr(previewHandle);
                RECT lpRect = new RECT();
                bool bGetRect = Win32API.GetClientRect(pPreviewHnd, ref lpRect);

                HwndSourceParameters sourceParams = new HwndSourceParameters("sourceParams");

                sourceParams.PositionX = 0;
                sourceParams.PositionY = 0;
                sourceParams.Height = lpRect.Bottom - lpRect.Top;
                sourceParams.Width = lpRect.Right - lpRect.Left;
                sourceParams.ParentWindow = pPreviewHnd;
                sourceParams.WindowStyle = (int)(WindowStyles.WS_VISIBLE | WindowStyles.WS_CHILD | WindowStyles.WS_CLIPCHILDREN);

                winWPFContent = new HwndSource(sourceParams);
                winWPFContent.Disposed += (o, args) => theWindow.Close();
                winWPFContent.RootVisual = theWindow.MainGrid;
            } else {
                ConfigIt configWindow = new ConfigIt();
                configWindow.Show();
            } // End if (e.Args.Length == 0 || e.Args[0].ToLower().StartsWith("/s"))
        } // End AppStart(object sender, StartupEventArgs e)
    } // End public partial class App
} // End namespace Paranoia
