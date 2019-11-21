using CodeWalker;
using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VOAD
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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

            Cursor = Cursors.WaitCursor;


            string inputpath = InputFolderTextBox.Text;
            string outputpath = OutputFolderTextBox.Text;
            string[] oadfiles = Directory.GetFiles(inputpath, "*.oad", SearchOption.TopDirectoryOnly);

            if (!outputpath.EndsWith("\\"))
            {
                outputpath = outputpath + "\\";
            }

            StringBuilder errorlog = new StringBuilder();



            List<OadFile> oadlist = new List<OadFile>();

            foreach (var oadfile in oadfiles) //load oad and onim files...
            {
                try
                {
                    OadFile oad = new OadFile();
                    oad.Load(oadfile);
                    oadlist.Add(oad);
                }
                catch (Exception ex)
                {
                    string err = "Error loading " + oadfile + ":\n" + ex.ToString();
                    errorlog.AppendLine(err);
                }
            }


            foreach (var oadfile in oadlist)
            {
                //try
                //{
                    byte[] data = oadfile.BuildYcd();
                    if (data != null)
                    {
                        string filename = outputpath + oadfile.Name + ".ycd";
                        File.WriteAllBytes(filename, data);
                    }
                    else
                    {
                        string err = "Unspecified error converting " + oadfile + " :(";
                        errorlog.AppendLine(err);
                    }
                //}
                //catch (Exception ex)
                //{
                //    string err = "Error converting " + oadfile + ":\n" + ex.ToString();
                //    errorlog.AppendLine(err);
                //}
            }


            Cursor = Cursors.Default;


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
    }



    public class OadFile
    {
        public string Name { get; set; } = string.Empty;
        public string[] Version { get; set; }
        public List<OnimFile> OnimFiles { get; set; } = new List<OnimFile>();


        public void Load(string oadfile)
        {
            Name = Path.GetFileNameWithoutExtension(oadfile).ToLowerInvariant();
            var basepath = Path.GetDirectoryName(oadfile) + "\\";

            var lines = File.ReadAllLines(oadfile);

            int depth = 0;
            var spacedelim = new[] { ' ' };

            var onimpaths = new List<string>();

            foreach (var line in lines)
            {
                string tline = line.Trim();
                if (string.IsNullOrEmpty(tline)) continue; //blank line
                //if (tline.StartsWith("#")) continue; //commented out

                string[] parts = tline.Split(spacedelim, StringSplitOptions.RemoveEmptyEntries);

                if (tline.StartsWith("{")) { depth++; continue; }
                if (tline.StartsWith("}")) { depth--; } //need to handle the closing cases

                if (depth == 0)
                {
                    if (tline.StartsWith("Version"))
                    {
                        Version = parts;
                    }
                }

                if (depth == 1)
                {
                    if (tline.StartsWith("crAnimation") && (tline.Length > 11))
                    {
                        onimpaths.Add(tline.Substring(11).Trim());
                    }
                }
            }


            OnimFiles.Clear();

            foreach (var onimpath in onimpaths)
            {
                var onimfile = new OnimFile();
                onimfile.Load(basepath + onimpath);
                OnimFiles.Add(onimfile);
            }


        }


        public byte[] BuildYcd()
        {
            YcdFile ycd = new YcdFile();

            ycd.ClipDictionary = new ClipDictionary();


            var clipmap = new List<ClipMapEntry>();
            var animmap = new List<AnimationMapEntry>();

            foreach (var onim in OnimFiles)
            {
                var anim = new Animation();
                anim.Hash = JenkHash.GenHash(onim.Name.ToLowerInvariant());
                anim.Frames = (ushort)onim.Frames;
                anim.SequenceFrameLimit = (ushort)onim.SequenceFrameLimit;
                anim.Duration = onim.Duration;

                JenkIndex.Ensure(onim.Name.ToLowerInvariant());//just to make it nicer to debug really

                bool isUV = false;
                bool hasRootMotion = false;
                var boneIds = new List<AnimationBoneId>();
                var seqs = new List<Sequence>();
                var aseqs = new List<List<AnimSequence>>();
                foreach (var oseq in onim.SequenceList)
                {
                    var boneid = new AnimationBoneId();
                    boneid.BoneId = (ushort)oseq.BoneID; //TODO: bone ID mapping
                    boneid.Unk0 = 0;//what to use here?
                    switch (oseq.Track)
                    {
                        case "BonePosition": boneid.Track = 0;
                            break;
                        case "BoneRotation": boneid.Track = 1;
                            break;
                        case "ModelPosition": boneid.Track = 5; hasRootMotion = true;
                            break;
                        case "ModelRotation": boneid.Track = 6; hasRootMotion = true;
                            break;
                        case "UV0": boneid.Track = 17; isUV = true;
                            break;
                        case "UV1": boneid.Track = 18; isUV = true;
                            break;
                        case "LightColor": boneid.Track = 0;//what should this be?
                            break;
                        case "LightRange": boneid.Track = 0;//what should this be?
                            break;
                        case "LightIntensity1": boneid.Track = 0;//what should this be?
                            break;
                        case "LightIntensity2": boneid.Track = 0;//what should this be?
                            break;
                        case "LightDirection": boneid.Track = 0;//what should this be?
                            break;
                        case "Type21": boneid.Track = 0;//what should this be?
                            break;
                        case "CameraPosition": boneid.Track = 7;
                            break;
                        case "CameraRotation": boneid.Track = 8;
                            break;
                        case "CameraFOV": boneid.Track = 0;//what should this be?
                            break;
                        case "CameraDof": boneid.Track = 0;//what should this be?
                            break;
                        case "CameraMatrixRotateFactor": boneid.Track = 0;//what should this be?
                            break;
                        case "CameraControl": boneid.Track = 0;//what should this be?
                            break;
                        case "ActionFlags"://not sure what this is for? just ignore it for now
                            continue;
                        default:
                            break;
                    }
                    boneIds.Add(boneid);

                    for (int i = 0; i < oseq.FramesData.Count; i++)
                    {
                        var framesData = oseq.FramesData[i];
                        if (i > 0)
                        { }
                        Sequence seq = null;
                        List<AnimSequence> aseqlist = null;
                        while (i >= seqs.Count)
                        {
                            seq = new Sequence();
                            seqs.Add(seq);
                            aseqlist = new List<AnimSequence>();
                            aseqs.Add(aseqlist);
                        }
                        seq = seqs[i];
                        aseqlist = aseqs[i];


                        var chanlist = new List<AnimChannel>();
                        if (framesData.IsStatic)
                        {
                            var vals = (framesData.Channels.Count > 0) ? framesData.Channels[0].Values : null;
                            if (vals != null)
                            {
                                if (vals.Length == 1)
                                {
                                    var acsf = new AnimChannelStaticFloat();
                                    acsf.Value = vals[0];
                                    chanlist.Add(acsf);
                                }
                                else if (vals.Length == 3)
                                {
                                    var acsv = new AnimChannelStaticVector3();
                                    acsv.Value = new Vector3(vals[0], vals[1], vals[2]);
                                    chanlist.Add(acsv);
                                }
                                else if (vals.Length == 4)
                                {
                                    var acsq = new AnimChannelStaticQuaternion();
                                    acsq.Value = new Quaternion(vals[0], vals[1], vals[2], vals[3]);
                                    chanlist.Add(acsq);
                                }
                                else
                                { }
                            }
                            else
                            { }
                        }
                        else
                        {
                            int chanCount = framesData.Channels.Count;

                            for (int c = 0; c < chanCount; c++)
                            {
                                var ochan = framesData.Channels[c];
                                var vals = ochan.Values;
                                if (vals.Length == 1)//static channel...
                                {
                                    var acsf = new AnimChannelStaticFloat();
                                    acsf.Value = vals[0];
                                    chanlist.Add(acsf);
                                }
                                else //if (vals.Length == onim.Frames)
                                {
                                    float minval = float.MaxValue;
                                    float maxval = float.MinValue;
                                    float lastval = 0;
                                    float mindelta = float.MaxValue;
                                    foreach (var val in vals)
                                    {
                                        minval = Math.Min(minval, val);
                                        maxval = Math.Max(maxval, val);
                                        if (val != lastval)
                                        {
                                            float adelta = Math.Abs(val - lastval);
                                            mindelta = Math.Min(mindelta, adelta);
                                        }
                                        lastval = val;
                                    }
                                    if (mindelta == float.MaxValue) mindelta = 0;
                                    float range = maxval - minval;
                                    float minquant = range / 1048576.0f;
                                    float quantum = Math.Max(mindelta, minquant);

                                    var acqf = new AnimChannelQuantizeFloat();
                                    acqf.Values = vals;
                                    acqf.Offset = minval;
                                    acqf.Quantum = quantum;
                                    chanlist.Add(acqf);
                                }
                            }

                            if (chanCount == 4)
                            {
                                //assume it's a quaternion... add the extra quaternion channel
                                var acq1 = new AnimChannelCachedQuaternion(AnimChannelType.CachedQuaternion2);
                                acq1.QuatIndex = 3;//what else should it be?
                                chanlist.Add(acq1);
                            }
                        }

                        if (chanlist.Count == 4)
                        { }//shouldn't happen

                        AnimSequence aseq = new AnimSequence();
                        aseq.Channels = chanlist.ToArray();

                        aseqlist.Add(aseq);

                    }


                }

                int remframes = anim.Frames;
                for (int i = 0; i < seqs.Count; i++)
                {
                    var seq = seqs[i];
                    var aseqlist = aseqs[i];

                    seq.Unknown_00h = 0;//what to set this???
                    seq.NumFrames = (ushort)Math.Max(Math.Min(anim.SequenceFrameLimit, remframes), 0);
                    seq.Sequences = aseqlist.ToArray();

                    seq.AssociateSequenceChannels();

                    remframes -= anim.SequenceFrameLimit;
                }


                anim.BoneIds = new ResourceSimpleList64_s<AnimationBoneId>();
                anim.BoneIds.data_items = boneIds.ToArray();

                anim.Sequences = new ResourcePointerList64<Sequence>();
                anim.Sequences.data_items = seqs.ToArray();

                anim.Unknown_10h = hasRootMotion ? (byte)16 : (byte)0;
                anim.Unknown_1Ch = 0; //???

                anim.AssignSequenceBoneIds();



                var cliphash = anim.Hash;
                if (isUV)
                {
                    var name = onim.Name.ToLowerInvariant();
                    var uvind = name.IndexOf("_uv_");
                    if (uvind < 0)
                    { }
                    var modelname = name.Substring(0, uvind);
                    var geoindstr = name.Substring(uvind + 4);
                    var geoind = 0u;
                    uint.TryParse(geoindstr, out geoind);
                    cliphash = JenkHash.GenHash(modelname) + geoind + 1;
                }
                else
                { }


                var clip = new ClipAnimation();
                clip.Animation = anim;
                clip.StartTime = 0.0f;
                clip.EndTime = anim.Duration;
                clip.Rate = 1.0f;
                clip.Name = "pack:/" + onim.Name + ".clip"; //pack:/name.clip
                clip.Unknown_30h = 0; //what's this then?
                clip.Properties = new ClipPropertyMap();
                clip.Properties.CreatePropertyMap(null);//TODO?
                clip.Tags = new ClipTagList(); //TODO?

                var cme = new ClipMapEntry();
                cme.Clip = clip;
                cme.Hash = cliphash;
                clipmap.Add(cme);

                var ame = new AnimationMapEntry();
                ame.Hash = anim.Hash;//is this right? what else to use?
                ame.Animation = anim;
                animmap.Add(ame);
            }


            ycd.ClipDictionary.CreateClipsMap(clipmap.ToArray());
            ycd.ClipDictionary.CreateAnimationsMap(animmap.ToArray());

            ycd.ClipDictionary.BuildMaps();
            ycd.ClipDictionary.UpdateUsageCounts();
            ycd.InitDictionaries();

            byte[] data = ycd.Save();
            return data;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class OnimFile
    {
        public string Name { get; set; } = string.Empty;
        public string[] Version { get; set; }
        public string[] Flags { get; set; }
        public int Frames { get; set; }
        public int SequenceFrameLimit { get; set; }
        public float Duration { get; set; }
        public uint _f10 { get; set; }
        public string[] ExtraFlags { get; set; }
        public int Sequences { get; set; }
        public int MaterialID { get; set; }
        public List<OnimSequence> SequenceList { get; set; } = new List<OnimSequence>();


        public void Load(string onimfile)
        {
            Name = Path.GetFileNameWithoutExtension(onimfile).ToLowerInvariant();

            var lines = File.ReadAllLines(onimfile);


            int depth = 0;
            var spacedelim = new[] { ' ' };

            bool inanim = false;


            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                string tline = line.Trim();
                if (string.IsNullOrEmpty(tline)) continue; //blank line
                //if (tline.StartsWith("#")) continue; //commented out

                string[] parts = tline.Split(spacedelim, StringSplitOptions.RemoveEmptyEntries);

                if (tline.StartsWith("{"))
                {
                    depth++;
                    continue;
                }
                if (tline.StartsWith("}"))
                {
                    depth--;
                    if (inanim) { inanim = false; }
                    continue;
                }

                if (depth == 0)
                {
                    if (tline.StartsWith("Version"))
                    {
                        Version = parts;
                    }
                }

                if (depth == 1)
                {
                    var hasval = (parts?.Length > 1) ;
                    var values = parts.Skip(1).ToArray();
                    if (tline.StartsWith("Flags"))
                    {
                        Flags = values;
                    }
                    else if (tline.StartsWith("Frames"))
                    {
                        Frames = hasval ? TryParseInt(values[0]) : 0;
                    }
                    else if (tline.StartsWith("SequenceFrameLimit"))
                    {
                        SequenceFrameLimit = hasval ? TryParseInt(values[0]) : 0;
                    }
                    else if (tline.StartsWith("Duration"))
                    {
                        Duration = hasval ? TryParseFloat(values[0]) : 0;
                    }
                    else if (tline.StartsWith("_f10"))
                    {
                        _f10 = hasval ? TryParseUInt(values[0]) : 0;
                    }
                    else if (tline.StartsWith("ExtraFlags"))
                    {
                        ExtraFlags = values;
                    }
                    else if (tline.StartsWith("Sequences"))
                    {
                        Sequences = hasval ? TryParseInt(values[0]) : 0;
                    }
                    else if (tline.StartsWith("MaterialID"))
                    {
                        MaterialID = hasval ? TryParseInt(values[0]) : 0;
                    }
                    else if (tline.StartsWith("Animation"))
                    {
                        inanim = true;
                    }
                    else
                    { }
                }

                if ((depth == 2) && inanim)
                {
                    OnimSequence seq = new OnimSequence();
                    if (seq.Read(lines, ref i))
                    {
                        SequenceList.Add(seq);
                    }
                }


            }


        }



        public static int TryParseInt(string s)
        {
            int v = 0;
            int.TryParse(s, out v);
            return v;
        }
        public static uint TryParseUInt(string s)
        {
            uint v = 0;
            uint.TryParse(s, out v);
            return v;
        }
        public static float TryParseFloat(string s)
        {
            return FloatUtil.Parse(s);
        }


        public override string ToString()
        {
            return Name;
        }
    }


    public class OnimSequence
    {
        public string Track { get; set; }
        public string Type { get; set; }
        public int BoneID { get; set; }
        public List<OnimFramesData> FramesData { get; set; } = new List<OnimFramesData>();

        public bool Read(string[] lines, ref int i)
        {
            var line = lines[i];
            string tline = line.Trim();
            if (string.IsNullOrEmpty(tline)) return false; //blank line
            //if (tline.StartsWith("#")) continue; //commented out
            var spacedelim = new[] { ' ' };
            string[] parts = tline.Split(spacedelim, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            { return false; }

            Track = parts[0];
            Type = parts[1];
            BoneID = OnimFile.TryParseInt(parts[2]);

            i++;

            int depth = 0;
            while (i < lines.Length)
            {
                line = lines[i];
                tline = line.Trim();
                if (string.IsNullOrEmpty(tline)) { i++; continue; }
                if (tline.StartsWith("{")) { depth++; }
                if (tline.StartsWith("}")) { depth--; }
                if (depth <= 0) { break; }

                if (depth == 1)
                {
                    if (tline.StartsWith("FramesData"))
                    {
                        OnimFramesData framesData = new OnimFramesData();
                        if (framesData.Read(lines, ref i))
                        {
                            FramesData.Add(framesData);
                        }
                    }
                }

                i++;
            }



            return true;
        }

        public override string ToString()
        {
            return Track + " " + Type + " " + BoneID.ToString();
        }

    }
    public class OnimFramesData
    {
        public string Type { get; set; }
        public bool IsStatic { get; set; }
        public List<OnimFramesDataChannel> Channels { get; set; } = new List<OnimFramesDataChannel>();

        public bool Read(string[] lines, ref int i)
        {
            var line = lines[i];
            string tline = line.Trim();
            if (string.IsNullOrEmpty(tline)) return false; //blank line
            //if (tline.StartsWith("#")) continue; //commented out
            var spacedelim = new[] { ' ' };
            string[] parts = tline.Split(spacedelim, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            { return false; }

            Type = parts[1];
            IsStatic = ((parts.Length > 2) && (parts[2] == "Static"));

            i++;

            int depth = 0;
            while (i < lines.Length)
            {
                line = lines[i];
                tline = line.Trim();
                if (string.IsNullOrEmpty(tline)) { i++; continue; }
                if (tline.StartsWith("{")) { depth++; }
                if (tline.StartsWith("}")) { depth--; }
                if (depth <= 0) { break; }

                if (Type == "SingleChannel")
                {
                    OnimFramesDataChannel chan = new OnimFramesDataChannel();
                    if (chan.Read(lines, ref i))
                    {
                        Channels.Add(chan);
                        i--;
                    }
                }
                else if (Type == "MultiChannel")
                {
                    if (tline.StartsWith("channel"))
                    {
                        i++;
                        OnimFramesDataChannel chan = new OnimFramesDataChannel();
                        if (chan.Read(lines, ref i))
                        {
                            Channels.Add(chan);
                        }
                    }
                }
                else
                { }

                i++;
            }


            return true;
        }

        public override string ToString()
        {
            return Type + " " + (IsStatic ? "Static " : "") + "(" + (Channels?.Count ?? 0).ToString() + " channels)";
        }

    }

    public class OnimFramesDataChannel
    {
        public float[] Values { get; set; }

        public bool Read(string[] lines, ref int i)
        {
            List<float> vals = new List<float>();

            var spacedelim = new[] { ' ' };
            int depth = 0;
            while (i < lines.Length)
            {
                var line = lines[i];
                var tline = line.Trim();
                i++;
                if (string.IsNullOrEmpty(tline)) continue;
                if (tline.StartsWith("{")) { depth++; continue; }
                if (tline.StartsWith("}")) { depth--; }
                if (depth <= 0)
                { i--; break; }

                if (depth == 1)
                {
                    string[] parts = tline.Split(spacedelim, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        if (string.IsNullOrEmpty(part)) continue;
                        var val = OnimFile.TryParseFloat(part);
                        vals.Add(val);
                    }
                }
                else
                { return false; }

            }

            Values = vals.ToArray();

            return true;
        }


        public override string ToString()
        {
            return (Values?.Length ?? 0).ToString() + " values";
        }

    }


}
