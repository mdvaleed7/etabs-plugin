using System;
using System.Collections.Generic;
using ETABSv1;

namespace CSiNET8PluginExample1
{
    /// <summary>
    /// Creates all IS 875 + IS 1893:2016 load patterns in the ETABS model.
    ///
    /// Pattern naming convention used:
    ///   DEAD   — Self-weight (SW multiplier = 1.0)  IS 875 Part 1
    ///   SDL    — Superimposed dead (finishes, partitions, MEP)  IS 875 Part 1
    ///   LIVE   — Floor live load  IS 875 Part 2
    ///   EQX    — Seismic X-direction  IS 1893:2016
    ///   EQY    — Seismic Y-direction  IS 1893:2016
    ///   WLX    — Wind X-direction  IS 875 Part 3
    ///   WLY    — Wind Y-direction  IS 875 Part 3
    ///
    /// The EQX / EQY patterns are created as Quake-type placeholders; the actual
    /// seismic force distribution is handled by the Response Spectrum load case
    /// (created in LoadCaseCreator).  A static Quake pattern is also useful for
    /// manual story force assignment or notional load purposes.
    ///
    /// WLX / WLY patterns are created as Wind-type placeholders; actual wind
    /// pressures per IS 875 Part 3 must be applied separately (wind tunnel or
    /// code-based lateral pressures applied to faces of the structure).
    /// </summary>
    public class LoadPatternCreator
    {
        private readonly cSapModel _sapModel;

        public LoadPatternCreator(cSapModel sapModel)
        {
            _sapModel = sapModel;
        }

        /// <summary>
        /// Creates all required load patterns. Returns a log of actions taken.
        /// Skips any pattern that already exists to avoid overwriting user work.
        /// </summary>
        public string CreateAllPatterns(BuildingConfig cfg)
        {
            var log = new System.Text.StringBuilder();
            log.AppendLine("═══ Creating Load Patterns ═══");

            // ── DEAD: self-weight (SW multiplier = 1.0) ───────────────────────
            // IS 875 Part 1: unit weights of materials. ETABS applies SW × density
            // × element volume automatically when SW multiplier = 1.0.
            AddPattern(cfg.PatternDead, eLoadPatternType.Dead,  1.0, true,  log);

            // ── SDL: superimposed dead (finishes, partitions, services) ────────
            // IS 875 Part 1: typically 1.0–2.5 kN/m² depending on finishes.
            // SW multiplier = 0.0 since ETABS self-weight covers structural members.
            AddPattern(cfg.PatternSDL,  eLoadPatternType.SuperDead, 0.0, true, log);

            // ── LIVE: floor live load (IS 875 Part 2) ─────────────────────────
            AddPattern(cfg.PatternLive, eLoadPatternType.Live,  0.0, true,  log);

            // ── EQX / EQY: seismic (IS 1893:2016) ────────────────────────────
            // These are Quake-type patterns. The RS load case references the RS
            // function, not these patterns directly. Patterns are kept for:
            //   (a) Notional seismic loads (if ESF method is needed)
            //   (b) ETABS auto-lateral load assignment (future extension)
            // addLoadCase = false: the RS case is created explicitly in LoadCaseCreator.
            AddPattern(cfg.PatternEQX, eLoadPatternType.Quake, 0.0, false, log);
            AddPattern(cfg.PatternEQY, eLoadPatternType.Quake, 0.0, false, log);

            // ── WLX / WLY: wind (IS 875 Part 3) ──────────────────────────────
            AddPattern(cfg.PatternWLX, eLoadPatternType.Wind,  0.0, false, log);
            AddPattern(cfg.PatternWLY, eLoadPatternType.Wind,  0.0, false, log);

            log.AppendLine($"Load patterns complete. LL={cfg.LiveLoad} kN/m², SDL={cfg.SDL} kN/m²");
            return log.ToString();
        }

        private void AddPattern(string name, eLoadPatternType type, double swMult,
                                bool addCase, System.Text.StringBuilder log)
        {
            // Check if pattern already exists
            int numPat = 0;
            string[] names = null;
            _sapModel.LoadPatterns.GetNameList(ref numPat, ref names);

            if (names != null && Array.Exists(names, n =>
                string.Equals(n, name, StringComparison.OrdinalIgnoreCase)))
            {
                log.AppendLine($"  SKIP  {name,-12} (already exists)");
                return;
            }

            int ret = _sapModel.LoadPatterns.Add(name, type, swMult, addCase);
            if (ret == 0)
                log.AppendLine($"  OK    {name,-12} Type={type,-12} SW={swMult:F1}" +
                               (addCase ? "  + auto-case" : ""));
            else
                log.AppendLine($"  FAIL  {name,-12} (API return code {ret})");
        }
    }
}
