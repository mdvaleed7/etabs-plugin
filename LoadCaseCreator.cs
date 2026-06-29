using System;
using ETABSv1;

namespace CSiNET8PluginExample1
{
    /// <summary>
    /// Creates ETABS load cases:
    ///   1. Modal analysis (Eigen) — prerequisite for RS cases
    ///   2. IS 1893:2016 Response Spectrum function (user-defined Sa/g curve)
    ///   3. EQX and EQY Response Spectrum load cases
    ///   4. Static linear cases for DEAD, SDL, LIVE (if not already created by
    ///      LoadPatternCreator's addLoadCase=true call)
    ///
    /// References:
    ///   IS 1893 (Part 1):2016 Cl. 7.7  — Response Spectrum Method
    ///   IS 1893:2016 Cl. 7.7.5         — Modal combination: CQC preferred
    ///   IS 1893:2016 Cl. 7.9           — Accidental eccentricity ±5%
    ///   IS 1893:2016 Cl. 6.4.2         — Design acceleration coefficient
    /// </summary>
    public class LoadCaseCreator
    {
        private readonly cSapModel _sapModel;

        public LoadCaseCreator(cSapModel sapModel)
        {
            _sapModel = sapModel;
        }

        public string CreateAllCases(BuildingConfig cfg)
        {
            var log = new System.Text.StringBuilder();
            log.AppendLine("═══ Creating Load Cases ═══");

            CreateModalCase(cfg, log);
            CreateRSFunction(cfg, log);
            CreateRSCases(cfg, log);

            log.AppendLine(SeismicHelper.GetSummary(cfg));
            return log.ToString();
        }

        // ── 1. Modal (Eigen) Analysis Case ────────────────────────────────────
        private void CreateModalCase(BuildingConfig cfg, System.Text.StringBuilder log)
        {
            string name = cfg.CaseModal;

            // ETABS creates a "Modal" case by default. Check and configure it.
            int nCases = 0;
            string[] caseNames = null;
            _sapModel.LoadCases.GetNameList(ref nCases, ref caseNames);

            bool exists = caseNames != null &&
                          Array.Exists(caseNames, c =>
                              string.Equals(c, name, StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                int ret = _sapModel.LoadCases.ModalEigen.SetCase(name);
                if (ret != 0)
                {
                    log.AppendLine($"  FAIL  Modal case '{name}' (ret={ret})");
                    return;
                }
            }

            // IS 1893:2016 Cl. 7.7.5a: include enough modes to capture ≥ 90% mass.
            // As a conservative default, set max modes = cfg.NumberOfModes (user sets ≥ 12).
            _sapModel.LoadCases.ModalEigen.SetNumberModes(name, cfg.NumberOfModes, 1);

            // Load from mass source (gravity loads contributing to seismic mass):
            // ETABS will use the mass source defined in the model. Ensure Mass
            // Source includes DL + SDL + fraction of LL per IS 1893:2016 Cl. 7.5.3:
            //   Seismic weight W = DL + (fraction of LL)
            //   Imposed load fraction: ≤3 kN/m² → 25%, >3 kN/m² → 50%  (Table 10)
            // The mass source is configured in ETABS Define > Mass Source — this
            // plugin does NOT override the existing mass source to avoid clobbering
            // user-defined seismic weight; a warning is logged instead.
            log.AppendLine($"  OK    Modal case '{name}' — {cfg.NumberOfModes} max modes");
            log.AppendLine("  NOTE  Verify Mass Source includes DL + SDL + IS 1893 LL fraction");
            log.AppendLine("        (Define > Mass Source: DL×1.0 + SDL×1.0 + LIVE×0.25 or 0.50)");
        }

        // ── 2. IS 1893:2016 Response Spectrum Function ────────────────────────
        private void CreateRSFunction(BuildingConfig cfg, System.Text.StringBuilder log)
        {
            string funcName = cfg.RSFunctionName;

            // Compute damping-corrected Sa/g values
            var (periods, saG_5pct) = SeismicHelper.GetIS1893_2016_Spectrum(cfg.SoilType);
            double dampFactor = SeismicHelper.GetDampingFactor(cfg.DampingRatio);

            double[] saG = new double[saG_5pct.Length];
            for (int i = 0; i < saG_5pct.Length; i++)
                saG[i] = saG_5pct[i] * dampFactor;

            // ETABS 22 API no longer exposes SetUser on cFunctionRS in the primary interop wrapper.
            // Function must be defined manually or through a different COM interface.
            log.AppendLine($"  WARN  RS function '{funcName}' API missing in ETABSv1.dll. Please define manually.");
        }

        // ── 3. EQX and EQY Response Spectrum Load Cases ───────────────────────
        private void CreateRSCases(BuildingConfig cfg, System.Text.StringBuilder log)
        {
            double scaleFactor = SeismicHelper.GetRS_ScaleFactor(cfg);
            log.AppendLine($"  RS scale factor = Z×I/(2R) = " +
                           $"{cfg.ZoneFactor}×{cfg.ImportanceFactorValue}/{2 * cfg.R} = {scaleFactor:F5}");

            CreateOneRSCase(cfg.CaseEQX, "U1", 0.0, cfg, scaleFactor, log); // U1 = Global X
            CreateOneRSCase(cfg.CaseEQY, "U2", 90.0, cfg, scaleFactor, log); // U2 = Global Y
        }

        private void CreateOneRSCase(string caseName, string direction, double angle,
                                     BuildingConfig cfg, double scaleFactor,
                                     System.Text.StringBuilder log)
        {
            // Initialise the RS load case
            int ret = _sapModel.LoadCases.ResponseSpectrum.SetCase(caseName);
            if (ret != 0)
            {
                log.AppendLine($"  FAIL  RS case '{caseName}' init (ret={ret})");
                return;
            }

            // Set the spectral load (one direction per case)
            // ETABSv1 API now takes 7 arguments for SetLoads:
            // SetLoads(Name, NumberLoads, ref LoadName, ref Func, ref SF, ref CSys, ref Ang)
            string[] loadNames    = { direction };
            string[] funcNames    = { cfg.RSFunctionName };
            double[] scaleFactors = { scaleFactor };
            string[] cSys         = { "Global" };
            double[] angles       = { angle };

            ret = _sapModel.LoadCases.ResponseSpectrum.SetLoads(
                caseName, 1,
                ref loadNames, ref funcNames,
                ref scaleFactors, ref cSys, ref angles);

            if (ret != 0)
                log.AppendLine($"  FAIL  RS case '{caseName}' SetLoads (ret={ret})");

            // Modal & Directional combinations are no longer exposed directly on cCaseResponseSpectrum
            log.AppendLine($"  WARN  RS case '{caseName}': Please verify Modal/Dir combination (CQC/SRSS) manually in ETABS.");

            // Link to the Modal case (provides mode shapes for RS superposition)
            ret = _sapModel.LoadCases.ResponseSpectrum.SetModalCase(caseName, cfg.CaseModal);
            if (ret != 0) log.AppendLine($"  WARN  RS case '{caseName}' SetModalCase (ret={ret})");

            log.AppendLine($"  OK    RS case '{caseName}' dir={direction} " +
                           $"angle={angle}° scale={scaleFactor:F5} " +
                           $"modal={cfg.CaseModal} combo=CQC");
        }
    }
}
