using ETABSv1;
using System;
using System.Windows.Forms;

namespace CSiNET8PluginExample1
{
    public class cPlugin : cPluginContract
    {
        private static string _version = "2.0";
        private int errorCode = 0;

        public int Info(ref string Text)
        {
            Text =
                "Load Automation Plugin  v" + _version + Environment.NewLine +
                "Developed by: Advatech Structural Engineers" + Environment.NewLine +
                Environment.NewLine +
                "TAB 1 — Building Configuration" + Environment.NewLine +
                "  • Seismic zone, soil type, importance factor I, R factor" + Environment.NewLine +
                "    → IS 1893 (Part 1):2016 Tables 3, 8, 9" + Environment.NewLine +
                "  • Occupancy-based live load, SDL, cladding, parapet" + Environment.NewLine +
                "    → IS 875 Parts 1 & 2" + Environment.NewLine +
                Environment.NewLine +
                "TAB 2 — Load Definition (run in order)" + Environment.NewLine +
                "  Step ①: Create load patterns  (DEAD SW=1, SDL, LIVE, EQX, EQY, WLX, WLY)" + Environment.NewLine +
                "  Step ②: Create load cases     (Modal Eigen + IS 1893:2016 RS function + EQX/EQY RS cases)" + Environment.NewLine +
                "  Step ③: Assign loads          (SDL/LL to slabs; cladding to exterior beams; parapet to roof)" + Environment.NewLine +
                "  Step ④: Create combinations   (IS 875 Part 5 ULS + IS 456 SLS + Envelope)" + Environment.NewLine +
                Environment.NewLine +
                "Wind loads (IS 875 Part 3) are applied manually to the WLX/WLY cases.";
            return 0;
        }

        public void Main(ref cSapModel sapModel, ref cPluginCallback pluginCallback)
        {
            var aForm = new Form1();
            try
            {
                aForm.SetSapModel(ref sapModel, ref pluginCallback);
                aForm.Show(); // Non-modal
            }
            catch (Exception ex)
            {
                errorCode = 1;
                MessageBox.Show("Plugin startup error:\n" + ex.Message);
                try { pluginCallback.Finish(errorCode); } catch { }
            }
        }
    }
}
