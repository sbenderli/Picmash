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

namespace Picmash
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            Factory f = new Factory(100, rootPath, 10000000);
            f.StartWork();

















            //string picDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            //DirectoryInfo dirInfo = new DirectoryInfo(picDir);
            //Console.WriteLine(dirInfo.Attributes.ToString());
            

            //// Get the subdirectories directly that is under the root. 
            //// See "How to: Iterate Through a Directory Tree" for an example of how to
            //// iterate through an entire tree.
            //System.IO.DirectoryInfo[] dirInfos = dirInfo.GetDirectories("*.*");

            //foreach (System.IO.DirectoryInfo d in dirInfos)
            //{
            //    Console.WriteLine(d.Name);
            //}

            //System.IO.FileInfo[] fileNames = dirInfo.GetFiles("*.*");
            //foreach (System.IO.FileInfo fi in fileNames)
            //{
            //    Console.WriteLine("{0}: {1}: {2}", fi.Name, fi.LastAccessTime, fi.Length);
            //}
            
        }
    }
}
