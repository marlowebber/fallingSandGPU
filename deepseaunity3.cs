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
    "                 =====          ",
    "                 =   =          ",
    "==================   ========== ",
    "=                             ==",
    "=                              =",
    "=                             ==",
    "=============================== ",

};


}




public class deepseaunity3 : MonoBehaviour
{
    public RenderTexture _renderTextureOutput1;
    public RenderTexture _renderTextureOutput2;
    public RenderTexture _renderTextureOutput3;
    public RenderTexture _renderTextureOutput4;


    public ComputeShader _computeShader;
    public Canvas canvas;
    public GameObject go;
    public Sprite bds;
    public Image bdi;
    public GameObject bdg;
    public Texture2D cstex_1 ;
    public Texture2D cstex_2 ;
    public Texture2D tex_white_square;
    public Sprite sprite_white_square;

    public GameObject mouse_go;
    public Image mouse_indicator;
    public Rigidbody2D rb;
    public BoxCollider2D col;

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
     
    // Start is called before the first frame update
    void Start()
    {
        this.prepared = false;
        this.g_size = 1024;
        this.go = new GameObject();
        this.canvas = this.go.AddComponent<Canvas>();
        this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        setup_gpu_stuff();
        this.tex_white_square = new Texture2D(1, 1);
        this.sprite_white_square = Sprite.Create(tex_white_square, new Rect(0.0f, 0.0f, this.tex_white_square.width, this.tex_white_square.height), new Vector2(this.tex_white_square.width/2, this.tex_white_square.height/2), 1.0f);
     
        this.mouse_go = new GameObject();
        this.mouse_go.transform.SetParent(this.canvas.transform);
        this.mouse_indicator = this.mouse_go.AddComponent<Image>();
        this.mouse_indicator.sprite = this.sprite_white_square;
        this.col = this.mouse_go.AddComponent<BoxCollider2D>();
        this.rb = this.mouse_go.AddComponent<Rigidbody2D>();
        this.rb.gravityScale = 0.0f;
        this.rb.transform.position = new Vector3(this.g_size/2, this.g_size/2, 0.0f);
     
        this.prepared = true;

        this.divergence1_kernel = _computeShader.FindKernel("Particle");

        _computeShader.SetFloat( "particle_rand",  (UnityEngine.Random.value - 0.5f) * 2.0f);
        _computeShader.SetTexture(divergence1_kernel, "particle_input",  _renderTextureOutput1);
        _computeShader.SetTexture(divergence1_kernel, "particle_output",  _renderTextureOutput2);
        _computeShader.SetTexture(divergence1_kernel, "rigidbody_information",  _renderTextureOutput4);
        _computeShader.GetKernelThreadGroupSizes(divergence1_kernel,   out  divergence1_xgroupsize, out  divergence1_ygroupsize, out  divergence1_zgroupsize);
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

            cstex_2.SetPixel(0, 0, new Color(this.rb.velocity.x * k, this.rb.velocity.y * k, 0.0f, 1.0f));
            cstex_2.Apply();
            Graphics.Blit( cstex_2, _renderTextureOutput4);

            _computeShader.Dispatch(divergence1_kernel, _renderTextureOutput1.width / (int) this.divergence1_xgroupsize, _renderTextureOutput1.height / (int) this.divergence1_ygroupsize,
                1);

            ToTexture2D( this._renderTextureOutput2, this.cstex_1);
        
            Vector3 mousePos = this.mouse_go.transform.position;

            if (Input.GetButton("Fire1"))
            {
                    Vector3 mp = Input.mousePosition;
                    Vector3 pdif = mp - this.mouse_go.transform.position ; 
                    this.rb.AddForce( pdif );
            }

            Debug.Log(mousePos);

            const float mouse_k = 1.0f;
              
            int bloopsize = 100;//this.mouse_go.transform.localScale;
            float angle = ((this.mouse_go.transform.eulerAngles.z / 360.0f) - (0.5f)) * (2.0f * Mathf.PI);

            float ca = Mathf.Cos(angle);
            float sa = Mathf.Sin(angle);
                
            int hbloop = bloopsize/2;
            for(int y = -hbloop; y < hbloop; y++)
            {
                for(int x = -hbloop; x < hbloop; x++)
                {
                    int herex =  (int)(      ca*(x) - sa*(y)         + (mousePos.x)         );
                    int herey =  (int)(      ca*(y) + sa*(x)         + (mousePos.y)         );

                    if (herex >= 0 && herex < g_size && herey >= 0 && herey < g_size)
                    {

                        Color moo = cstex_1.GetPixel(herex, herey);
                        if (y == 0 || x == -1 || x == - bloopsize || y  == bloopsize-1 )
                        {
                            const float k_reactive = 0.02f;
                            float compx = moo.r*k_reactive;
                            float compy = moo.g*k_reactive;

                                            this.rb.AddForceAtPosition(  new Vector2(compx, compy) ,    new Vector2(herex, herey) );

                                            moo.r -= compx;
                                            moo.g -= compy;
                        }

                        moo.a = 0.0f;
                        cstex_1.SetPixel(herex, herey, moo);

                    }
                }
            }

            cstex_1.Apply();
            Graphics.Blit(this.cstex_1, this._renderTextureOutput1);

        }
    }
}
