using System;
using System.Collections.Generic;
using ETABSv1;

namespace CSiNET8PluginExample1
{
    /// <summary>
    /// Creates IS 456:2000 + IS 875 Part 5 Ultimate Limit State (ULS)
    /// and Serviceability Limit State (SLS) load combinations in ETABS.
    ///
    /// Combination table — IS 875 Part 5 / IS 456 Cl. 18.2
    /// ═══════════════════════════════════════════════════════════
    /// GRAVITY (ULS)
    ///   G1:  1.5(DL + SDL + LL)                  — max gravity
    ///   G2:  1.5(DL + SDL)                        — LL absent (uplift check)
    ///
    /// SEISMIC (ULS) — EQX and EQY, both ± directions
    ///   S1:  1.2(DL + SDL + LL + EQX)
    ///   S2:  1.2(DL + SDL + LL − EQX)
    ///   S3:  1.2(DL + SDL + LL + EQY)
    ///   S4:  1.2(DL + SDL + LL − EQY)
    ///   S5:  0.9×DL + 1.5×EQX           — upward seismic / overturning
    ///   S6:  0.9×DL − 1.5×EQX
    ///   S7:  0.9×DL + 1.5×EQY
    ///   S8:  0.9×DL − 1.5×EQY
    ///
    /// WIND (ULS)
    ///   W1:  1.2(DL + SDL + LL + WLX)
    ///   W2:  1.2(DL + SDL + LL − WLX)
    ///   W3:  1.2(DL + SDL + LL + WLY)
    ///   W4:  1.2(DL + SDL + LL − WLY)
    ///   W5:  0.9×DL + 1.5×WLX
    ///   W6:  0.9×DL − 1.5×WLX
    ///   W7:  0.9×DL + 1.5×WLY
    ///   W8:  0.9×DL − 1.5×WLY
    ///
    /// SERVICEABILITY (SLS) — IS 456 Cl. 23.2 (deflection/crack width)
    ///   SLS1: 1.0(DL + SDL + LL)         — total service load (deflection check)
    ///   SLS2: 1.0(DL + SDL)              — permanent load (long-term deflection)
    ///   SLS3: 1.0(DL + SDL + LL + EQX)  — quasi-permanent seismic service
    ///   SLS4: 1.0(DL + SDL + LL + EQY)
    ///
    /// ENVELOPE combinations (for quick design review)
    ///   ENV_ULS: envelope of all ULS combinations (Linear Envelope type in ETABS)
    ///   ENV_SLS: envelope of all SLS combinations
    ///
    /// Note on EQX sign convention in ETABS:
    ///   ETABS Response Spectrum cases produce absolute (unsigned) results.
    ///   To get ±EQ in combinations, set scale factor to +1.0 and −1.0 separately.
    ///   This is handled by using the RS case with ±1.0 factors below.
    ///
    /// For wind: WLX and WLY are static load cases; ±WL uses +1.0 and −1.0 factors.
    /// </summary>
    public class LoadCombinationCreator
    {
        private readonly cSapModel _sapModel;

        public LoadCombinationCreator(cSapModel sapModel)
        {
            _sapModel = sapModel;
        }

        public string CreateAllCombinations(BuildingConfig cfg)
        {
            var log = new System.Text.StringBuilder();
            log.AppendLine("═══ Creating Load Combinations ═══");

            int nBefore = CountCombos();

            // ── ULS: Gravity ─────────────────────────────────────────────────
            AddLinear("IS875_G1_ULS", log, cfg,
                (cfg.PatternDead, 1.5), (cfg.PatternSDL, 1.5), (cfg.PatternLive, 1.5));

            AddLinear("IS875_G2_ULS_NoLL", log, cfg,
                (cfg.PatternDead, 1.5), (cfg.PatternSDL, 1.5));

            // ── ULS: Seismic (IS 1893:2016 Cl. 6.3.4) ───────────────────────
            // Factors: 1.2 for DL+LL+EQ; 0.9 DL for overturning check.
            // IS 1893:2016 does not reduce LL for seismic combination here;
            // IS 875 Part 5 / IS 456 Table 18 use 1.2 on ALL loads including LL.
            AddLinear("IS875_S1_EQX+", log, cfg,
                (cfg.PatternDead, 1.2), (cfg.PatternSDL, 1.2),
                (cfg.PatternLive, 1.2), (cfg.CaseEQX,   +1.2));

            AddLinear("IS875_S2_EQX-", log, cfg,
                (cfg.PatternDead, 1.2), (cfg.PatternSDL, 1.2),
                (cfg.PatternLive, 1.2), (cfg.CaseEQX,   -1.2));

            AddLinear("IS875_S3_EQY+", log, cfg,
                (cfg.PatternDead, 1.2), (cfg.PatternSDL, 1.2),
                (cfg.PatternLive, 1.2), (cfg.CaseEQY,   +1.2));

            AddLinear("IS875_S4_EQY-", log, cfg,
                (cfg.PatternDead, 1.2), (cfg.PatternSDL, 1.2),
                (cfg.PatternLive, 1.2), (cfg.CaseEQY,   -1.2));

            // Overturning / uplift combinations (0.9 DL ± 1.5 EQ)
            AddLinear("IS875_S5_09DL+EQX", log, cfg,
                (cfg.PatternDead, 0.9), (cfg.CaseEQX, +1.5));
            AddLinear("IS875_S6_09DL-EQX", log, cfg,
                (cfg.PatternDead, 0.9), (cfg.CaseEQX, -1.5));
            AddLinear("IS875_S7_09DL+EQY", log, cfg,
                (cfg.PatternDead, 0.9), (cfg.CaseEQY, +1.5));
            AddLinear("IS875_S8_09DL-EQY", log, cfg,
                (cfg.PatternDead, 0.9), (cfg.CaseEQY, -1.5));

            // ── ULS: Wind ─────────────────────────────────────────────────────
            AddLinear("IS875_W1_WLX+", log, cfg,
                (cfg.PatternDead, 1.2), (cfg.PatternSDL, 1.2),
                (cfg.PatternLive, 1.2), (cfg.PatternWLX, +1.2));
            AddLinear("IS875_W2_WLX-", log, cfg,
                (cfg.PatternDead, 1.2), (cfg.PatternSDL, 1.2),
                (cfg.PatternLive, 1.2), (cfg.PatternWLX, -1.2));
            AddLinear("IS875_W3_WLY+", log, cfg,
                (cfg.PatternDead, 1.2), (cfg.PatternSDL, 1.2),
                (cfg.PatternLive, 1.2), (cfg.PatternWLY, +1.2));
            AddLinear("IS875_W4_WLY-", log, cfg,
                (cfg.PatternDead, 1.2), (cfg.PatternSDL, 1.2),
                (cfg.PatternLive, 1.2), (cfg.PatternWLY, -1.2));

            AddLinear("IS875_W5_09DL+WLX", log, cfg,
                (cfg.PatternDead, 0.9), (cfg.PatternWLX, +1.5));
            AddLinear("IS875_W6_09DL-WLX", log, cfg,
                (cfg.PatternDead, 0.9), (cfg.PatternWLX, -1.5));
            AddLinear("IS875_W7_09DL+WLY", log, cfg,
                (cfg.PatternDead, 0.9), (cfg.PatternWLY, +1.5));
            AddLinear("IS875_W8_09DL-WLY", log, cfg,
                (cfg.PatternDead, 0.9), (cfg.PatternWLY, -1.5));

            // ── SLS: Serviceability (IS 456 Cl. 23.2) ────────────────────────
            // Unfactored combinations for deflection and crack width checks.
            AddLinear("IS456_SLS1_Total", log, cfg,
                (cfg.PatternDead, 1.0), (cfg.PatternSDL, 1.0), (cfg.PatternLive, 1.0));
            AddLinear("IS456_SLS2_Perm", log, cfg,
                (cfg.PatternDead, 1.0), (cfg.PatternSDL, 1.0));
            AddLinear("IS456_SLS3_EQX", log, cfg,
                (cfg.PatternDead, 1.0), (cfg.PatternSDL, 1.0),
                (cfg.PatternLive, 1.0), (cfg.CaseEQX, 1.0));
            AddLinear("IS456_SLS4_EQY", log, cfg,
                (cfg.PatternDead, 1.0), (cfg.PatternSDL, 1.0),
                (cfg.PatternLive, 1.0), (cfg.CaseEQY, 1.0));

            // ── Envelope combinations ─────────────────────────────────────────
            CreateEnvelope("ENV_ULS", log,
                "IS875_G1_ULS", "IS875_G2_ULS_NoLL",
                "IS875_S1_EQX+", "IS875_S2_EQX-", "IS875_S3_EQY+", "IS875_S4_EQY-",
                "IS875_S5_09DL+EQX", "IS875_S6_09DL-EQX", "IS875_S7_09DL+EQY", "IS875_S8_09DL-EQY",
                "IS875_W1_WLX+", "IS875_W2_WLX-", "IS875_W3_WLY+", "IS875_W4_WLY-",
                "IS875_W5_09DL+WLX", "IS875_W6_09DL-WLX", "IS875_W7_09DL+WLY", "IS875_W8_09DL-WLY");

            CreateEnvelope("ENV_SLS", log,
                "IS456_SLS1_Total", "IS456_SLS2_Perm",
                "IS456_SLS3_EQX", "IS456_SLS4_EQY");

            int nAfter = CountCombos();
            log.AppendLine($"  Total: {nAfter - nBefore} new combinations created  " +
                           $"({nAfter} total in model)");
            return log.ToString();
        }

        // ── Helper: create one Linear Add combination ─────────────────────────
        // Cases can be either load patterns (eCNameType.LoadCase for auto-generated
        // static cases) or RS load cases (same type). ETABS identifies them by name.
        private void AddLinear(string comboName, System.Text.StringBuilder log,
                               BuildingConfig cfg, params (string name, double sf)[] cases)
        {
            // Skip if already exists
            if (ComboExists(comboName)) { log.AppendLine($"  SKIP  {comboName}"); return; }

            int ret = _sapModel.RespCombo.Add(comboName, 0); // 0 = Linear Add
            if (ret != 0) { log.AppendLine($"  FAIL  {comboName} Add (ret={ret})"); return; }

            bool allOk = true;
            foreach (var (name, sf) in cases)
            {
                eCNameType ct = eCNameType.LoadCase;
                int r2 = _sapModel.RespCombo.SetCaseList(comboName, ref ct, name, sf);
                if (r2 != 0) { allOk = false; log.AppendLine($"  WARN  {comboName}: case '{name}' (ret={r2})"); }
            }

            if (allOk)
                log.AppendLine($"  OK    {comboName}  [{string.Join("  ", Array.ConvertAll(cases, c => $"{(c.sf >= 0 ? "+" : "")}{c.sf:F1}×{c.name}"))}]");
        }

        // ── Helper: create an Envelope combination ────────────────────────────
        private void CreateEnvelope(string comboName, System.Text.StringBuilder log,
                                    params string[] subCombos)
        {
            if (ComboExists(comboName)) { log.AppendLine($"  SKIP  {comboName}"); return; }

            int ret = _sapModel.RespCombo.Add(comboName, 1); // 1 = Envelope
            if (ret != 0) { log.AppendLine($"  FAIL  {comboName} Envelope (ret={ret})"); return; }

            foreach (string sub in subCombos)
            {
                eCNameType ct = eCNameType.LoadCombo;
                _sapModel.RespCombo.SetCaseList(comboName, ref ct, sub, 1.0);
            }
            log.AppendLine($"  OK    {comboName} (envelope of {subCombos.Length} combos)");
        }

        private bool ComboExists(string name)
        {
            int n = 0; string[] names = null;
            _sapModel.RespCombo.GetNameList(ref n, ref names);
            return names != null &&
                   Array.Exists(names, c => string.Equals(c, name, StringComparison.OrdinalIgnoreCase));
        }

        private int CountCombos()
        {
            int n = 0; string[] names = null;
            _sapModel.RespCombo.GetNameList(ref n, ref names);
            return n;
        }
    }
}
