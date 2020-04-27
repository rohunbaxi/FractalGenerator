using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Snowflake
{
	public class Segment
	{
		protected Point p1;
		protected Point p2;

		public Point P1
		{
			get { return p1; }
			set { p1 = value; }
		}

		public Point P2
		{
			get { return p2; }
			set { p2 = value; }
		}

		public int Height
		{
			get { return p2.Y - p1.Y; }
		}

		public int Width
		{
			get { return p2.X - p1.X; }
		}

		public double Length
		{
			get { return Math.Sqrt(Height * Height + Width * Width); }
		}

		public Segment(int x1, int y1, int x2, int y2)
		{
			p1 = new Point(x1, y1);
			p2 = new Point(x2, y2);
		}
	}

	public class Flake
	{
		protected Pen pen;
		protected List<Segment> segments;

		public Flake(Pen pen, List<Segment> segments)
		{
			this.pen = pen;
			this.segments = segments;
		}

		public void Draw(Graphics gr)
		{
			foreach (Segment seg in segments)
			{
				gr.DrawLine(pen, seg.P1, seg.P2);
			}
		}
	}

	public partial class Form1 : Form
	{
		private const int width = 400;
		private const int height = 300;
		private const double scale = 0.5;
		private const int xoffset = width / 4;
		private const int yoffset = height / 6;
		private const double rangle = -60 * Math.PI / 180.0;
		private const int maxFlakes = 2;

		protected Brush backBrush;
		protected Pen segPen;
		protected Rectangle windowRect;
		protected Timer timer;
		protected Random rand;

		protected List<Segment> segments;
		protected List<Flake> flakes;
		protected List<Pen> pens;

		public Form1()
		{
			InitializeComponent();
			rand=new Random((int)DateTime.Now.Ticks);
			FormBorderStyle = FormBorderStyle.None;
			StartPosition = FormStartPosition.CenterScreen;
			ClientSize = new Size(width, height);
			backBrush = new SolidBrush(Color.LightBlue);
			pens = new List<Pen>(new Pen[] {new Pen(Color.White), new Pen(Color.Red), new Pen(Color.Green)});
			segPen = pens[0];
			windowRect = new Rectangle(0, 0, width, height);
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			segments = new List<Segment>();
			flakes = new List<Flake>();
			segments.Add(new Segment(0, height, width/2, 0));
			segments.Add(new Segment(width/2, 0, width, height));
			segments.Add(new Segment(width, height, 0, height));
			timer = new Timer();
			timer.Interval = 100;
			timer.Tick += new EventHandler(OnTick);
			timer.Start();
		}

		void OnTick(object sender, EventArgs e)
		{
			List<Segment> newSegments = new List<Segment>();

			foreach (Segment seg in segments)
			{
				double length=seg.Length/3;
				double a = Math.Atan2(seg.Height, seg.Width);
				a = a + rangle;
				Point p1=new Point(seg.P1.X + seg.Width / 3, seg.P1.Y + seg.Height / 3);
				Point p2 = new Point(seg.P1.X + seg.Width * 2 / 3, seg.P1.Y + seg.Height * 2 / 3);
				Segment cutSeg = new Segment(p1.X, p1.Y, p2.X, p2.Y);
				Point p = new Point((int)(cutSeg.P1.X + length * Math.Cos(a)), (int)(cutSeg.P1.Y + length * Math.Sin(a)));
				newSegments.Add(new Segment(seg.P1.X, seg.P1.Y, p1.X, p1.Y));
				newSegments.Add(new Segment(p1.X, p1.Y, p.X, p.Y));
				newSegments.Add(new Segment(p.X, p.Y, p2.X, p2.Y));
				newSegments.Add(new Segment(p2.X, p2.Y, seg.P2.X, seg.P2.Y));
			}

			if (segments[0].Length <= 1)
			{
				foreach (Segment seg in newSegments)
				{
					seg.P1 = Adjust(seg.P1);
					seg.P2 = Adjust(seg.P2);
				}

				flakes.Add(new Flake(segPen, newSegments));

				if (flakes.Count == maxFlakes)
				{
					flakes.RemoveAt(0);
				}

				segments.Clear();

				int rndX = rand.Next(width);
				int rndY = rand.Next(height/2)+height/2;
				int rndW = rand.Next(width - rndX);

				double a = rangle;
				Point p = new Point((int)(rndX + rndW * Math.Cos(a)), (int)(rndY + rndW * Math.Sin(a)));

				segments.Add(new Segment(rndX, rndY, p.X, p.Y));
				segments.Add(new Segment(p.X, p.Y, rndX + rndW, rndY));
				segments.Add(new Segment(rndX + rndW, rndY, rndX, rndY));
				segPen = pens[rand.Next(3)];
			}
			else
			{
				segments = newSegments;
			}

			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			foreach (Segment seg in segments)
			{
				e.Graphics.DrawLine(segPen, Adjust(seg.P1), Adjust(seg.P2));
			}

			foreach (Flake flake in flakes)
			{
				flake.Draw(e.Graphics);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			e.Graphics.FillRectangle(backBrush, windowRect);
		}

		private Point Adjust(Point p)
		{
			Point retp=new Point();
			retp.X = (int)(p.X*scale) + xoffset;
			retp.Y = (int)(p.Y*scale) + yoffset;
			return retp;
		}
	}
}