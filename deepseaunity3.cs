using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using TMPro;


public static class Content
{


public static string [] test_sub = new string [] 
{

    "         =         ",
    "        ===        ",
    "       =====       ",
    "       =====       ",
    "      =======      ",
    "      =======      ",
    "      =======      ",
    "     =========     ",
    "     =========     ",
    "     =========     ",
    "     =========     ",
    "     =========     ",
    "     =========     ",
    "     =========     ",
    "     =========     ",
    "     =========     ",
    "     =========     ",
    "     =========     ",
    "     =========     ",
    "     =========     ",
    "===================",
    "===================",
    "===================",
    "===================",
    "     PPPPPPPPP     ",
    "     PPPPPPPPP     ",

};





public enum Materials
{
    Air,
    Steel,
    Propeller,
};

public static Dictionary<char,Materials> material_display = new Dictionary<char,Materials>()
{
  {' ', Materials.Air},
  {'=', Materials.Steel},
  {'P', Materials.Propeller},
};



}



public class Part
{
    public Content.Materials mat;
    public bool border;

    public Part(Content.Materials pmat)
    {
        this.mat = pmat;
        this.border = false;
    }

}



public class Vehicle
{
    public int size_x;
    public int size_y;
    

    public GameObject go;
    public Image img;
    public Rigidbody2D rb;
    public BoxCollider2D col;
    public Texture2D tex;
    public Sprite spr;


    public float throttle;
    public float steering_angle;

    public Part [] parts;



    public void part_check_border(int i)
    {
            this.parts[i].border = false;
                if (this.parts[i].mat != Content.Materials.Air)
                {
                    for(int xx = -1; xx <= 1; x++)
                    {
                        for(int yy = -1; y <= 1; y++)
                        {
                            if (xx!=0 && yy !=0)
                            {
                                int nbr = ( (y+yy) * size_x  ) + (x + xx);
                                if (nbr >= 0 && nbr < (size_x*size_y))
                                {
                                    if (parts[nbr].mat != Content.Materials.Air)
                                    {
                                        parts[i].border = true;
                                    }
                                }
                            }
                        }
                    }
                }
    }

 

    public void detect_borders()
    {
        foreach (Part p in this.parts)
        {
            this.part_check_border(p);
        }
    }



    public Vehicle(string [] blueprint, Vector2 position , deepseaunity3 world)
    {

        this.size_x = blueprint[0].Length;
        this.size_y = blueprint.Length;

        this.tex = new Texture2D(this.size_x, this.size_y);
        this.spr = Sprite.Create(this.tex, new Rect(0.0f, 0.0f, this.size_x, this.size_y), new Vector2(this.size_x/2, this.size_y/2), 1.0f);
        this.go = new GameObject();
        this.go.transform.SetParent(world.canvas.transform);
        this.img = this.go.AddComponent<Image>();
        this.img.sprite = this.spr;
        this.col = this.go.AddComponent<BoxCollider2D>();
        this.rb = this.go.AddComponent<Rigidbody2D>();
        this.rb.gravityScale = 0.0f;
        this.rb.transform.position = new Vector3((world.g_size/2) + position.x, (world.g_size/2 )+ position.y, 0.0f);
        this.rb.velocity = new Vector3(1.0f, 0.0f, 0.0f);

        this.parts = new Part [this.size_x * this.size_y];

        int herey = 0;
        foreach(string s in blueprint)
        {
            int herex = 0;
            foreach(char c in s)
            {
                this.tex.SetPixel(herex, herey, new Color(0.0f,0.0f,0.0f,0.0f));

                Content.Materials pmat = Content.material_display[c] ;
                if ( pmat != Content.Materials.Air  )
                {

                    this.tex.SetPixel(herex, herey, new Color(1.0f,1.0f,1.0f,1.0f));
                }
                int i = (herey * this.size_x) + herex;
                this.parts[i] = new Part(pmat);
                herex++;
            }
            herey++;
        }
        this.tex.Apply();   


        const float vscale = 100.0f;
        this.go.transform.localScale = new Vector3(this.size_x/vscale, this.size_y/vscale, 1.0f);

        this.detect_borders();

        Debug.Log("New ! of size " + this.size_x.ToString() + " " + this.size_y.ToString());
    }

}



public class deepseaunity3 : MonoBehaviour
{
    public RenderTexture _renderTextureOutput1;
    public RenderTexture _renderTextureOutput2;
    public RenderTexture _renderTextureOutput3;
    public RenderTexture _renderTextureOutput4;


    public List<Vehicle> vehicles;


    public ComputeShader _computeShader;
    public Canvas canvas;
    public GameObject go;
    public Sprite bds;
    public Image bdi;
    public GameObject bdg;
    public Texture2D cstex_1 ;
    public Texture2D cstex_2 ;

    public bool prepared;
    public int g_size;

    public const float fluid_scale = 10.0f;

    void ToTexture2D( RenderTexture rTex, Texture2D tex)
    {
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
    }

    void setup_gpu_stuff()
    {
        _renderTextureOutput1 = new RenderTexture(this.g_size, this.g_size, 24, RenderTextureFormat.ARGBFloat);
        _renderTextureOutput1.filterMode = FilterMode.Point;
        _renderTextureOutput1.enableRandomWrite = true;
        _renderTextureOutput1.Create();

        _renderTextureOutput2 = new RenderTexture(this.g_size, this.g_size, 24, RenderTextureFormat.ARGBFloat);
        _renderTextureOutput2.filterMode = FilterMode.Point;
        _renderTextureOutput2.enableRandomWrite = true;
        _renderTextureOutput2.Create();


        _renderTextureOutput3 = new RenderTexture(this.g_size, this.g_size, 24, RenderTextureFormat.ARGBFloat);
        _renderTextureOutput3.filterMode = FilterMode.Point;
        _renderTextureOutput3.enableRandomWrite = true;
        _renderTextureOutput3.Create();

   
        _renderTextureOutput4 = new RenderTexture(1, 1, 24, RenderTextureFormat.ARGBFloat);
        _renderTextureOutput4.filterMode = FilterMode.Point;
        _renderTextureOutput4.enableRandomWrite = true;
        _renderTextureOutput4.Create();


        this.bdg = new GameObject();
        this.bdg.transform.SetParent(this.canvas.transform);

        this.bdg.transform.position = new Vector3 (   this.g_size/2, this.g_size/2, 0.0f   ) ;  
        this.bdg.transform.localScale = new Vector3(fluid_scale, fluid_scale, 1.0f);

        this.bdi = this.bdg.AddComponent<Image>();

        this. cstex_1 = new Texture2D(this.g_size, this.g_size, TextureFormat.RGBAFloat, -1, false);
        this. cstex_2 = new Texture2D(1, 1, TextureFormat.RGBAFloat, -1, false);

        this.bds = Sprite.Create(this.cstex_1, new Rect(0.0f, 0.0f, this.g_size, this.g_size), new Vector2(this.g_size/2, this.g_size/2), 1.0f);
        this.bdi.sprite = this.bds;

        for(int y = 0; y < this.g_size; y++)
        {
            for(int x = 0; x < this.g_size; x++)
            {
                int addr_here = (y * this.g_size) + x;

                float bval = 0.0f;
                if (UnityEngine.Random.value < 0.5f)
                {
                    bval  = 1.0f;
                }

                cstex_1.SetPixel(x, y, new Color(UnityEngine.Random.value * 0.1f, 0.0f, bval, 1.0f));         
            }
        }
        cstex_1.Apply();
        Graphics.Blit( cstex_1, _renderTextureOutput1);
        Graphics.Blit( cstex_1, _renderTextureOutput2);
        Graphics.Blit( cstex_1, _renderTextureOutput3);

        _computeShader.SetFloat("_Resolution", _renderTextureOutput1.width);
    }



    void setup_world()
    {
        for(int i = 0 ; i < 10; i++)
        {

            Vehicle s = new Vehicle( Content.test_sub, new Vector2(UnityEngine.Random.value * 100.0f, UnityEngine.Random.value * 100.0f), this );

            this.vehicles.Add(s);


        }
    }


     
    // Start is called before the first frame update
    void Start()
    {
        this.prepared = false;

        this.vehicles = new List<Vehicle>();
        this.g_size = 1024;
        this.go = new GameObject();
        this.canvas = this.go.AddComponent<Canvas>();
        this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        setup_gpu_stuff();

        this.setup_world();
     

        this.divergence1_kernel = _computeShader.FindKernel("Particle");

        _computeShader.SetFloat( "particle_rand",  (UnityEngine.Random.value - 0.5f) * 2.0f);
        _computeShader.SetTexture(divergence1_kernel, "particle_input",  _renderTextureOutput1);
        _computeShader.SetTexture(divergence1_kernel, "particle_output",  _renderTextureOutput2);
        _computeShader.SetTexture(divergence1_kernel, "rigidbody_information",  _renderTextureOutput4);
        _computeShader.GetKernelThreadGroupSizes(divergence1_kernel,   out  divergence1_xgroupsize, out  divergence1_ygroupsize, out  divergence1_zgroupsize);


        this.prepared = true;
    }

    int divergence1_kernel; 
    uint divergence1_xgroupsize;
    uint divergence1_ygroupsize;
    uint divergence1_zgroupsize;

    int divergence2_kernel;
    uint divergence2_xgroupsize;
    uint divergence2_ygroupsize;
    uint divergence2_zgroupsize;

    int advect1_kernel;
    uint advect1_xgroupsize;
    uint advect1_ygroupsize;
    uint advect1_zgroupsize;
    
    int diffuse1_kernel;
    uint diffuse1_xgroupsize;
    uint diffuse1_ygroupsize;
    uint diffuse1_zgroupsize;

    // Update is called once per frame
    int count = 0;
    void Update()
    {


        if (this.prepared)
        {
            const float k = 0.01f;


            _computeShader.Dispatch(divergence1_kernel, _renderTextureOutput1.width / (int) this.divergence1_xgroupsize, _renderTextureOutput1.height / (int) this.divergence1_ygroupsize,
                1);

            ToTexture2D( this._renderTextureOutput2, this.cstex_1);
        
            // Vector3 mousePos = this.mouse_go.transform.position;





            foreach(Vehicle v in this.vehicles)
            {

                cstex_2.SetPixel(0, 0, new Color(v.rb.velocity.x * k, v.rb.velocity.y * k, 0.0f, 1.0f));





                const float mouse_k = 1.0f;
              
            // int bloopsize = 100;//this.mouse_go.transform.localScale;
            float angle = ((v.go.transform.eulerAngles.z / 360.0f) - (0.5f)) * (2.0f * Mathf.PI) + Mathf.PI;

            float ca = Mathf.Cos(angle);
            float sa = Mathf.Sin(angle);
                
            int hx = v.size_x/2;
            int hy = v.size_y/2;
            // for(int y = -hy; y < hy; y++)
            // {
            //     for(int x = -hx; x < hx; x++)
            //     {
            int i = 0;
            foreach (Part p in v.parts)
            {
                // Debug.Log(p);
            //         Part p = v.parts[  ((hy + y) * v.size_x) + (hx+x)  ]; 
                    if (p.mat != Content.Materials.Air)
                    {

                    int x = (i % v.size_x) - hx;
                    int y = (i / v.size_x) - hy;
                    int herex =  (int)(      ca*(x) - sa*(y)         + (v.go.transform.position.x)       );
                    int herey =  (int)(      ca*(y) + sa*(x)         + (v.go.transform.position.y)      );

                    if (herex >= 0 && herex < g_size && herey >= 0 && herey < g_size)
                    {

                        Color moo = cstex_1.GetPixel(herex, herey);
                        if (p.border)
                        {
                            const float k_reactive = 0.005f;


                            Vector3 surface_v  = v.rb.GetPointVelocity(new Vector3(herex, herey, v.go.transform.position.z) );

                            // surface_v = surface_v / 5.0f;

                            const float f_to_surface_ratio = 100.0f;

                            float compx = ((moo.r *f_to_surface_ratio)- surface_v.x) * k_reactive;
                            float compy = ((moo.g *f_to_surface_ratio)- surface_v.y) * k_reactive;



                            if (p.mat == Content.Materials.Propeller)
                            {

                                if (Input.GetKey("w"))
                                {
                                    compx += ca;
                                    compy += sa;
                                }

                            }








                            v.rb.AddForceAtPosition( new Vector2(compx, compy),    new Vector2(herex, herey) );


                            moo.r -= compx / f_to_surface_ratio;
                            moo.g -= compy / f_to_surface_ratio;
                            // moo.r += ((surface_v.x / f_to_surface_ratio )- moo.r) * k_reactive;
                            // moo.g += ((surface_v.y / f_to_surface_ratio )- moo.g) * k_reactive;

                            // Debug.Log( " moo: " + moo.r.ToString() + ", surface: " + surface_v.x.ToString());
                        }
                        else
                        {
                            moo.a = 0.0f;
                        }

                        cstex_1.SetPixel(herex, herey, moo);

                    }
                    }


            i++;
            }


            //     }
            // }
            }
            // this.vehicles[0].go.transform.position = new Vector2(this.g_size/2, this.g_size/2);

            cstex_2.Apply();
            Graphics.Blit( cstex_2, _renderTextureOutput4);

            // if (Input.GetButton("Fire1"))
            // {
            //         Vector3 mp = Input.mousePosition;
            //         Vector3 pdif = mp - this.mouse_go.transform.position ; 
            //         this.rb.AddForce( pdif );
            // }

            // Debug.Log(mousePos);

 

            cstex_1.Apply();
            Graphics.Blit(this.cstex_1, this._renderTextureOutput1);

        }
    }
}


