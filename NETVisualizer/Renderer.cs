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

    /// <summary>
    /// The main class that renders a network using OpenGL
    /// </summary>
    public class Renderer : GameWindow
    {

        /// <summary>
        /// The main threat that implements the rendering loop
        /// </summary>
   		private static Thread _mainThread;
		
        /// <summary>
        /// The network to render
        /// </summary>
		private IRenderableNet _network;

        /// <summary>
        /// A colorizer class that can be used to color vertices and edges
        /// </summary>
        private NetworkColorizer _colorizer;

        /// <summary>
        /// The layout algorithm
        /// </summary>
		private static LayoutProvider _layout;

        /// <summary>
        /// A bitmap representation of the last grabbed frame
        /// </summary>
		private static Bitmap _grabbedFrame = null;

        /// <summary>
        /// Whether or not a frame is currently being grabbed
        /// </summary>
		private bool _grabbingFrame = false;
		
        /// <summary>
        /// The point in screen coordinates where the user started to pan the mouse
        /// </summary>
		private System.Drawing.Point _panStart;

        /// <summary>
        /// Whether or not the user is currently panning the mouse
        /// </summary>
        private bool _panning = false;

        /// <summary>
        /// The global panning offset. This is zero in the beginning and will be adjusted whenever a panning has been completed. 
        /// </summary>
		private double _panningTranslationX = 0d;
		private double _panningTranslationY = 0d;

        /// <summary>
        /// The current delta resulting from an ongoing panning operation
        /// </summary>
		private double _panningDeltaX = 0d;
        private double _panningdeltaY = 0d;

        /// <summary>
        /// The current zoom factor
        /// </summary>
		private double _zoom = 1d;
		
        /// <summary>
        /// Whether or not to draw a red selection marker
        /// </summary>
		private bool _drawMarker = false;
		
        /// <summary>
        /// A wait handle that will be signaled as soon as the device initialization has been completed
        /// </summary>
		private static AutoResetEvent _initialized = new AutoResetEvent(false);

        /// <summary>
        /// A wait handle that will be signaled as soon as a frame grabbing operation has been completed
        /// </summary>
		private static AutoResetEvent _frameGrabbingComplete = new AutoResetEvent(false);		
	
        /// <summary>
        /// A singleton instance of the Renderer
        /// </summary>
		private static Renderer Instance;
		
        /// <summary>
        /// A reference to the last selected vertex
        /// </summary>
		public static string SelectedVertex = null;
		
        /// <summary>
        /// A function used to compute node sizes
        /// </summary>
		public static Func<string, float> ComputeNodeSize;

        /// <summary>
        /// A function used to compute the edge thickness
        /// </summary>
		public static Func<Tuple<string,string>, float> ComputeEdgeThickness;
		
        /// <summary>
        /// The view matrix
        /// </summary>
		double[] matView = new double[16];

        /// <summary>
        /// The projection matric
        /// </summary>
		double[] matProj = new double[16];

        /// <summary>
        /// The view port matrix
        /// </summary>
		int[] viewport = new int[4];
		
        /// <summary>
        /// Gets or sets the layout provider. The layout provider will be initialized based on the current instance whenver this property is set. 
        /// </summary>
		public static LayoutProvider Layout { 
			get { return _layout; } 
			set { value.Init(Instance.Width, Instance.Height, Instance._network);				
				 _layout = value;
			}
		}

        public static int RenderWidth
        {
            get { return Instance.Width; }
        }

        public static int RenderHeight
        {
            get { return Instance.Height; }
        }

        public static bool CurvedEdges
        {
            get;
            set;
        }

        public static float EdgeCurvatureGamma
        {
            get;
            set;
        }

        public static float EdgeCurvatureSegments
        {
            get;
            set;
        }

        private static Dictionary<string, bool> rendered = new Dictionary<string, bool>();

        /// <summary>
        /// Basic constructor that initializes all events, default values and fields
        /// </summary>
        /// <param name="network"></param>
        /// <param name="layout"></param>
        /// <param name="colorizer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        internal Renderer(IRenderableNet network, LayoutProvider layout, NetworkColorizer colorizer, int width, int height)
            : base(width, height, OpenTK.Graphics.GraphicsMode.Default, "NETVisualizer")
		{
            // Register key and mouse events
			Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);
			Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
			Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);
			Mouse.Move += new EventHandler<MouseMoveEventArgs>(Mouse_Move);		
			Mouse.WheelChanged += new EventHandler<MouseWheelEventArgs>(Mouse_WheelChanged);	
			
            // Set default node size
			ComputeNodeSize = new Func<string, float>(v => {
				return 2f;
			});
			
            // Set default edge thickness
			ComputeEdgeThickness = new Func<Tuple<string,string>, float>( e => {
				return 0.05f;
			});

            EdgeCurvatureGamma = (float)Math.PI / 3f;
            EdgeCurvatureSegments = 15;

            // Set network and initialize layout algorithm
			_network = network;			
			_layout = layout;
			_layout.Init(Width, Height, network);
			
            // Initialize colorizer
			if (colorizer == null)
				_colorizer = new NetworkColorizer();
			else
				_colorizer = colorizer;
		}
 
        /// <summary>
        /// Close the window if the user presses escape
        /// </summary>
		void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				Exit();
		}
		
        /// <summary>
        /// React to mouse click: Start panning when the left button is pressed, draw selection marker when right button is pressed
        /// </summary>
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
		
        /// <summary>
        /// React to mouse button release: End panning when left mouse button is released, select node when right mouse button is released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
		{			
			if (e.Button == MouseButton.Left)
			{
				_panning = false;
				_panningTranslationX += _panningDeltaX;
				_panningTranslationY += _panningdeltaY;
				_panningDeltaX = 0d;
				_panningdeltaY = 0d;
			}
			else if (e.Button == MouseButton.Right)
			{
				
				SelectedVertex = GetVertexFromPosition(e.Position);						
				_drawMarker = false;
			}		
		}
		
        /// <summary>
        /// Unprojects the screen coordinates into the world coordinates (i.e. the coordinates of nodes)
        /// </summary>
        /// <param name="screencoord">The screen coordinates to unproject into world coordinates</param>
        /// <returns>World coordinates</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		internal static OpenTK.Vector3 ScreenToWorld(OpenTK.Vector3 screencoord)
		{
			OpenTK.Vector3 worldcoord = new OpenTK.Vector3();
			screencoord.Y = Instance.Height-screencoord.Y;
			screencoord.Z = 0;
			OpenTK.Graphics.Glu.UnProject(screencoord, Instance.matView, Instance.matProj, Instance.viewport, out worldcoord);
			return worldcoord;
		}
		
        /// <summary>
        /// Searches for a vertex based on a screen position
        /// </summary>
        /// <param name="position">The screen position</param>
        /// <returns>The closest vertex or null if no vertex is within a certain range</returns>
		string GetVertexFromPosition(System.Drawing.Point position)
		{			
			Vector3 screencoord = new Vector3(position.X, position.Y, 0);							
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
				Console.WriteLine("Selected node: "  + selected);
            else
                Console.WriteLine("No node. Clicked at " + position.X + "," + position.Y);
			return selected;
		}
		
        /// <summary>
        /// Changes the zoom factor
        /// </summary>       
		void Mouse_WheelChanged(object sender, MouseWheelEventArgs e)
		{
			_zoom += e.DeltaPrecise/4f;
		}
		
        /// <summary>
        /// Implements panning if the left mouse button is pressed
        /// </summary>
		void Mouse_Move(object sender, MouseEventArgs e)
		{
			if(_panning)
			{
				_panningDeltaX = e.X - _panStart.X;
				_panningdeltaY = e.Y - _panStart.Y;				
			}
		}
		
        /// <summary>
        /// When the window is resized, matrices need to be adjusted
        /// </summary>
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
 
        /// <summary>
        /// Initializes the matrices
        /// </summary>
        /// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			
            GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
			GL.Ortho(0, Width, Height, 0, -1, 1);
			GL.Viewport(0, 0, Width, Height);
			
			// Store matrices for unprojecting ... 
			GL.GetDouble(GetPName.ModelviewMatrix, matView);
			GL.GetDouble(GetPName.ProjectionMatrix, matProj);
			GL.GetInteger(GetPName.Viewport, viewport);
			
		 	GL.ClearColor(_colorizer.DefaultBackgroundColor);
		}
 
        /// <summary>
        /// Compute the frame rate and add them to the window title
        /// </summary>
        /// <param name="e"></param>
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e); 					
 
			Title = string.Format("NETVisualizer ({0} fps, {1} nodes, {2} edges)", FPSCounter.GetFps(e.Time), _network.GetVertexCount(), _network.GetEdgeCount());
		}

        /// <summary>
        /// The actual rendering of the network
        /// </summary>
        /// <param name="e"></param>
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
			GL.Translate(_panningTranslationX+_panningDeltaX, _panningTranslationY+_panningdeltaY, 0);
			
			// Store matrices for unprojecting ... 
			GL.GetDouble(GetPName.ModelviewMatrix, matView);
			GL.GetDouble(GetPName.ProjectionMatrix, matProj);
			GL.GetInteger(GetPName.Viewport, viewport);
			
			// Clear the buffer
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(_colorizer.DefaultBackgroundColor);

            foreach (string v in _network.GetVertexArray())
                rendered[v] = false;
			
			// Draw the edges
            foreach (var edge in _network.GetEdgeArray())
            {                
                if(string.Compare(edge.Item1, edge.Item2)>=0)   
                    DrawEdge(edge.Item1, edge.Item2, _colorizer[edge], ComputeEdgeThickness(edge));
                else
                    DrawEdge(edge.Item2, edge.Item1, _colorizer[edge], ComputeEdgeThickness(edge));
            }
			
			if(SelectedVertex != null)
				foreach(string w in this._network.GetSuccessorArray(SelectedVertex))
					DrawEdge(SelectedVertex, w, Color.Red, ComputeEdgeThickness(new Tuple<string,string>(SelectedVertex, w)), true);
			
			// Draw the vertices
			foreach(string v in _network.GetVertexArray())
                DrawVertex(v, _colorizer[v], (int)(10 * _zoom), ComputeNodeSize(v));
			
			if(SelectedVertex != null)
				DrawVertex(SelectedVertex, Color.Red, (int)(10*_zoom), ComputeNodeSize(SelectedVertex), true);
			
			if(_drawMarker)
				DrawMarker(Color.Red, 10, 2);
			
			// Swap screen and backbuffer
			SwapBuffers();
			
			if(_grabbingFrame)
				GrabImage();
		}

        /// <summary>
        /// Quickly computes whether the sequence of lines (p,q) (q,r) represents a right-turn (positive result) or a left-turn (negative result)
        /// </summary>
        /// <param name="p">The first point in the sequence</param>
        /// <param name="q">The second point in the sequence</param>
        /// <param name="r">The The third point in the sequence</param>
        /// <returns>A value whose sign tells whether r is on the left-hand side of the line pq (negative) or on the right-hand side (positive)</returns>
        internal static int orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            return Math.Sign((q.X - p.X) * (r.Y - p.Y) - (q.Y - p.Y) * (r.X - p.X));
        }
		
		/// <summary>
		/// Draws an edge as a straight or a curved line
		/// </summary>
		void DrawEdge(string v, string w, Color c, float thickness, bool drawselected = false)
		{
			if ( !drawselected && SelectedVertex!=null && (v == SelectedVertex || w == SelectedVertex))
				return;

            // Draw curved edge
            if (CurvedEdges)
            {
                // The two end points of the arc
                Vector2 pos_v = new Vector2(Layout.GetPositionOfNode(v).X, Layout.GetPositionOfNode(v).Y);
                Vector2 pos_w = new Vector2(Layout.GetPositionOfNode(w).X, Layout.GetPositionOfNode(w).Y);

                if (pos_v == pos_w)
                    return;

                // The vector vw representing one point of an equilateral triangle (v,w,x) with x being the center of the circle 
                Vector2 vw = Vector2.Subtract(pos_w, pos_v);

                // In an equilateral triangle, the two angles adjacent to vw are alpha
                float alpha = ((float)Math.PI - EdgeCurvatureGamma) / 2f;

                // Compute the radius based on the sine rule ... 
                float radius = Math.Abs((float)(vw.Length * Math.Sin(alpha) / Math.Sin(EdgeCurvatureGamma)));

                // Compute center point
                Vector2 center = vw;
                center.Normalize();
                float rotated_x = (float) (center.X * Math.Cos(alpha) - center.Y * Math.Sin(alpha));
                float rotated_y = (float) (center.X * Math.Sin(alpha) + center.Y * Math.Cos(alpha));
                center.X = rotated_x;
                center.Y = rotated_y;
                Vector2 pos_center = Vector2.Add(pos_v, Vector2.Multiply(center, radius));                                                            

                // The vector xv that will be rotatd in every step ... 
                Vector2 xv = Vector2.Subtract(pos_v, pos_center);

                // The point on the circle to use for the next line segment
                Vector2 circle_point = xv;

                // Initiate the drawing ... 
                GL.Color3(c);
                GL.LineWidth(thickness);
                GL.Begin(BeginMode.LineStrip);

                // Gradually rotate the vector circle_point to get the points on the circle to connect to a curved edge
                if (orientation(pos_center, pos_v, pos_w) > 0)
                    for (int i = 0; i <= EdgeCurvatureSegments; i++)
                    {
                        GL.Vertex2(Vector2.Add(pos_center, circle_point));
                        float new_x = (float)(circle_point.X * Math.Cos(EdgeCurvatureGamma / EdgeCurvatureSegments) - circle_point.Y * Math.Sin(EdgeCurvatureGamma / EdgeCurvatureSegments));
                        float new_y = (float)(circle_point.X * Math.Sin(EdgeCurvatureGamma / EdgeCurvatureSegments) + circle_point.Y * Math.Cos(EdgeCurvatureGamma / EdgeCurvatureSegments));
                        circle_point = new Vector2(new_x, new_y);
                    }
                else
                    for (int i = 0; i <= EdgeCurvatureSegments; i++)
                    {
                        GL.Vertex2(Vector2.Add(pos_center, circle_point));
                        float new_x = (float)(circle_point.X * Math.Cos(-EdgeCurvatureGamma / EdgeCurvatureSegments) - circle_point.Y * Math.Sin(-EdgeCurvatureGamma / EdgeCurvatureSegments));
                        float new_y = (float)(circle_point.X * Math.Sin(-EdgeCurvatureGamma / EdgeCurvatureSegments) + circle_point.Y * Math.Cos(-EdgeCurvatureGamma / EdgeCurvatureSegments));
                        circle_point = new Vector2(new_x, new_y);
                    }
                GL.End();
            }
            // Draw straight edge as a simple line between the vertices
            else
            {
                GL.Color3(c);
                GL.LineWidth(thickness);        
                GL.Begin(BeginMode.Lines);
                GL.Vertex2(Layout.GetPositionOfNode(v).X, Layout.GetPositionOfNode(v).Y);
                GL.Vertex2(Layout.GetPositionOfNode(w).X, Layout.GetPositionOfNode(w).Y);
                GL.End();
            }						
		}
		
        /// <summary>
        /// Draws a red marker used for selecting vertices
        /// </summary>
		void DrawMarker(Color c, int segments, int radius)
		{
			OpenTK.Vector3 pos = ScreenToWorld(new OpenTK.Vector3(Mouse.X, Mouse.Y, 0));
			GL.Color3(c);
            GL.Begin(BeginMode.TriangleFan);

            for (int i = 0; i < 360; i+=360/segments)
            {
                double degInRad = i * (float) Math.PI/180f;
                GL.Vertex2(pos.X + Math.Cos(degInRad) * radius, pos.Y+Math.Sin(degInRad) * radius);
            }
			GL.End();
		}
		
		/// <summary>
		/// Draws a vertex as a simple circle made up from a configurable number of triangle segments
		/// </summary>
		/// <param name='v'>The vertex to draw</param>
		/// <param name='c'>The color to use for the vertex</param>
		/// <param name='segments'>The number of triangle segments to use. A higher number will look more pretty but will take more time to render</param>
		void DrawVertex(string v, Color c, int segments, double radius, bool drawselected = false)
        {
			if(!drawselected && SelectedVertex !=null && v == SelectedVertex)
				return;
			
        	GL.Color3(c);
            GL.Begin(BeginMode.Polygon);

            segments = Math.Max(3, segments);

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
        /// <param name="network">The network to render</param>
        /// <param name="layout">The layout algorithm to use</param>
        /// <param name="colorizer">The colorizer to apply. Default colors will be used if this is set to null (which is the default)</param>
        /// <param name="width">The width of the rendering window. 800 pixels by default</param>
        /// <param name="height">The height of the rendering window. 600 pixels by default</param>
		public static void Start(IRenderableNet network, LayoutProvider layout, NetworkColorizer colorizer = null, int width=800, int height=600)
		{			
			// The actual rendering needs to be done in a separate thread placed in the single thread appartment state
			_mainThread = new Thread(new ThreadStart(new Action(delegate() {				
					Instance =  new Renderer(network, layout, colorizer, width, height);
					_initialized.Set();
					Instance.Run(80f);
            })));

            // Set single thread appartment
            _mainThread.SetApartmentState(ApartmentState.STA);
            _mainThread.Name = "STA Thread for NETVisualizer";
			
			// Fire up the thread and wait until initialization has been completed
            _mainThread.Start();
			_initialized.WaitOne();
		}
		
        /// <summary>
        /// Grabs the current frame and stores it as a bitmap
        /// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
	    private static void GrabImage()
        {
            if (OpenTK.Graphics.GraphicsContext.CurrentContext == null)
                throw new OpenTK.Graphics.GraphicsContextMissingException();
 
            if(_grabbedFrame==null)
				_grabbedFrame = new Bitmap(Instance.ClientSize.Width, Instance.ClientSize.Height);
			
			try {
				lock(_grabbedFrame)
				{
					 System.Drawing.Imaging.BitmapData data =
		             _grabbedFrame.LockBits(Instance.ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, 
							System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					
		            GL.ReadPixels(0, 0, Instance.ClientSize.Width, Instance.ClientSize.Height,PixelFormat.Bgra,
						PixelType.UnsignedByte, data.Scan0);
					
		            _grabbedFrame.UnlockBits(data);
				}
				_frameGrabbingComplete.Set();
			}
			catch {
				Console.WriteLine("Error while copying screen buffer to bitmap.");
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
			Instance._grabbingFrame = true;
			
			// Wait until the frame grabbing is complete
			_frameGrabbingComplete.WaitOne();
			
			if(_grabbedFrame != null && filename != null)
			{
				lock(_grabbedFrame)
					_grabbedFrame.Save(filename);				
			}
			else
			{
				Console.WriteLine("Could not save network image");
			}
			
			Instance._grabbingFrame = false;
			_frameGrabbingComplete.Reset();	
		}
	}
}

