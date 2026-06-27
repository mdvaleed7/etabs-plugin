using System;

namespace CSiNET8PluginExample1
{
    /// <summary>
    /// Drives the slab-thickness-optimisation loop for One-Way, Two-Way and
    /// Cantilever slabs.  Flat slabs are delegated to FlatSlabEngine.
    ///
    /// PATCH NOTES (v2):
    ///  • fck, fy, cover and bar diameters now come from SlabData (which is
    ///    populated from the ETABS material + the user UI), not hard-coded.
    ///  • Effective depth uses BarDiaMain/2 instead of "5 mm".
    ///  • dy (Y-direction effective depth) uses (BarDiaMain + BarDiaDist/2)
    ///    instead of subtracting a flat 10 mm.
    ///  • Over-reinforcement flag from ReinforcementDesignEngine triggers a
    ///    thickness bump instead of silently capping Mu.
    ///  • Service / permanent moments use the correct positive-moment
    ///    coefficient for each slab type, not a borrowed cantilever value.
    /// </summary>
    public class SlabDesignEngine
    {
        public static void DesignSlab(SlabData slab)
        {
            if (slab.Type == SlabType.FlatSlab)
            {
                FlatSlabEngine.DesignFlatSlab(slab);
                return;
            }

            // Pull user / model inputs from SlabData (PATCH)
            double fck   = slab.Fck > 0 ? slab.Fck : 25;
            double fy    = slab.Fy  > 0 ? slab.Fy  : 500;
            double cover = slab.Cover;
            double dbMain = slab.BarDiaMain;
            double dbDist = slab.BarDiaDist;

            bool isSafe = false;
            int  iterations = 0;
            const int MAX_ITER = 20;

            double limitA = 0;
            double finalDeflection = 0;

            while (!isSafe && iterations < MAX_ITER)
            {
                // ── Effective depth: cover + ½ bar (Y-layer sits above X-layer) ──
                double d  = slab.Thickness - cover - dbMain / 2.0;
                double dy = slab.Thickness - cover - dbMain - dbDist / 2.0;
                if (d <= 0 || dy <= 0)
                {
                    slab.Thickness += 10; iterations++; continue;
                }

                // ── Effective spans (IS 456 Cl. 22.2) ─────────────────────
                double Lx_eff = EffectiveSpanCalculator.CalculateEffectiveSpan(
                    slab.Lx, slab.SupportWidthX1, slab.SupportWidthX2,
                    slab.IsContinuousX1, slab.IsContinuousX2, d, slab.Type);

                double Ly_eff = EffectiveSpanCalculator.CalculateEffectiveSpan(
                    slab.Ly, slab.SupportWidthY1, slab.SupportWidthY2,
                    slab.IsContinuousY1, slab.IsContinuousY2, d, slab.Type);

                // ── Loads ──────────────────────────────────────────────────
                double selfWeight   = (slab.Thickness / 1000.0) * 25.0;        // kN/m²
                double totalDL      = selfWeight + slab.DeadLoad + slab.SuperimposedDeadLoad;
                double totalService = totalDL + slab.LiveLoad;
                double wFactored    = 1.5 * totalService;                       // IS 875 Pt 5

                double Lx_m  = Lx_eff / 1000.0;
                double Ly_m  = Ly_eff / 1000.0;

                // ── Bending moments per metre (kN·m/m) ────────────────────
                double Mx_pos = 0, My_pos = 0, Mx_neg = 0, My_neg = 0;
                double ax_pos_use = 0;    // coefficient used for deflection moment lookup

                if (slab.Type == SlabType.TwoWay)
                {
                    double lyLx = Ly_eff / Lx_eff;
                    double ax_pos = Is456Table26.GetAlphaXPos(slab.BoundaryCase, lyLx);
                    double ax_neg = Is456Table26.GetAlphaXNeg(slab.BoundaryCase, lyLx);
                    double ay_pos = Is456Table26.GetAlphaYPos(slab.BoundaryCase);
                    double ay_neg = Is456Table26.GetAlphaYNeg(slab.BoundaryCase);

                    Mx_pos = ax_pos * wFactored * Lx_m * Lx_m;
                    Mx_neg = ax_neg * wFactored * Lx_m * Lx_m;
                    My_pos = ay_pos * wFactored * Lx_m * Lx_m;
                    My_neg = ay_neg * wFactored * Lx_m * Lx_m;
                    ax_pos_use = ax_pos;
                }
                else if (slab.Type == SlabType.OneWay)
                {
                    // IS 456 Cl. 22.5 simplified coefficients for continuous beams
                    double ax_pos = 1.0 / 12.0;
                    double ax_neg = 1.0 / 10.0;
                    Mx_pos = ax_pos * wFactored * Lx_m * Lx_m;
                    Mx_neg = ax_neg * wFactored * Lx_m * Lx_m;
                    ax_pos_use = ax_pos;
                }
                else if (slab.Type == SlabType.Cantilever)
                {
                    // Root moment governs: Mu = w·L²/2  ⇒ α_root = 0.5
                    double ax_root = 0.5;
                    Mx_neg = ax_root * wFactored * Lx_m * Lx_m;
                    // For the deflection moment lookup we use the same root
                    // moment, because Annex C cantilever formula
                    // δ_tip = (1/4) · M_root · L²/(EI) ≡ wL⁴/(8 EI).
                    ax_pos_use = ax_root;
                }

                // ── Required steel & over-reinforcement check ─────────────
                slab.Ast_x_bot = ReinforcementDesignEngine.CalculateAst(
                    Mx_pos, d,  slab.Thickness, fck, fy, out bool over1);
                slab.Ast_y_bot = ReinforcementDesignEngine.CalculateAst(
                    My_pos, dy, slab.Thickness, fck, fy, out bool over2);
                slab.Ast_x_top = ReinforcementDesignEngine.CalculateAst(
                    Mx_neg, d,  slab.Thickness, fck, fy, out bool over3);
                slab.Ast_y_top = ReinforcementDesignEngine.CalculateAst(
                    My_neg, dy, slab.Thickness, fck, fy, out bool over4);

                bool anyOver = over1 || over2 || over3 || over4;
                slab.IsOverReinforced = anyOver;

                int[] preferredDia = { (int)dbDist, (int)dbMain, 12, 16 };
                slab.Bars_x_bot = ReinforcementDesignEngine.SelectBars(slab.Ast_x_bot, d,  true,  preferredDia);
                slab.Bars_y_bot = ReinforcementDesignEngine.SelectBars(slab.Ast_y_bot, dy, true,  preferredDia);
                slab.Bars_x_top = ReinforcementDesignEngine.SelectBars(slab.Ast_x_top, d,  true,  preferredDia);
                slab.Bars_y_top = ReinforcementDesignEngine.SelectBars(slab.Ast_y_top, dy, true,  preferredDia);

                if (anyOver)
                {
                    // Section can't carry Mu as singly-reinforced → grow depth.
                    slab.Thickness += 10.0;
                    iterations++;
                    continue;
                }

                // ── Service / permanent moments for deflection check ──────
                double M_service = ax_pos_use * totalService * Lx_m * Lx_m;
                double M_perm    = ax_pos_use * totalDL      * Lx_m * Lx_m;

                var deflResult = Is456DeflectionEngine.CheckThickness(
                    slab, M_service, M_perm, slab.Ast_x_bot, fck, fy, Lx_eff);

                if (deflResult.Status == "SAFE")
                {
                    isSafe          = true;
                    limitA          = deflResult.AllowableDeflection;
                    finalDeflection = deflResult.CalculatedDeflection;

                    slab.DesignStatus = "SAFE";
                    slab.Notes =
                        $"Leff={Lx_eff:F0}mm, D={slab.Thickness:F0}mm. " +
                        $"a_tot={finalDeflection:F1}mm ≤ lim={limitA:F1}mm. " +
                        $"Mx+={Mx_pos:F2}kNm/m, Mx-={Mx_neg:F2}kNm/m. " +
                        $"wFact={wFactored:F2}kN/m². " +
                        $"fck={fck:F0}, fy={fy:F0}.";
                    break;
                }
                else
                {
                    slab.Thickness += 10.0;
                    iterations++;
                }
            }

            if (!isSafe)
            {
                slab.DesignStatus = "REVISE";
                slab.Notes =
                    $"Deflection FAIL after {MAX_ITER} iterations (max D tried = " +
                    $"{slab.Thickness:F0} mm). Review span, loading, or material grade.";
            }
        }
    }
}
