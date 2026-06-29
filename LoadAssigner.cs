using System;
using System.Collections.Generic;
using ETABSv1;

namespace CSiNET8PluginExample1
{
    /// <summary>
    /// Assigns gravity loads to all area and frame objects in the ETABS model.
    ///
    /// Strategy
    /// ────────
    /// 1. Identify stories: the topmost story receives ROOF loads (RoofLL, Parapet).
    ///    All other stories receive FLOOR loads (LL, SDL, Cladding).
    ///
    /// 2. Area objects (slabs):
    ///    • SDL assigned uniformly to all slab area objects.
    ///    • LL  assigned uniformly to floor slabs; RoofLL to roof slabs.
    ///    • Self-weight is handled by the DEAD load pattern's SW multiplier = 1.0.
    ///
    /// 3. Frame objects (beams / columns):
    ///    • Cladding/façade line load applied to EXTERIOR beams only.
    ///      Exterior beams are identified as frame objects whose mid-point lies
    ///      on or near the plan bounding box perimeter (within a tolerance of 0.2 m).
    ///    • Parapet line load applied to roof-level exterior beams.
    ///    • Interior beams and all columns receive NO additional loads here;
    ///      their self-weight is captured by the DEAD pattern (SW mult = 1.0).
    ///
    /// ETABS API direction convention used throughout:
    ///   SetLoadUniform  — MyDir = 6 (Global Z), Value = -load (downward = -Z)
    ///   SetLoadDistributed — MyDir = 6 (Global Z), Val = -load (downward)
    ///   The Global Z axis in ETABS points upward; downward loads require negative values.
    ///
    /// IS code references
    /// ──────────────────
    ///   SDL:       IS 875 Part 1 (unit weights, superimposed finishes)
    ///   LL:        IS 875 Part 2 (imposed loads on floors/roofs)
    ///   Cladding:  IS 875 Part 1 (façade self-weight as line load on beams)
    ///   Parapet:   IS 875 Part 1
    /// </summary>
    public class LoadAssigner
    {
        private readonly cSapModel _sapModel;

        // Tolerance (metres) for perimeter beam detection:
        // a beam mid-point within this distance of the plan bounding box is "exterior".
        private const double PERIMETER_TOL = 0.30; // m

        public LoadAssigner(cSapModel sapModel)
        {
            _sapModel = sapModel;
        }

        public string AssignAllLoads(BuildingConfig cfg)
        {
            var log = new System.Text.StringBuilder();
            log.AppendLine("═══ Assigning Loads ═══");

            // Identify roof story (highest elevation)
            double roofElev = 0;
            string roofStory = GetRoofStory(log, out roofElev);

            // Assign loads to area objects (slabs)
            AssignSlabLoads(cfg, roofStory, roofElev, log);

            // Assign loads to frame objects (beams): cladding on exterior beams
            AssignBeamLoads(cfg, roofStory, roofElev, log);

            return log.ToString();
        }

        // ── Identify the top-most story ───────────────────────────────────────
        private string GetRoofStory(System.Text.StringBuilder log, out double maxElev)
        {
            maxElev = 0;
            int nStories = 0;
            string[] storyNames = null;
            double[] storyElev = null;
            double[] storyHt = null;
            bool[] isMaster = null;
            string[] simTo = null;
            bool[] spliceAbove = null;
            double[] spliceHt = null;

            int ret = _sapModel.Story.GetStories(
                ref nStories, ref storyNames, ref storyElev,
                ref storyHt, ref isMaster, ref simTo,
                ref spliceAbove, ref spliceHt);

            if (ret != 0 || nStories == 0 || storyNames == null)
            {
                log.AppendLine("  WARN  Could not read story list — roof detection skipped");
                return "";
            }

            // Find the story with the maximum elevation
            int topIdx = 0;
            maxElev = storyElev[0];
            for (int i = 1; i < nStories; i++)
                if (storyElev[i] > maxElev) { maxElev = storyElev[i]; topIdx = i; }

            string roofStory = storyNames[topIdx];
            log.AppendLine($"  INFO  Roof story identified: '{roofStory}' at elev {maxElev:F2} m");
            return roofStory;
        }

        // ── Slab / Area object loads ──────────────────────────────────────────
        private void AssignSlabLoads(BuildingConfig cfg, string roofStory, double roofElev,
                                     System.Text.StringBuilder log)
        {
            int nAreas = 0;
            string[] areaNames = null;
            int ret = _sapModel.AreaObj.GetNameList(ref nAreas, ref areaNames);
            if (ret != 0 || nAreas == 0 || areaNames == null)
            {
                log.AppendLine("  FAIL  No area objects found");
                return;
            }

            int floorSDL = 0, floorLL = 0, roofLL = 0, skipped = 0;

            foreach (string aName in areaNames)
            {
                // Determine which story this area belongs to
                int numPoints = 0;
                string[] pointNames = null;
                _sapModel.AreaObj.GetPoints(aName, ref numPoints, ref pointNames);
                double z = 0;
                if (numPoints > 0 && pointNames != null)
                {
                    double ptX = 0, ptY = 0;
                    _sapModel.PointObj.GetCoordCartesian(pointNames[0], ref ptX, ref ptY, ref z);
                }

                bool isRoof = Math.Abs(z - roofElev) < 0.1;

                // SDL: same for floor and roof (finishes, waterproofing etc.)
                // IS 875 Part 1: finishes 0.5–1.5 kN/m², waterproofing ~1.0 kN/m²
                // Direction 6 = Global Z; negative value = downward
                int r1 = _sapModel.AreaObj.SetLoadUniform(
                    aName, cfg.PatternSDL, -cfg.SDL, 6, true, "Global", eItemType.Objects);
                if (r1 == 0) floorSDL++;

                // LL: floor LL or roof LL depending on story
                double ll = isRoof ? cfg.RoofLiveLoad : cfg.LiveLoad;
                string pat = cfg.PatternLive;
                int r2 = _sapModel.AreaObj.SetLoadUniform(
                    aName, pat, -ll, 6, true, "Global", eItemType.Objects);
                if (r2 == 0)
                { if (isRoof) roofLL++; else floorLL++; }
                else skipped++;
            }

            log.AppendLine($"  OK    SDL={cfg.SDL} kN/m² → {floorSDL} areas");
            log.AppendLine($"  OK    Floor LL={cfg.LiveLoad} kN/m² → {floorLL} areas");
            log.AppendLine($"  OK    Roof LL={cfg.RoofLiveLoad} kN/m² → {roofLL} areas");
            if (skipped > 0) log.AppendLine($"  WARN  {skipped} area assignments failed — check model");
        }

        // ── Beam / Frame object loads: cladding on exterior beams ─────────────
        private void AssignBeamLoads(BuildingConfig cfg, string roofStory, double roofElev,
                                     System.Text.StringBuilder log)
        {
            int nFrames = 0;
            string[] frameNames = null;
            int ret = _sapModel.FrameObj.GetNameList(ref nFrames, ref frameNames);
            if (ret != 0 || nFrames == 0 || frameNames == null)
            {
                log.AppendLine("  WARN  No frame objects found — beam load assignment skipped");
                return;
            }

            // Compute plan bounding box (X-Y) of all frame mid-points
            double xMin = double.MaxValue, xMax = double.MinValue;
            double yMin = double.MaxValue, yMax = double.MinValue;

            var midpoints = new Dictionary<string, (double x, double y, double z, string story)>();

            foreach (string fName in frameNames)
            {
                string pt1 = "", pt2 = "";
                _sapModel.FrameObj.GetPoints(fName, ref pt1, ref pt2);

                double x1=0, y1=0, z1=0, x2=0, y2=0, z2=0;
                _sapModel.PointObj.GetCoordCartesian(pt1, ref x1, ref y1, ref z1);
                _sapModel.PointObj.GetCoordCartesian(pt2, ref x2, ref y2, ref z2);

                double mx = (x1 + x2) / 2.0;
                double my = (y1 + y2) / 2.0;
                double mz = (z1 + z2) / 2.0;

                midpoints[fName] = (mx, my, mz, "");

                if (mx < xMin) xMin = mx; if (mx > xMax) xMax = mx;
                if (my < yMin) yMin = my; if (my > yMax) yMax = my;
            }

            // Unit conversion: bounding box is in model units → need to apply
            // PERIMETER_TOL in the same units. Query and convert:
            eUnits u = _sapModel.GetPresentUnits();
            string uName = u.ToString();
            double tolModelUnits = PERIMETER_TOL; // default for m model
            if (uName.Contains("_mm_"))  tolModelUnits = PERIMETER_TOL * 1000.0;
            else if (uName.Contains("_in_")) tolModelUnits = PERIMETER_TOL * 39.37;
            else if (uName.Contains("_ft_")) tolModelUnits = PERIMETER_TOL * 3.281;

            int nCladding = 0, nParapet = 0, nSkip = 0;

            foreach (var kvp in midpoints)
            {
                string fName = kvp.Key;
                var (mx, my, mz, _) = kvp.Value;

                // Skip vertical members (columns): their length is predominantly in Z.
                // Heuristic: if |Δz| > max(|Δx|, |Δy|) → column → skip.
                string pt1="", pt2="";
                _sapModel.FrameObj.GetPoints(fName, ref pt1, ref pt2);
                double x1=0,y1=0,z1=0,x2=0,y2=0,z2=0;
                _sapModel.PointObj.GetCoordCartesian(pt1, ref x1, ref y1, ref z1);
                _sapModel.PointObj.GetCoordCartesian(pt2, ref x2, ref y2, ref z2);
                double dz = Math.Abs(z2 - z1);
                double dxy = Math.Sqrt((x2-x1)*(x2-x1) + (y2-y1)*(y2-y1));
                if (dz > dxy) continue; // skip columns

                // Check if beam mid-point is on the perimeter
                bool onPerim = (mx <= xMin + tolModelUnits) || (mx >= xMax - tolModelUnits) ||
                               (my <= yMin + tolModelUnits) || (my >= yMax - tolModelUnits);
                if (!onPerim) continue;

                bool isRoof = Math.Abs(mz - roofElev) < 0.1;

                if (isRoof)
                {
                    // Parapet load on roof perimeter beams (kN/m)
                    // IS 875 Part 1: masonry parapet ~2–4 kN/m
                    int r = ApplyBeamLineLoad(fName, cfg.PatternSDL, cfg.ParapetLoad_kNm);
                    if (r == 0) nParapet++; else nSkip++;
                }
                else
                {
                    // Cladding load on exterior floor beams (kN/m)
                    // IS 875 Part 1: typical glass/ACP cladding 0.5–1.5 kN/m²×storey ht
                    int r = ApplyBeamLineLoad(fName, cfg.PatternSDL, cfg.CladdingLoad_kNm);
                    if (r == 0) nCladding++; else nSkip++;
                }
            }

            log.AppendLine($"  OK    Cladding {cfg.CladdingLoad_kNm} kN/m → {nCladding} perimeter beams");
            log.AppendLine($"  OK    Parapet  {cfg.ParapetLoad_kNm} kN/m → {nParapet} roof perimeter beams");
            if (nSkip > 0) log.AppendLine($"  WARN  {nSkip} beam assignments failed");
        }

        /// <summary>
        /// Applies a uniform distributed line load (kN/m) to a single frame object.
        /// Direction 6 (Global Z), negative value = downward gravity.
        /// </summary>
        private int ApplyBeamLineLoad(string frameName, string patternName, double load_kNm)
        {
            // SetLoadDistributed: MyType=1 (force), Dir=6 (Global Z)
            // Dist1=0, Dist2=1 (full span), Val1=Val2=-load (uniform)
            // RelDist=true (relative distances 0.0–1.0)
            // Replace=false (add to existing; SDL may have multiple contributions)
            return _sapModel.FrameObj.SetLoadDistributed(
                frameName, patternName,
                1,       // MyType: 1=force, 2=moment
                6,       // Dir: global Z
                0.0,     // Dist1 (start, relative)
                1.0,     // Dist2 (end, relative)
                -load_kNm, // Val1: negative = downward
                -load_kNm, // Val2: negative = downward
                "Global",
                true,    // relative distances
                false,   // do not replace existing loads on this frame
                eItemType.Objects);  // by object
        }
    }
}
