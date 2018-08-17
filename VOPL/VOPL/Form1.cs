using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VOPL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void ProcessButton_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(InputFolderTextBox.Text))
            {
                MessageBox.Show("Input folder doesn't exist: " + InputFolderTextBox.Text);
                return;
            }
            if (!Directory.Exists(OutputFolderTextBox.Text))
            {
                MessageBox.Show("Output folder doesn't exist: " + OutputFolderTextBox.Text);
                return;
            }

            string inputpath = InputFolderTextBox.Text;
            string outputpath = OutputFolderTextBox.Text;
            string[] oplfiles = Directory.GetFiles(inputpath, "*.opl", SearchOption.TopDirectoryOnly);
            string[] videfiles = Directory.GetFiles(inputpath, "*.vopl.txt", SearchOption.TopDirectoryOnly);
            float hdloddist = float.Parse(HDLODDistTextBox.Text, CultureInfo.InvariantCulture);
            float lodloddist = float.Parse(LODLODDistTextBox.Text, CultureInfo.InvariantCulture);
            float slodloddist = float.Parse(SLODLODDistTextBox.Text, CultureInfo.InvariantCulture);
            float offsetx = float.Parse(OffsetXTextBox.Text, CultureInfo.InvariantCulture);
            float offsety = float.Parse(OffsetYTextBox.Text, CultureInfo.InvariantCulture);
            float offsetz = float.Parse(OffsetZTextBox.Text, CultureInfo.InvariantCulture);
            bool cargens = CarGeneratorsCheckBox.Checked;
            uint cargenflags = uint.Parse(CarGenFlagsTextBox.Text, CultureInfo.InvariantCulture);

            if (!outputpath.EndsWith("\\"))
            {
                outputpath = outputpath + "\\";
            }

            StringBuilder errorlog = new StringBuilder();

            Dictionary<string, MapFile> mapdict = new Dictionary<string, MapFile>();
            Dictionary<string, VideInterior> intdict = new Dictionary<string, VideInterior>();

            string stringsfile = inputpath + (inputpath.EndsWith("\\") ? "" : "\\") + "strings.txt";
            if (File.Exists(stringsfile))
            {
                string[] strings = File.ReadAllLines(stringsfile);
                foreach (var str in strings)
                {
                    JenkIndex.Ensure(str);
                }
            }


            foreach (var videfile in videfiles) //load interior info files output from VIDE
            {
                try
                {
                    VideFile vf = new VideFile();
                    vf.Load(videfile);
                    foreach (var interior in vf.Interiors)
                    {
                        intdict[interior.Name] = interior;
                    }
                }
                catch (Exception ex)
                {
                    string err = "Error loading " + videfile + ":\n" + ex.ToString();
                    errorlog.AppendLine(err);
                }
            }


            foreach (var oplfile in oplfiles) //load opl files...
            {
                try
                {
                    MapFile mf = new MapFile();
                    mf.Load(oplfile);
                    mapdict[mf.Name] = mf;
                }
                catch (Exception ex)
                {
                    string err = "Error loading " + oplfile + ":\n" + ex.ToString();
                    errorlog.AppendLine(err);
                }
            }


            foreach (var mf in mapdict.Values) //create map file hierarchy
            {
                int uscind = mf.Name.LastIndexOf('_');
                if (uscind > 0)
                {
                    string pname = mf.Name.Substring(0, uscind);
                    MapFile parent = null;
                    if (mapdict.TryGetValue(pname, out parent))
                    {
                        mf.Parent = parent;
                        parent.Children.Add(mf);
                    }
                    else
                    { errorlog.AppendLine("Couldn't find parent " + pname + ".opl for " + mf.Name + "."); }
                }
            }

            foreach (var mf in mapdict.Values) //find parent entities
            {
                foreach (var ent in mf.AllEntities)
                {
                    if (ent.ParentIndex >= 0)
                    {
                        if (mf.Parent != null)
                        {
                            if (ent.ParentIndex < mf.Parent.AllEntities.Count)
                            {
                                ent.Parent = mf.Parent.AllEntities[ent.ParentIndex];
                            }
                            else
                            { errorlog.AppendLine("Couldn't find parent entity for " + ent.Name + ". ParentIndex " + ent.ParentIndex.ToString() + " was out of range!"); }
                        }
                        else
                        {
                            if (ent.ParentIndex < mf.AllEntities.Count)
                            {
                                ent.Parent = mf.AllEntities[ent.ParentIndex];
                            }
                            else
                            { errorlog.AppendLine("Couldn't find parent entity for " + ent.Name + ". ParentIndex " + ent.ParentIndex.ToString() + " was out of range!"); }
                        }
                    }
                    if (ent.Parent != null)
                    {
                        ent.Parent.NumChildren++;
                    }
                }
            }

            foreach (var mf in mapdict.Values)//assign ChildDepth values and InteriorInfo to entities for later use
            {
                foreach (var ent in mf.AllEntities)
                {
                    int depth = 0;
                    var pent = ent.Parent;
                    while (pent != null)
                    {
                        depth++;
                        pent.ChildDepth = Math.Max(pent.ChildDepth, depth);
                        pent = pent.Parent;
                    }

                    VideInterior interior;
                    if (intdict.TryGetValue(ent.Name, out interior))
                    {
                        ent.InteriorInfo = interior;
                    }
                }
            }

            foreach (var mf in mapdict.Values) //process entities in map files, and assign new indices
            {
                mf.ProcessEntities();
            }

            foreach (var mf in mapdict.Values) //assign map file parent names
            {
                var parent = mf.Parent;
                if (parent != null)
                {
                    mf.ParentName = (parent.AllLodEntities.Count > 0) ? parent.Name + "_lod" : parent.Name;
                }
            }

            foreach (var mf in mapdict.Values)
            {
                if ((mf.Parent != null) && (mf.AllLodEntities.Count > 0))
                {
                    errorlog.AppendLine(mf.Name + " contains LOD entities, but also has a parent OPL. This is not supported and may result in problems!");
                }

                foreach (var ent in mf.AllEntities)
                {
                    ent.PosX += offsetx; //offset entity positions
                    ent.PosY += offsety;
                    ent.PosZ += offsetz;

                    if (ent.Parent != null) //assign new parent indices
                    {
                        ent.NewParentIndex = ent.Parent.NewIndex;
                    }
                }
                foreach (var ent in mf.SlodEntities) //set SLOD entity properties
                {
                    ent.LODLevel = "LODTYPES_DEPTH_SLOD1";
                    ent.LODDist = slodloddist;
                    ent.ChildLODDist = lodloddist;
                    ent.Flags = 1572888;
                }
                foreach (var ent in mf.LodEntities) //set LOD entity properties
                {
                    ent.LODLevel = "LODTYPES_DEPTH_LOD";
                    ent.LODDist = (ent.ParentIndex < 0) ? slodloddist : lodloddist;
                    ent.ChildLODDist = hdloddist;
                    ent.Flags = 1572864;
                }
                foreach (var ent in mf.AllHdEntities)
                {
                    if (ent.ParentIndex >= 0) //set HD entity properties
                    {
                        ent.LODLevel = "LODTYPES_DEPTH_HD";
                        ent.LODDist = hdloddist;
                        ent.Flags = 1572872;
                    }
                    else //set ORPHANHD properties
                    {
                        ent.LODLevel = "LODTYPES_DEPTH_ORPHANHD";
                        ent.Flags = 1572872;
                    }
                }
                foreach (var ent in mf.AllEntities)
                {
                    if (ent.InteriorInfo != null)
                    {
                        if (ent.ParentIndex >= 0)
                        {
                            errorlog.AppendLine("Entity " + ent.Name + " in " + mf.Name + " was marked as an interior, but its parentIndex is " + ent.ParentIndex.ToString() + ". This shouldn't happen, LODs may be broken!");
                        }
                        ent.LODLevel = "LODTYPES_DEPTH_HD"; //milo entities should be HD, but they were processed as orphan.
                        ent.LODDist = ent.InteriorInfo.LodDist;
                        ent.ChildLODDist = 0;
                        ent.Flags = 1572872;
                    }
                }

                foreach (var cg in mf.CarGenerators)
                {
                    cg.PosX += offsetx; //offset cargen positions
                    cg.PosY += offsety;
                    cg.PosZ += offsetz;
                    cg.Flags = cargenflags;
                }
            }

            foreach (var mf in mapdict.Values)
            {
                try
                {
                    MapFile lodmf = null;
                    MapFile hdmf = null;
                    MapFile milomf = null;

                    if (mf.AllLodEntities.Count > 0)
                    {
                        lodmf = new MapFile();
                        lodmf.Name = mf.Name + "_lod";
                        lodmf.ParentName = mf.ParentName;
                        lodmf.AllEntities = mf.AllLodEntities;
                        lodmf.CalcExtents(lodloddist);
                        lodmf.Flags = 2;
                        lodmf.ContentFlags = (mf.SlodEntities.Count > 0) ? 18 : 2;
                        lodmf.Save(outputpath);
                    }
                    if (mf.AllHdEntities.Count > 0)
                    {
                        hdmf = new MapFile();
                        hdmf.Name = mf.Name;
                        hdmf.ParentName = lodmf?.Name ?? mf.ParentName;
                        hdmf.AllEntities = mf.AllHdEntities;
                        hdmf.CalcExtents(hdloddist);
                        hdmf.Flags = 0;
                        hdmf.ContentFlags = (mf.HdEntities.Count > 0) ? 1 : 0;
                        hdmf.CarGenerators = cargens ? mf.CarGenerators : new List<CarGen>();
                        hdmf.Save(outputpath);
                    }
                    foreach (var intent in mf.InteriorEntities)
                    {
                        milomf = new MapFile();
                        milomf.Name = mf.Name + "_" + intent.Name + "_milo_";
                        milomf.ParentName = string.Empty;
                        milomf.AllEntities.Add(intent);
                        milomf.CalcExtents((intent.LODDist) > 0 ? intent.LODDist : 100f);
                        milomf.Flags = 0;
                        milomf.ContentFlags = 73;
                        milomf.Save(outputpath);
                    }
                }
                catch (Exception ex)
                {
                    string err = "Error saving " + mf.Name + ":\n" + ex.ToString();
                    errorlog.AppendLine(err);
                }
            }




            if (errorlog.Length > 0)
            {
                File.WriteAllText(outputpath + "errorlog.txt", errorlog.ToString());
                MessageBox.Show("Process complete with errors, see errorlog.txt.");
            }
            else
            {
                MessageBox.Show("Process complete.");
            }

        }



        private void InputFolderBrowseButton_Click(object sender, EventArgs e)
        {
            BrowseFolderDialog.SelectedPath = InputFolderTextBox.Text;
            DialogResult res = BrowseFolderDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                InputFolderTextBox.Text = BrowseFolderDialog.SelectedPath;
            }
        }

        private void OutputFolderBrowseButton_Click(object sender, EventArgs e)
        {
            BrowseFolderDialog.SelectedPath = OutputFolderTextBox.Text;
            DialogResult res = BrowseFolderDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                OutputFolderTextBox.Text = BrowseFolderDialog.SelectedPath;
            }
        }


    }




    public class MapFile
    {
        public string Name { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public int Flags { get; set; } = 0;
        public int ContentFlags { get; set; } = 0;


        public MapFile Parent { get; set; } = null;
        public List<MapFile> Children { get; set; } = new List<MapFile>();

        public List<Entity> AllEntities { get; set; } = new List<Entity>();
        public List<Entity> AllLodEntities { get; set; } = new List<Entity>();
        public List<Entity> AllHdEntities { get; set; } = new List<Entity>();
        public List<Entity> OEntities { get; set; } = new List<Entity>();
        public List<Entity> HdEntities { get; set; } = new List<Entity>();
        public List<Entity> LodEntities { get; set; } = new List<Entity>();
        public List<Entity> SlodEntities { get; set; } = new List<Entity>();
        public List<Entity> InteriorEntities { get; set; } = new List<Entity>();

        public List<CarGen> CarGenerators { get; set; } = new List<CarGen>();

        public float EntitiesExtentsMinX { get; set; }
        public float EntitiesExtentsMinY { get; set; }
        public float EntitiesExtentsMinZ { get; set; }
        public float EntitiesExtentsMaxX { get; set; }
        public float EntitiesExtentsMaxY { get; set; }
        public float EntitiesExtentsMaxZ { get; set; }
        public float StreamingExtentsMinX { get; set; }
        public float StreamingExtentsMinY { get; set; }
        public float StreamingExtentsMinZ { get; set; }
        public float StreamingExtentsMaxX { get; set; }
        public float StreamingExtentsMaxY { get; set; }
        public float StreamingExtentsMaxZ { get; set; }


        public void CalcExtents(float loddist)
        {

            float eeminx = -8192, eeminy = -8192, eeminz = -2048;
            float eemaxx = +8192, eemaxy = +8192, eemaxz = +2048;
            float seminx = -8192, seminy = -8192, seminz = -2048;
            float semaxx = +8192, semaxy = +8192, semaxz = +2048;

            eeminx = float.MaxValue; eeminy = float.MaxValue; eeminz = float.MaxValue;
            eemaxx = float.MinValue; eemaxy = float.MinValue; eemaxz = float.MinValue;
            seminx = float.MaxValue; seminy = float.MaxValue; seminz = float.MaxValue;
            semaxx = float.MinValue; semaxy = float.MinValue; semaxz = float.MinValue;
            foreach (var ent in AllEntities)
            {
                //this should really use entity bounding box, but it's not available here.....
                float sedist = loddist * 2.0f;
                eeminx = Math.Min(eeminx, ent.PosX - loddist);
                eeminy = Math.Min(eeminy, ent.PosY - loddist);
                eeminz = Math.Min(eeminz, ent.PosZ - loddist);
                eemaxx = Math.Max(eemaxx, ent.PosX + loddist);
                eemaxy = Math.Max(eemaxy, ent.PosY + loddist);
                eemaxz = Math.Max(eemaxz, ent.PosZ + loddist);
                seminx = Math.Min(seminx, ent.PosX - sedist);
                seminy = Math.Min(seminy, ent.PosY - sedist);
                seminz = Math.Min(seminz, ent.PosZ - sedist);
                semaxx = Math.Max(semaxx, ent.PosX + sedist);
                semaxy = Math.Max(semaxy, ent.PosY + sedist);
                semaxz = Math.Max(semaxz, ent.PosZ + sedist);
            }

            EntitiesExtentsMinX = eeminx;
            EntitiesExtentsMinY = eeminy;
            EntitiesExtentsMinZ = eeminz;
            EntitiesExtentsMaxX = eemaxx;
            EntitiesExtentsMaxY = eemaxy;
            EntitiesExtentsMaxZ = eemaxz;
            StreamingExtentsMinX = seminx;
            StreamingExtentsMinY = seminy;
            StreamingExtentsMinZ = seminz;
            StreamingExtentsMaxX = semaxx;
            StreamingExtentsMaxY = semaxy;
            StreamingExtentsMaxZ = semaxz;

        }


        public void Load(string oplfile)
        {
            Name = Path.GetFileNameWithoutExtension(oplfile).ToLowerInvariant();

            var lines = File.ReadAllLines(oplfile);
            bool ininst = false;
            bool incars = false;
            foreach (var line in lines)
            {
                string tline = line.Trim();
                if (string.IsNullOrEmpty(tline)) continue; //blank line
                if (tline.StartsWith("#")) continue; //commented out
                if (tline.StartsWith("version")) continue;
                if (tline.StartsWith("inst")) { ininst = true; continue; } //start of instances
                if (tline.StartsWith("cars")) { incars = true; continue; } //start of car gens
                if (tline.StartsWith("end") && (ininst || incars))
                {
                    ininst = false;
                    incars = false;
                    continue;
                }
                if (ininst)
                {
                    Entity ent = new Entity(tline);
                    AllEntities.Add(ent);
                }
                if (incars)
                {
                    CarGen cgen = new CarGen(tline);
                    CarGenerators.Add(cgen);
                }
            }

            //ProcessEntities();
        }



        public void ProcessEntities()
        {
            foreach (var ent in AllEntities)
            {
                var depth = ent.ChildDepth;

                if (depth == 2)
                {
                    SlodEntities.Add(ent);
                }
                else if (depth == 1)
                {
                    LodEntities.Add(ent);
                }
                else if (depth == 0)
                {
                    if (ent.InteriorInfo != null)
                    {
                        InteriorEntities.Add(ent);
                    }
                    else if (ent.Parent != null)
                    {
                        HdEntities.Add(ent);
                    }
                    else
                    {
                        OEntities.Add(ent);
                    }
                }
                else
                {
                    SlodEntities.Add(ent);//not really correct here, but what else to do? slod2/3?
                }





                //if (ent.Name.StartsWith("slod"))
                //{
                //    SlodEntities.Add(ent);
                //}
                //else if (ent.Name.StartsWith("lod"))
                //{
                //    LodEntities.Add(ent);
                //}
                //else
                //{
                //    if (ent.ParentIndex < 0)
                //    {
                //        OEntities.Add(ent);
                //    }
                //    else
                //    {
                //        HdEntities.Add(ent);
                //    }
                //}
            }


            AllLodEntities.Clear();
            AllLodEntities.AddRange(SlodEntities);
            AllLodEntities.AddRange(LodEntities);

            AllHdEntities.Clear();
            AllHdEntities.AddRange(HdEntities);
            AllHdEntities.AddRange(OEntities);


            for (int i = 0; i < AllLodEntities.Count; i++)
            {
                AllLodEntities[i].NewIndex = i;
            }
            for (int i = 0; i < AllHdEntities.Count; i++)
            {
                AllHdEntities[i].NewIndex = i;
            }
        }



        public void Save(string outputpath)
        {

            string filename = outputpath + Name + ".ymap.xml";

            string tname = Name;
            int idx = 0;
            while (File.Exists(filename))
            {
                Name = tname + idx.ToString();
                filename = outputpath + Name + ".ymap.xml";
                idx++;
            }

            //if (File.Exists(filename))
            //{
            //    if (MessageBox.Show("The file " + filename + " already exists. Do you want to overwrite it?", "Confirm file overwrite", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //    {
            //        return; //skip this one...
            //    }
            //}

            StringBuilder sb = new StringBuilder();
            List<string> physDicts = new List<string>();

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<CMapData>");
            sb.AppendLine(" <name>" + Name + "</name>");
            if (!string.IsNullOrEmpty(ParentName))
            {
                sb.AppendLine(" <parent>" + ParentName + "</parent>");
            }
            else
            {
                sb.AppendLine(" <parent/>");
            }
            sb.AppendLine(" <flags value=\"" + Flags.ToString() + "\"/>");
            sb.AppendLine(" <contentFlags value=\"" + ContentFlags.ToString() + "\"/>");
            sb.AppendLine(" <streamingExtentsMin x=\"" + FStr(StreamingExtentsMinX) + "\" y=\"" + FStr(StreamingExtentsMinY) + "\" z=\"" + FStr(StreamingExtentsMinZ) + "\"/>");
            sb.AppendLine(" <streamingExtentsMax x=\"" + FStr(StreamingExtentsMaxX) + "\" y=\"" + FStr(StreamingExtentsMaxY) + "\" z=\"" + FStr(StreamingExtentsMaxZ) + "\"/>");
            sb.AppendLine(" <entitiesExtentsMin x=\"" + FStr(EntitiesExtentsMinX) + "\" y=\"" + FStr(EntitiesExtentsMinY) + "\" z=\"" + FStr(EntitiesExtentsMinZ) + "\"/>");
            sb.AppendLine(" <entitiesExtentsMax x=\"" + FStr(EntitiesExtentsMaxX) + "\" y=\"" + FStr(EntitiesExtentsMaxY) + "\" z=\"" + FStr(EntitiesExtentsMaxZ) + "\"/>");
            sb.AppendLine(" <entities>");
            foreach (var ent in AllEntities)
            {
                bool ismlo = ent.InteriorInfo != null;
                if (ismlo)
                {
                    sb.AppendLine("  <Item type=\"CMloInstanceDef\">");
                    physDicts.Add(ent.Name);
                }
                else
                {
                    sb.AppendLine("  <Item type=\"CEntityDef\">");
                }
                sb.AppendLine("   <archetypeName>" + ent.Name + "</archetypeName>");
                sb.AppendLine("   <flags value=\"" + ent.Flags.ToString() + "\"/>");
                sb.AppendLine("   <guid value=\"0\"/>");
                sb.AppendLine("   <position x=\"" + FStr(ent.PosX) + "\" y=\"" + FStr(ent.PosY) + "\" z=\"" + FStr(ent.PosZ) + "\"/>");
                sb.AppendLine("   <rotation x=\"" + FStr(ent.RotX) + "\" y=\"" + FStr(ent.RotY) + "\" z=\"" + FStr(ent.RotZ) + "\" w=\"" + FStr(ent.RotW) + "\"/>");
                sb.AppendLine("   <scaleXY value=\"1.0\"/>");
                sb.AppendLine("   <scaleZ value=\"1.0\"/>");
                sb.AppendLine("   <parentIndex value=\"" + ent.NewParentIndex.ToString() + "\"/>");
                sb.AppendLine("   <lodDist value=\"" + FStr(ent.LODDist) + "\"/>");
                sb.AppendLine("   <childLodDist value=\"" + FStr(ent.ChildLODDist) + "\"/>");
                sb.AppendLine("   <lodLevel>" + ent.LODLevel + "</lodLevel>");
                sb.AppendLine("   <numChildren value=\"" + ent.NumChildren.ToString() + "\"/>");
                sb.AppendLine("   <priorityLevel>PRI_REQUIRED</priorityLevel>");
                sb.AppendLine("   <extensions/>");
                sb.AppendLine("   <ambientOcclusionMultiplier value=\"255\"/>");
                sb.AppendLine("   <artificialAmbientOcclusion value=\"255\"/>");
                sb.AppendLine("   <tintValue value=\"0\"/>");
                if (ismlo)
                {
                    sb.AppendLine("   <groupId value=\"0\"/>");
                    sb.AppendLine("   <floorId value=\"0\"/>");
                    sb.AppendLine("   <defaultEntitySets/>");
                    sb.AppendLine("   <numExitPortals value=\"" + ent.InteriorInfo.ExitPortalCount.ToString() + "\"/>");
                    sb.AppendLine("   <MLOInstflags value=\"2\"/>");
                }
                sb.AppendLine("  </Item>");
            }
            sb.AppendLine(" </entities>");
            sb.AppendLine(" <containerLods/>");
            sb.AppendLine(" <boxOccluders/>");
            sb.AppendLine(" <occludeModels/>");
            if (physDicts.Count > 0)
            {
                sb.AppendLine(" <physicsDictionaries>");
                foreach (var physDict in physDicts)
                {
                    sb.AppendLine("  <Item>" + physDict + "</Item>");
                }
                sb.AppendLine(" </physicsDictionaries>");
            }
            else
            {
                sb.AppendLine(" <physicsDictionaries/>");
            }
            sb.AppendLine(" <instancedData>");
            sb.AppendLine("  <ImapLink/>");
            sb.AppendLine("  <PropInstanceList/>");
            sb.AppendLine("  <GrassInstanceList/>");
            sb.AppendLine(" </instancedData>");
            sb.AppendLine(" <timeCycleModifiers/>");
            if (CarGenerators.Count > 0)
            {
                sb.AppendLine(" <carGenerators>");
                foreach (var cg in CarGenerators)
                {
                    sb.AppendLine("  <Item>");
                    sb.AppendLine("   <position x=\"" + FStr(cg.PosX) + "\" y=\"" + FStr(cg.PosY) + "\" z=\"" + FStr(cg.PosZ) + "\"/>");
                    sb.AppendLine("   <orientX value=\"" + FStr(cg.RotX) + "\"/>");
                    sb.AppendLine("   <orientY value=\"" + FStr(cg.RotY) + "\"/>");
                    sb.AppendLine("   <perpendicularLength value=\"" + FStr(cg.Length) + "\"/>");
                    //if (!string.IsNullOrEmpty(cg.ModelName))
                    //{
                    //    sb.AppendLine("   <carModel>" + cg.ModelName + "</carModel>"); //need to do translation!
                    //}
                    //else
                    {
                        sb.AppendLine("   <carModel/>");
                    }
                    sb.AppendLine("   <flags value=\"" + cg.Flags.ToString() + "\"/>");
                    sb.AppendLine("   <bodyColorRemap1 value=\"" + cg.CarColor1.ToString() + "\"/>");
                    sb.AppendLine("   <bodyColorRemap2 value=\"" + cg.CarColor2.ToString() + "\"/>");
                    sb.AppendLine("   <bodyColorRemap3 value=\"" + cg.CarColor3.ToString() + "\"/>");
                    sb.AppendLine("   <bodyColorRemap4 value=\"" + cg.SpecularColor.ToString() + "\"/>");
                    sb.AppendLine("   <popGroup/>");
                    sb.AppendLine("   <livery value=\"-1\"/>");
                    sb.AppendLine("  </Item>");
                }
                sb.AppendLine(" </carGenerators>");
            }
            else
            {
                sb.AppendLine(" <carGenerators/>");
            }
            sb.AppendLine(" <LODLightsSOA>");
            sb.AppendLine("  <direction/>");
            sb.AppendLine("  <falloff/>");
            sb.AppendLine("  <falloffExponent/>");
            sb.AppendLine("  <timeAndStateFlags/>");
            sb.AppendLine("  <hash/>");
            sb.AppendLine("  <coneInnerAngle/>");
            sb.AppendLine("  <coneOuterAngleOrCapExt/>");
            sb.AppendLine("  <coronaIntensity/>");
            sb.AppendLine(" </LODLightsSOA>");
            sb.AppendLine(" <DistantLODLightsSOA>");
            sb.AppendLine("  <position/>");
            sb.AppendLine("  <RGBI/>");
            sb.AppendLine("  <numStreetLights value=\"0\"/>");
            sb.AppendLine("  <category value=\"0\"/>");
            sb.AppendLine(" </DistantLODLightsSOA>");
            sb.AppendLine(" <block>");
            sb.AppendLine("  <version value=\"0\"/>");
            sb.AppendLine("  <flags value=\"0\"/>");
            sb.AppendLine("  <name>" + Name + "</name>");
            sb.AppendLine("  <exportedBy>VOPL by dexyfex</exportedBy>");
            sb.AppendLine("  <owner/>");
            sb.AppendLine("  <time>" + DateTime.UtcNow.ToString("dd MMMM yyyy HH:mm") + "</time>");
            sb.AppendLine(" </block>");
            sb.AppendLine("</CMapData>");

            File.WriteAllText(filename, sb.ToString());
        }




        public string FStr(float f)
        {
            return f.ToString(CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return Name;
        }
    }


    public class Entity
    {
        public string OPLStr { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        public float RotW { get; set; }
        public string Name { get; set; }
        public int Flags { get; set; }
        public int ParentIndex { get; set; }
        public int Unk1 { get; set; }
        public float Unk2 { get; set; }

        public int ChildDepth { get; set; } = 0;
        public int NewIndex { get; set; } = -1;
        public int NewParentIndex { get; set; } = -1;
        public Entity Parent { get; set; } = null;
        public string LODLevel { get; set; } = string.Empty;
        public float LODDist { get; set; } = -1;
        public float ChildLODDist { get; set; } = 0;
        public int NumChildren { get; set; } = 0;
        public VideInterior InteriorInfo { get; set; }

        public Entity(string oplstr)
        {
            OPLStr = oplstr;
            string[] parts = oplstr.Split(',');

            PosX = float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
            PosY = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
            PosZ = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
            RotX = float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
            RotY = float.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);
            RotZ = float.Parse(parts[5].Trim(), CultureInfo.InvariantCulture);
            RotW = float.Parse(parts[6].Trim(), CultureInfo.InvariantCulture);
            Name = parts[7].Trim().ToLowerInvariant();
            Flags = int.Parse(parts[8].Trim(), CultureInfo.InvariantCulture);
            ParentIndex = int.Parse(parts[9].Trim(), CultureInfo.InvariantCulture);
            Unk1 = int.Parse(parts[10].Trim(), CultureInfo.InvariantCulture);
            Unk2 = float.Parse(parts[11].Trim(), CultureInfo.InvariantCulture);

            if (Name.StartsWith("hash:"))
            {
                string[] hparts = Name.Split(':');
                uint hash;
                if (uint.TryParse(hparts[1].Trim(), out hash))
                {
                    string str = JenkIndex.TryGetString(hash);
                    if (!string.IsNullOrEmpty(str))
                    {
                        Name = str.ToLowerInvariant();
                    }
                    else
                    {
                        Name = "hash_" + hash.ToString("X").ToLowerInvariant();
                    }
                }
            }
        }


        public override string ToString()
        {
            return Name;
        }
    }

    public class CarGen
    {
        public string OPLStr { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float Length { get; set; }
        public string ModelName { get; set; }
        public int CarColor1 { get; set; }
        public int CarColor2 { get; set; }
        public int CarColor3 { get; set; }
        public int SpecularColor { get; set; }
        public uint Flags { get; set; }
        public int Alarm { get; set; }
        public int Unk2 { get; set; }

        public CarGen(string oplstr)
        {
            OPLStr = oplstr;
            string[] parts = oplstr.Split(',');

            PosX = float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
            PosY = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
            PosZ = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
            RotX = float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
            RotY = float.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);
            Length = float.Parse(parts[5].Trim(), CultureInfo.InvariantCulture);
            ModelName = parts[6].Trim().ToLowerInvariant();
            CarColor1 = int.Parse(parts[7].Trim(), CultureInfo.InvariantCulture);
            CarColor2 = int.Parse(parts[8].Trim(), CultureInfo.InvariantCulture);
            CarColor3 = int.Parse(parts[9].Trim(), CultureInfo.InvariantCulture);
            SpecularColor = int.Parse(parts[10].Trim(), CultureInfo.InvariantCulture);
            Flags = uint.Parse(parts[11].Trim(), CultureInfo.InvariantCulture);
            Alarm = int.Parse(parts[12].Trim(), CultureInfo.InvariantCulture);
            Unk2 = int.Parse(parts[13].Trim(), CultureInfo.InvariantCulture);

            //if (model.StartsWith("hash:"))
            //{
            //    ModelName = model.Substring(5);
            //    if (ModelName == "0") ModelName = string.Empty;
            //}
            //else
            //{
            //    ModelName = model;
            //}
            if (ModelName.StartsWith("hash:"))
            {
                string[] hparts = ModelName.Split(':');
                uint hash;
                if (uint.TryParse(hparts[1].Trim(), out hash))
                {
                    string str = JenkIndex.TryGetString(hash);
                    if (!string.IsNullOrEmpty(str))
                    {
                        ModelName = str.ToLowerInvariant();
                    }
                    else
                    {
                        ModelName = "hash_" + hash.ToString("X").ToLowerInvariant();
                    }
                }
            }


        }

        public override string ToString()
        {
            return ModelName + ": " + Flags.ToString() + ": " + Length.ToString();
        }
    }





    public class VideFile
    {
        public List<VideInterior> Interiors { get; set; } = new List<VideInterior>();

        public void Load(string oplfile)
        {
            var lines = File.ReadAllLines(oplfile);
            foreach (var line in lines)
            {
                string tline = line.Trim();
                if (string.IsNullOrEmpty(tline)) continue; //blank line
                VideInterior interior = new VideInterior(tline);
                Interiors.Add(interior);
            }
        }
    }

    public class VideInterior
    {
        public string Name { get; set; }
        public float LodDist { get; set; }
        public int ExitPortalCount { get; set; }


        public VideInterior(string videstr)
        {
            string[] parts = videstr.Split(',');
            Name = parts[0].Trim().ToLowerInvariant();
            LodDist = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
            ExitPortalCount = int.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
        }

    }

}
