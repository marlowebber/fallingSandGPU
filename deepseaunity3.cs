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

    public HingeJoint2D joint;
    public int connectedTo;


    public Segment(Vector2 pos)
    {
        int psize = 1;
        this.tex = new Texture2D(psize, psize);
        this.sprite = Sprite.Create(this.tex, new Rect(0.0f, 0.0f, psize, psize), new Vector2(psize/2, psize/2), 1);
        this.go = new GameObject();
        this.sr =  this.go.AddComponent<SpriteRenderer>();
        this.sr.sprite = this.sprite;
        this.rb = this.go.AddComponent<Rigidbody2D>();
        this.rb.gravityScale = 0.0f;

        this.joint = this.go.AddComponent<HingeJoint2D>();

        this.connectedTo = UnityEngine.Random.Range(0,8);

        this.go.transform.localScale = new Vector3( 1.0f , 1.0f, this.go.transform.localScale.z);

        this.go.transform.position = pos;//new Vector3(0.0f, 0.0f, 0.0f);
    }
}




class Animal
{
    public Segment [] segments = new Segment [8];
    public int index;


    void update_segment_joints()
    {

        for(int i = 0; i<8;i++)
        {
            
            JointMotor2D jmo = new JointMotor2D();
            jmo.maxMotorTorque = 1000.0f;
            jmo.motorSpeed = 0.0f;

            this.segments[i].joint.enableCollision = false;
            this.segments[i].joint.breakForce = Mathf.Infinity;
            this.segments[i].joint.autoConfigureConnectedAnchor = false;
            this.segments[i].joint.connectedBody = this.segments[segments[i].connectedTo].rb;
            this.segments[i].joint.anchor = new Vector2(0.0f, (this.segments[i].go.transform.localScale.x/2 ));
            this.segments[i].joint.connectedAnchor = new Vector2(0.0f, -(this.segments[segments[i].connectedTo].go.transform.localScale.x/2));
            this.segments[i].joint.useMotor = true;
            this.segments[i].joint.motor = jmo;
            this.segments[i].joint.useLimits = false;

        }

        // this.segments[0].rb.AddForce(new Vector2(100.0f, 1.0f));
    }


    public Animal(int new_index)
    {
        this.index = new_index;
        for(int i = 0; i<8;i++)
        {
            this.segments[i] = new Segment(new Vector2((float)i, 0.0f));
        }
        update_segment_joints();
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

    public void run(ComputeShader _computeShader, List<Texture2D> inputs)
    {
     _computeShader.SetFloat("_Resolution", _renderTextureOutput.width);

        var main = _computeShader.FindKernel("Airflow");

        _computeShader.SetFloat( "airflow_rand", UnityEngine.Random.value);
        _computeShader.SetTexture(main, "airflow_input", inputs[0]);
        _computeShader.SetTexture(main, "airflow_output", _renderTextureOutput);

        _computeShader.GetKernelThreadGroupSizes(main, out uint xGroupSize, out uint yGroupSize, out uint zGroupSize);
        _computeShader.Dispatch(main, _renderTextureOutput.width / (int) xGroupSize, _renderTextureOutput.height / (int) yGroupSize,
            1);


            /*


the network is made of 5 textures using their RGBA channels to store data.

each pixel represents 1 neuron.

connection address is from 0 to neurons_per_animal.



how big is the texture?

say there are 1024 animals and each one has 64 neurons. that means 65536 neurons total.
sqrt(65536) is 256.





1.
R: current value
G: bias
B: connection address 1
A: connection weight 1

2.
R: connection address 2
G: connection weight 2
B: connection address 3
A: connection weight 3

3.
R: connection address 4
G: connection weight 4
B: connection address 5
A: connection weight 5

4.
R: connection address 6
G: connection weight 6
B: connection address 7
A: connection weight 7

5.
R: connection address 8
G: connection weight 8
B: nothing
A: nothing

6.
output texture with updated values which replaces texture 1 next round.


*/


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


    void setup_fishies()
    {

        // Segment moo = new Segment();
        // Segment moot = new Segment();
        // moot.go.transform.position = new Vector3 (   moot.go.transform.position.x + 5, moot.go.transform.position.y, 0.0f   ) ; 

        Animal shamoo = new Animal(0); 
    }


    void setup_gpu_stuff()
    {
   int g_size = 1024;
        this.csr = new TemplateComputeShaderRunner(g_size);

        this.bdg= new GameObject();
        this.bdg.transform.SetParent(this.canvas.transform);

        this.bdg.transform.position = new Vector3 (   500.0f, 400.0f, 0.0f   ) ;  
        this.bdg.transform.localScale = new Vector3(4.0f, 4.0f, 1.0f);

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
     
    // Start is called before the first frame update
    void Start()
    {

        this.go = new GameObject();
        this.canvas = this.go.AddComponent<Canvas>();
        this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        this.tex_white_square = new Texture2D(1, 1);
        this.sprite_white_square = Sprite.Create(tex_white_square, new Rect(0.0f, 0.0f, this.tex_white_square.width, this.tex_white_square.height), new Vector2(this.tex_white_square.width/2, this.tex_white_square.height/2), 1.0f);
        this.mouse_go = new GameObject();
        this.mouse_go.transform.SetParent(this.canvas.transform);
        this.mouse_indicator = this.mouse_go.AddComponent<Image>();
        this.mouse_indicator.sprite = this.sprite_white_square;
        this.mouse_indicator.transform.localScale  = new Vector3(1.0f, 0.1f, 1.0f);

        setup_fishies();

        setup_gpu_stuff();

     
    }

    // Update is called once per frame
    void Update()
    {
        this.csr.run(shader, new List<Texture2D> { this.moocow_brown } );
        ToTexture2D( this.csr._renderTextureOutput, this.moocow_brown);
    }
}
