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

class Segment
{
    public Texture2D tex;
    public Sprite sprite;
    public SpriteRenderer sr;
    public GameObject go;
    public Rigidbody2D rb;
    public BoxCollider2D col;

    public Segment()
    {
        int psize = 8;
        this.tex = new Texture2D(psize, psize);
        this.sprite = Sprite.Create(this.tex, new Rect(0.0f, 0.0f, psize, psize), new Vector2(psize/2, psize/2));
        this.go = new GameObject();
        this.sr =  this.go.AddComponent<SpriteRenderer>();
        this.sr.sprite = this.sprite;
        this.rb = this.go.AddComponent<Rigidbody2D>();
        this.col = this.go.AddComponent<BoxCollider2D>();
    }
}



public class TemplateComputeShaderRunner
{
    public RenderTexture _renderTextureOutput;
    public TemplateComputeShaderRunner(int _size)
    {
        _renderTextureOutput = new RenderTexture(_size, _size, 24);
        _renderTextureOutput.filterMode = FilterMode.Point;
        _renderTextureOutput.enableRandomWrite = true;
        _renderTextureOutput.Create();
    }

    public void run(ComputeShader _computeShader, Texture2D input)
    {
     _computeShader.SetFloat("_Resolution", _renderTextureOutput.width);

        var main = _computeShader.FindKernel("Airflow");

        _computeShader.SetFloat( "airflow_rand", UnityEngine.Random.value);
        _computeShader.SetTexture(main, "airflow_input", input);
        _computeShader.SetTexture(main, "airflow_output", _renderTextureOutput);

        _computeShader.GetKernelThreadGroupSizes(main, out uint xGroupSize, out uint yGroupSize, out uint zGroupSize);
        _computeShader.Dispatch(main, _renderTextureOutput.width / (int) xGroupSize, _renderTextureOutput.height / (int) yGroupSize,
            1);
    }

    public void render(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_renderTextureOutput, dest);
    }
}


public class deepseaunity3 : MonoBehaviour
{

    public ComputeShader shader;
    public TemplateComputeShaderRunner csr;
    public Canvas canvas;
    public GameObject go;

    public Sprite bds;
    public Image bdi;
    public GameObject bdg;

    Texture2D moocow_brown ;




    public Texture2D tex_white_square;
    public Sprite sprite_white_square;
    public GameObject mouse_go;
    public Image mouse_indicator;





    void ToTexture2D( RenderTexture rTex, Texture2D tex)
    {
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
    }
     
    // Start is called before the first frame update
    void Start()
    {

        this.go = new GameObject();
        this.canvas = this.go.AddComponent<Canvas>();
        this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        this.tex_white_square = new Texture2D(64, 64);
        this.sprite_white_square = Sprite.Create(tex_white_square, new Rect(0.0f, 0.0f, this.tex_white_square.width, this.tex_white_square.height), new Vector2(0.5f, 0.5f), 100.0f);
        this.mouse_go = new GameObject();
        this.mouse_go.transform.SetParent(this.canvas.transform);
        this.mouse_indicator = this.mouse_go.AddComponent<Image>();
        this.mouse_indicator.sprite = this.sprite_white_square;
        this.mouse_indicator.transform.localScale  = new Vector3(1.0f, 0.1f, 1.0f);


        Segment moo = new Segment();
        Segment moot = new Segment();
        moot.go.transform.position = new Vector3 (   moot.go.transform.position.x + 5, moot.go.transform.position.y, 0.0f   ) ;  

        int g_size = 1024;
        this.csr = new TemplateComputeShaderRunner(g_size);

        this.bdg= new GameObject();
        this.bdg.transform.SetParent(this.canvas.transform);

        this.bdg.transform.position = new Vector3 (   500.0f, 400.0f, 0.0f   ) ;  
        this.bdg.transform.localScale = new Vector3(10.0f, 8.0f, 1.0f);

        this.bdi = this.bdg.AddComponent<Image>();
        
        this. moocow_brown = new Texture2D(g_size, g_size);

        this.bds = Sprite.Create(moocow_brown, new Rect(0.0f, 0.0f, g_size, g_size), new Vector2(g_size/2, g_size/2), 1.0f);
        this.bdi.sprite = this.bds;

        Texture2D initial_conditions = new Texture2D(g_size, g_size);
        for(int i = 0; i < g_size; i++)
        {
            for(int j = 0; j < g_size; j++)
            {
              
                    initial_conditions.SetPixel(i, j, new Color(0.5f, 0.5f, 0.5f, 1.0f));
                





                    if (i > 300 && i < 400)
                    {


                        if (j > 300 && j < 400)
                        {

                            initial_conditions.SetPixel(i, j, new Color(0.5f, 0.5f, 1000000.0f, 1.0f));
                        }
                    }




            }
        }
        initial_conditions.Apply();
        Graphics.Blit(   initial_conditions , this.csr._renderTextureOutput);
        ToTexture2D( this.csr._renderTextureOutput, moocow_brown);
    }

    // Update is called once per frame
    void Update()
    {
        this.csr.run(shader, moocow_brown);
        ToTexture2D( this.csr._renderTextureOutput, moocow_brown);
    }
}
