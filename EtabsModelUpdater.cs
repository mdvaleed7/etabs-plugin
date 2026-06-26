using System;
using ETABSv1;
using System.Windows.Forms;

namespace CSiNET8PluginExample1
{
    public class EtabsModelUpdater
    {
        private cSapModel _sapModel;

        public EtabsModelUpdater(cSapModel sapModel)
        {
            _sapModel = sapModel;
        }

        public void PushOptimizedThickness(SlabData slab)
        {
            try
            {
                // Create a new shell section property named "SLAB_XXX"
                string newPropName = $"SLAB_{slab.Thickness}";
                
                // Define the property (Material: typically "M25" or similar from the model, we assume a default for now)
                // 1 = slab thin
                _sapModel.PropArea.SetSlab(newPropName, eSlabType.Slab, eShellType.ShellThin, "", slab.Thickness);
                
                // Assign the new property to the area object
                int ret = _sapModel.AreaObj.SetProperty(slab.Name, newPropName);
                
                if (ret == 0)
                {
                    MessageBox.Show($"Successfully updated slab {slab.Name} to thickness {slab.Thickness}mm (Property: {newPropName}).", "Success");
                }
                else
                {
                    MessageBox.Show($"Failed to assign property to slab {slab.Name}.", "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating ETABS model: {ex.Message}", "Error");
            }
        }
    }
}
