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
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace GoogleTakoutSorter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            // this app will move all jpg's from the folder (and subfolders) in the textBox1.Text to 
            // thte folder specified in the textBox2.Text
            // in case if file name already exists, the app will add _001,2,3...
            InitializeComponent();
            textBox1.Text = @"D:\Archives\photoalbum\2021.11 Tanya iPhone raw";
            textBox2.Text = @"D:\Archives\photoalbum\2021.11 Tanya iPhone";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.IO.DirectoryInfo rootDir = new System.IO.DirectoryInfo(textBox1.Text);
            WalkDirectoryTree(rootDir);
        }

        private static Regex r = new Regex(":");

        public static DateTime GetDateTakenFromImage(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (Image myImage = Image.FromStream(fs, false, false))
                {
                    PropertyItem propItem = myImage.GetPropertyItem(36867);
                    string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                    return DateTime.Parse(dateTaken);
                }
            }
            catch
            {
                DateTime dt = new DateTime();
                if (path.Contains("Photos from"))
                {
                    string year = path.Substring(path.IndexOf("Photos from") + 12, 4);
                    dt = new DateTime(int.Parse(year), 01, 01);
                    return (dt);
                }
                else
                {
                    dt = DateTime.Now;
                    return (dt);
                }    
                //return null;
            }
        }

        void WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                // log.Add(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    

                    if (fi.Extension.ToLower() == ".jpg")
                    {
                        Console.WriteLine(fi.FullName);
                        DateTime date = GetDateTakenFromImage(fi.FullName);

                        //date.Year.ToString();
                        string destDir = textBox2.Text + @"\" + date.Year.ToString();
                        System.IO.Directory.CreateDirectory(destDir);


                        string name = date.ToString();
                        string ext = ".jpg";
                        name = Regex.Replace(name, "[^a-zA-Z0-9% ._]", string.Empty);


                        int i = 0;
                        string baseName = name;
                        while (File.Exists(destDir + @"\" + name + ext))
                        {
                            name = baseName + "_" + i.ToString();
                            i++;
                        }

                        File.Move(fi.FullName, destDir + @"\" + name + ext);


                    }

                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo);
                }
            }
        }

    }
}
