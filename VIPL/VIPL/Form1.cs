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

namespace VIPL
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
            string[] iplfiles = Directory.GetFiles(inputpath, "*.ipl", SearchOption.TopDirectoryOnly);
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

            foreach (var iplfile in iplfiles) //load ipl files...
            {
                try
                {
                    MapFile mf = new MapFile();
                    mf.Load(iplfile);
                    mapdict[mf.Name] = mf;
                }
                catch (Exception ex)
                {
                    string err = "Error loading " + iplfile + ":\n" + ex.ToString();
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
                        mf.ParentName = (parent.AllLodEntities.Count > 0) ? parent.Name + "_lod" : parent.Name;
                        parent.Children.Add(mf);
                    }
                    else
                    { errorlog.AppendLine("Couldn't find parent " + pname + ".ipl for " + mf.Name + "."); }
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
                }
            }

            foreach (var mf in mapdict.Values)
            {
                if ((mf.Parent != null) && (mf.AllLodEntities.Count > 0))
                {
                    errorlog.AppendLine(mf.Name + " contains LOD entities, but also has a parent IPL. This is not supported and may result in problems!");
                }

                foreach (var ent in mf.AllEntities)
                {
                    ent.PosX += offsetx; //offset entity positions
                    ent.PosY += offsety;
                    ent.PosZ += offsetz;

                    if (ent.Parent != null) //assign new parent indices and numchildren
                    {
                        ent.NewParentIndex = ent.Parent.NewIndex;
                        ent.Parent.NumChildren++;
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


        public void Load(string iplfile)
        {
            Name = Path.GetFileNameWithoutExtension(iplfile).ToLowerInvariant();

            var lines = File.ReadAllLines(iplfile);
            bool ininst = false;
            bool incars = false;
            foreach (var line in lines)
            {
                string tline = line.Trim();
                if (string.IsNullOrEmpty(tline)) continue; //blank line
                if (tline.StartsWith("#")) continue; //commented out
                //if (tline.StartsWith("version")) continue;
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
                    if (ent.Name.StartsWith("slod"))
                    {
                        SlodEntities.Add(ent);
                    }
                    else if (ent.Name.StartsWith("lod"))
                    {
                        LodEntities.Add(ent);
                    }
                    else
                    {
                        if (ent.ParentIndex < 0)
                        {
                            OEntities.Add(ent);
                        }
                        else
                        {
                            HdEntities.Add(ent);
                        }
                    }
                }
                if (incars)
                {
                    CarGen cgen = new CarGen(tline);
                    CarGenerators.Add(cgen);
                }
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


            if (File.Exists(filename))
            {
                if (MessageBox.Show("The file " + filename + " already exists. Do you want to overwrite it?", "Confirm file overwrite", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return; //skip this one...
                }
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<CMapData>");
            sb.AppendLine(" <name>" + Name + "</name>");
            sb.AppendLine(" <parent>" + ParentName + "</parent>");
            sb.AppendLine(" <flags value=\"" + Flags.ToString() + "\"/>");
            sb.AppendLine(" <contentFlags value=\"" + ContentFlags.ToString() + "\"/>");
            sb.AppendLine(" <streamingExtentsMin x=\"" + FStr(StreamingExtentsMinX) + "\" y=\"" + FStr(StreamingExtentsMinY) + "\" z=\"" + FStr(StreamingExtentsMinZ) + "\"/>");
            sb.AppendLine(" <streamingExtentsMax x=\"" + FStr(StreamingExtentsMaxX) + "\" y=\"" + FStr(StreamingExtentsMaxY) + "\" z=\"" + FStr(StreamingExtentsMaxZ) + "\"/>");
            sb.AppendLine(" <entitiesExtentsMin x=\"" + FStr(EntitiesExtentsMinX) + "\" y=\"" + FStr(EntitiesExtentsMinY) + "\" z=\"" + FStr(EntitiesExtentsMinZ) + "\"/>");
            sb.AppendLine(" <entitiesExtentsMax x=\"" + FStr(EntitiesExtentsMaxX) + "\" y=\"" + FStr(EntitiesExtentsMaxY) + "\" z=\"" + FStr(EntitiesExtentsMaxZ) + "\"/>");
            sb.AppendLine(" <entities>");
            foreach (var ent in AllEntities)
            {
                sb.AppendLine("  <Item type=\"CEntityDef\">");
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
                sb.AppendLine("  </Item>");
            }
            sb.AppendLine(" </entities>");
            sb.AppendLine(" <containerLods/>");
            sb.AppendLine(" <boxOccluders/>");
            sb.AppendLine(" <occludeModels/>");
            sb.AppendLine(" <physicsDictionaries/>");
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
                    sb.AppendLine("   <bodyColorRemap4 value=\"" + cg.CarColor4.ToString() + "\"/>");
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
            sb.AppendLine("  <exportedBy>VIPL by dexyfex</exportedBy>");
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
        public string IPLStr { get; set; }

        public int ID { get; set; }
        public string Name { get; set; }
        public int Interior { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        public float RotW { get; set; }
        public int ParentIndex { get; set; }

        public int NewIndex { get; set; } = -1;
        public int NewParentIndex { get; set; } = -1;
        public Entity Parent { get; set; } = null;
        public string LODLevel { get; set; } = string.Empty;
        public float LODDist { get; set; } = -1;
        public float ChildLODDist { get; set; } = 0;
        public int NumChildren { get; set; } = 0;
        public int Flags { get; set; } = 0;

        public Entity(string iplstr)
        {
            IPLStr = iplstr;
            string[] parts = iplstr.Split(',');

            ID = int.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
            Name = parts[1].Trim().ToLowerInvariant();
            Interior = int.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
            PosX = float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
            PosY = float.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);
            PosZ = float.Parse(parts[5].Trim(), CultureInfo.InvariantCulture);
            RotX = float.Parse(parts[6].Trim(), CultureInfo.InvariantCulture);
            RotY = float.Parse(parts[7].Trim(), CultureInfo.InvariantCulture);
            RotZ = float.Parse(parts[8].Trim(), CultureInfo.InvariantCulture);
            RotW = float.Parse(parts[9].Trim(), CultureInfo.InvariantCulture);
            ParentIndex = int.Parse(parts[10].Trim(), CultureInfo.InvariantCulture);

        }


        public override string ToString()
        {
            return ID.ToString() + ", " + Name;
        }
    }

    public class CarGen
    {
        public string IPLStr { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float Angle { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float Length { get; set; }
        public int CarID { get; set; } = -1;
        public int CarColor1 { get; set; } = -1;
        public int CarColor2 { get; set; } = -1;
        public int CarColor3 { get; set; } = -1;
        public int CarColor4 { get; set; } = -1;
        public uint Flags { get; set; }
        public int ForceSpawn { get; set; }
        public int Alarm { get; set; }
        public int DoorLock { get; set; }
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }

        public CarGen(string iplstr)
        {
            IPLStr = iplstr;
            string[] parts = iplstr.Split(',');


            PosX = float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
            PosY = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
            PosZ = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
            Angle = float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
            CarID = int.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);
            CarColor1 = int.Parse(parts[5].Trim(), CultureInfo.InvariantCulture);
            CarColor2 = int.Parse(parts[6].Trim(), CultureInfo.InvariantCulture);
            ForceSpawn = int.Parse(parts[7].Trim(), CultureInfo.InvariantCulture);
            Alarm = int.Parse(parts[8].Trim(), CultureInfo.InvariantCulture);
            DoorLock = int.Parse(parts[9].Trim(), CultureInfo.InvariantCulture);
            Unknown1 = int.Parse(parts[10].Trim(), CultureInfo.InvariantCulture);
            //Unknown2 = int.Parse(parts[11].Trim(), CultureInfo.InvariantCulture);

            Length = 3.5f;
            //PosZ += Length * 0.4f;
            RotX = (float)Math.Sin(Angle) * Length;
            RotY = (float)Math.Cos(Angle) * Length;
        }

        public override string ToString()
        {
            return CarID.ToString() + ": " + Flags.ToString() + ": " + Length.ToString();
        }
    }



}
