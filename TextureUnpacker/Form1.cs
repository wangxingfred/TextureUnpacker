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

namespace TextureUnpacker
{
    public partial class Form1 : Form
    {

        AtlasLoad atlasLoad;
		PlistLoad plistLoad;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

            String path = openFileDialog1.FileName;
            if (path != "") SetPlist(path);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();

            String path = openFileDialog2.FileName;
            if (path != "")
            {
                textBox2.Text = path;
            }
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            OpenImage();
        }


        private void BtnUnpack_Click(object sender, EventArgs e)
        {
			String path1 = textBox1.Text;
			String path2 = textBox2.Text;

			if (path1 == "" || path2 == "")
			{
				return;
			}

			String unpackDir = Path.Combine(Path.GetDirectoryName(path1), Path.GetFileNameWithoutExtension(path1));
			if (!Directory.Exists(unpackDir))
			{
				Directory.CreateDirectory(unpackDir);
			}

            int savedCount = 0;

            Bitmap source = ImageLoad.FileToBitmap(path2);

            //导出
            //Plist
            if (r1.Checked == true)
            {
				foreach(PlistFrame frame in  plistLoad.plistFile.frames)
				{
					Bitmap bmp;

					if (frame.rotated == true)
					{
						bmp = new Bitmap(frame.sourceSize.Height, frame.sourceSize.Width);
						Graphics g = Graphics.FromImage(bmp);

						g.DrawImage(source,
							new Rectangle(
								(frame.sourceSize.Height - frame.frame.Height) / 2 + frame.offset.Y,
								(frame.sourceSize.Width - frame.frame.Width) / 2 + frame.offset.X,
								frame.frame.Height,
								frame.frame.Width),
							new Rectangle(
								frame.frame.Left,
								frame.frame.Top,
								frame.frame.Height,
								frame.frame.Width
								),
							GraphicsUnit.Pixel);

						bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
					}
					else
					{
						bmp = new Bitmap(frame.sourceSize.Width, frame.sourceSize.Height);
						Graphics g = Graphics.FromImage(bmp);

						Rectangle r = new Rectangle(
								(frame.sourceSize.Width - frame.frame.Width) / 2 + frame.offset.X,
								(frame.sourceSize.Height - frame.frame.Height) / 2 + frame.offset.Y,
								frame.frame.Width,
								frame.frame.Height);

						g.DrawImage(source,
							new Rectangle(
								(frame.sourceSize.Width - frame.frame.Width )/ 2 + frame.offset.X,
								(frame.sourceSize.Height - frame.frame.Height) / 2 - frame.offset.Y,
								frame.frame.Width,
								frame.frame.Height),
							frame.frame,
							GraphicsUnit.Pixel);
					}

                    var savePath = Path.Combine(unpackDir, flatten.Checked ? Path.GetFileName(frame.name) : frame.name);
                    var saveDirectory = Path.GetDirectoryName(savePath);

                    if (!Directory.Exists(saveDirectory))
                    {
                        Directory.CreateDirectory(saveDirectory);
                    }

                    bmp.Save(savePath);
                    ++savedCount;

                }
            }
            //Atlas
            else if (r2.Checked == true)
            {
                foreach (AtlasRegion region in atlasLoad.List_atlasFile[0].region)
                {
                    Bitmap bmp = new Bitmap(region.orig.Width, region.orig.Height);
                    Graphics g = Graphics.FromImage(bmp);
                    if (region.rotate == true)
                    {
                        g.TranslateTransform(region.orig.Width, 0.0F);
                        g.RotateTransform(90.0F);
                        g.DrawImage(source,
                            new Rectangle(new Point(region.offset.Y, region.offset.X), new Size(region.size.Height, region.size.Width)),
                            new Rectangle(region.xy, new Size(region.size.Height, region.size.Width)),
                            GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.DrawImage(source,
                            new Rectangle(region.offset, region.orig),
                            new Rectangle(region.xy, region.size),
                            GraphicsUnit.Pixel);
                    }
                    pictureBox2.Image = bmp;


                    var saveName = region.name + ".png";
                    var savePath = Path.Combine(unpackDir, flatten.Checked ? Path.GetFileName(saveName) : saveName);
                    var saveDirectory = Path.GetDirectoryName(savePath);

                    if (!Directory.Exists(saveDirectory))
                    {
                        Directory.CreateDirectory(saveDirectory);
                    }

                    bmp.Save(savePath);
                    ++savedCount;
                }
            }

            if (savedCount > 0)
            {
                var res = MessageBox.Show($"成功切割出{savedCount}张图片，是否打开目录?", "Succeed", 
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

                if (res == DialogResult.OK)
                {
                    System.Diagnostics.Process.Start("Explorer.exe", unpackDir);
                }
            }
            else
            {
                MessageBox.Show($"未生成任何图片，资源可能存在异常", "Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

		private void textBox1_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Link; //重要代码：表明是链接类型的数据，比如文件路径
			else e.Effect = DragDropEffects.None;
		}

		private void textBox1_DragDrop(object sender, DragEventArgs e)
		{
			var path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();

            SetPlist(path);
        }

		private void textBox2_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Link;
			else e.Effect = DragDropEffects.None;
		}

		private void textBox2_DragDrop(object sender, DragEventArgs e)
		{
			string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
			textBox2.Text = path;
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			BtnOpen_Click(sender, e);
		}


        private void SetPlist(string filePath)
        {
            var ext = Path.GetExtension(filePath);

            if (ext == ".plist") r1.Checked = true;
            else 
            if (ext == ".atlas") r2.Checked = true;
            else return;

            textBox1.Text = filePath;

            var dir = Path.GetDirectoryName(filePath);
            var filename = Path.GetFileNameWithoutExtension(filePath);

            var files = Directory.GetFiles(dir);
            foreach (var file in files)
            {
                if (file == filePath) continue;

                if (Path.GetFileNameWithoutExtension(file) == filename)
                {
                    textBox2.Text = file;

                    if (OpenImage()) return;
                }
            }
        }

        private bool OpenImage()
        {
            try {
                String path1 = textBox1.Text;
                String path2 = textBox2.Text;

                if (path1 == "" || path2 == "") return false;

                //Plist
                if (r1.Checked == true)
                {
                    plistLoad = new PlistLoad(path1);

                    //Image img = Image.FromFile(path2);
                    //Bitmap bmp = new Bitmap(img);
                    Bitmap source = ImageLoad.FileToBitmap(path2);
                    if (checkBox1.Checked == true)
                    {
                        Graphics g = Graphics.FromImage(source);
                        Pen pen = new Pen(Color.Red, 1);

                        foreach (PlistFrame frame in plistLoad.plistFile.frames)
                        {
                            if (frame.rotated == true)
                            {
                                g.DrawRectangle(pen, new Rectangle(
                                    frame.frame.Left,
                                    frame.frame.Top,
                                    frame.frame.Height,
                                    frame.frame.Width));
                            }
                            else
                            {
                                g.DrawRectangle(pen, frame.frame);
                            }
                        }
                    }
                    pictureBox1.Image = source;
                }
                //Atlas
                else if (r2.Checked == true)
                {
                    atlasLoad = new AtlasLoad(path1);

                    Bitmap bmp = ImageLoad.FileToBitmap(path2);

                    if (checkBox1.Checked == true)
                    {
                        Graphics g = Graphics.FromImage(bmp);
                        Pen pen = new Pen(Color.Red, 1);

                        foreach (AtlasRegion region in atlasLoad.List_atlasFile[0].region)
                        {
                            if (region.rotate == true)
                            {
                                g.DrawRectangle(pen, new Rectangle(region.xy, new Size(region.size.Height, region.size.Width)));
                            }
                            else
                            {
                                g.DrawRectangle(pen, new Rectangle(region.xy, region.size));
                            }
                        }
                    }
                    pictureBox1.Image = bmp;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "无法打开图片", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
    }
}
