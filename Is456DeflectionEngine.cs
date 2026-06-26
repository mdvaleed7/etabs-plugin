using System;

namespace CSiNET8PluginExample1
{
    public class Is456DeflectionEngine
    {
        public static double GetEc(double fck)
        {
            return 5000 * Math.Sqrt(fck); // IS 456 Cl. 6.2.3.1
        }

        public static (string Status, double RequiredThickness, double CalculatedDeflection, double AllowableDeflection) CheckAndOptimizeThickness(SlabData slab, double M_service, double M_perm, double Ast)
        {
            double b = 1000;
            double fck = 25; // default M25
            double fy = 500; // default Fe500
            double Ec = GetEc(fck);
            double Es = 200000;
            double m = Es / Ec;
            
            double L = Math.Min(slab.Lx, slab.Ly); 
            if (slab.Type == SlabType.Cantilever)
            {
                L = slab.Lx; // Single span for cantilever
            }
            
            double D = slab.Thickness;
            bool isSafe = false;
            
            double finalDeflection = 0;
            double allowableDeflection = L / 250; 
            
            int iterations = 0;
            
            while (!isSafe && iterations < 20)
            {
                double d = D - 20 - 5; // Cover 20, Bar dia 10
                
                // 1. Gross inertia
                double Igr = b * Math.Pow(D, 3) / 12;
                
                // 2. Cracking moment
                double fcr = 0.7 * Math.Sqrt(fck);
                double yt = D / 2;
                double Mcr = (fcr * Igr / yt) / 1e6;
                
                // 3. Cracked inertia (simplified neutral axis for singly reinforced)
                double x = (-m * Ast + Math.Sqrt(Math.Pow(m * Ast, 2) + 2 * b * m * Ast * d)) / b;
                double Icr = (b * Math.Pow(x, 3) / 3) + m * Ast * Math.Pow(d - x, 2);
                
                // 4. Effective inertia
                double Ms = Math.Abs(M_service);
                double z = d - x / 3;
                double Ieff = Igr;
                
                if (Ms > 0.001 && Mcr < Ms)
                {
                    double factor = 1.2 - (Mcr / Ms) * (z / d) * (1 - x / d);
                    Ieff = factor > 0 ? Icr / factor : Igr;
                    Ieff = Math.Max(Icr, Math.Min(Igr, Ieff));
                }
                
                // 5. Short term deflection
                double alpha = 5.0 / 48.0; // Assume simply supported for this example port
                if (slab.Type == SlabType.Cantilever) alpha = 1.0 / 4.0;
                else if (slab.BoundaryCase == 1) alpha = 1.0 / 16.0; // Interior continuous
                
                double ai = alpha * Ms * 1e6 * L * L / (Ec * Ieff);
                
                // 6. Long term deflection (simplified creep + shrinkage factor for mockup)
                double a_creep_shrinkage = ai * 1.5; 
                double a_total = ai + a_creep_shrinkage;
                
                finalDeflection = a_total;
                
                if (a_total <= allowableDeflection)
                {
                    isSafe = true;
                }
                else
                {
                    // Increment thickness and try again
                    D += 10; 
                    iterations++;
                }
            }
            
            if (isSafe)
            {
                return ("SAFE", D, finalDeflection, allowableDeflection);
            }
            return ("FAIL", D, finalDeflection, allowableDeflection);
        }
    }
}
