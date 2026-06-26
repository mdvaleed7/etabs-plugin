using System;

namespace CSiNET8PluginExample1
{
    public class SlabDesignEngine
    {
        public static void DesignSlab(SlabData slab)
        {
            // Port of slabEngine.ts IS 456 Logic
            double wFactored = (slab.DeadLoad + slab.SuperimposedDeadLoad + slab.LiveLoad) * 1.5;
            
            // IS 456 Table 26 Alpha coefficients (simplified mockup for interior panel)
            double ax_pos = 0.024;
            double ay_pos = 0.024;
            double ax_neg = 0.032;
            double ay_neg = 0.032;
            
            double Lx_m = slab.Lx / 1000.0;
            
            double Mx_pos = ax_pos * wFactored * Math.Pow(Lx_m, 2);
            double My_pos = ay_pos * wFactored * Math.Pow(Lx_m, 2);
            double Mx_neg = ax_neg * wFactored * Math.Pow(Lx_m, 2);
            double My_neg = ay_neg * wFactored * Math.Pow(Lx_m, 2);
            
            double maxMu = Math.Max(Mx_pos, Math.Max(My_pos, Math.Max(Mx_neg, My_neg)));
            double fck = 25; // M25
            double fy = 500; // Fe500
            double coeff = 0.133; // for Fe500
            
            // Dummy flexure Ast for deflection
            double d_initial = slab.Thickness - 20 - 5;
            double Ast_req = (maxMu * 1e6) / (0.87 * fy * 0.8 * d_initial); 
            
            // Run Iterative Deflection Loop
            var deflResult = Is456DeflectionEngine.CheckAndOptimizeThickness(slab, maxMu, maxMu * 0.6, Ast_req);
            
            if (deflResult.Status == "SAFE")
            {
                slab.DesignStatus = "SAFE";
                slab.Thickness = deflResult.RequiredThickness;
                slab.Notes = $"Required thickness for deflection: {deflResult.RequiredThickness} mm. Deflection {deflResult.CalculatedDeflection:F1}mm <= {deflResult.AllowableDeflection:F1}mm. Moments: Mx+={Mx_pos:F1} kNm";
            }
            else
            {
                slab.DesignStatus = "REVISE";
                slab.Notes = $"Deflection FAIL! Required > 350mm. Deflection {deflResult.CalculatedDeflection:F1}mm > {deflResult.AllowableDeflection:F1}mm.";
            }
        }
    }
}
