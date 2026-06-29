using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ETABSv1;

namespace CSiNET8PluginExample1
{
    public partial class Form1 : Form
    {
        private cSapModel _sapModel;
        private cPluginCallback _pluginCallback;

        // ── Creators / helpers (lazily instantiated after _sapModel is set) ───
        private LoadPatternCreator    _patCreator;
        private LoadCaseCreator       _caseCreator;
        private LoadAssigner          _assigner;
        private LoadCombinationCreator _comboCreator;

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
        }

        public void SetSapModel(ref cSapModel sapModel, ref cPluginCallback callback)
        {
            _sapModel      = sapModel;
            _pluginCallback = callback;

            _patCreator   = new LoadPatternCreator(_sapModel);
            _caseCreator  = new LoadCaseCreator(_sapModel);
            _assigner     = new LoadAssigner(_sapModel);
            _comboCreator = new LoadCombinationCreator(_sapModel);



            Log("Plugin connected to ETABS model.");
            Log($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        // ── Config tab: zone changed → update R hint ─────────────────────────
        private void OnZoneChanged()
        {
            // Log seismic summary preview in the log tab
            try
            {
                var cfg = BuildConfig();
                Log($"Zone updated → {SeismicHelper.GetSummary(cfg)}");
            }
            catch { }
        }

        private void OnSysChanged()
        {
            // Auto-fill the R text box when system changes
            double r = cbSys.SelectedIndex switch
            {
                0 => 3.0,   // RC OMRF
                1 => 5.0,   // RC SMRF
                2 => 4.0,   // RC SW + OMRF
                3 => 5.0,   // RC SW + SMRF
                4 => 3.0,   // Steel OMRF
                5 => 5.0,   // Steel SMRF
                6 => 4.0,   // Steel CBF
                7 => 1.5,   // URM
                _ => 3.0
            };
            txtR.Text = r.ToString("F1");
        }

        private void OnOccChanged()
        {
            double[] llLookup = { 2.0, 4.0, 4.0, 4.0, 5.0, 7.5, 12.0, 4.0, 4.0 };
            if (cbOcc.SelectedIndex < llLookup.Length)
                txtLL.Text = llLookup[cbOcc.SelectedIndex].ToString("F1");
        }

        // ── Build BuildingConfig from form values ─────────────────────────────
        private BuildingConfig BuildConfig()
        {
            // Parse R factor
            if (!double.TryParse(txtR.Text, out double rVal)) rVal = 5.0;
            if (!double.TryParse(txtDamp.Text, out double dampPct)) dampPct = 5.0;
            if (!int.TryParse(txtModes.Text, out int nModes)) nModes = 12;
            if (!double.TryParse(txtLL.Text, out double ll)) ll = 4.0;
            if (!double.TryParse(txtSDL.Text, out double sdl)) sdl = 1.5;
            if (!double.TryParse(txtRoofLL.Text, out double roofLL)) roofLL = 1.5;
            if (!double.TryParse(txtCladding.Text, out double clad)) clad = 8.0;
            if (!double.TryParse(txtParapet.Text, out double par)) par = 2.0;

            SeismicZone zone = cbZone.SelectedIndex switch
            {
                0 => SeismicZone.II, 1 => SeismicZone.III,
                2 => SeismicZone.IV, 3 => SeismicZone.V,
                _ => SeismicZone.III
            };
            SiteClass soil = cbSoil.SelectedIndex switch
            {
                0 => SiteClass.Type_I_Hard, 1 => SiteClass.Type_II_Medium,
                2 => SiteClass.Type_III_Soft, _ => SiteClass.Type_II_Medium
            };
            ImportanceFactor imp = cbImp.SelectedIndex switch
            {
                0 => ImportanceFactor.Cat_III_Normal, 1 => ImportanceFactor.Cat_II_Important,
                2 => ImportanceFactor.Cat_I_Critical, _ => ImportanceFactor.Cat_III_Normal
            };
            StructuralSystem sys = cbSys.SelectedIndex switch
            {
                0 => StructuralSystem.RC_OMRF,           1 => StructuralSystem.RC_SMRF,
                2 => StructuralSystem.RC_ShearWall_OMRF, 3 => StructuralSystem.RC_ShearWall_SMRF,
                4 => StructuralSystem.Steel_OMRF,        5 => StructuralSystem.Steel_SMRF,
                6 => StructuralSystem.Steel_CBF,         7 => StructuralSystem.UnreinforcedMasonry,
                _ => StructuralSystem.RC_SMRF
            };

            return new BuildingConfig
            {
                Zone           = zone,
                SoilType       = soil,
                Importance     = imp,
                StructSystem   = sys,
                R              = rVal,
                DampingRatio   = dampPct / 100.0,
                NumberOfModes  = Math.Max(6, nModes),
                LiveLoad       = ll,
                SDL            = sdl,
                RoofLiveLoad   = roofLL,
                CladdingLoad_kNm = clad,
                ParapetLoad_kNm  = par,
                // Pattern names from the bottom row of the Config tab
                PatternDead    = string.IsNullOrWhiteSpace(txtPDead.Text) ? "DEAD" : txtPDead.Text.Trim(),
                PatternSDL     = string.IsNullOrWhiteSpace(txtPSDL.Text)  ? "SDL"  : txtPSDL.Text.Trim(),
                PatternLive    = string.IsNullOrWhiteSpace(txtPLive.Text) ? "LIVE" : txtPLive.Text.Trim(),
                PatternEQX     = string.IsNullOrWhiteSpace(txtPEQX.Text)  ? "EQX"  : txtPEQX.Text.Trim(),
                PatternEQY     = string.IsNullOrWhiteSpace(txtPEQY.Text)  ? "EQY"  : txtPEQY.Text.Trim(),
                CaseEQX        = string.IsNullOrWhiteSpace(txtPEQX.Text)  ? "EQX"  : txtPEQX.Text.Trim(),
                CaseEQY        = string.IsNullOrWhiteSpace(txtPEQY.Text)  ? "EQY"  : txtPEQY.Text.Trim(),
            };
        }

        // ── Load Setup steps ──────────────────────────────────────────────────
        private void RunStep1()
        {
            if (_sapModel == null) { LogError("Not connected to ETABS"); return; }
            mainTabs.SelectedTab = tabLoads;
            try
            {
                var cfg = BuildConfig();
                Log("\n" + _patCreator.CreateAllPatterns(cfg));
            }
            catch (Exception ex) { LogError($"Step 1 failed: {ex.Message}"); }
        }

        private void RunStep2()
        {
            if (_sapModel == null) { LogError("Not connected to ETABS"); return; }
            mainTabs.SelectedTab = tabLoads;
            try
            {
                var cfg = BuildConfig();
                Log("\n" + _caseCreator.CreateAllCases(cfg));
            }
            catch (Exception ex) { LogError($"Step 2 failed: {ex.Message}"); }
        }

        private void RunStep3()
        {
            if (_sapModel == null) { LogError("Not connected to ETABS"); return; }
            mainTabs.SelectedTab = tabLoads;
            try
            {
                var cfg = BuildConfig();
                Log("\n" + _assigner.AssignAllLoads(cfg));
            }
            catch (Exception ex) { LogError($"Step 3 failed: {ex.Message}"); }
        }

        private void RunStep4()
        {
            if (_sapModel == null) { LogError("Not connected to ETABS"); return; }
            mainTabs.SelectedTab = tabLoads;
            try
            {
                var cfg = BuildConfig();
                Log("\n" + _comboCreator.CreateAllCombinations(cfg));
            }
            catch (Exception ex) { LogError($"Step 4 failed: {ex.Message}"); }
        }

        private void RunAllSteps()
        {
            if (_sapModel == null) { LogError("Not connected to ETABS"); return; }
            mainTabs.SelectedTab = tabLoads;
            Log("\n══════════════════════════════════════════");
            Log($"  IS 875 + IS 1893:2016 Load Automation");
            Log($"  {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Log("══════════════════════════════════════════");
            try
            {
                var cfg = BuildConfig();
                Log("\n" + _patCreator.CreateAllPatterns(cfg));
                Log("\n" + _caseCreator.CreateAllCases(cfg));
                Log("\n" + _assigner.AssignAllLoads(cfg));
                Log("\n" + _comboCreator.CreateAllCombinations(cfg));
                Log("\n✔  All steps complete. Refresh ETABS view and verify.");
                Log("   IMPORTANT: Set Mass Source (Define > Mass Source) for seismic weight.");
                Log("   Apply IS 875 Part 3 wind pressures to WLX/WLY cases manually.");
            }
            catch (Exception ex)
            {
                LogError($"Run failed: {ex.Message}\n{ex.StackTrace}");
            }
        }



        // ── Logging helpers ───────────────────────────────────────────────────
        private void Log(string msg)
        {
            if (rtbLog.InvokeRequired)
                rtbLog.Invoke(new Action(() => Log(msg)));
            else
            {
                rtbLog.AppendText(msg + "\r\n");
                rtbLog.ScrollToCaret();
            }
        }

        private void LogError(string msg)
        {
            if (rtbLog.InvokeRequired)
                rtbLog.Invoke(new Action(() => LogError(msg)));
            else
            {
                int start = rtbLog.TextLength;
                rtbLog.AppendText("  ✘  " + msg + "\r\n");
                rtbLog.Select(start, msg.Length + 6);
                rtbLog.SelectionColor = Color.FromArgb(255, 100, 100);
                rtbLog.SelectionLength = 0;
                rtbLog.ScrollToCaret();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { _pluginCallback?.Finish(0); }
            catch { }
        }
    }
}
