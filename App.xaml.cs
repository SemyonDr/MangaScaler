using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MangaScaler {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        void App_Startup(Object sender, StartupEventArgs args) {
            //Model.BlurTester_RGB8.TestBatch(".\\Tests\\Blur\\RGB8\\sample_mng_clr.jpg");

            //Model.DotGainTester_RGB8.TestDotGain(".\\Tests\\DotGain\\RGB8\\sample_mng_small.jpg", 50, 50, 0.5);
            //Model.DotGainTester_RGB8.TestBatchStrength(".\\Tests\\DotGain\\RGB8\\sample_mng_small_clr.jpg");
            //Model.DotGainTester_RGB8.TestBatchSpread(".\\Tests\\DotGain\\RGB8\\sample_mng_small.jpg");
            Model.DownscalerTester_RGB8.TestDownscaler(".\\Tests\\Downscale\\RGB8\\sample_mng_blast.jpg", 0.45, true, 50,50);
            Model.DownscalerTester_RGB8.TestDownscaler(".\\Tests\\Downscale\\RGB8\\sample_mng_blast.jpg", 0.45, false, 50, 50);
        }
    }
}
