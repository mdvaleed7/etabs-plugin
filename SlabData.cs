using System;
using System.Collections.Generic;

namespace CSiNET8PluginExample1
{
    public enum SlabType
    {
        OneWay,
        TwoWay,
        Cantilever,
        FlatSlab,
        Unknown
    }

    public enum SupportCondition
    {
        SimplySupported,
        Continuous,
        Cantilever,
        OneEndContinuous
    }

    public class SlabData
    {
        public string Name { get; set; }
        public string StoryName { get; set; }
        
        // Dimensions
        public double Lx { get; set; } // Short span (mm)
        public double Ly { get; set; } // Long span (mm)
        public double Thickness { get; set; } // mm
        
        // Loads (kN/m2)
        public double DeadLoad { get; set; }
        public double LiveLoad { get; set; }
        public double SuperimposedDeadLoad { get; set; }
        
        // Classification
        public SlabType Type { get; set; }
        public int BoundaryCase { get; set; } // 1 to 9 (for TwoWay)
        public SupportCondition Support { get; set; } // for OneWay
        
        // Design Results
        public string DesignStatus { get; set; }
        public string Notes { get; set; }
    }
}
