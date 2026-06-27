using System;
using System.Collections.Generic;
using ETABSv1;
using System.Windows.Forms;

namespace CSiNET8PluginExample1
{
    /// <summary>
    /// Writes optimised slab thicknesses back into ETABS as new shell-section
    /// properties and re-assigns the panels.
    ///
    /// PATCH NOTES (v2):
    ///  • New PushOptimizedThicknessSilent(slab) — no popup, returns a status
    ///    code.  Used by "Push ALL" so the user gets one summary popup at the
    ///    end instead of N message boxes.
    ///  • PushOptimizedThickness still exists for the single-slab button.
    ///  • RefreshView() is called once at the end of any batch so the model
    ///    redraws with the new thicknesses.
    ///  • Properties are de-duplicated: if SLAB_{D}mm already exists for the
    ///    same material we skip the redundant SetSlab call.
    /// </summary>
    public class EtabsModelUpdater
    {
        private readonly cSapModel _sapModel;
        private readonly HashSet<string> _propertiesCreated = new HashSet<string>();

        public EtabsModelUpdater(cSapModel sapModel)
        {
            _sapModel = sapModel;
        }

        private double MmToModelUnits()
        {
            eUnits u = _sapModel.GetPresentUnits();
            string uName = u.ToString();
            if (uName.Contains("_mm_")) return 1.0;
            if (uName.Contains("_in_")) return 1.0 / 25.4;
            if (uName.Contains("_ft_")) return 1.0 / 304.8;
            return 1.0 / 1000.0;                 // → metres
        }

        /// <summary>Single-slab push with popup feedback.</summary>
        public void PushOptimizedThickness(SlabData slab)
        {
            var (ok, msg) = PushOptimizedThicknessSilent(slab);
            MessageBox.Show(msg,
                ok ? "Success" : "Error",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (ok) TryRefreshView();
        }

        /// <summary>
        /// Batch push: no popup per slab. Caller is expected to summarise.
        /// </summary>
        public (bool ok, string message) PushOptimizedThicknessSilent(SlabData slab)
        {
            try
            {
                string newPropName = $"SLAB_{(int)Math.Round(slab.Thickness)}mm";
                double thicknessModelUnits = slab.Thickness * MmToModelUnits();

                string matProp = string.IsNullOrEmpty(slab.MaterialName)
                    ? "M25" : slab.MaterialName;

                // De-duplicate SetSlab calls inside one batch
                string cacheKey = $"{newPropName}|{matProp}";
                if (!_propertiesCreated.Contains(cacheKey))
                {
                    int retSet = _sapModel.PropArea.SetSlab(
                        newPropName, eSlabType.Slab, eShellType.ShellThin,
                        matProp, thicknessModelUnits);

                    if (retSet != 0)
                        return (false, $"SetSlab() failed for '{newPropName}' (ret={retSet}). " +
                                       $"Check material '{matProp}' and units.");

                    _propertiesCreated.Add(cacheKey);
                }

                int retAssign = _sapModel.AreaObj.SetProperty(slab.Name, newPropName);
                if (retAssign != 0)
                    return (false, $"SetProperty() failed for slab '{slab.Name}' (ret={retAssign}).");

                return (true, $"Slab '{slab.Name}' → {slab.Thickness:F0} mm " +
                              $"(property '{newPropName}', material '{matProp}').");
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>Force ETABS to redraw the active view after the batch.</summary>
        public void TryRefreshView()
        {
            try { _sapModel.View.RefreshView(0, false); }
            catch { /* non-critical */ }
        }
    }
}
