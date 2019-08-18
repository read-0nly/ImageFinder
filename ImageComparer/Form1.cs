using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace ImageComparer
{
    public partial class Form1 : Form
    {
        CurrentSearch search = new CurrentSearch();
        TinyImage processor;
        public Form1()
        {
            InitializeComponent();
        }

        private void pickTarget_Click(object sender, EventArgs e)
        {
            DialogResult dR1 = folderBrowserDialog1.ShowDialog();
            if (dR1.ToString() == "OK") { search.targetFolder = folderBrowserDialog1.SelectedPath; }

        }

        private void setFolder_Click(object sender, EventArgs e)
        {
            DialogResult dR1 = folderBrowserDialog1.ShowDialog();
            if (dR1.ToString() == "OK") { search.rootFolder = folderBrowserDialog1.SelectedPath; }
        }

        private void scanBtn_Click(object sender, EventArgs e)
        {
            progressBar1.Maximum = 3;
            progressLbl.Text = "0 / " + progressBar1.Maximum;
            List<String> f1 = System.IO.Directory.EnumerateFiles(search.targetFolder).ToList<string>();
            progressBar1.Value = 1;
            progressLbl.Text = "1 / " + progressBar1.Maximum;
            Application.DoEvents();

            List<String> f2 = System.IO.Directory.EnumerateFiles(search.rootFolder).ToList<string>();
            progressBar1.Value = 2;
            progressLbl.Text = "2 / " + progressBar1.Maximum;
            Application.DoEvents();

            List<String> dir1 = System.IO.Directory.EnumerateDirectories(search.rootFolder).ToList<string>();
            progressBar1.Value = 3;
            progressLbl.Text = "3 / " + progressBar1.Maximum;
            Application.DoEvents();

            int i = 0;

            progressBar1.Maximum = dir1.Count;
            progressLbl.Text = "0 / " + progressBar1.Maximum;
            foreach (String s1 in dir1)
            {
                f2.AddRange(getFiles(s1));
                i++;
                progressBar1.Value = i;
                progressLbl.Text = progressBar1.Value+" / " + progressBar1.Maximum;
                Application.DoEvents();

            }
            reloadList(f1, f2);


        }
        private List<String> getFiles(string folderpath)
        {
            List<String> f2 = System.IO.Directory.EnumerateFiles(folderpath).ToList<string>();
            List<String> dir1 = System.IO.Directory.EnumerateDirectories(folderpath).ToList<string>();
            foreach(String s1 in dir1)
            {
                f2.AddRange(getFiles(s1));
            }
            return f2;
        }

        private void reloadList(List<String> f1, List<String> f2)
        {
            search.targetFiles.Clear();
            search.rootFiles.Clear();
            targetBox.Items.Clear();
            rootBox.Items.Clear();

            progressBar1.Maximum = f1.Count;
            progressLbl.Text = "0 / " + progressBar1.Maximum;
            int i = 0;
            foreach (string s1 in f1)
            {
                string ext = s1.Split('.')[(s1.Split('.')).Count()-1];
                if (ext == "jpg" || ext == "jpeg" || ext == "png" || ext == "bmp" || ext == "gif") { 
                    search.targetFiles.Add(s1, null);
                    targetBox.Items.Add(s1);
                }
                i++;
                progressBar1.Value = i;
                progressLbl.Text = progressBar1.Value + " / " + progressBar1.Maximum;
                Application.DoEvents();
            }
            i = 0;
            progressBar1.Maximum = f2.Count;
            progressLbl.Text = "0 / " + progressBar1.Maximum;
            foreach (string s1 in f2)
            {
                string ext = s1.Split('.')[(s1.Split('.')).Count()-1];
                if (ext == "jpg" || ext == "jpeg" || ext == "png" || ext == "bmp" || ext == "gif")
                {
                    search.rootFiles.Add(s1, new List<float> { 255.0f });
                    rootBox.Items.Add(s1);
                }
                i++;
                progressBar1.Value = i;
                progressLbl.Text = progressBar1.Value + " / " + progressBar1.Maximum;
                Application.DoEvents();
            }
        }
        private void reloadList(List<String> f1, Dictionary<String,List<float>> f2)
        {
            search.targetFiles.Clear();
            search.rootFiles.Clear();
            targetBox.Items.Clear();
            rootBox.Items.Clear();

            foreach (string s1 in f1)
            {
                search.targetFiles.Add(s1, null);
                targetBox.Items.Add(s1);
            }
            foreach (string s1 in f2.Keys)
            {
                search.rootFiles.Add(s1, f2[s1]);
                string listStr = s1;
                foreach (float fl in f2[s1]) { listStr += "|" + (fl.ToString("n1")); }
                rootBox.Items.Add(listStr);
            }
        }

        private void rootBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try { pictureBox1.BackgroundImage = new Bitmap(rootBox.SelectedItem.ToString().Split('|')[0]); }
            catch (Exception ex)
            {
                if (!(rootBox.SelectedItem == null))
                {
                    rootBox.Items.Remove(rootBox.SelectedItem.ToString());
                }
            }
            pictureBox1.Refresh();
        }


        private void targetBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.BackgroundImage = new Bitmap(targetBox.SelectedItem.ToString().Split('|')[0]);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            processor = new TinyImage(int.Parse(sizeTB.Text), int.Parse(modeTB.Text));
            Dictionary<String, List<float>> s3 = search.currentSearch;
            s3.Clear();
            int i = 0;
            int j = 0;
            rootBox.Items.Clear();
            foreach (string s2 in search.rootFiles.Keys)
            {
                bool goodFile = true;
                Image currentTargetImg = new Bitmap(1,1);
                try
                {
                    currentTargetImg = processor.getResized(new Bitmap(s2));

                }
                catch (Exception ex)
                {
                    i += search.targetFiles.Keys.Count;
                    j += search.targetFiles.Keys.Count;
                    goodFile=false;
                }
                if (goodFile && currentTargetImg!=null)
                {
                    foreach (string s1 in search.targetFiles.Keys.ToArray())
                    {
                        i++;
                        j++;
                        if (search.targetFiles[s1] == null) { search.targetFiles[s1] = processor.getResized(new Bitmap(s1)); }
                        float presult = processor.compareImages(currentTargetImg, search.targetFiles[s1]);
                        float result = presult / (float)processor.sizes[processor.setSize];
                        if ((thresholdTB.Text != ""))
                        {
                            if (!(result < float.Parse(thresholdTB.Text)))
                            {
                                result = -255;
                            }
                        }
                        if (!s3.Keys.Contains(s2))
                        {
                            s3.Add(s2, new List<float> { result });
                        }
                        else
                        {
                            s3[s2].Add(result);
                        }
                        progressBar1.Maximum = (int)((float)search.rootFiles.Keys.Count() * (float)search.targetFiles.Keys.Count());
                        progressBar1.Value = i;
                        progressLbl.Text = progressBar1.Value + " / " + progressBar1.Maximum;
                        if (result < float.Parse(displayTb.Text) && result > 0)
                        {
                            rootBox.Items.Add(s2);
                            Console.Beep();
                        }
                        if (j >= 10)
                        {
                            GC.Collect();
                            Application.DoEvents();
                            j = 0;
                        }
                        if (result < -256)
                        {
                            i += search.targetFiles.Keys.Count - 1; break;
                        }
                    }
                    currentTargetImg.Dispose();
                }
            }
        
            if (percentileTB.Text != "") {
                int loopC1 = 0; 
                while (loopC1 < search.targetFiles.Count)
                {
                    SortedSet<float> sLF = new SortedSet<float>();

                    foreach (string k1 in s3.Keys)
                    {
                        sLF.Add(s3[k1][loopC1]);
                    }
                    Console.WriteLine(sLF.ElementAt((int)((float)sLF.Count * float.Parse(percentileTB.Text))).ToString());
                    foreach (string k1 in s3.Keys)
                    {
                        if (s3[k1][loopC1] > sLF.ElementAt((int)((float)sLF.Count * float.Parse(percentileTB.Text))))
                        {

                            s3[k1][loopC1] = -255.0f;
                        }
                    }
                    loopC1++;

                }
            }
            List<string> deleteTargets = new List<string>();
            Dictionary<String, List<float>> s4 = new Dictionary<String, List<float>>();
            foreach (string k1 in s3.Keys)
            {
                bool keep = false;
                foreach (float f in s3[k1])
                {
                    keep = (keep == true || !(f < 0));
                }
                if (keep) { s4.Add(k1,s3[k1]); }
            }
            foreach (string deltgt in deleteTargets)
            {
                s3.Remove(deltgt);
            }
            Application.DoEvents();
            reloadList(search.targetFiles.Keys.ToList<String>(), s4);
        }
        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="fileName"></param>
        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                }
            }
            catch (Exception ex)
            {
                //Log exception here
            }
        }


        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                //Log exception here
            }

            return objectOut;
        }
    }

}
