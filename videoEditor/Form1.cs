using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace videoEditor
{
	public partial class Form1 : Form
	{
		private VideoController _video = new VideoController();
		private System.Drawing.Point _selectStart = new System.Drawing.Point(0, 0);
		private System.Drawing.Point _selectEnd = new System.Drawing.Point(0, 0);
		private bool _editRect = false; // 矩形編集中

		public Form1()
		{
			InitializeComponent();
			pictureBox1.AllowDrop = true;

			_video.OnShow += _video_OnShow;
		}


		// 動画が表示された時
		private void _video_OnShow(int nowFrame)
		{
			hScrollBar1.Value = Math.Max(Math.Min(hScrollBar1.Maximum, nowFrame), hScrollBar1.Minimum);
			pictureBox1.Invalidate();
		}

		// 画像更新
		private void UpdateImage(Graphics g)
		{
			if (_video == null)
				return;

			// １度も動画を作っていない
			if (_video.LastImage == null)
				return;



			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

			// 倍率の小さい方を採用する
			var scale = Math.Min((float)pictureBox1.Width / _video.LastImage.Width,
								(float)pictureBox1.Height / _video.LastImage.Height);

#if false
			_video.LastMat.Resize(.Rec.ConvertScaleAbs

			new Mat( _video.LastMat, 
#endif


			var bmp = new Bitmap(_video.LastImage, (int)(scale * _video.LastImage.Width), (int)(scale * _video.LastImage.Height));



			// 領域を描画
			var sx = Math.Max(0, Math.Min(_selectStart.X, _selectEnd.X));
			var sy = Math.Max(0, Math.Min(_selectStart.Y, _selectEnd.Y));
			var w = Math.Min(Math.Abs(_selectStart.X - _selectEnd.X), bmp.Width - sx);
			var h = Math.Min(Math.Abs(_selectStart.Y - _selectEnd.Y), bmp.Height - sy-1);

			// 面積があるなら
			if ((w > 0) && (h > 0))
			{
				var bmpData = bmp.LockBits(new Rectangle(sx, sy, w, h),
										 System.Drawing.Imaging.ImageLockMode.ReadWrite
										, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

				IntPtr ptr = bmpData.Scan0;
				byte[] pixels = new byte[bmpData.Stride * h];
				System.Runtime.InteropServices.Marshal.Copy(ptr, pixels, 0, pixels.Length);



				// エフェクト
				EdgeEffect.Effect(pixels, bmpData.Stride, w, h);


				System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, pixels.Length);
				bmp.UnlockBits(bmpData);
			}

			g.DrawImage(bmp, 0.0f, 0.0f);

			bmp.Dispose();


			Pen blackPen = new Pen(Color.Black);

			// 描画する線を点線に設定
			blackPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
			g.DrawRectangle(blackPen, sx, sy, w, h);

			blackPen.Dispose();
		}


		private void オープンToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//はじめのファイル名を指定する
			//はじめに「ファイル名」で表示される文字列を指定する
//			ofd.FileName = "default.html";
			//はじめに表示されるフォルダを指定する
			//指定しない（空の文字列）の時は、現在のディレクトリが表示される
			openFileDialog1.InitialDirectory = @"C:\";
			//[ファイルの種類]に表示される選択肢を指定する
			//指定しないとすべてのファイルが表示される
			openFileDialog1.Filter = "動画ファイル(*.mp4;*.avi)|*.mp4;*.avi|すべてのファイル(*.*)|*.*";
			//[ファイルの種類]ではじめに選択されるものを指定する
			//2番目の「すべてのファイル」が選択されているようにする
			openFileDialog1.FilterIndex = 1;
			//タイトルを設定する
			openFileDialog1.Title = "開くファイルを選択してください";
			//ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
			openFileDialog1.RestoreDirectory = true;
			//存在しないファイルの名前が指定されたとき警告を表示する
			//デフォルトでTrueなので指定する必要はない
			openFileDialog1.CheckFileExists = true;
			//存在しないパスが指定されたとき警告を表示する
			//デフォルトでTrueなので指定する必要はない
			openFileDialog1.CheckPathExists = true;

			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				this.PlayVideo(openFileDialog1.FileName);
			}
		}


		private void PlayVideo(string path)
		{
			_video.Play(path);
			_video.SetScroll(hScrollBar1);
			_video.Show();
			pictureBox1.Invalidate();
		}


		private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
		{
			_video.Show(e.NewValue);
		}




		// 画像内の矩形表示
		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			_editRect = true;
			_selectStart = e.Location;
			pictureBox1.Invalidate();
		}

		private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			if (_editRect)
			{
				_selectEnd = e.Location;
				pictureBox1.Invalidate();
			}
		}

		private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
		{
			_editRect = false;
			_selectEnd = e.Location;
			pictureBox1.Invalidate();
		}

		// 再描画時
		private void pictureBox1_Paint(object sender, PaintEventArgs e)
		{
			UpdateImage(e.Graphics);
		}

		// ファイルドロップ
		private void pictureBox1_DragDrop(object sender, DragEventArgs e)
		{
			var files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
 
			if ((files.Length > 0) && File.Exists(files[0]))
			{
				// 動画ファイルなら
				if (files[0].EndsWith(".mp4") ||
					files[0].EndsWith(".mkv") ||
					files[0].EndsWith(".avi"))
				{
					this.PlayVideo(files[0]);
				}
			}
		}

		private void pictureBox1_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.All;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}
	}
}
