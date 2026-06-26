using System;
using System.Collections.Generic;
using ETABSv1;

namespace CSiNET8PluginExample1
{
    public class EtabsDataExtractor
    {
        private cSapModel _sapModel;

        public EtabsDataExtractor(cSapModel sapModel)
        {
            _sapModel = sapModel;
        }

        public List<SlabData> ExtractSlabs()
        {
            List<SlabData> slabs = new List<SlabData>();
            
            int numberNames = 0;
            string[] myNames = null;
            
            // Get all area objects
            int ret = _sapModel.AreaObj.GetNameList(ref numberNames, ref myNames);
            if (ret != 0 || numberNames == 0 || myNames == null)
            {
                return slabs;
            }

            foreach (var name in myNames)
            {
                SlabData slab = new SlabData();
                slab.Name = name;
                
                // Get points to determine dimensions Lx, Ly
                int numPoints = 0;
                string[] pointNames = null;
                _sapModel.AreaObj.GetPoints(name, ref numPoints, ref pointNames);
                
                // For a typical rectangular slab with 4 points
                if (numPoints == 4)
                {
                    double x1 = 0, y1 = 0, z1 = 0;
                    double x2 = 0, y2 = 0, z2 = 0;
                    double x3 = 0, y3 = 0, z3 = 0;
                    
                    _sapModel.PointObj.GetCoordCartesian(pointNames[0], ref x1, ref y1, ref z1);
                    _sapModel.PointObj.GetCoordCartesian(pointNames[1], ref x2, ref y2, ref z2);
                    _sapModel.PointObj.GetCoordCartesian(pointNames[2], ref x3, ref y3, ref z3);
                    
                    double dist12 = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
                    double dist23 = Math.Sqrt(Math.Pow(x2 - x3, 2) + Math.Pow(y2 - y3, 2));
                    
                    // Assume ETABS model is in m, multiply by 1000 for mm
                    slab.Lx = Math.Min(dist12, dist23) * 1000;
                    slab.Ly = Math.Max(dist12, dist23) * 1000;
                }
                else
                {
                    slab.Lx = 4000; // placeholder if not rectangle
                    slab.Ly = 4000;
                }
                
                // Classify slab type based on Ly / Lx
                if (slab.Lx > 0 && slab.Ly > 0)
                {
                    if (slab.Ly / slab.Lx > 2.0)
                        slab.Type = SlabType.OneWay;
                    else
                        slab.Type = SlabType.TwoWay;
                }

                // Placeholder values for thicknesses and loads
                slab.Thickness = 150; 
                slab.DeadLoad = 1.0; 
                slab.LiveLoad = 3.0;
                slab.SuperimposedDeadLoad = 1.5;
                slab.BoundaryCase = 1; // Default to interior panel

                slabs.Add(slab);
            }
            
            return slabs;
        }
    }
}
