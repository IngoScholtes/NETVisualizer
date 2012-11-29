using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Runtime.CompilerServices;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;


namespace NETVisualizer
{
    public class Renderer : GameWindow
    {
   		private static Thread _mainThread;
		
		private IRenderableNet _network;
        private NetworkColorizer _colorizer;
		private static LayoutProvider _layout;
		private static Bitmap _screenshot = null;
		private bool screenshot = false;
		
		private System.Drawing.Point _panStart;			
		private bool _panning = false;		
		private double _panX = 0d;
		private double _panY = 0d;
		private double _deltaX = 0d;
		private double _deltaY = 0d;		
		private double _zoom = 1d;
		
		private bool _drawMarker = false;
		
		private static AutoResetEvent _initialized = new AutoResetEvent(false);
		private static AutoResetEvent _screenshotExists = new AutoResetEvent(false);		
	
		private static Renderer Instance;
		
		public static string SelectedVertex = null;
		
		public static Func<string, float> ComputeNodeSize;
		public static Func<Tuple<string,string>, float> ComputeEdgeWidth;
		
		double[] matView = new double[16];
		double[] matProj = new double[16];
		int[] viewport = new int[4];
		
		public static LayoutProvider Layout { 
			get { return _layout; } 
			set { value.Init(Instance.Width, Instance.Height, Instance._network);				
				 _layout = value;
			}
		}


        internal Renderer(IRenderableNet network, LayoutProvider layout, NetworkColorizer colorizer, int width, int height)
            : base(width, height, OpenTK.Graphics.GraphicsMode.Default, "NETGen Display")
		{
			Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);
			Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
			Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);
			Mouse.Move += new EventHandler<MouseMoveEventArgs>(Mouse_Move);		
			Mouse.WheelChanged += new EventHandler<MouseWheelEventArgs>(Mouse_WheelChanged);	
			
			ComputeNodeSize = new Func<string, float>(v => {
				return 2f;
			});
			
			ComputeEdgeWidth = new Func<Tuple<string,string>, float>( e => {
				return 0.05f;
			});
			_network = network;
			
			_layout = layout;
			_layout.Init(Width, Height, network);
			
			if (colorizer == null)
				_colorizer = new NetworkColorizer();
			else
				_colorizer = colorizer;
		}
 
		void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				Exit();
		}
		
		void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.Button == MouseButton.Left)
			{
				_panning = true;
				_panStart = new Point((int) e.Position.X, (int) e.Position.Y);
			}
			else if (e.Button == MouseButton.Right)
				_drawMarker = true;
		}
		
		void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
		{			
			if (e.Button == MouseButton.Left)
			{
				_panning = false;
				_panX += _deltaX;
				_panY += _deltaY;
				_deltaX = 0d;
				_deltaY = 0d;
			}
			else if (e.Button == MouseButton.Right)
			{
				
				SelectedVertex = GetVertexFromPosition(e.Position);						
				_drawMarker = false;
			}		
		}
		
		[MethodImpl(MethodImplOptions.Synchronized)]
		OpenTK.Vector3 ScreenToWorld(OpenTK.Vector3 screencoord)
		{
			OpenTK.Vector3 worldcoord = new OpenTK.Vector3();
			screencoord.Y = Height-screencoord.Y;
			screencoord.Z = 0;
			OpenTK.Graphics.Glu.UnProject(screencoord, matView, matProj, viewport, out worldcoord);
			return worldcoord;
		}
		
		string GetVertexFromPosition(System.Drawing.Point position)
		{			
			Vector3 screencoord = new Vector3(position.X, position.Y, 0);				
			Console.WriteLine("Clicked at " + position.X + "," + position.Y);
			Vector3 worldcoord = ScreenToWorld(screencoord);
			
			string selected = null;
			double dist = double.MaxValue;
			
			Vector3 clickPos = new Vector3(worldcoord.X, worldcoord.Y, 0f);
			foreach(string v in _network.GetVertexArray())
			{
				Vector3 p = Layout.GetPositionOfNode(v);										
				p.Z = 0f;			

				if ((p-clickPos).Length<dist && (p - clickPos).Length<2)
				{
					dist = (p - clickPos).Length;
					selected = v;
				}
			}
			if (selected!=null)
				Console.WriteLine(selected + " at distance " + dist);
			return selected;
		}
		
		void Mouse_WheelChanged(object sender, MouseWheelEventArgs e)
		{
			_zoom += e.DeltaPrecise/4f;
		}
		
		void Mouse_Move(object sender, MouseEventArgs e)
		{
			if(_panning)
			{
				_deltaX = e.X - _panStart.X;
				_deltaY = e.Y - _panStart.Y;				
			}
		}
		
		protected override void OnResize(EventArgs e)
		{
			
			base.OnResize(e);
			
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
			GL.Ortho(0, Width, Height, 0, -1, 1);
			GL.Viewport(0, 0, Width, Height);
			
			// Store matrices for unprojecting ... 
			GL.GetDouble(GetPName.ModelviewMatrix, matView);
			GL.GetDouble(GetPName.ProjectionMatrix, matProj);
			GL.GetInteger(GetPName.Viewport, viewport);
		}
 
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			
            GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(0, Width, Height, 0, -1, 1);
			GL.Viewport(0, 0, Width, Height);
			
			// Store matrices for unprojecting ... 
			GL.GetDouble(GetPName.ModelviewMatrix, matView);
			GL.GetDouble(GetPName.ProjectionMatrix, matProj);
			GL.GetInteger(GetPName.Viewport, viewport);
			
		 	GL.ClearColor(_colorizer.DefaultBackgroundColor);
		}
 
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e); 					
 
			Title = "Rendering network at "+ FPSCounter.GetFps(e.Time).ToString() + " fps";
		}

 		[MethodImpl(MethodImplOptions.Synchronized)]
		protected override void OnRenderFrame(FrameEventArgs e)
		{			
			base.OnRenderFrame(e);
 
			// Create an identity matrix, apply orthogonal projection and viewport			
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(0, Width, Height, 0, -1, 1);
			GL.Viewport(0, 0, Width, Height);		
			
			// Apply panning and zooming state			
			GL.Scale(_zoom, _zoom, _zoom);
			GL.Translate(_panX+_deltaX, _panY+_deltaY, 0);
			
			// Store matrices for unprojecting ... 
			GL.GetDouble(GetPName.ModelviewMatrix, matView);
			GL.GetDouble(GetPName.ProjectionMatrix, matProj);
			GL.GetInteger(GetPName.Viewport, viewport);
			
			// Clear the buffer
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.ClearColor(_colorizer.DefaultBackgroundColor);		
			
			// Draw the edges
			foreach(var edge in _network.GetEdgeArray())
				DrawEdge(edge.Item1, edge.Item2, _colorizer[edge], ComputeEdgeWidth(edge));
			
			if(SelectedVertex != null)
				foreach(string w in this._network.GetSuccessorArray(SelectedVertex))
					DrawEdge(SelectedVertex, w, Color.Red, ComputeEdgeWidth(new Tuple<string,string>(SelectedVertex, w)), true);
			
			// Draw the vertices
			foreach(string v in _network.GetVertexArray())
				DrawVertex(v, _colorizer[v], 10, ComputeNodeSize(v));
			
			if(SelectedVertex != null)
				DrawVertex(SelectedVertex, Color.Red, 10, ComputeNodeSize(SelectedVertex), true);
			
			if(_drawMarker)
				DrawMarker(Color.Red, 10, 2);
			
			// Swap screen and backbuffer
			SwapBuffers();
			
			if(screenshot)
				GrabImage();
		}
		
		/// <summary>
		/// Draws an edge as a simple line between two node positions
		/// </summary>
		/// <param name='e'>
		/// The edge to paint
		/// </param>
		/// <param name='c'>
		/// The color to use for the edge
		/// </param>
		void DrawEdge(string v, string w, Color c, float width, bool drawselected = false)
		{
			if ( !drawselected && SelectedVertex!=null && (v == SelectedVertex || w == SelectedVertex))
				return;
			
			GL.Color3(c);
			GL.LineWidth(width);
			GL.Begin(BeginMode.Lines);
			
			GL.Vertex2(Layout.GetPositionOfNode(v).X, Layout.GetPositionOfNode(v).Y);
			GL.Vertex2(Layout.GetPositionOfNode(w).X, Layout.GetPositionOfNode(w).Y);
			
			GL.End();
		}
		
		void DrawMarker(Color c, int segments, int radius)
		{
			OpenTK.Vector3 pos = ScreenToWorld(new OpenTK.Vector3(Mouse.X, Mouse.Y, 0));
			GL.Color3(c);
            GL.Begin(BeginMode.TriangleFan);

            for (int i = 0; i < 360; i+=360/segments)
            {
                double degInRad = i * 3.1416/180;
                GL.Vertex2(pos.X + Math.Cos(degInRad) * radius, pos.Y+Math.Sin(degInRad) * radius);
            }
			GL.End();
		}
		
		/// <summary>
		/// Draws a vertex as a simple circle made up from a configurable number of triangle segments
		/// </summary>
		/// <param name='v'>
		/// The vertex to draw
		/// </param>
		/// <param name='c'>
		/// The Color to use for the vertex
		/// </param>
		/// <param name='segments'>
		/// The number of triangle segments to use. A higher number will look more prety but will take more time to render
		/// </param>
		void DrawVertex(string v, Color c, int segments, double radius, bool drawselected = false)
        {
			if(!drawselected && SelectedVertex !=null && v == SelectedVertex)
				return;
			
        	GL.Color3(c);
            GL.Begin(BeginMode.TriangleFan);

            for (int i = 0; i < 360; i+=360/segments)
            {
                double degInRad = i * 3.1416/180;
                GL.Vertex2(Layout.GetPositionOfNode(v).X + Math.Cos(degInRad) * radius, Layout.GetPositionOfNode(v).Y+Math.Sin(degInRad) * radius);
            }
			GL.End();
		}
 
		/// <summary>
		/// Creates a new instance of a Networkvisualizer which renders the specified network in real-time
		/// </summary>
		/// <param name='n'>
		/// N.
		/// </param>
		/// <param name='layout'>
		/// Layout.
		/// </param>
		public static void Start(IRenderableNet network, LayoutProvider layout, NetworkColorizer colorizer = null, int width=800, int height=600)
		{			
			// The actual rendering needs to be done in a separate thread placed in the single thread appartment state
			_mainThread = new Thread(new ThreadStart(new Action(delegate() {				
					Instance =  new Renderer(network, layout, colorizer, width, height);
					_initialized.Set();
					Instance.Run(80f);
            })));
						
            _mainThread.SetApartmentState(ApartmentState.STA);
            _mainThread.Name = "STA Thread for NETGen Visualizer";
			
			// Fire up the thread
            _mainThread.Start();
			_initialized.WaitOne();
		}
		
		[MethodImpl(MethodImplOptions.Synchronized)]
	    private static void GrabImage()
        {
            if (OpenTK.Graphics.GraphicsContext.CurrentContext == null)
                throw new OpenTK.Graphics.GraphicsContextMissingException();
 
            if(_screenshot==null)
				_screenshot = new Bitmap(Instance.ClientSize.Width, Instance.ClientSize.Height);
			
			try {
				lock(_screenshot)
				{
					 System.Drawing.Imaging.BitmapData data =
		             _screenshot.LockBits(Instance.ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, 
							System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					
		            GL.ReadPixels(0, 0, Instance.ClientSize.Width, Instance.ClientSize.Height,PixelFormat.Bgra,
						PixelType.UnsignedByte, data.Scan0);
					
		            _screenshot.UnlockBits(data);
				}
				_screenshotExists.Set();
			}
			catch {
				// Logger.AddMessage(LogEntryType.Warning, "Error while copying screen buffer to bitmap.");
			}

        }
		
		/// <summary>
		/// Saves the last rendered image to a bitmap file. If the path to an existing file is given, the file will be overwritten. This call will block until there is a rendered screenshot available. 
		/// </summary>
		/// <param name='filename'>
		/// The filename of the saved image.
		/// </param>
		public static void SaveCurrentImage(string filename)
		{
			Instance.screenshot = true;
			
			// Wait until there is a screenshot
			_screenshotExists.WaitOne();
			
			if(_screenshot != null && filename != null)
			{
				lock(_screenshot)
					_screenshot.Save(filename);
				// Logger.AddMessage(LogEntryType.Info, "Network image has been written to  file");
			}
			else
			{
				// Logger.AddMessage(LogEntryType.Warning, "Could not save network image");
			}
			
			Instance.screenshot = false;
			_screenshotExists.Reset();
			
		}
			

	}

}

