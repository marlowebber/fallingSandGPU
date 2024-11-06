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
    public enum BodyParts
    {
        None,
        Head,
        LeftArm,
        RightArm,
        LeftHand,
        RightHand,
        Chest,
        Abdomen,
        LeftLeg,
        RightLeg,
        LeftFoot,
        RightFoot,

    };
    public static Dictionary<char, BodyParts> part_display = new Dictionary<char, BodyParts>
    {
        {  ' ',  BodyParts.None        },
        {  'o',  BodyParts.Head        },
        {  'a',  BodyParts.LeftArm     },
        {  'A',  BodyParts.RightArm    },
        {  'h',  BodyParts.LeftHand    },
        {  'H',  BodyParts.RightHand   },
        {  'T',  BodyParts.Chest       },
        {  'G',  BodyParts.Abdomen     },
        {  'l',  BodyParts.LeftLeg     },
        {  'L',  BodyParts.RightLeg    },
        {  'f',  BodyParts.LeftFoot    },
        {  'F',  BodyParts.RightFoot   },
    };
    public static   Dictionary<string, string[][] > poses = new  Dictionary<string, string[][] > 
    {
        {
            "waving",
            new string [][]
            {

                new string []
                {
                    "      ",
                    "  oH  ",
                    " aTA  ",
                    "h G   ",
                    " l L  ",
                    " f F  ",
                },
                 new string []
                {
                    "      ",
                    "  o H ",
                    " aTA  ",
                    "h G   ",
                    " l L  ",
                    " f F  ",
                }
                ,
                  new string []
                {
                    "      ",
                    "  o   ",
                    " aTAH ",
                    "h G   ",
                    " l L  ",
                    " f F  ",
                }
                ,
                 new string []
                {
                    "      ",
                    "  o H ",
                    " aTA  ",
                    "h G   ",
                    " l L  ",
                    " f F  ",
                }
                ,
            }
        }
    };

    public static float landscape(float x)
    {
        return Mathf.PerlinNoise(x * 0.014f, x* 0.267f);
    }
}

public class Dude
{
    public Dictionary<Content.BodyParts, bool> parts;
    public Vector2 position;
    public string pose;
    public int pose_phase;
    public int pose_frames;
    public GameObject go;
    public Image img;
    public Texture2D tex;
    public Sprite spr;

    public Dude(Vector2 npos , deepseaunity3 world)
    {
        this.tex = new Texture2D(6, 6);
        this.spr = Sprite.Create(this.tex, new Rect(0.0f, 0.0f, 6, 6), new Vector2(3, 3), 1.0f);
        this.go = new GameObject();
        this.go.transform.SetParent(world.canvas.transform);
        this.img = this.go.AddComponent<Image>();
        this.img.sprite = this.spr;
        this.parts = new Dictionary<Content.BodyParts, bool>();
        this.position = npos;
        this.pose = "waving";
        this.pose_phase = 0;
        this.pose_frames = 0;

        int herey = 0;
        foreach(string s in Content.poses[this.pose][this.pose_phase])
        {
            int herex = 0;
            foreach(char c in s)
            {
                this.tex.SetPixel(herex, 6 - herey - 1, new Color(0.0f,0.0f,0.0f,0.0f));
                if ( c != ' ' )
                {
                    this.tex.SetPixel(herex, 6 - herey - 1, new Color(0.0f,0.0f,0.0f,1.0f));

                    this.parts.Add( Content.part_display[c], true  );
                }
                herex++;
            }
            herey++;
        }

        this.tex.Apply();   
        this.tex.filterMode = FilterMode.Point;
        const float vscale = 100.0f;
        this.go.transform.localScale = new Vector3(6/vscale, 6/vscale, 1.0f);
    }
}
public class deepseaunity3 : MonoBehaviour
{
    public RenderTexture _renderTextureOutputA;

    public RenderTexture _renderTextureInputA;

    public RenderTexture _renderTextureDisplay;
    public List<Dude> vehicles;
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
    public float zoom;
    void ToTexture2D( RenderTexture rTex, Texture2D tex)
    {
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
    }
    void setup_gpu_stuff()
    {
        _renderTextureInputA = new RenderTexture(this.g_size, this.g_size, 24, RenderTextureFormat.ARGBFloat);
        _renderTextureInputA.filterMode = FilterMode.Point;
        _renderTextureInputA.enableRandomWrite = true;
        _renderTextureInputA.Create();

        _renderTextureOutputA = new RenderTexture(this.g_size, this.g_size, 24, RenderTextureFormat.ARGBFloat);
        _renderTextureOutputA.filterMode = FilterMode.Point;
        _renderTextureOutputA.enableRandomWrite = true;
        _renderTextureOutputA.Create();

        _renderTextureDisplay = new RenderTexture(this.g_size, this.g_size, 24, RenderTextureFormat.ARGBFloat);
        _renderTextureDisplay.filterMode = FilterMode.Point;
        _renderTextureDisplay.enableRandomWrite = true;
        _renderTextureDisplay.Create();

        this.bdg = new GameObject();
        this.bdg.transform.SetParent(this.canvas.transform);
        this.bdi = this.bdg.AddComponent<Image>();
        this. cstex_1 = new Texture2D(this.g_size, this.g_size, TextureFormat.RGBAFloat, -1, false);
        this. cstex_2= new Texture2D(this.g_size, this.g_size, TextureFormat.RGBAFloat, -1, false);

        this.bds = Sprite.Create(this.cstex_2, new Rect(0.0f, 0.0f, this.g_size, this.g_size), new Vector2(this.g_size/2, this.g_size/2), 1.0f);
        this.bdi.sprite = this.bds;
        for(int y = 0; y < this.g_size; y++)
        {
            for(int x = 0; x < this.g_size; x++)
            {
                cstex_1.SetPixel(x, y, new Color( 0.0f,  0.0f, 0.25f, 0.25f));   

            }
        }
        cstex_1.Apply();
        Graphics.Blit( cstex_1, _renderTextureInputA);

        Graphics.Blit( cstex_1, _renderTextureOutputA);

        _computeShader.SetFloat("_Resolution", _renderTextureOutputA.width);
    }
    void setup_world()
    {
        for(int i = 0 ; i < 10; i++)
        {
            this.vehicles.Add( new Dude( new Vector2((float)i * 10.0f, 1.0f), this) );
        }
    }
    void controls()
    {
        if (Input.GetKey(KeyCode.Minus))
        {
            this.zoom *= 0.9f;
        }
        if (Input.GetKey(KeyCode.Equals))
        {
            this.zoom *= 1.1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            vehicles[0].position.x -= 1.0f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            vehicles[0].position.x += 1.0f;
        }

            Vector3 mousePos = Input.mousePosition;
            if (Input.GetMouseButton(0))
            {
                for(int yy = 0 ; yy < 10; yy++)
                { 
                    for(int xx = 0 ; xx < 10; xx++)
                    {
                        int herex = (int) mousePos.x + xx;
                        int herey = (int) mousePos.y + yy;
                        Color moo = cstex_1.GetPixel( herex , herey );

                        moo.b += 1.0f;

                        cstex_1.SetPixel(herex, herey, moo);
                    }
                }
            }

        if (Input.GetMouseButtonDown(1))
        {

        }

        if (Input.GetMouseButtonDown(2))
        {

        }
    }
    void Start()
    {
        this.prepared = false;
        this.vehicles = new List<Dude>();
        this.g_size = 512;
        this.zoom = 10.0f;
        this.go = new GameObject();
        this.canvas = this.go.AddComponent<Canvas>();
        setup_gpu_stuff();
        this.setup_world();
        this.divergence1_kernel = _computeShader.FindKernel("Particle");
        _computeShader.SetFloat( "particle_rand",  (UnityEngine.Random.value - 0.5f) * 2.0f);

        _computeShader.SetTexture(divergence1_kernel, "particle_input_a",   _renderTextureInputA);

        _computeShader.SetTexture(divergence1_kernel, "particle_output_a",   _renderTextureOutputA);

        _computeShader.SetTexture(divergence1_kernel, "particle_display",  _renderTextureDisplay);

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
    int count = 0;

    void Update()
    {
        if (this.prepared)
        {
            const float k = 0.01f;
            this.zoom *= 1.0f + (Input.mouseScrollDelta.y * 0.1f);
            _computeShader.Dispatch(divergence1_kernel, _renderTextureOutputA.width / (int) this.divergence1_xgroupsize, _renderTextureOutputA.height / (int) this.divergence1_ygroupsize,
                1);
            ToTexture2D( this._renderTextureOutputA, this.cstex_1);
            ToTexture2D( this._renderTextureDisplay, this.cstex_2);

            Camera.main.transform.position = new Vector3(0.0f, 0.0f, -this.zoom); 
            Camera.main.orthographicSize = this.zoom;

            controls();
            for( int jj = 0; jj < this.vehicles.Count; jj++ )
            {
                Dude v = this.vehicles[jj];  
                int p = 0;
                foreach(string s in Content.poses[v.pose][v.pose_phase]  )
                {
                    foreach(char c in s )
                    {
                        if (c != ' ')
                        {
                            Content.BodyParts b  = Content.part_display[c];
                            int herex = (int) (v.position.x + p % 6);
                            int herey = (int)( v.position.y + p / 6) ;
                            Color moo = cstex_1.GetPixel( herex , herey );
                            const float k_water_coupling = 0.5f;
                            Vector2 water_v = new Vector2(moo.r , moo.g ) ;
                            Vector2 adjust = -water_v * k_water_coupling;
                            moo.r -= adjust.x;
                            moo.g -= adjust.y;
                            cstex_1.SetPixel(herex, herey, moo);
                        }
                        p++;
                    }
                }
                v.pose_frames++;
                if (v.pose_frames > 8)
                {
                    v.pose_frames = 0;
                    v.pose_phase++;
                    if (v.pose_phase >= Content.poses[v.pose].Length)
                    {
                        v.pose_phase = 0;
                    }
                }
                v.go.transform.position = new Vector3( v.position.x, v.position.y, 0.0f );
            }
            _computeShader.SetFloat( "advx",  0.0f );
            _computeShader.SetFloat( "advy",  0.0f );
            cstex_1.Apply();

            Graphics.Blit( cstex_1, _renderTextureInputA);

        }
    }
}