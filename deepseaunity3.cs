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

public class deepseaunity3 : MonoBehaviour
{
    public RenderTexture _renderTextureOutput;
    public ComputeShader _computeShader;
    public Canvas canvas;
    public GameObject go;
    public Sprite bds;
    public Image bdi;
    public GameObject bdg;
    public Texture2D cstex_1 ;
    // public List<Texture2D> textures;
    public Texture2D tex_white_square;
    public Sprite sprite_white_square;
    public GameObject mouse_go;
    public Image mouse_indicator;
    public bool prepared;
    public int g_size;

    void ToTexture2D( RenderTexture rTex, Texture2D tex)
    {
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
    }

    void setup_gpu_stuff()
    {
        _renderTextureOutput = new RenderTexture(this.g_size, this.g_size, 24);
        _renderTextureOutput.filterMode = FilterMode.Point;
        _renderTextureOutput.enableRandomWrite = true;
        _renderTextureOutput.Create();



        this.bdg= new GameObject();
        this.bdg.transform.SetParent(this.canvas.transform);

        this.bdg.transform.position = new Vector3 (   500.0f, 400.0f, 0.0f   ) ;  
        this.bdg.transform.localScale = new Vector3(8.0f, 8.0f, 8.0f);

        this.bdi = this.bdg.AddComponent<Image>();

        this. cstex_1 = new Texture2D(this.g_size, this.g_size);
        // this. cstex_2 = new Texture2D(this.g_size, this.g_size);
        this.bds = Sprite.Create(cstex_1, new Rect(0.0f, 0.0f, this.g_size, this.g_size), new Vector2(this.g_size/2, this.g_size/2), 1.0f);
        this.bdi.sprite = this.bds;

        for(int y = 0; y < this.g_size; y++)
        {
            for(int x = 0; x < this.g_size; x++)
            {
                int addr_here = (y * this.g_size) + x;

                // cstex_1.SetPixel(x, y, new Color(0.0f, 0.0f, 0.0f, 1.0f));
                cstex_1.SetPixel(x, y, new Color(UnityEngine.Random.value, UnityEngine.Random.value, 0.5f, 1.0f));
         
            }
        }
        cstex_1.Apply();

        _computeShader.SetFloat("_Resolution", _renderTextureOutput.width);

        // var main = _computeShader.FindKernel("Airflow");



    }
     
    // Start is called before the first frame update
    void Start()
    {
        this.prepared = false;
        // Content.total_size =  1024 * 1024 ;//   Content.n_animals * Content.neurons_per_animal;
        // Content.square_size = 1024 ;//   (int)(Mathf.Sqrt(Content.total_size));
        this.g_size = 1024;//(int)(Mathf.Pow(2, Mathf.Ceil(Mathf.Log(Content.square_size)/Mathf.Log(2)))); // https://stackoverflow.com/questions/466204/rounding-up-to-next-power-of-2
        // Content.drag_coeff = 1.0f;
        // Content.pi_on_180 = Mathf.PI / 180.0f;
        this.go = new GameObject();
        this.canvas = this.go.AddComponent<Canvas>();
        this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        setup_gpu_stuff();
        // this.textures = new List<Texture2D> { this.cstex_1} ;
        this.tex_white_square = new Texture2D(1, 1);
        this.sprite_white_square = Sprite.Create(tex_white_square, new Rect(0.0f, 0.0f, this.tex_white_square.width, this.tex_white_square.height), new Vector2(this.tex_white_square.width/2, this.tex_white_square.height/2), 1.0f);
        this.mouse_go = new GameObject();
        this.mouse_go.transform.SetParent(this.canvas.transform);
        this.mouse_indicator = this.mouse_go.AddComponent<Image>();
        this.mouse_indicator.sprite = this.sprite_white_square;
        this.mouse_indicator.transform.localScale  = new Vector3(1.0f, 0.1f, 1.0f);
        this.prepared = true;




             this.divergence1_kernel = _computeShader.FindKernel("Divergence1");
        _computeShader.SetTexture(divergence1_kernel, "divergence1_input", this.cstex_1);
        _computeShader.SetTexture(divergence1_kernel, "divergence1_output", _renderTextureOutput);
            _computeShader.GetKernelThreadGroupSizes(divergence1_kernel,   out  divergence1_xgroupsize, out  divergence1_ygroupsize, out  divergence1_zgroupsize);


             this.divergence2_kernel = _computeShader.FindKernel("Divergence2");
        _computeShader.SetTexture(divergence2_kernel, "divergence2_input", this.cstex_1);
        _computeShader.SetTexture(divergence2_kernel, "divergence2_output", _renderTextureOutput);
            _computeShader.GetKernelThreadGroupSizes(divergence2_kernel,   out  divergence2_xgroupsize, out  divergence2_ygroupsize, out  divergence2_zgroupsize);


             this.advect1_kernel = _computeShader.FindKernel("Advect1");
        _computeShader.SetTexture(advect1_kernel, "advect1_input", this.cstex_1);
        _computeShader.SetTexture(advect1_kernel, "advect1_output", _renderTextureOutput);
            _computeShader.GetKernelThreadGroupSizes(advect1_kernel, out  advect1_xgroupsize, out  advect1_ygroupsize, out  advect1_zgroupsize);


             this.diffuse1_kernel = _computeShader.FindKernel("Diffuse1");
        _computeShader.SetTexture(diffuse1_kernel, "diffuse1_input", this.cstex_1);
        _computeShader.SetTexture(diffuse1_kernel, "diffuse1_output", _renderTextureOutput);
            _computeShader.GetKernelThreadGroupSizes(diffuse1_kernel, out  diffuse1_xgroupsize, out  diffuse1_ygroupsize, out  diffuse1_zgroupsize);
        




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
    void Update()
    {
        if (this.prepared)
        {
            _computeShader.Dispatch(divergence1_kernel, _renderTextureOutput.width / (int) this.divergence1_xgroupsize, _renderTextureOutput.height / (int) this.divergence1_ygroupsize,
                1);
            ToTexture2D( this._renderTextureOutput, this.cstex_1);

//    var 
            _computeShader.Dispatch(divergence2_kernel, _renderTextureOutput.width / (int) this.divergence2_xgroupsize, _renderTextureOutput.height / (int) this.divergence2_ygroupsize,
                1);
            ToTexture2D( this._renderTextureOutput, this.cstex_1);


            // _computeShader.Dispatch(diffuse1_kernel, _renderTextureOutput.width / (int) this.advect1_xgroupsize, _renderTextureOutput.height / (int) this.advect1_ygroupsize,
            //     1);
            // ToTexture2D( this._renderTextureOutput, this.cstex_1);











            // ToTexture2D( this._renderTextureOutput2, this.cstex_2);



            float finfx=UnityEngine.Random.value;
            float finfy=UnityEngine.Random.value;
            if (Input.GetKey(KeyCode.Space))
            {

                const int bloopsize = 100;
 for(int y = -bloopsize/2; y < bloopsize/2; y++)
        {
            for(int x = -bloopsize/2; x < bloopsize/2; x++)
            {
                // int addr_here = (100 * this.g_size)  + 100 + (y * this.g_size) + x;

                float fx = (float)x;
                float fy = (float)y;

                if (Mathf.Sqrt( (fx*fx) + (fy*fy)  )  < bloopsize/2)
                {
                    
                cstex_1.SetPixel(this.g_size/2 + x, this.g_size/2 + y, new Color( finfx ,finfy,0.0f,1.0f) );
                }

         
            }
        }
        cstex_1.Apply();
            }


        }
    }
}
