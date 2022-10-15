using Godot;
using System;
using System.Collections.Generic;

public class Cloud : StaticBody2D
{
	[Export]
	private int seed = 111;
	[Export]
	private int updateRate = 3;
	[Export]
	private int updateRateOffset = 0;

	private int updateTimer;

	[Export]
	private float width = 500;
	[Export]
	private float height = 300;

	[Export]
	private Vector2 navigationRange = new Vector2(1000,500);
	[Export]
	private float navigationSpeed = 500;
	[Export]
	private float minimumAltitude = 0;
	
	[Export]
	private int detail = 24;

	[Export]
	private Material material;
	[Export]
	private float waveNoiseAmp = 300f;
	[Export]
	private float waveNoise2Amp = 50f;
	private MeshInstance2D visual;
	private CollisionPolygon2D collision;
	private CullRenderRange cull;

	private List<float> heightMap = new List<float>();

	private List<Vector2> verts = new List<Vector2>();
	private List<Vector2> uvs = new List<Vector2>();
	private List<Vector3> normals = new List<Vector3>();

	private OpenSimplexNoise noise = new OpenSimplexNoise();

	private List<Vector2> p = new List<Vector2>();

	private Vector2 origin = new Vector2(0,0);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		origin = GlobalPosition;
		visual = GetNode<MeshInstance2D>(new NodePath("./MeshInstance2D"));
		collision = FindNode("CollisionPolygon2D") as CollisionPolygon2D;
		cull = GetNode<CullRenderRange>(new NodePath("./CullRenderRange"));

		noise.Seed = seed;
		noise.Octaves = 4;
		noise.Period = 20f;
		noise.Persistence = 0.8f;

		CreateCollisionMesh();

		updateTimer = updateRateOffset;

		if(GD.Randf()>.5f){
			navigationSpeed*=-1f;
		}
	}

	private float GetSineNoise(float x_){
		//if(!InRange()){return 1;}
		var time = OS.GetTicksMsec()/1000f;
		return (noise.GetNoise2d(x_/10f,time)-1f)*waveNoiseAmp+
		(noise.GetNoise2d(x_*2f+2.1f,time+202.13f)-1f)*waveNoise2Amp;
	}


	


	public override void _PhysicsProcess(float delta){
		UpdatePosition(delta);
		updateTimer++;
		if(updateTimer%updateRate==0){
			UpdateMeshes();
			updateTimer=0;
		}
	}

	private void UpdatePosition(float delta){
		var time = OS.GetTicksMsec()/1000f;
		var pos = new Vector2(Mathf.Cos(time*delta*navigationSpeed)*navigationRange.x*.5f,Mathf.Sin(time*delta*navigationSpeed)*navigationRange.y*.5f);
		
		GlobalPosition = origin+pos;
		if(GlobalPosition.y>minimumAltitude){
			GlobalPosition = new Vector2(GlobalPosition.x,minimumAltitude);
		}
	}

	private void UpdateMeshes(){
		if(!InRange()){return;}
		UpdateCollisionMesh();
		UpdateVisualMesh();
	}

	private bool InRange(){
		return cull.CameraInRange(GlobalPosition.x,GlobalPosition.y,200);
	}

	private void CreateCollisionMesh(){
		List<Vector2> p = new List<Vector2>();
		heightMap.Clear();
		
		for(int x=0;x<detail;x++){
			var x_ = (float)x/(float)detail*3.14f*2f;
			var height_ = GetSineNoise(x);
			p.Add(GlobalPosition+new Vector2(Mathf.Cos(x_)*width,Mathf.Sin(x_)*height)*height_);
			heightMap.Add(height_);
		}

		collision.Polygon = p.ToArray();
	}

	private void UpdateCollisionMesh(){
		p.Clear();

		for(int x=0;x<detail;x++){
			var x_ = (float)x/(float)detail*3.14f*2f;
			var height_ = GetSineNoise(x);
			p.Add(GlobalPosition+new Vector2(Mathf.Cos(x_)*width,Mathf.Sin(x_)*height)*height_);
			heightMap[x] = height_;
		}

		collision.Polygon = p.ToArray();
	}

	private void UpdateVisualMesh(){
		verts.Clear();
		normals.Clear();
		uvs.Clear();
		for(int i=0;i<detail-1;i++){
			var x_ = (float)i/(float)detail*3.14f*2f;
			var xUV = Mathf.Abs(Mathf.Cos(x_));
			var yUV = 1f-(Mathf.Sin(x_)+1f)/2f;

			var x2_ = (float)(i+1)/(float)detail*3.14f*2f;
			var x2UV = Mathf.Abs(Mathf.Cos(x2_));
			var y2UV = 1f-(Mathf.Sin(x2_)+1f)/2f;


			normals.Add(new Vector3(0,0,1));
			uvs.Add(new Vector2(xUV,yUV));
			verts.Add(p[i]);
			normals.Add(new Vector3(0,0,1));
			uvs.Add(new Vector2(x2UV,y2UV));
			verts.Add(p[i+1]);
			normals.Add(new Vector3(0,0,1));
			uvs.Add(new Vector2(.5f,.5f));
			verts.Add(GlobalPosition);

			if(i==0){
				var x3_ = (float)(detail-1)/(float)detail*3.14f*2f;
				var x3UV = Mathf.Abs(Mathf.Cos(x3_));
				var y3UV = 1f-(Mathf.Sin(x3_)+1f)/2f;

				normals.Add(new Vector3(0,0,1));
				uvs.Add(new Vector2(xUV,yUV));
				verts.Add(p[i]);
				normals.Add(new Vector3(0,0,1));
				uvs.Add(new Vector2(x3UV,y3UV));
				verts.Add(p[detail-1]);
				normals.Add(new Vector3(0,0,1));
				uvs.Add(new Vector2(.5f,.5f));
				verts.Add(GlobalPosition);
			}

		}
		ArrayMesh arrayMesh = new ArrayMesh();
		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)ArrayMesh.ArrayType.Max);
		arrays[(int)ArrayMesh.ArrayType.Vertex] = verts.ToArray();
		arrays[(int)ArrayMesh.ArrayType.TexUv] = uvs.ToArray();
		arrays[(int)ArrayMesh.ArrayType.Normal] = normals.ToArray();
		arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles,arrays);
		//arrayMesh.SurfaceSetMaterial(0,material);
		visual.Mesh = arrayMesh;
	}
	

}
