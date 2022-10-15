using Godot;
using System;
using System.Collections.Generic;

public class Ocean : StaticBody2D
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
	private float depth = 300;
	[Export]
	private int detail = 64;

	[Export]
	private Material material;
	[Export]
	private float waveNoiseAmp = 300f;
	[Export]
	private float waveNoise2Amp = 50f;
	[Export]
	private float waveAmp = 100f;

	[Export]
	private float waveFreq = 5f;
	private MeshInstance2D visual;
	private CollisionPolygon2D collision;
	private CullRenderRange cull;

	private List<float> heightMap = new List<float>();

	private List<Vector2> verts = new List<Vector2>();
	private List<Vector2> uvs = new List<Vector2>();
	private List<Vector3> normals = new List<Vector3>();

	private OpenSimplexNoise noise = new OpenSimplexNoise();

	private List<Vector2> p = new List<Vector2>();

	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		visual = GetNode<MeshInstance2D>(new NodePath("./MeshInstance2D"));
		collision = FindNode("CollisionPolygon2D") as CollisionPolygon2D;
		cull = GetNode<CullRenderRange>(new NodePath("./CullRenderRange"));

		noise.Seed = seed;
		noise.Octaves = 4;
		noise.Period = 20f;
		noise.Persistence = 0.8f;

		CreateCollisionMesh();

		updateTimer = updateRateOffset;
	}

	private float GetSineNoise(float x_){
		var xWorld_ = width/((float)detail)*x_;
		if(!XInRange(xWorld_)){return 1;}
		var time = OS.GetTicksMsec()/1000f;
		return (noise.GetNoise2d(x_/10f*waveFreq,time)-1f)*waveNoiseAmp+
		(noise.GetNoise2d(x_*2f*waveFreq+2.1f,time+202.13f)-1f)*waveNoise2Amp+
		(Mathf.Sin(x_*.33f*waveFreq+time*5.1f+3f)-1f)*waveAmp*.34f+
		(Mathf.Sin(x_*.1f*waveFreq+time*2.5f)-1f)*waveAmp;
	}


	


	public override void _PhysicsProcess(float delta){
		updateTimer++;
		if(updateTimer%updateRate==0){
			UpdateMeshes();
			updateTimer=0;
		}
	}


	private void UpdateMeshes(){
		UpdateCollisionMesh();
		UpdateVisualMesh();
	}

	private bool XInRange(float x_){
		return cull.CameraInRange(x_+GlobalPosition.x);
	}

	private void CreateCollisionMesh(){
		List<Vector2> p = new List<Vector2>();
		heightMap.Clear();
		
		for(int x=0;x<detail;x++){
			var x_ = width/((float)detail)*x;
			var height_ = GetSineNoise(x);
			p.Add(new Vector2(x_,height_));
			heightMap.Add(height_);
		}
		p.Add(new Vector2(width,depth));
		p.Add(new Vector2(0,depth));

		collision.Polygon = p.ToArray();
	}

	private void UpdateCollisionMesh(){
		p.Clear();
		for(int x=0;x<detail;x++){
			var x_ = width/((float)detail)*x;
			if(!XInRange(x_) && x!=0 && x!=detail-1){continue;}
			var height_ = GetSineNoise(x);
			p.Add(new Vector2(x_,height_));
			heightMap[x] = height_;
		}
		p.Add(new Vector2(width,depth));
		p.Add(new Vector2(0,depth));

		collision.Polygon = p.ToArray();
	}

	private void UpdateVisualMesh(){
		verts.Clear();
		normals.Clear();
		uvs.Clear();
		var uvWidth = (int)Math.Floor(detail*(depth/width));
		for(int i=0;i<detail-1;i++){
			var x_ = width/((float)detail)*i;
			if(!XInRange(x_) && i!=0 && i!=detail-2){continue;}
			var y_ = heightMap[i];
			var uvX_ = (i%uvWidth)/((float)uvWidth);
			var x2_ = width/((float)detail)*(i+1);
			var y2_ = heightMap[i+1];
			var uvX2_ = ((i+1)%(uvWidth))/((float)uvWidth);
			if(uvX2_==0){
				uvX2_ = 1;
			}
			normals.Add(new Vector3(0,0,1));
			uvs.Add(new Vector2(uvX_,0));
			verts.Add(new Vector2(x_,y_));

			normals.Add(new Vector3(0,0,1));
			uvs.Add(new Vector2(uvX_,1));
			verts.Add(new Vector2(x_,depth));

			normals.Add(new Vector3(0,0,1));
			uvs.Add(new Vector2(uvX2_,0));
			verts.Add(new Vector2(x2_,y2_));

			normals.Add(new Vector3(0,0,1));
			uvs.Add(new Vector2(uvX2_,0));
			verts.Add(new Vector2(x2_,y2_));

			normals.Add(new Vector3(0,0,1));
			uvs.Add(new Vector2(uvX2_,1));
			verts.Add(new Vector2(x2_,depth));

			normals.Add(new Vector3(0,0,1));
			uvs.Add(new Vector2(uvX_,1));
			verts.Add(new Vector2(x_,depth));

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
