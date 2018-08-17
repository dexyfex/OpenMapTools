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

namespace VIDE
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            IDEFormatComboBox.SelectedIndex = 0;
        }


        public static List<string> WarningsList { get; } = new List<string>();


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
            string[] idefiles = Directory.GetFiles(inputpath, "*.ide", SearchOption.TopDirectoryOnly);
            string ideformat = IDEFormatComboBox.Text;
            bool createvoplfiles = VOPLInteriorsCheckBox.Checked;
            bool creategtxd = GtxdMetaCheckBox.Checked;
            bool separatelods = LODsCheckBox.Checked;
            bool idephysdicts = IDEPhysDictNamesCheckBox.Checked;
            bool ideclipdicts = IDEClipDictsCheckBox.Checked;

            if (!outputpath.EndsWith("\\"))
            {
                outputpath = outputpath + "\\";
            }

            StringBuilder errorlog = new StringBuilder();


            Dictionary<string, TypeFile> typesdict = new Dictionary<string, TypeFile>();


            string stringsfile = inputpath + (inputpath.EndsWith("\\") ? "" : "\\") + "strings.txt";
            if (File.Exists(stringsfile))
            {
                string[] strings = File.ReadAllLines(stringsfile);
                foreach (var str in strings)
                {
                    JenkIndex.Ensure(str);
                }
            }


            foreach (var idefile in idefiles) //load ide files...
            {
                try
                {
                    TypeFile tf = new TypeFile();
                    tf.Load(idefile);
                    typesdict[tf.Name] = tf;
                }
                catch (Exception ex)
                {
                    string err = "Error loading " + idefile + ":\n" + ex.ToString();
                    errorlog.AppendLine(err);
                }
            }

            foreach (var typfile in typesdict.Values) //make sure phys and clip dicts are correct...
            {
                foreach (var typ in typfile.Archetypes)
                {
                    if (idephysdicts && string.IsNullOrEmpty(typ.OutPhysicsDictionary))
                    {
                        typ.OutPhysicsDictionary = typfile.Name;
                    }
                    if (!ideclipdicts && !string.IsNullOrEmpty(typ.OutClipDictionary))
                    {
                        typ.OutClipDictionary = null;
                    }
                }
            }

            if (separatelods)
            {
                List<BaseArchetype> hdarchs = new List<BaseArchetype>();
                List<BaseArchetype> lodarchs = new List<BaseArchetype>();
                List<TypeFile> lodfiles = new List<TypeFile>();
                foreach (var typfile in typesdict.Values)
                {
                    hdarchs.Clear();
                    lodarchs.Clear();
                    foreach (var typ in typfile.Archetypes)
                    {
                        var namel = typ.Name.ToLowerInvariant();
                        if (namel.StartsWith("lod") || namel.StartsWith("slod"))
                        {
                            lodarchs.Add(typ);
                        }
                        else
                        {
                            hdarchs.Add(typ);
                        }
                    }
                    if (lodarchs.Count > 0)
                    {

                        TypeFile lodtf = new TypeFile();
                        lodtf.Name = typfile.Name + "_lod";
                        lodtf.Archetypes.AddRange(lodarchs);
                        if (typesdict.ContainsKey(lodtf.Name))
                        {
                            errorlog.AppendLine(lodtf.Name + " already exists - LODs not separated for " + typfile.Name + ".");
                            continue;
                        }
                        lodfiles.Add(lodtf);

                        typfile.ArchetypeDict.Clear();
                        typfile.Archetypes.Clear();
                        typfile.Archetypes.AddRange(hdarchs); //switch original typefile archetypes to HD only ones
                    }
                }
                foreach (var lodtf in lodfiles)
                {
                    typesdict[lodtf.Name] = lodtf;
                }
            }

            foreach (var typfile in typesdict.Values)
            {
                try
                {
                    typfile.Save(outputpath, createvoplfiles);
                }
                catch (Exception ex)
                {
                    string err = "Error saving " + typfile.Name + ":\n" + ex.ToString();
                    errorlog.AppendLine(err);
                }
            }

            if (creategtxd)
            {
                try
                {
                    Dictionary<string, TextureDependency> allgtxds = new Dictionary<string, TextureDependency>();
                    foreach (var ytypfile in typesdict.Values)
                    {
                        foreach (var txd in ytypfile.TextureDependencies)
                        {
                            allgtxds[txd.ToString()] = txd;
                        }
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sb.AppendLine("<CMapParentTxds>");
                    sb.AppendLine(" <txdRelationships>");
                    foreach (var txd in allgtxds.Values)
                    {
                        sb.AppendLine("  <Item>");
                        sb.AppendLine("   <parent>" + txd.Parent + "</parent>");
                        sb.AppendLine("   <child>" + txd.Child + "</child>");
                        sb.AppendLine("  </Item>");
                    }
                    sb.AppendLine(" </txdRelationships>");
                    sb.AppendLine("</CMapParentTxds>");

                    string filename = outputpath + "gtxd.meta";
                    File.WriteAllText(filename, sb.ToString());
                }
                catch (Exception ex)
                {
                    string err = "Error saving gtxd.meta:\n" + ex.ToString();
                    errorlog.AppendLine(err);
                }
            }


            if (WarningsList.Count > 0)
            {
                File.WriteAllLines(outputpath + "warninglog.txt", WarningsList.ToArray());
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


    public class TypeFile
    {
        public string Name { get; set; } = string.Empty;

        public List<BaseArchetype> Archetypes { get; set; } = new List<BaseArchetype>();
        public Dictionary<string, BaseArchetype> ArchetypeDict { get; set; } = new Dictionary<string, BaseArchetype>();

        public List<Base2DFX> Effects { get; set; } = new List<Base2DFX>();

        public List<TextureDependency> TextureDependencies { get; set; } = new List<TextureDependency>();

        public static Random Rnd = new Random();


        public void Load(string idefile)
        {
            Name = Path.GetFileNameWithoutExtension(idefile).ToLowerInvariant();

            var lines = File.ReadAllLines(idefile);
            bool inobjs = false;
            bool intobj = false;
            bool inanim = false;
            bool inmlos = false;
            bool in2dfx = false;
            bool intxdp = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string tline = line.Trim();
                bool insomething = (inobjs || intobj || inanim || inmlos || in2dfx || intxdp);
                if (string.IsNullOrEmpty(tline)) continue; //blank line
                if (tline.StartsWith("#")) continue; //commented out
                //if (tline.StartsWith("version")) continue;
                if (!insomething)
                {
                    if (tline.StartsWith("objs")) { inobjs = true; continue; } //start of objects
                    if (tline.StartsWith("tobj")) { intobj = true; continue; } //start of timed objects
                    if (tline.StartsWith("anim")) { inanim = true; continue; } //start of animated objects
                    if (tline.StartsWith("mlo")) { inmlos = true; continue; } //start of mlo
                    if (tline.StartsWith("2dfx")) { in2dfx = true; continue; } //start of 2D fx
                    if (tline.StartsWith("txdp")) { intxdp = true; continue; } //start of texture dependencies
                }
                if (tline.StartsWith("end") && insomething)
                {
                    inobjs = false;
                    intobj = false;
                    inanim = false;
                    inmlos = false;
                    in2dfx = false;
                    intxdp = false;
                    continue;
                }
                if (inobjs)
                {
                    BaseArchetype arch = new BaseArchetype();
                    arch.Load(tline);
                    Archetypes.Add(arch);
                    ArchetypeDict[arch.Name] = arch;
                }
                if (intobj)
                {
                    TimeArchetype tarch = new TimeArchetype();
                    tarch.Load(tline);
                    Archetypes.Add(tarch);
                    ArchetypeDict[tarch.Name] = tarch;
                }
                if (inanim)
                {
                    BaseArchetype arch = new BaseArchetype();
                    arch.LoadAnim(tline);
                    Archetypes.Add(arch);
                    ArchetypeDict[arch.Name] = arch;
                }
                if (inmlos)
                {
                    MloArchetype march = new MloArchetype();
                    i = march.LoadMLO(lines, i);
                    Archetypes.Add(march);
                    ArchetypeDict[march.Name] = march;
                }
                if (in2dfx)
                {
                    Base2DFX basefx = Base2DFX.Create(tline);
                    Effects.Add(basefx);
                    BaseArchetype ownerArch;
                    if (ArchetypeDict.TryGetValue(basefx.ObjectID, out ownerArch))
                    {
                        ownerArch.AddEffect(basefx);
                    }
                    else
                    { } //couldn't find archetype!
                }
                if (intxdp)
                {
                    TextureDependency txd = new TextureDependency();
                    txd.Load(tline);
                    TextureDependencies.Add(txd);
                }
            }

        }

        public void Save(string outputpath, bool createvoplfiles)
        {
            string filename = outputpath + Name + ".ytyp.xml";

            if (File.Exists(filename))
            {
                if (MessageBox.Show("The file " + filename + " already exists. Do you want to overwrite it?", "Confirm file overwrite", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return; //skip this one...
                }
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<CMapTypes>");
            sb.AppendLine(" <extensions/>");
            sb.AppendLine(" <archetypes>");
            foreach (var arch in Archetypes)
            {
                sb.AppendLine("  <Item type=\"" + arch.OutItemType + "\">");
                arch.SaveXML(sb);
                sb.AppendLine("  </Item>");
            }
            sb.AppendLine(" </archetypes>");
            sb.AppendLine(" <name>" + Name + "</name>");
            sb.AppendLine(" <dependencies/>");
            sb.AppendLine(" <compositeEntityTypes/>");
            sb.AppendLine("</CMapTypes>");

            File.WriteAllText(filename, sb.ToString());


            if (createvoplfiles)
            {
                sb.Clear();
                foreach (var arch in Archetypes)
                {
                    if (arch is MloArchetype)
                    {
                        var mloarch = arch as MloArchetype;
                        sb.AppendLine(arch.Name + ", " + FStr(mloarch.LodDist1) + ", " + IStr(mloarch.ExitPortalCount));
                    }
                }
                if (sb.Length > 0)
                {
                    filename = outputpath + Name + ".vopl.txt";
                    File.WriteAllText(filename, sb.ToString());
                }
            }

        }

        public static string FStr(float f)
        {
            return f.ToString(CultureInfo.InvariantCulture);
        }
        public static string UStr(uint u)
        {
            return u.ToString(CultureInfo.InvariantCulture);
        }
        public static string IStr(int i)
        {
            return i.ToString(CultureInfo.InvariantCulture);
        }

        public static string FixHashes(string namestr)
        {
            if (namestr.StartsWith("hash:"))
            {
                string[] hparts = namestr.Split(':');
                uint hash;
                if (uint.TryParse(hparts[1].Trim(), out hash))
                {
                    string str = JenkIndex.TryGetString(hash);
                    if (!string.IsNullOrEmpty(str))
                    {
                        namestr = str.ToLowerInvariant();
                    }
                    else
                    {
                        namestr = "hash_" + hash.ToString("X").ToLowerInvariant();
                    }
                }
            }
            return namestr;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class BaseArchetype
    {
        public string ObjStr { get; set; }
        public string Name { get; set; }
        public string TxdName { get; set; }
        public float DrawDistance { get; set; }
        public int Flags { get; set; }
        public float Unknown1 { get; set; }
        public float MinX { get; set; }
        public float MinY { get; set; }
        public float MinZ { get; set; }
        public float MaxX { get; set; }
        public float MaxY { get; set; }
        public float MaxZ { get; set; }
        public float SphereX { get; set; }
        public float SphereY { get; set; }
        public float SphereZ { get; set; }
        public float Radius { get; set; }
        public string LODModel { get; set; }

        public List<Base2DFX> InEffects { get; set; }
        public List<BaseExtension> OutExtensions { get; set; }
        public virtual string OutItemType { get { return "CBaseArchetypeDef"; } }
        public virtual string OutAssetType
        {
            get
            {
                return IsDrawableDict ? "ASSET_TYPE_DRAWABLEDICTIONARY" : "ASSET_TYPE_DRAWABLE";
            }
        }
        public int OutSpecialAttribute { get; set; } = 0;
        public int OutFlags { get; set; } = 0;
        public string OutPhysicsDictionary { get; set; }
        public string OutClipDictionary { get; set; }
        private bool IsDrawableDict
        {
            get
            {
                return !string.IsNullOrEmpty(LODModel) && (LODModel.ToLowerInvariant() != "null");
            }
        }

        public void Load(string objstr)
        {
            ObjStr = objstr;
            string[] parts = objstr.Split(',');
            Load(parts);
        }

        public virtual void Load(string[] parts)
        {
            Name = TypeFile.FixHashes(parts[0].Trim().ToLowerInvariant());
            TxdName = TypeFile.FixHashes(parts[1].Trim().ToLowerInvariant());
            DrawDistance = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
            Flags = int.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
            Unknown1 = float.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);
            MinX = float.Parse(parts[5].Trim(), CultureInfo.InvariantCulture);
            MinY = float.Parse(parts[6].Trim(), CultureInfo.InvariantCulture);
            MinZ = float.Parse(parts[7].Trim(), CultureInfo.InvariantCulture);
            MaxX = float.Parse(parts[8].Trim(), CultureInfo.InvariantCulture);
            MaxY = float.Parse(parts[9].Trim(), CultureInfo.InvariantCulture);
            MaxZ = float.Parse(parts[10].Trim(), CultureInfo.InvariantCulture);
            SphereX = float.Parse(parts[11].Trim(), CultureInfo.InvariantCulture);
            SphereY = float.Parse(parts[12].Trim(), CultureInfo.InvariantCulture);
            SphereZ = float.Parse(parts[13].Trim(), CultureInfo.InvariantCulture);
            Radius = float.Parse(parts[14].Trim(), CultureInfo.InvariantCulture);
            LODModel = TypeFile.FixHashes(parts[15].Trim().ToLowerInvariant());
        }

        public void LoadAnim(string animstr)
        {
            ObjStr = animstr;
            string[] parts = animstr.Split(',');
            Name = TypeFile.FixHashes(parts[0].Trim().ToLowerInvariant());
            TxdName = TypeFile.FixHashes(parts[1].Trim().ToLowerInvariant());
            OutClipDictionary = TypeFile.FixHashes(parts[2].Trim().ToLowerInvariant());
            DrawDistance = float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
            Flags = int.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);
            Unknown1 = float.Parse(parts[5].Trim(), CultureInfo.InvariantCulture);
            MinX = float.Parse(parts[6].Trim(), CultureInfo.InvariantCulture);
            MinY = float.Parse(parts[7].Trim(), CultureInfo.InvariantCulture);
            MinZ = float.Parse(parts[8].Trim(), CultureInfo.InvariantCulture);
            MaxX = float.Parse(parts[9].Trim(), CultureInfo.InvariantCulture);
            MaxY = float.Parse(parts[10].Trim(), CultureInfo.InvariantCulture);
            MaxZ = float.Parse(parts[11].Trim(), CultureInfo.InvariantCulture);
            SphereX = float.Parse(parts[12].Trim(), CultureInfo.InvariantCulture);
            SphereY = float.Parse(parts[13].Trim(), CultureInfo.InvariantCulture);
            SphereZ = float.Parse(parts[14].Trim(), CultureInfo.InvariantCulture);
            Radius = float.Parse(parts[15].Trim(), CultureInfo.InvariantCulture);
            LODModel = TypeFile.FixHashes(parts[16].Trim().ToLowerInvariant());
        }

        public virtual void SaveXML(StringBuilder sb)
        {
            sb.AppendLine("   <lodDist value=\"" + TypeFile.FStr(DrawDistance) + "\"/>");
            sb.AppendLine("   <flags value=\"" + TypeFile.UStr((uint)OutFlags) + "\"/>");
            sb.AppendLine("   <specialAttribute value=\"" + TypeFile.IStr(OutSpecialAttribute) + "\"/>");
            sb.AppendLine("   <bbMin x=\"" + TypeFile.FStr(MinX) + "\" y=\"" + TypeFile.FStr(MinY) + "\" z=\"" + TypeFile.FStr(MinZ) + "\"/>");
            sb.AppendLine("   <bbMax x=\"" + TypeFile.FStr(MaxX) + "\" y=\"" + TypeFile.FStr(MaxY) + "\" z=\"" + TypeFile.FStr(MaxZ) + "\"/>");
            sb.AppendLine("   <bsCentre x=\"" + TypeFile.FStr(SphereX) + "\" y=\"" + TypeFile.FStr(SphereY) + "\" z=\"" + TypeFile.FStr(SphereZ) + "\"/>");
            sb.AppendLine("   <bsRadius value=\"" + TypeFile.FStr(Radius) + "\"/>");
            sb.AppendLine("   <hdTextureDist value=\"0\"/>");
            sb.AppendLine("   <name>" + Name + "</name>");
            if (!string.IsNullOrEmpty(TxdName)) sb.AppendLine("   <textureDictionary>" + TxdName + "</textureDictionary>");
            else sb.AppendLine("   <textureDictionary/>");
            if (!string.IsNullOrEmpty(OutClipDictionary)) sb.AppendLine("   <clipDictionary>" + OutClipDictionary + "</clipDictionary>");
            else sb.AppendLine("   <clipDictionary/>");
            if (IsDrawableDict) sb.AppendLine("   <drawableDictionary>" + LODModel + "</drawableDictionary>");
            else sb.AppendLine("   <drawableDictionary/>");
            if (!string.IsNullOrEmpty(OutPhysicsDictionary)) sb.AppendLine("   <physicsDictionary>" + OutPhysicsDictionary + "</physicsDictionary>");
            else sb.AppendLine("   <physicsDictionary/>");
            sb.AppendLine("   <assetType>" + OutAssetType + "</assetType>");
            sb.AppendLine("   <assetName>" + Name + "</assetName>");
            if (OutExtensions != null)
            {
                sb.AppendLine("   <extensions>");
                foreach (var ext in OutExtensions)
                {
                    ext.WriteXML(sb);
                }
                sb.AppendLine("   </extensions>");
            }
            else
            {
                sb.AppendLine("   <extensions/>");
            }
        }

        public void AddEffect(Base2DFX effect)
        {
            if (InEffects == null) InEffects = new List<Base2DFX>();
            InEffects.Add(effect);

            if (effect is Particle2DFX)
            {
                AddExtension(new ParticleExtension(effect as Particle2DFX));
                OutFlags = OutFlags | 0x20000000;
            }
            if (effect is Ladder2DFX)
            {
                AddExtension(new LadderExtension(effect as Ladder2DFX));
                OutSpecialAttribute = 2;
                OutFlags = OutFlags | 0x20000000;
            }
            if (effect is SpawnPoint2DFX)
            {
                AddExtension(new SpawnPointExtension(effect as SpawnPoint2DFX));
                OutFlags = OutFlags | 0x20020000;
            }
            if (effect is LightShaft2DFX)
            {
                AddExtension(new LightShaftExtension(effect as LightShaft2DFX));
                OutFlags = OutFlags | 0x20000000; //TODO: check this
            }
        }

        public void AddExtension(BaseExtension extension)
        {
            if (OutExtensions == null) OutExtensions = new List<BaseExtension>();
            OutExtensions.Add(extension);
        }

        public override string ToString()
        {
            return "Base: " + Name;
        }
    }

    public class TimeArchetype : BaseArchetype
    {
        public override string OutItemType { get { return "CTimeArchetypeDef"; } }

        public int TimeFlags { get; set; }

        public override void Load(string[] parts)
        {
            base.Load(parts);

            TimeFlags = int.Parse(parts[16].Trim(), CultureInfo.InvariantCulture);
        }

        public override void SaveXML(StringBuilder sb)
        {
            base.SaveXML(sb);

            sb.AppendLine("   <timeFlags value=\"" + TypeFile.IStr(TimeFlags) + "\"/>");
        }

        public override string ToString()
        {
            return "Time: " + Name;
        }
    }

    public class MloArchetype : BaseArchetype
    {
        public override string OutItemType { get { return "CMloArchetypeDef"; } }
        public override string OutAssetType { get { return "ASSET_TYPE_ASSETLESS"; } }

        public int RoomCount { get; set; }
        public int PortalCount { get; set; }
        public int EntityCountHD { get; set; }
        public float LodDist1 { get; set; }
        public float LodDist2 { get; set; }
        public float LodDist3 { get; set; }

        public List<MloEntity> Entities { get; set; } = new List<MloEntity>();
        public List<MloRoom> Rooms { get; set; } = new List<MloRoom>();
        public List<MloPortal> Portals { get; set; } = new List<MloPortal>();
        public int ExitPortalCount { get; set; } = 0;

        public override void Load(string[] parts)
        {
            //base.Load(parts);
        }

        public int LoadMLO(string[] lines, int i)
        {
            //name, flags, roomcount, portalcount, entitycountHD, loddist1, loddist2, loddist3?
            string[] parts = lines[i].Split(',');
            Name = TypeFile.FixHashes(parts[0].Trim().ToLowerInvariant());
            Flags = int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
            RoomCount = int.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
            PortalCount = int.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
            EntityCountHD = int.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);
            LodDist1 = float.Parse(parts[5].Trim(), CultureInfo.InvariantCulture);
            LodDist2 = float.Parse(parts[6].Trim(), CultureInfo.InvariantCulture);
            LodDist3 = float.Parse(parts[7].Trim(), CultureInfo.InvariantCulture);

            DrawDistance = (LodDist1 > 0) ? LodDist1 : 100; //this will be output to XML
            OutPhysicsDictionary = Name;

            bool inmlo = true;
            bool inents = true; //MLO starts with entity list
            bool inrooms = false;
            bool inportals = false;

            while (inmlo && (i < lines.Length))
            {
                i++;
                string tline = lines[i].Trim();
                if (tline == "mloend")
                {
                    inmlo = false;
                }
                else if (tline == "mloroomstart")
                {
                    inents = false;
                    inrooms = true;
                    inportals = false;
                }
                else if (tline == "mloportalstart")
                {
                    inents = false;
                    inrooms = false;
                    inportals = true;
                }
                else if (inents)
                {
                    MloEntity ent = new MloEntity();
                    ent.Load(tline);
                    Entities.Add(ent);
                }
                else if (inrooms)
                {
                    MloRoom room = new MloRoom();
                    i = room.Load(lines, i);
                    Rooms.Add(room);
                }
                else if (inportals)
                {
                    MloPortal portal = new MloPortal();
                    portal.Load(tline);
                    Portals.Add(portal);
                    if (portal.RoomTo == 0) ExitPortalCount++;
                }
            }

            return i;
        }

        public override void SaveXML(StringBuilder sb)
        {
            base.SaveXML(sb);

            sb.AppendLine("   <mloFlags value=\"0\"/>");
            sb.AppendLine("   <entities>");
            foreach (var entity in Entities)
            {
                if (entity.LodLevel != 0) continue; //lod entities need to be output to ymap..
                sb.AppendLine("    <Item type=\"CEntityDef\">");
                entity.SaveXML(sb);
                sb.AppendLine("    </Item>");
            }
            sb.AppendLine("   </entities>");
            sb.AppendLine("   <rooms>");
            foreach (var room in Rooms)
            {
                sb.AppendLine("    <Item>");
                room.SaveXML(sb);
                sb.AppendLine("    </Item>");
            }
            sb.AppendLine("   </rooms>");
            sb.AppendLine("   <portals>");
            foreach (var portal in Portals)
            {
                sb.AppendLine("    <Item>");
                portal.SaveXML(sb);
                sb.AppendLine("    </Item>");
            }
            sb.AppendLine("   </portals>");
            sb.AppendLine("   <entitySets/>");
            sb.AppendLine("   <timeCycleModifiers/>");
        }

        public override string ToString()
        {
            return base.ToString();
        }

    }
    public class MloEntity
    {
        public string Name { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        public float RotW { get; set; }
        public int LodLevel { get; set; }
        public uint Flags { get; set; }

        public void Load(string line)
        {
            //name, posX, posY, posZ, rotX, rotY, rotZ, rotW, lodLevel, flags
            string[] parts = line.Split(',');
            Name = TypeFile.FixHashes(parts[0].Trim().ToLowerInvariant());
            PosX = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
            PosY = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
            PosZ = float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
            RotX = float.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);
            RotY = float.Parse(parts[5].Trim(), CultureInfo.InvariantCulture);
            RotZ = float.Parse(parts[6].Trim(), CultureInfo.InvariantCulture);
            RotW = float.Parse(parts[7].Trim(), CultureInfo.InvariantCulture);
            LodLevel = int.Parse(parts[8].Trim(), CultureInfo.InvariantCulture);
            Flags = uint.Parse(parts[9].Trim(), CultureInfo.InvariantCulture);
        }

        public void SaveXML(StringBuilder sb)
        {
            string lodLevel = "LODTYPES_DEPTH_ORPHANHD";
            if (LodLevel == 1) lodLevel = "LODTYPES_DEPTH_LOD";//not really required, only HD should be output here...
            if (LodLevel == 2) lodLevel = "LODTYPES_DEPTH_SLOD1";

            sb.AppendLine("     <archetypeName>" + Name + "</archetypeName>");
            sb.AppendLine("     <flags value=\"1572864\"/>");//18350082 for no reflections
            sb.AppendLine("     <guid value=\"" + TypeFile.UStr((uint)TypeFile.Rnd.Next()) + "\"/>"); //generate guids..
            sb.AppendLine("     <position x=\"" + TypeFile.FStr(PosX) + "\" y=\"" + TypeFile.FStr(PosY) + "\" z=\"" + TypeFile.FStr(PosZ) + "\"/>");
            sb.AppendLine("     <rotation x=\"" + TypeFile.FStr(RotX) + "\" y=\"" + TypeFile.FStr(RotY) + "\" z=\"" + TypeFile.FStr(RotZ) + "\" w=\"" + TypeFile.FStr(RotW) + "\"/>");
            sb.AppendLine("     <scaleXY value=\"1\"/>");
            sb.AppendLine("     <scaleZ value=\"1\"/>");
            sb.AppendLine("     <parentIndex value=\"-1\"/>");
            sb.AppendLine("     <lodDist value=\"-1\"/>");
            sb.AppendLine("     <childLodDist value=\"0\"/>");
            sb.AppendLine("     <lodLevel>" + lodLevel + "</lodLevel>");
            sb.AppendLine("     <numChildren value=\"0\"/>");
            sb.AppendLine("     <priorityLevel>PRI_REQUIRED</priorityLevel>");
            sb.AppendLine("     <extensions/>");
            sb.AppendLine("     <ambientOcclusionMultiplier value=\"255\"/>");
            sb.AppendLine("     <artificialAmbientOcclusion value=\"255\"/>");
            sb.AppendLine("     <tintValue value=\"0\"/>");

        }

    }
    public class MloRoom
    {
        public string Name { get; set; }
        public int EntityCount { get; set; }
        public int PortalCount { get; set; }
        public float MaxX { get; set; }
        public float MaxY { get; set; }
        public float MaxZ { get; set; }
        public float MinX { get; set; }
        public float MinY { get; set; }
        public float MinZ { get; set; }
        public float Blend { get; set; }
        public uint TimeCycle { get; set; }
        public uint Flags { get; set; }
        public List<int> EntityIDs { get; set; } = new List<int>();

        public int Load(string[] lines, int i)
        {
            string[] parts = lines[i].Split(',');

            //name, entitycount, portalcount, maxX, maxY, maxZ, minX, minY, minZ, blend(float?), timecycle, flags
            Name = TypeFile.FixHashes(parts[0].Trim().ToLowerInvariant());
            EntityCount = int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
            PortalCount = int.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
            MaxX = float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
            MaxY = float.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);
            MaxZ = float.Parse(parts[5].Trim(), CultureInfo.InvariantCulture);
            MinX = float.Parse(parts[6].Trim(), CultureInfo.InvariantCulture);
            MinY = float.Parse(parts[7].Trim(), CultureInfo.InvariantCulture);
            MinZ = float.Parse(parts[8].Trim(), CultureInfo.InvariantCulture);
            Blend = float.Parse(parts[9].Trim(), CultureInfo.InvariantCulture);
            TimeCycle = uint.Parse(parts[10].Trim(), CultureInfo.InvariantCulture); //0, 2327134064 - hash
            Flags = uint.Parse(parts[11].Trim(), CultureInfo.InvariantCulture); //0, 96

            bool inroom = true;
            while (inroom && (i < lines.Length))
            {
                i++;
                string tline = lines[i].Trim();
                if (tline == "roomend")
                {
                    inroom = false;
                }
                else
                {
                    string[] idstrs = tline.Split(',');
                    //entity indexes... 16 per row (but maybe not all used, depending on entitycount)
                    for (int n = 0; n < idstrs.Length; n++)
                    {
                        string idstr = idstrs[n].Trim();
                        if (string.IsNullOrEmpty(idstr)) continue;
                        int id = int.Parse(idstr, CultureInfo.InvariantCulture);
                        if (id >= 0)
                        {
                            EntityIDs.Add(id);
                        }
                        else break;
                    }
                }
            }

            return i;
        }

        public void SaveXML(StringBuilder sb)
        {
            sb.AppendLine("     <name>" + Name + "</name>");
            sb.AppendLine("     <bbMin x=\"" + TypeFile.FStr(MinX) + "\" y=\"" + TypeFile.FStr(MinY) + "\" z=\"" + TypeFile.FStr(MinZ) + "\"/>");
            sb.AppendLine("     <bbMax x=\"" + TypeFile.FStr(MaxX) + "\" y=\"" + TypeFile.FStr(MaxY) + "\" z=\"" + TypeFile.FStr(MaxZ) + "\"/>");
            sb.AppendLine("     <blend value=\"1\"/>");
            sb.AppendLine("     <timecycleName/>");
            sb.AppendLine("     <secondaryTimecycleName/>");
            sb.AppendLine("     <flags value=\"96\"/>");
            sb.AppendLine("     <portalCount value=\"" + TypeFile.IStr(PortalCount) + "\"/>");
            sb.AppendLine("     <floorId value=\"0\"/>");
            sb.AppendLine("     <exteriorVisibiltyDepth value=\"-1\"/>");
            sb.AppendLine("     <attachedObjects content=\"int_array\">");
            foreach (var id in EntityIDs)
            {
                sb.AppendLine("      " + TypeFile.IStr(id));
            }
            sb.AppendLine("     </attachedObjects>");
        }

    }
    public class MloPortal
    {
        public int RoomFrom { get; set; }
        public int RoomTo { get; set; }
        public float Corner1X { get; set; }
        public float Corner1Y { get; set; }
        public float Corner1Z { get; set; }
        public float Corner2X { get; set; }
        public float Corner2Y { get; set; }
        public float Corner2Z { get; set; }
        public float Corner3X { get; set; }
        public float Corner3Y { get; set; }
        public float Corner3Z { get; set; }
        public float Corner4X { get; set; }
        public float Corner4Y { get; set; }
        public float Corner4Z { get; set; }
        public int Entity1 { get; set; }
        public int Entity2 { get; set; }
        public int Entity3 { get; set; }
        public int Entity4 { get; set; }
        public uint Flags { get; set; }
        public uint Times { get; set; }
        public float Unk { get; set; }

        public uint OutFlags { get; set; } = 0;

        public void Load(string line)
        {
            string[] parts = line.Split(',');

            //roomFrom, roomTo, corner1(x,y,z), corner2, corner3, corner4, ent1(int), ent2, ent3, ent4, flags, times, unk
            RoomFrom = int.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
            RoomTo = int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
            Corner1X = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
            Corner1Y = float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
            Corner1Z = float.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);
            Corner2X = float.Parse(parts[5].Trim(), CultureInfo.InvariantCulture);
            Corner2Y = float.Parse(parts[6].Trim(), CultureInfo.InvariantCulture);
            Corner2Z = float.Parse(parts[7].Trim(), CultureInfo.InvariantCulture);
            Corner3X = float.Parse(parts[8].Trim(), CultureInfo.InvariantCulture);
            Corner3Y = float.Parse(parts[9].Trim(), CultureInfo.InvariantCulture);
            Corner3Z = float.Parse(parts[10].Trim(), CultureInfo.InvariantCulture);
            Corner4X = float.Parse(parts[11].Trim(), CultureInfo.InvariantCulture);
            Corner4Y = float.Parse(parts[12].Trim(), CultureInfo.InvariantCulture);
            Corner4Z = float.Parse(parts[13].Trim(), CultureInfo.InvariantCulture);
            Entity1 = int.Parse(parts[14].Trim(), CultureInfo.InvariantCulture);
            Entity2 = int.Parse(parts[15].Trim(), CultureInfo.InvariantCulture);
            Entity3 = int.Parse(parts[16].Trim(), CultureInfo.InvariantCulture);
            Entity4 = int.Parse(parts[17].Trim(), CultureInfo.InvariantCulture);
            Flags = uint.Parse(parts[18].Trim(), CultureInfo.InvariantCulture);
            Times = uint.Parse(parts[19].Trim(), CultureInfo.InvariantCulture);
            Unk = float.Parse(parts[20].Trim(), CultureInfo.InvariantCulture);

            if (Flags == 84)
            {
                OutFlags = 1284;
            }
        }

        public void SaveXML(StringBuilder sb)
        {
            bool att = (Entity1 >= 0) || (Entity2 >= 0) || (Entity3 >= 0) || (Entity4 >= 0);

            sb.AppendLine("     <roomFrom value=\"" + TypeFile.IStr(RoomFrom) + "\"/>");
            sb.AppendLine("     <roomTo value=\"" + TypeFile.IStr(RoomTo) + "\"/>");
            sb.AppendLine("     <flags value=\"" + TypeFile.UStr(OutFlags) + "\"/>"); //64? 8192? 1796? //1284 for mirror
            sb.AppendLine("     <mirrorPriority value=\"0\"/>");
            sb.AppendLine("     <opacity value=\"0\"/>");
            sb.AppendLine("     <audioOcclusion value=\"0\"/>");
            sb.AppendLine("     <corners content=\"vector3_array\">");
            sb.AppendLine("      " + TypeFile.FStr(Corner1X) + "\t" + TypeFile.FStr(Corner1Y) + "\t" + TypeFile.FStr(Corner1Z));
            sb.AppendLine("      " + TypeFile.FStr(Corner2X) + "\t" + TypeFile.FStr(Corner2Y) + "\t" + TypeFile.FStr(Corner2Z));
            sb.AppendLine("      " + TypeFile.FStr(Corner3X) + "\t" + TypeFile.FStr(Corner3Y) + "\t" + TypeFile.FStr(Corner3Z));
            sb.AppendLine("      " + TypeFile.FStr(Corner4X) + "\t" + TypeFile.FStr(Corner4Y) + "\t" + TypeFile.FStr(Corner4Z));
            sb.AppendLine("     </corners>");
            if (att)
            {
                sb.AppendLine("     <attachedObjects content=\"int_array\">");
                if (Entity1 >= 0) sb.AppendLine("      " + TypeFile.IStr(Entity1));
                if (Entity2 >= 0) sb.AppendLine("      " + TypeFile.IStr(Entity2));
                if (Entity3 >= 0) sb.AppendLine("      " + TypeFile.IStr(Entity3));
                if (Entity4 >= 0) sb.AppendLine("      " + TypeFile.IStr(Entity4));
                sb.AppendLine("     </attachedObjects>");
            }
            else
            {
                sb.AppendLine("     <attachedObjects/>");
            }

        }
    }

    public class Base2DFX
    {
        public string FXStr { get; set; }
        public string[] FXParts { get; set; }
        public string ObjectID { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public int Type { get; set; }



        public virtual void Load(string fxstr)
        {
            FXStr = fxstr;
            FXParts = fxstr.Split(',');
            ObjectID = TypeFile.FixHashes(FXParts[0].Trim().ToLowerInvariant());
            PosX = float.Parse(FXParts[1].Trim(), CultureInfo.InvariantCulture);
            PosY = float.Parse(FXParts[2].Trim(), CultureInfo.InvariantCulture);
            PosZ = float.Parse(FXParts[3].Trim(), CultureInfo.InvariantCulture);
            Type = int.Parse(FXParts[4].Trim(), CultureInfo.InvariantCulture);
        }

        public static Base2DFX Create(string fxstr)
        {
            var basefx = new Base2DFX();
            basefx.Load(fxstr);
            bool isbase = false;
            switch (basefx.Type)
            {
                case 1: //particles
                    basefx = new Particle2DFX();
                    break;
                case 14: //ladder
                    basefx = new Ladder2DFX();
                    break;
                case 17: //spawn point
                    basefx = new SpawnPoint2DFX();
                    break;
                case 18: //light shaft
                    basefx = new LightShaft2DFX();
                    break;
                default:
                    isbase = true;
                    break;
            }
            if (!isbase)
            {
                basefx.Load(fxstr);
            }
            return basefx;
        }

        public override string ToString()
        {
            return ObjectID + ": " + Type.ToString();
        }
    }
    public class Particle2DFX : Base2DFX
    {
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }
        public float RotationW { get; set; }
        public string FXName { get; set; }
        public int FXType { get; set; }
        public int Unk1 { get; set; }
        public float Size { get; set; }
        public float LodDist { get; set; }
        public int Unk2 { get; set; }
        public uint ColourR { get; set; }
        public uint ColourG { get; set; }
        public uint ColourB { get; set; }

        public override void Load(string fxstr)
        {
            base.Load(fxstr);

            RotationX = float.Parse(FXParts[5].Trim(), CultureInfo.InvariantCulture);
            RotationY = float.Parse(FXParts[6].Trim(), CultureInfo.InvariantCulture);
            RotationZ = float.Parse(FXParts[7].Trim(), CultureInfo.InvariantCulture);
            RotationW = float.Parse(FXParts[8].Trim(), CultureInfo.InvariantCulture);
            FXName = FXParts[9].Trim();
            FXType = int.Parse(FXParts[10].Trim(), CultureInfo.InvariantCulture);
            Unk1 = int.Parse(FXParts[11].Trim(), CultureInfo.InvariantCulture);
            Size = float.Parse(FXParts[12].Trim(), CultureInfo.InvariantCulture);
            LodDist = float.Parse(FXParts[13].Trim(), CultureInfo.InvariantCulture);
            Unk2 = int.Parse(FXParts[14].Trim(), CultureInfo.InvariantCulture);
            ColourR = uint.Parse(FXParts[15].Trim(), CultureInfo.InvariantCulture);
            ColourG = uint.Parse(FXParts[16].Trim(), CultureInfo.InvariantCulture);
            ColourB = uint.Parse(FXParts[17].Trim(), CultureInfo.InvariantCulture);


            switch (FXType)
            {
                case 0: //AMB
                case 1: //COL
                case 2: //SHT
                case 3: //BRK
                case 4: //DST
                    break;
                default:
                    break;
            }
        }
    }
    public class Ladder2DFX : Base2DFX
    {
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }
        public float RotationW { get; set; }
        public float StartX { get; set; }
        public float StartY { get; set; }
        public float StartZ { get; set; }
        public float EndX { get; set; }
        public float EndY { get; set; }
        public float EndZ { get; set; }
        public float DirX { get; set; }
        public float DirY { get; set; }
        public float DirZ { get; set; }
        public int Flags { get; set; }


        public override void Load(string fxstr)
        {
            base.Load(fxstr);

            RotationX = float.Parse(FXParts[5].Trim(), CultureInfo.InvariantCulture);
            RotationY = float.Parse(FXParts[6].Trim(), CultureInfo.InvariantCulture);
            RotationZ = float.Parse(FXParts[7].Trim(), CultureInfo.InvariantCulture);
            RotationW = float.Parse(FXParts[8].Trim(), CultureInfo.InvariantCulture);
            StartX = float.Parse(FXParts[9].Trim(), CultureInfo.InvariantCulture);
            StartY = float.Parse(FXParts[10].Trim(), CultureInfo.InvariantCulture);
            StartZ = float.Parse(FXParts[11].Trim(), CultureInfo.InvariantCulture);
            EndX = float.Parse(FXParts[12].Trim(), CultureInfo.InvariantCulture);
            EndY = float.Parse(FXParts[13].Trim(), CultureInfo.InvariantCulture);
            EndZ = float.Parse(FXParts[14].Trim(), CultureInfo.InvariantCulture);
            DirX = float.Parse(FXParts[15].Trim(), CultureInfo.InvariantCulture);
            DirY = float.Parse(FXParts[16].Trim(), CultureInfo.InvariantCulture);
            DirZ = float.Parse(FXParts[17].Trim(), CultureInfo.InvariantCulture);
            Flags = int.Parse(FXParts[18].Trim(), CultureInfo.InvariantCulture);

        }


    }
    public class SpawnPoint2DFX : Base2DFX
    {
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }
        public float RotationW { get; set; }
        public uint SpawnType { get; set; }
        public uint PedType { get; set; }
        public uint Flags1 { get; set; }
        public uint Flags2 { get; set; }

        public override void Load(string fxstr)
        {
            base.Load(fxstr);

            RotationX = float.Parse(FXParts[5].Trim(), CultureInfo.InvariantCulture);
            RotationY = float.Parse(FXParts[6].Trim(), CultureInfo.InvariantCulture);
            RotationZ = float.Parse(FXParts[7].Trim(), CultureInfo.InvariantCulture);
            RotationW = float.Parse(FXParts[8].Trim(), CultureInfo.InvariantCulture);
            SpawnType = uint.Parse(FXParts[9].Trim(), CultureInfo.InvariantCulture);
            PedType = uint.Parse(FXParts[10].Trim(), CultureInfo.InvariantCulture);
            Flags1 = uint.Parse(FXParts[11].Trim(), CultureInfo.InvariantCulture);
            Flags2 = uint.Parse(FXParts[12].Trim(), CultureInfo.InvariantCulture);

        }
    }
    public class LightShaft2DFX : Base2DFX
    {
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }
        public float RotationW { get; set; }
        public float CornerAX { get; set; }
        public float CornerAY { get; set; }
        public float CornerAZ { get; set; }
        public float CornerBX { get; set; }
        public float CornerBY { get; set; }
        public float CornerBZ { get; set; }
        public float CornerCX { get; set; }
        public float CornerCY { get; set; }
        public float CornerCZ { get; set; }
        public float CornerDX { get; set; }
        public float CornerDY { get; set; }
        public float CornerDZ { get; set; }
        public int ShaftType { get; set; }
        public float ShaftLength { get; set; }
        public float Unk1 { get; set; }

        public override void Load(string fxstr)
        {
            base.Load(fxstr);

            RotationX = float.Parse(FXParts[5].Trim(), CultureInfo.InvariantCulture);
            RotationY = float.Parse(FXParts[6].Trim(), CultureInfo.InvariantCulture);
            RotationZ = float.Parse(FXParts[7].Trim(), CultureInfo.InvariantCulture);
            RotationW = float.Parse(FXParts[8].Trim(), CultureInfo.InvariantCulture);
            CornerAX = float.Parse(FXParts[9].Trim(), CultureInfo.InvariantCulture);
            CornerAY = float.Parse(FXParts[10].Trim(), CultureInfo.InvariantCulture);
            CornerAZ = float.Parse(FXParts[11].Trim(), CultureInfo.InvariantCulture);
            CornerBX = float.Parse(FXParts[12].Trim(), CultureInfo.InvariantCulture);
            CornerBY = float.Parse(FXParts[13].Trim(), CultureInfo.InvariantCulture);
            CornerBZ = float.Parse(FXParts[14].Trim(), CultureInfo.InvariantCulture);
            CornerCX = float.Parse(FXParts[15].Trim(), CultureInfo.InvariantCulture);
            CornerCY = float.Parse(FXParts[16].Trim(), CultureInfo.InvariantCulture);
            CornerCZ = float.Parse(FXParts[17].Trim(), CultureInfo.InvariantCulture);
            CornerDX = float.Parse(FXParts[18].Trim(), CultureInfo.InvariantCulture);
            CornerDY = float.Parse(FXParts[19].Trim(), CultureInfo.InvariantCulture);
            CornerDZ = float.Parse(FXParts[20].Trim(), CultureInfo.InvariantCulture);
            ShaftType = int.Parse(FXParts[21].Trim(), CultureInfo.InvariantCulture);
            ShaftLength = float.Parse(FXParts[22].Trim(), CultureInfo.InvariantCulture);
            Unk1 = float.Parse(FXParts[23].Trim(), CultureInfo.InvariantCulture);

        }


    }

    public class BaseExtension
    {
        public static Dictionary<uint, string> BuildHashLookup(string vals)
        {
            Dictionary<uint, string> dict = new Dictionary<uint, string>();
            string[] valarr = vals.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string val in valarr)
            {
                var tval = val.Trim();
                if (tval.Length == 0) continue;
                string[] parts = tval.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    string keystr = parts[0].Trim();
                    string valstr = parts[1].Trim();
                    uint key = JenkHash.GenHash(keystr.ToLowerInvariant());
                    dict[key] = valstr;
                }
                else
                { }
            }
            return dict;
        }
        public static Dictionary<string, string> BuildStringLookup(string vals)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string[] valarr = vals.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string val in valarr)
            {
                var tval = val.Trim();
                if (tval.Length == 0) continue;
                string[] parts = tval.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    string keystr = parts[0].Trim();
                    string valstr = parts[1].Trim();
                    string key = keystr.ToLowerInvariant();
                    dict[key] = valstr;
                }
                else
                { }
            }
            return dict;
        }


        public virtual void WriteXML(StringBuilder sb)
        {
        }
    }
    public class ParticleExtension : BaseExtension
    {
        public Particle2DFX ParticleFX { get; set; }

        public string FXName { get; set; } = "AMB_STEAM_VENT_ROUND";
        public int BoneTag { get; set; } = 0;
        public int Probability { get; set; } = 100;
        public uint Flags { get; set; } = 0;



        private static Dictionary<string, string> _FXNameLookup;
        public static Dictionary<string, string> FXNameLookup
        {
            get
            {
                if (_FXNameLookup == null)
                {
                    BuildFXNameLookup();
                }
                return _FXNameLookup;
            }
        }
        private static void BuildFXNameLookup()
        {
            string fxnames = @"
AMB_BUTTERFLIES, AMB_BUTTERFLYS
AMB_CASTLE_FOUNTAIN_LOW, AMB_FOUNTAIN_MANSION2
AMB_CASTLE_FOUNTAIN_UP, AMB_FOUNTAIN_MANSION2
AMB_CIG_SMOKE_ASHTRAY, AMB_CIG_SMOKE_LINGER
AMB_CIG_SMOKE_CLOUDS, AMB_CIG_SMOKE_LINGER
AMB_COCKROACHES, AMB_COCKROACHES
AMB_DRIPS_WATER, AMB_WATER_ROOF_DRIPS
AMB_DRIPS_WATER_INTERIOR, AMB_WATER_ROOF_DRIPS
AMB_DRY_ICE, AMB_DRY_ICE_AREA
AMB_DUST_LIT_WINDOWS, AMB_DUST_MOTES
AMB_ELECTRICAL_SPARKS, AMB_ELECTRIC_CRACKLE
AMB_FIRE_GENERIC, AMB_BARREL_FIRE
AMB_FLIES_CIRCLING, AMB_FLY_SWARM
AMB_FLIES_ZAPPED, AMB_FLY_ZAPPED
AMB_FOUNTAIN_CENTRAL, AMB_FOUNTAIN_DOUBLE
AMB_FOUNTAIN_CITYHALL, AMB_FOUNTAIN_ARCADIUS
AMB_FOUNTAIN_CORNERS, AMB_FOUNTAIN_RODEO
AMB_FOUNTAIN_GENERIC, AMB_FOUNTAIN_ARCADIUS
AMB_FOUNTAIN_POUR, AMB_FOUNTAIN_POUR
AMB_FOUNTAIN_ROCKEFELLER, AMB_FOUNTAIN_MANSION2
AMB_FOUNTAIN_SPOUT, AMB_FOUNTAIN_POUR
AMB_FOUNTAIN_UPPER_TIER, AMB_FOUNTAIN_ARCADIUS
AMB_HORN_BLAST, AMB_FOUNDRY_FOGBALL
AMB_HORN_SMOKE, AMB_GENERATOR_SMOKE
AMB_HOT_TUB, AMB_CLUCK_BATH_STEAM
AMB_INDUST_GAS_CONSTANT, AMB_SMOKE_GASWORKS
AMB_INDUST_GAS_INTERMITTENT, AMB_FIRE_GASWORKS
AMB_INDUST_PLUME_SM, AMB_SMOKE_GEN_FACTORY
AMB_INDUST_PLUME_LG, AMB_SMOKE_FOUNDRY
AMB_RAIN_OVERHANGS_5M, AMB_WATER_ROOF_DRIPS
AMB_RAIN_OVERHANGS_10M, AMB_WATER_ROOF_DRIPS
AMB_RESPRAY_CAN, AMB_RAPID_AREA_SPRAY
AMB_STEAM_MANHOLE, AMB_STEAM_VENT_ROUND
AMB_STEAM_CHIMNEY, AMB_STEAM_VENT_RND_HVY
AMB_STEAM_COOKING_APPS, AMB_CLUCK_BATH_STEAM
AMB_STEAM_HOTDOG, AMB_STEAM_VENT_OPEN_LGT
AMB_STEAM_SLOW, AMB_STEAM_VENT_ROUND
AMB_STEAM_SMALL_PLUME, AMB_STEAM_VENT_RND_HVY
AMB_STEAM_STREET_EXHAUST, AMB_STEAM_VENT_ROUND
AMB_STEAM_VENT_OBLONG, AMB_STEAM_VENT_OPEN_HVY
AMB_STEAM_WALL_VENT, AMB_STEAM_DIRECTED
AMB_WATER_PIPE_V1, AMB_WATER_ROOF_DRIPS
AMB_WATER_PIPE_V2, AMB_WATER_ROOF_DRIPS
AMB_WIND_ALLEY_LITTER, AMB_WIND_LITTER
AMB_WIND_ALLEY_LEAVES, AMB_WIND_LEAVES
AMB_WIND_DOCKS_DUST, AMB_WIND_DUST
AMB_WIND_LEAVES_AUTUM, AMB_WIND_LEAVES
AMB_WIND_LEAVES_AUTUMN, AMB_WIND_LEAVES
AMB_WIND_LEAVES_PALE, AMB_WIND_LEAVES
AMB_WIND_SAND, AMB_WIND_DUST
BRK_BIN_STREET, BRK_METAL_FRAG
BRK_CARDBOARD_BOXES, BRK_BANKNOTES
BRK_CERAMIC_OBJECTS, BRK_CHAMPAGNE_CASE
BRK_ELECTRICAL, BRK_SPARKING_WIRES
BRK_ELECTRICAL_SMALL_FIRE, BRK_SPARKING_WIRES
BRK_ELECTRICAL_LARGE_FIRE, BRK_SPARKING_WIRES
BRK_LAUNDRY_BASKET, BRK_COINS
BRK_METAL_OBJECTS, BRK_METAL_FRAG
BRK_SPARKING_WIRES, BRK_SPARKING_WIRES
BRK_SPARKS, BRK_SPARKING_WIRES
BRK_STONE_OBJECTS, BRK_STONE
BRK_WATER_BARRELS, BRK_BLOOD
BRK_WOOD_CHUNKS, BRK_WOOD_SPLINTER
BRK_WOOD_OBJECTS, BRK_WOOD_SPLINTER
BRK_WOOD_PLANKS, BRK_WOOD_PLANKS
COL_ELECTRICAL, COL_ELECTRICAL
COL_FRUITPILE_MIX, COL_ORANGES
COL_LEAVES_PALE, COL_LEAVES
COL_LEAVES_AUTUMN, COL_PALM_LEAVES
COL_MAIL_BUNDLE, COL_ORANGES
COL_NAPKINS, COL_ELECTRICAL
DST_BIN_DUMPSTERS, DST_RUBBISH
DST_BIN_HOUSEHOLD, DST_RUBBISH
DST_BIN_STREET, DST_RUBBISH
DST_CARDBOARD_BOXES, DST_GEN_CARDBOARD
DST_CERAMIC, DST_CERAMICS
DST_CERAMIC_PLATE, DST_CERAMICS
DST_CHIPS, DST_CRISP_BAGS
DST_COIN_PILE, DST_GEN_GOBSTOP
DST_DINO_BONES, DST_ROCKS
DST_ELECTRICAL, DST_ELECTRICAL
DST_FLOWER_PETALS, DST_PLANT_LEAVES
DST_FOOD_DEBRIS, DST_BOX_NOODLE
DST_FRUITPILE_MIX, DST_PINEAPPLE
DST_GLASS_PANES, DST_GLASS_BOTTLES
DST_GLASS_CLEAR_EMPTY, DST_GLASS_BOTTLES
DST_GLASS_CLEAR_LAGER, DST_SHOP_GLASS_BOTTLES
DST_JUICE_CUPS, DST_GEN_LIQUID_BURST
DST_LIGHTBULB_ON, DST_GLASS_BULB
DST_LIGHTBULB_OFF, DST_GLASS_BULB
DST_MAIL_BUNDLE, DST_MAIL
DST_METAL_OBJECTS, DST_METAL_FRAG
DST_NAPKIN_PIECES, DST_RUBBISH
DST_NEWSPAPER_BUNDLE, DST_NEWSPAPER
DST_PAINT_POT, DST_PAINT_CANS
DST_PLASTIC, DST_GEN_PLASTIC_CONT
DST_PLASTIC_CLEAR_WATER, DST_GEN_LIQUID_BURST
DST_SAUCE_BOTTLE, DST_GEN_FOOD
DST_SODAPILE_ECOLA, DST_GEN_LIQUID_BURST
DST_SODAPILE_MIX, DST_GEN_LIQUID_BURST
DST_SODAPILE_SPRUNK, DST_GEN_LIQUID_BURST
DST_SPARKING_WIRES, DST_SPARKING_WIRES
DST_STONE_OBJECTS, DST_ROCKS
DST_STONE_PILLARS, DST_CONCRETE
DST_SWEETIES, DST_SWEET_BOXES
DST_WOODEN_CRATE, DST_WOOD_STRUCTURES
DST_WOODEN_OBJECTS, DST_WOOD_SPLINTER
SHT_ELECTRICAL, SHT_ELECTRICAL_BOX
SHT_GASPIPE, SHT_FIRE_EXTINGUISHER
SHT_GAS_PIPE_FLAME, SHT_FLAME
SHT_OIL_BARREL_GLUG, SHT_BEER_BARREL
SHT_POINT_00, SHT_OIL
SHT_STEAM_PIPE, SHT_STEAM
SHT_WATER_BARREL_GLUG, SHT_BEER_BARREL
SHT_WATER_PIPE, SHT_WATER
exp_molotov, DST_FOAM_EXTINGUISHER
";
            _FXNameLookup = BuildStringLookup(fxnames);
        }



        public ParticleExtension(Particle2DFX particlefx)
        {
            ParticleFX = particlefx;

            string name = ParticleFX.FXName.ToLowerInvariant();
            string fxname;
            if (FXNameLookup.TryGetValue(name, out fxname))
            {
                FXName = fxname;
            }
            else
            {
                Form1.WarningsList.Add("Couldn't find ParticleFX " + ParticleFX.FXName + " for " + ParticleFX.ObjectID + ", using default " + FXName + ".");
            }

        }
        public override void WriteXML(StringBuilder sb)
        {
            float posX = ParticleFX.PosX;
            float posY = ParticleFX.PosY;
            float posZ = ParticleFX.PosZ;
            float rotX = ParticleFX.RotationX;
            float rotY = ParticleFX.RotationY;
            float rotZ = ParticleFX.RotationZ;
            float rotW = ParticleFX.RotationW;
            uint rgb = 0xFF000000; //ABGR?
            rgb = rgb | ((ParticleFX.ColourR & 0xFF) << 0);
            rgb = rgb | ((ParticleFX.ColourG & 0xFF) << 8);
            rgb = rgb | ((ParticleFX.ColourB & 0xFF) << 16);

            sb.AppendLine("    <Item type=\"CExtensionDefParticleEffect\">");
            sb.AppendLine("     <name>" + ParticleFX.ObjectID + "</name>");
            sb.AppendLine("     <offsetPosition x=\"" + TypeFile.FStr(posX) + "\" y=\"" + TypeFile.FStr(posY) + "\" z=\"" + TypeFile.FStr(posZ) + "\" />");
            sb.AppendLine("     <offsetRotation x=\"" + TypeFile.FStr(rotX) + "\" y=\"" + TypeFile.FStr(rotY) + "\" z=\"" + TypeFile.FStr(rotZ) + "\" w=\"" + TypeFile.FStr(rotW) + "\" />");
            sb.AppendLine("     <fxName>" + FXName + "</fxName>");
            sb.AppendLine("     <fxType value=\"" + ParticleFX.FXType.ToString() + "\" />");
            sb.AppendLine("     <boneTag value=\"" + BoneTag.ToString() + "\" />");
            sb.AppendLine("     <scale value=\"" + TypeFile.FStr(ParticleFX.Size) + "\" />");
            sb.AppendLine("     <probability value=\"" + Probability.ToString() + "\" />");
            sb.AppendLine("     <flags value=\"" + Flags.ToString() + "\" />");
            sb.AppendLine("     <color value=\"0x" + rgb.ToString("X").PadLeft(8, '0') + "\" />");
            sb.AppendLine("    </Item>");

        }

    }
    public class LadderExtension : BaseExtension
    {
        public Ladder2DFX LadderFX { get; set; }

        public LadderExtension(Ladder2DFX ladderfx)
        {
            LadderFX = ladderfx;
        }
        public override void WriteXML(StringBuilder sb)
        {
            bool flip = LadderFX.EndZ < LadderFX.StartZ;
            float botX = flip ? LadderFX.EndX : LadderFX.StartX;
            float botY = flip ? LadderFX.EndY : LadderFX.StartY;
            float botZ = flip ? LadderFX.EndZ : LadderFX.StartZ;
            float topX = flip ? LadderFX.StartX : LadderFX.EndX;
            float topY = flip ? LadderFX.StartY : LadderFX.EndY;
            float topZ = flip ? LadderFX.StartZ : LadderFX.EndZ;
            float posX = LadderFX.PosX;
            float posY = LadderFX.PosY;
            float posZ = LadderFX.PosZ;
            float dirX = LadderFX.DirX;
            float dirY = LadderFX.DirY;
            float dirZ = LadderFX.DirZ;

            sb.AppendLine("    <Item type=\"CExtensionDefLadder\">");
            sb.AppendLine("     <name>" + LadderFX.ObjectID + "</name>");
            sb.AppendLine("     <offsetPosition x=\"" + TypeFile.FStr(posX) + "\" y=\"" + TypeFile.FStr(posY) + "\" z=\"" + TypeFile.FStr(posZ) + "\" />");
            sb.AppendLine("     <bottom x=\"" + TypeFile.FStr(botX) + "\" y=\"" + TypeFile.FStr(botY) + "\" z=\"" + TypeFile.FStr(botZ) + "\" />");
            sb.AppendLine("     <top x=\"" + TypeFile.FStr(topX) + "\" y=\"" + TypeFile.FStr(topY) + "\" z=\"" + TypeFile.FStr(topZ) + "\" />");
            sb.AppendLine("     <normal x=\"" + TypeFile.FStr(dirX) + "\" y=\"" + TypeFile.FStr(dirY) + "\" z=\"" + TypeFile.FStr(dirZ) + "\" />");
            sb.AppendLine("     <materialType>METAL_SOLID_LADDER</materialType>");
            sb.AppendLine("     <template>default</template>");
            sb.AppendLine("     <canGetOffAtTop value=\"true\" />");
            sb.AppendLine("     <canGetOffAtBottom value=\"true\" />");
            sb.AppendLine("    </Item>");

        }
    }
    public class SpawnPointExtension : BaseExtension
    {
        public SpawnPoint2DFX SpawnPointFX { get; set; }

        public string SpawnType { get; set; } = "STANDING";
        public string PedType { get; set; } = "Any";


        private static Dictionary<uint, string> _SpawnTypeLookup;
        public static Dictionary<uint, string> SpawnTypeLookup
        {
            get
            {
                if (_SpawnTypeLookup == null)
                {
                    BuildSpawnTypeLookup();
                }
                return _SpawnTypeLookup;
            }
        }
        private static void BuildSpawnTypeLookup()
        {
            string spawntypes = @"
Seat_Bench, PROP_HUMAN_SEAT_BENCH
Seat_StdChair, PROP_HUMAN_SEAT_CHAIR
Seat_StdCouch, PROP_HUMAN_SEAT_ARMCHAIR
Seat_CafeChair, PROP_HUMAN_SEAT_CHAIR_FOOD
Seat_RestaurantChair, PROP_HUMAN_SEAT_CHAIR_FOOD
Seat_BarChair, PROP_HUMAN_SEAT_CHAIR_DRINK_BEER
Seat_StripClubChair, PROP_HUMAN_SEAT_STRIP_WATCH
Seat_StripClubStool, PROP_HUMAN_SEAT_CHAIR_UPRIGHT
Seat_BoardGamePlayer, PROP_HUMAN_SEAT_CHAIR_MP_PLAYER
Seat_OnCar, PROP_HUMAN_SEAT_DECKCHAIR
Seat_Crate, PROP_HUMAN_SEAT_CHAIR
Seat_InternetCafe, PROP_HUMAN_SEAT_COMPUTER
Seat_OnSteps, WORLD_HUMAN_SEAT_STEPS
Seat_OnStepsB, WORLD_HUMAN_SEAT_STEPS
Seat_OnWall, WORLD_HUMAN_SEAT_LEDGE
Seat_OnStepsHangOut, WORLD_HUMAN_SEAT_STEPS
Seat_OnWallHangOut, WORLD_HUMAN_SEAT_LEDGE
Seat_HospitalWaiting, PROP_HUMAN_SEAT_BUS_STOP_WAIT
Seat_SlouchedDruggie, WORLD_HUMAN_BUM_SLUMPED
HangOut_Street, WORLD_HUMAN_HANG_OUT_STREET
HangOut_AlleyWay, WORLD_HUMAN_DRUG_DEALER_HARD
HangOut_OutsideLiquorStore, WORLD_HUMAN_HANG_OUT_STREET
HangOut_BaseballDiamonds, WORLD_HUMAN_CHEERING
HangOut_BoardGameWatcher, WORLD_HUMAN_PARTYING
HangOut_ProjectCourtyard, WORLD_HUMAN_CHEERING
HangOut_BasketballCourt, WORLD_HUMAN_CHEERING
HangOut_OutsideMassageParlours, WORLD_HUMAN_PROSTITUTE_HIGH_CLASS
HangOut_ClubHouse, WORLD_HUMAN_PARTYING
HangOut_WatchBoardGame, WORLD_HUMAN_CHEERING
Scenario_SecurityGuard, WORLD_HUMAN_GUARD_STAND
Scenario_TouristPhoto, WORLD_HUMAN_TOURIST_MOBILE
Scenario_Leaning, WORLD_HUMAN_LEANING
Scenario_SellingDrugs, WORLD_HUMAN_DRUG_DEALER
Scenario_RoadWorkers, WORLD_HUMAN_CONST_DRILL
Scenario_RoadWorkerWithSign, WORLD_HUMAN_HANG_OUT_STREET
Scenario_DiggingRoadWorkers, WORLD_HUMAN_CONST_DRILL
Scenario_DrillingRoadWorkers, WORLD_HUMAN_CONST_DRILL
Scenario_IndustrialWorkers, WORLD_HUMAN_HANG_OUT_STREET
Scenario_BuildingWorkers, WORLD_HUMAN_HANG_OUT_STREET
Scenario_DiggingBuildingWorkers, WORLD_HUMAN_CONST_DRILL
Scenario_DrillingBuildingWorkers, WORLD_HUMAN_CONST_DRILL
Scenario_HeavilyArmedPolice, WORLD_HUMAN_COP_IDLES
Scenario_SmokingOutsideOffice, WORLD_HUMAN_SMOKING
Scenario_PayPhone, WORLD_HUMAN_STAND_MOBILE
Scenario_DancingNightclub, WORLD_HUMAN_PROSTITUTE_HIGH_CLASS
Scenario_HospitalNurse, WORLD_HUMAN_CLIPBOARD
Scenario_HospitalDoctor, WORLD_HUMAN_CLIPBOARD
Scenario_AirWorkers, WORLD_HUMAN_HANG_OUT_STREET
Scenario_Homeless, WORLD_HUMAN_BUM_FREEWAY
Scenario_Brazier, WORLD_HUMAN_STAND_IMPATIENT
Scenario_Gardening, WORLD_HUMAN_GARDENER_PLANT
Scenario_ParkGardeners, WORLD_HUMAN_GARDENER_PLANT
Scenario_Sweeper, WORLD_HUMAN_GARDENER_LEAF_BLOWER
Scenario_NewspaperStand, WORLD_HUMAN_STAND_MOBILE
Scenario_StationedCop, WORLD_HUMAN_COP_IDLES
Scenario_PostMan, WORLD_HUMAN_STAND_FIRE
Scenario_UpTelegraphPoles, WORLD_HUMAN_STAND_FIRE
Scenario_ServiceWorker, WORLD_HUMAN_STAND_FIRE
Scenario_Binoculars, WORLD_HUMAN_BINOCULARS
Scenario_StreetPerformer, WORLD_HUMAN_HUMAN_STATUE
Scenario_Prostitute, WORLD_HUMAN_PROSTITUTE_LOW_CLASS
Scenario_StationarySweeper, WORLD_HUMAN_JANITOR
Scenario_WindowCleaner, WORLD_HUMAN_JANITOR
Scenario_LeaningForwards, WORLD_HUMAN_LEANING
Scenario_WatchingPoleDancer, WORLD_HUMAN_STRIP_WATCH_STAND
Scenario_Bouncer, WORLD_HUMAN_GUARD_STAND
Scenario_WaitingForTaxi, WORLD_HUMAN_STAND_IMPATIENT
Scenario_Preacher, WORLD_HUMAN_MUSICIAN
Scenario_Standing, Standing
Scenario_LayingDruggie, WORLD_HUMAN_BUM_SLUMPED
Scenario_HospitalBed, PROP_HUMAN_SEAT_SUNLOUNGER
Scenario_InvestigatingCop, WORLD_HUMAN_SECURITY_SHINE_TORCH
Scenario_TaiChi, WORLD_HUMAN_YOGA
Scenario_JoggerSpawn, WORLD_HUMAN_JOG
Scenario_StripperDancing, WORLD_HUMAN_PROSTITUTE_HIGH_CLASS
Scenario_StripperLapdance, WORLD_HUMAN_PROSTITUTE_HIGH_CLASS
Scenario_DrinkingAtBar, WORLD_HUMAN_PARTYING
Scenario_PoliceSniper, WORLD_HUMAN_PROSTITUTE_LOW_CLASS
Scenario_PoliceSpotter, WORLD_HUMAN_PROSTITUTE_LOW_CLASS
Location_DropOffPoint, WORLD_HUMAN_STAND_IMPATIENT
Location_ShopBrowsing, WORLD_HUMAN_WINDOW_SHOP_BROWSE
";
            _SpawnTypeLookup = BuildHashLookup(spawntypes);
        }

        private static Dictionary<uint, string> _PedTypeLookup;
        public static Dictionary<uint, string> PedTypeLookup
        {
            get
            {
                if (_PedTypeLookup == null)
                {
                    BuildPedTypeLookup();
                }
                return _PedTypeLookup;
            }
        }
        private static void BuildPedTypeLookup()
        {
            string pedtypes = @"
ANY, Any
";
            _PedTypeLookup = BuildHashLookup(pedtypes);
        }




        public SpawnPointExtension(SpawnPoint2DFX spawnpointfx)
        {
            SpawnPointFX = spawnpointfx;


            //try and map old spawn types to new spawn types...
            string spawnType;
            if (SpawnTypeLookup.TryGetValue(SpawnPointFX.SpawnType, out spawnType))
            {
                SpawnType = spawnType;
            }
            else
            {
                Form1.WarningsList.Add("Couldn't find SpawnType " + SpawnPointFX.SpawnType.ToString() + " for " + SpawnPointFX.ObjectID + ", using default " + SpawnType + ".");
            }

            string pedType;
            if (PedTypeLookup.TryGetValue(SpawnPointFX.PedType, out pedType))
            {
                PedType = pedType;
            }
            else
            {
                Form1.WarningsList.Add("Couldn't find PedType " + SpawnPointFX.PedType.ToString() + " for " + SpawnPointFX.ObjectID + ", using default " + PedType + ".");
            }


        }
        public override void WriteXML(StringBuilder sb)
        {
            float posX = SpawnPointFX.PosX;
            float posY = SpawnPointFX.PosY;
            float posZ = SpawnPointFX.PosZ;
            float rotX = SpawnPointFX.RotationX;
            float rotY = SpawnPointFX.RotationY;
            float rotZ = SpawnPointFX.RotationZ;
            float rotW = SpawnPointFX.RotationW;

            sb.AppendLine("    <Item type=\"CExtensionDefSpawnPoint\">");
            sb.AppendLine("     <name>" + SpawnPointFX.ObjectID + "</name>");
            sb.AppendLine("     <offsetPosition x=\"" + TypeFile.FStr(posX) + "\" y=\"" + TypeFile.FStr(posY) + "\" z=\"" + TypeFile.FStr(posZ) + "\" />");
            sb.AppendLine("     <offsetRotation x=\"" + TypeFile.FStr(rotX) + "\" y=\"" + TypeFile.FStr(rotY) + "\" z=\"" + TypeFile.FStr(rotZ) + "\" w=\"" + TypeFile.FStr(rotW) + "\" />");
            sb.AppendLine("     <spawnType>" + SpawnType.ToLowerInvariant() + "</spawnType>");
            sb.AppendLine("     <pedType>" + PedType.ToLowerInvariant() + "</pedType>");
            sb.AppendLine("     <group/>");
            sb.AppendLine("     <interior/>");
            sb.AppendLine("     <requiredImap/>");
            sb.AppendLine("     <availableInMpSp>kBoth</availableInMpSp>");
            sb.AppendLine("     <probability value=\"0.0\"/>");
            sb.AppendLine("     <timeTillPedLeaves value=\"0.0\"/>");
            sb.AppendLine("     <radius value=\"0.0\"/>");
            sb.AppendLine("     <start value=\"0\"/>");
            sb.AppendLine("     <end value=\"0\"/>");
            sb.AppendLine("     <flags/>");
            sb.AppendLine("     <highPri value=\"false\"/>");
            sb.AppendLine("     <extendedRange value=\"false\"/>");
            sb.AppendLine("     <shortRange value=\"false\"/>");
            sb.AppendLine("    </Item>");
        }
    }
    public class LightShaftExtension : BaseExtension
    {
        public LightShaft2DFX LightShaftFX { get; set; }

        public float DirX { get; set; } = 0.0f;
        public float DirY { get; set; } = 0.0f;
        public float DirZ { get; set; } = -1.0f;
        public uint Color { get; set; } = 0xFFFFFFFF;
        public float Intensity { get; set; } = 0.5f;
        public uint Flags { get; set; } = 51;
        public string DensityType { get; set; } = "LIGHTSHAFT_DENSITYTYPE_QUADRATIC_GRADIENT";
        public string VolumeType { get; set; } = "LIGHTSHAFT_VOLUMETYPE_SHAFT";
        public float Softness { get; set; } = 0.95f;
        public bool ScaleBySunIntensity { get; set; } = true;

        public LightShaftExtension(LightShaft2DFX lightshaftfx)
        {
            LightShaftFX = lightshaftfx;


            float rotX = LightShaftFX.RotationX;
            float rotY = LightShaftFX.RotationY;
            float rotZ = LightShaftFX.RotationZ;
            float rotW = LightShaftFX.RotationW;

            //TODO: DirX,DirY,DirZ, Color, Intensity, Flags, DensityType, VolumeType, Softness, ScaleBySunIntensity......

        }
        public override void WriteXML(StringBuilder sb)
        {
            float posX = LightShaftFX.PosX;
            float posY = LightShaftFX.PosY;
            float posZ = LightShaftFX.PosZ;

            sb.AppendLine("    <Item type=\"CExtensionDefLightShaft\">");
            sb.AppendLine("     <name>" + LightShaftFX.ObjectID + "</name>");
            sb.AppendLine("     <offsetPosition x=\"" + TypeFile.FStr(posX) + "\" y=\"" + TypeFile.FStr(posY) + "\" z=\"" + TypeFile.FStr(posZ) + "\" />");
            sb.AppendLine("     <cornerA x=\"" + TypeFile.FStr(LightShaftFX.CornerAX) + "\" y=\"" + TypeFile.FStr(LightShaftFX.CornerAY) + "\" z=\"" + TypeFile.FStr(LightShaftFX.CornerAZ) + "\" />");
            sb.AppendLine("     <cornerB x=\"" + TypeFile.FStr(LightShaftFX.CornerBX) + "\" y=\"" + TypeFile.FStr(LightShaftFX.CornerBY) + "\" z=\"" + TypeFile.FStr(LightShaftFX.CornerBZ) + "\" />");
            sb.AppendLine("     <cornerC x=\"" + TypeFile.FStr(LightShaftFX.CornerCX) + "\" y=\"" + TypeFile.FStr(LightShaftFX.CornerCY) + "\" z=\"" + TypeFile.FStr(LightShaftFX.CornerCZ) + "\" />");
            sb.AppendLine("     <cornerD x=\"" + TypeFile.FStr(LightShaftFX.CornerDX) + "\" y=\"" + TypeFile.FStr(LightShaftFX.CornerDY) + "\" z=\"" + TypeFile.FStr(LightShaftFX.CornerDZ) + "\" />");
            sb.AppendLine("     <direction x=\"" + TypeFile.FStr(DirX) + "\" y=\"" + TypeFile.FStr(DirY) + "\" z=\"" + TypeFile.FStr(DirZ) + "\" />");
            sb.AppendLine("     <directionAmount value=\"1.0\"/>");
            sb.AppendLine("     <length value=\"" + TypeFile.FStr(LightShaftFX.ShaftLength) + "\"/>");
            sb.AppendLine("     <fadeInTimeStart value=\"0.0\"/>");
            sb.AppendLine("     <fadeInTimeEnd value=\"0.0\"/>");
            sb.AppendLine("     <fadeOutTimeStart value=\"0.0\"/>");
            sb.AppendLine("     <fadeOutTimeEnd value=\"0.0\"/>");
            sb.AppendLine("     <fadeDistanceStart value=\"0.0\"/>");
            sb.AppendLine("     <fadeDistanceEnd value=\"0.0\"/>");
            sb.AppendLine("     <color value=\"0x" + Color.ToString("X").PadLeft(8, '0') + "\" />");
            sb.AppendLine("     <intensity value=\"" + TypeFile.FStr(Intensity) + "\"/>");
            sb.AppendLine("     <flashiness value=\"0\"/>");
            sb.AppendLine("     <flags value=\"" + Flags.ToString() + "\"/>");
            sb.AppendLine("     <densityType>" + DensityType + "</densityType>");
            sb.AppendLine("     <volumeType>" + VolumeType + "</volumeType>");
            sb.AppendLine("     <softness value=\"" + TypeFile.FStr(Softness) + "\"/>");
            sb.AppendLine("     <scaleBySunIntensity value=\"" + (ScaleBySunIntensity ? "true" : "false") + "\"/>");
            sb.AppendLine("    </Item>");

        }

    }



    public class TextureDependency
    {
        public string TXDPStr { get; set; }
        public string Child { get; set; }
        public string Parent { get; set; }

        public void Load(string txdpstr)
        {
            TXDPStr = txdpstr;
            string[] parts = txdpstr.Split(',');
            Child = TypeFile.FixHashes(parts[0].Trim().ToLowerInvariant());
            Parent = TypeFile.FixHashes(parts[1].Trim().ToLowerInvariant());
        }

        public override string ToString()
        {
            return Parent + ": " + Child;
        }
    }

}
