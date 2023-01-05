using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using OpenCvSharp;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;
using System.Windows.Forms;
// using OpenCvSharp.Extensions;       // Bitmap変換に必要

namespace videoEditor
{
	class VideoController
	{
        private VideoCapture _vcap = null;


        // 現在のフレーム位置取得
		public int PosFrames { get { return _vcap?.PosFrames ?? 0; } }

        // 最後に表示したイメージ
        public Bitmap LastImage { get; private set; } = null;

        public Mat LastMat { get; private set; } = new Mat();


        public event Action<int> OnShow = null; // 表示時


		// 動画再生開始
		public void Play(string path)
        {
            _vcap = new VideoCapture(path);
        }

        public void SetScroll(HScrollBar scroll)
		{
            scroll.Minimum = 0;
            scroll.Maximum = _vcap.FrameCount;
            scroll.Value = 0;
        }



        public void Close()
		{
            this.LastImage?.Dispose();
            this.LastImage = null;
            _vcap?.Dispose();//Memory release
            _vcap = null;
        }


        public void Show(int flame = -1)
        {
            if (_vcap == null)
                return;

            if (!_vcap.IsOpened())
                return;

            if (flame >= 0)
			{
                _vcap.PosFrames = flame;
            }


            if (!_vcap.Read(LastMat))
			{
                this.Close();
                return;
			}

            if (!LastMat.IsContinuous())
			{
                this.Close();
                return;
            }

            LastImage?.Dispose();
            LastImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LastMat);
            //            Thread.Sleep((int)(1000 / _vcap.Fps));
            this.OnShow?.Invoke(_vcap.PosFrames);
        }
    }
}
