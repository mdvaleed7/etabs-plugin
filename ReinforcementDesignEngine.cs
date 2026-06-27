using System;

namespace CSiNET8PluginExample1
{
    /// <summary>
    /// IS 456:2000 singly-reinforced flexural design + bar selection.
    ///
    /// PATCH NOTES (v2):
    ///  • CalculateAst no longer silently caps Mu at Mu_lim. It now signals
    ///    over-reinforcement via the optional out-flag so the caller (slab
    ///    design engine) can bump the thickness instead of returning a
    ///    misleading "design successful" result.
    ///  • Total depth D is passed in explicitly (was hard-coded as d + 25 mm)
    ///    so Ast_min is consistent with the actual slab.
    ///  • Bar diameter list is configurable.
    /// </summary>
    public static class ReinforcementDesignEngine
    {
        /// <summary>
        /// Computes required steel area Ast (mm² per metre strip) for a given
        /// ultimate moment Mu (kN·m / m).  Follows IS 456 Annex G singly-
        /// reinforced formulation.
        /// </summary>
        /// <param name="Mu_kNm">Design ultimate moment per metre width (kN·m/m).</param>
        /// <param name="d">Effective depth (mm).</param>
        /// <param name="D">Total slab depth (mm) — needed only for Ast,min.</param>
        /// <param name="fck">Concrete characteristic strength (N/mm²).</param>
        /// <param name="fy">Steel yield strength (N/mm²).</param>
        /// <param name="overReinforced">True when Mu > Mu_lim (section needs
        /// either greater depth or compression reinforcement).</param>
        public static double CalculateAst(
            double Mu_kNm, double d, double D,
            double fck, double fy,
            out bool overReinforced)
        {
            overReinforced = false;
            if (Mu_kNm <= 0 || d <= 0) return 0;

            double Mu = Mu_kNm * 1e6;   // kN·m → N·mm
            double b  = 1000.0;          // 1 m strip width

            // Limiting xu/d (IS 456 Cl. 38.1, Fig. 21)
            double xu_max_d = fy <= 250 ? 0.53 :
                              fy <= 415 ? 0.48 :
                              0.46;                        // Fe500 / Fe550

            double R_lim  = 0.36 * fck * xu_max_d * (1 - 0.42 * xu_max_d);
            double Mu_lim = R_lim * b * d * d;             // N·mm

            if (Mu > Mu_lim)
            {
                overReinforced = true;                     // PATCH: signal it
                Mu = Mu_lim;                               // safe value for sqrt
            }

            // Solve Mu = 0.87 fy Ast (d − 0.42 xu) with xu = (0.87 fy Ast)/(0.36 fck b)
            double rootTerm = 1 - (4.6 * Mu) / (fck * b * d * d);
            if (rootTerm < 0) rootTerm = 0;
            double Ast = (0.5 * fck / fy) * (1 - Math.Sqrt(rootTerm)) * b * d;

            // Minimum steel (IS 456 Cl. 26.5.2.1)
            double minPct  = (fy >= 415) ? 0.12 : 0.15;
            double Ast_min = (minPct / 100.0) * b * D;

            return Math.Max(Ast, Ast_min);
        }

        /// <summary>Convenience overload that ignores the over-reinforcement flag.</summary>
        public static double CalculateAst(double Mu_kNm, double d, double fck = 25, double fy = 500)
        {
            // Best-effort D estimate for back-compat (only used for Ast,min).
            double D = d + 25;
            return CalculateAst(Mu_kNm, d, D, fck, fy, out _);
        }

        /// <summary>
        /// Picks the smallest practical (bar Ø + spacing) combination that meets
        /// the required Ast while honouring IS 456 maximum-spacing rules.
        /// </summary>
        /// <param name="preferredDiameters">User-configurable bar diameter list
        /// (defaults to 8, 10, 12, 16 mm). The first diameter that yields a
        /// constructible spacing ≥ 75 mm is selected.</param>
        public static string SelectBars(
            double Ast_req, double d, bool isMainSteel = true,
            int[] preferredDiameters = null)
        {
            if (Ast_req <= 0) return "None";
            preferredDiameters ??= new[] { 8, 10, 12, 16 };

            double maxSpacingMain = Math.Min(3 * d, 300);   // IS 456 Cl. 26.3.3 (b)
            double maxSpacingDist = Math.Min(5 * d, 450);
            double maxSpacing     = isMainSteel ? maxSpacingMain : maxSpacingDist;

            foreach (int barDia in preferredDiameters)
            {
                double areaOneBar = Math.PI * barDia * barDia / 4.0;
                double reqSpacing = 1000.0 * areaOneBar / Ast_req;

                // Snap down to nearest 10 mm
                double spacing = Math.Floor(reqSpacing / 10.0) * 10.0;
                if (spacing > maxSpacing)
                    spacing = Math.Floor(maxSpacing / 10.0) * 10.0;

                if (spacing >= 75)
                    return $"T{barDia} @ {spacing:F0} c/c";
            }

            // Even the largest configured bar needs <75 mm spacing → heavy
            int biggest = preferredDiameters[preferredDiameters.Length - 1];
            return $"T{biggest} @ 75 c/c (Heavy — revise depth)";
        }
    }
}
