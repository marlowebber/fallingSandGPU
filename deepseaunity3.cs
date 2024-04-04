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




public class ExampleClass : MonoBehaviour
{
    // public Transform explosionPrefab;
    // public Animal owner;

    public float sensation = 0.0f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        
        // Debug.Log("Cucka Ween");

        this.sensation = 1.0f;

        ExampleClass ea = collision.gameObject.GetComponent<ExampleClass>();

        ea.sensation = 1.0f;


    }

    void OnTriggerLeave2D(Collision2D collision)
    {
        this.sensation = 0.0f;

        ExampleClass ea = collision.gameObject.GetComponent<ExampleClass>();
        ea.sensation = 0.0f;
    }
}


public class Segment
{
    public Texture2D tex;
    public Sprite sprite;
    public SpriteRenderer sr;
    public GameObject go;
    public Rigidbody2D rb;

    public HingeJoint2D joint;
    public int connectedTo;

    public BoxCollider2D col;

    public ExampleClass ea;
    // public float sensation;



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

        // this.sensation = 0.0f;

        this.ea = this.go.AddComponent<ExampleClass>();
        // this.ea.owner = a;

        Debug.Log("Ruu");
        this.connectedTo = UnityEngine.Random.Range(0,Content.segments_per_animal);

        this.col = this.go.AddComponent<BoxCollider2D>();
        this.col.isTrigger = false;
        this.go.transform.localScale = new Vector3( UnityEngine.Random.value * Content.default_segment_size, UnityEngine.Random.value * Content.default_segment_size, this.go.transform.localScale.z);




        this.joint = this.go.AddComponent<HingeJoint2D>();

        this.go.transform.position = pos;//new Vector3(0.0f, 0.0f, 0.0f);

        
    }
}




public class Animal
{
    public Segment [] segments = new Segment [Content.segments_per_animal];
    public int index;


    void update_segment_joints()
    {

        for(int i = 0; i< Content.segments_per_animal;i++)
        {
            
            JointMotor2D jmo = new JointMotor2D();
            jmo.maxMotorTorque = 1000.0f;
            jmo.motorSpeed = 0.1f;


        


            this.segments[i].joint.enableCollision = false;
            this.segments[i].joint.breakForce = Mathf.Infinity;
            this.segments[i].joint.autoConfigureConnectedAnchor = false;

            int connectedTo = segments[i].connectedTo;

            this.segments[i].joint.connectedBody = this.segments[connectedTo].rb;
            this.segments[i].joint.anchor = new Vector2(0.0f, (this.segments[i].go.transform.localScale.x/2 ));
            this.segments[i].joint.connectedAnchor = new Vector2(0.0f, -(this.segments[connectedTo].go.transform.localScale.x/2));
            this.segments[i].joint.useMotor = true;
            this.segments[i].joint.motor = jmo;
            this.segments[i].joint.useLimits = false;

        }

        // this.segments[0].rb.AddForce(new Vector2(100.0f, 1.0f));
    }


    public Animal(int new_index)
    {
        this.index = new_index;
        for(int i = 0; i<Content.segments_per_animal;i++)
        {
            this.segments[i] = new Segment(new Vector2((float)i, 0.0f));
        }
        update_segment_joints();


        Vector3 pos = new Vector3( (UnityEngine.Random.value * Content.arena_size) - (0.5f * Content.arena_size)  ,  (UnityEngine.Random.value * Content.arena_size) - (0.5f * Content.arena_size) , 0.0f);


        for(int i = 0; i<Content.segments_per_animal;i++)
        {
            this.segments[i].go.transform.position =  pos;
        }
    }



}



// public class TemplateComputeShaderRunner
// // {
//     public RenderTexture _renderTextureOutput;
    // public TemplateComputeShaderRunner(int _size)
    // {
    //     _renderTextureOutput = new RenderTexture(_size, _size, 24);
    //     _renderTextureOutput.filterMode = FilterMode.Point;
    //     _renderTextureOutput.enableRandomWrite = true;
    //     _renderTextureOutput.Create();
    // }

    // public void run(ComputeShader _computeShader, List<Texture2D> inputs)
    // {
    //  _computeShader.SetFloat("_Resolution", _renderTextureOutput.width);

    //     var main = _computeShader.FindKernel("NeuralNet");

    //     _computeShader.SetFloat( "nn_rand", UnityEngine.Random.value);
    //     _computeShader.SetTexture(main, "nn_1", inputs[0]);
    //     _computeShader.SetTexture(main, "nn_2", inputs[1]);
    //     _computeShader.SetTexture(main, "nn_3", inputs[2]);
    //     _computeShader.SetTexture(main, "nn_4", inputs[3]);
    //     _computeShader.SetTexture(main, "nn_5", inputs[4]);
    //     _computeShader.SetTexture(main, "nn_out", _renderTextureOutput);

    //     _computeShader.GetKernelThreadGroupSizes(main, out uint xGroupSize, out uint yGroupSize, out uint zGroupSize);
    //     _computeShader.Dispatch(main, _renderTextureOutput.width / (int) xGroupSize, _renderTextureOutput.height / (int) yGroupSize,
    //         1);


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


    // }

    // public void render(RenderTexture src, RenderTexture dest)
    // {
    //     Graphics.Blit(_renderTextureOutput, dest);
    // }
// }

public static class Content
{
    public static int segments_per_animal = 8;
    public static int neurons_per_animal = 64;
    public static int n_animals = 256;
    public static float default_segment_size = 2.0f;
    public static float arena_size = 100;
}


public class deepseaunity3 : MonoBehaviour
{



    public RenderTexture _renderTextureOutput;

    public ComputeShader _computeShader;
    // public TemplateComputeShaderRunner csr;
    public Canvas canvas;
    public GameObject go;

    public Sprite bds;
    public Image bdi;
    public GameObject bdg;

    public Texture2D cstex_1 ;
    public Texture2D cstex_2 ;
    public Texture2D cstex_3 ;
    public Texture2D cstex_4 ;
    public Texture2D cstex_5 ;

    public List<Texture2D> textures;


    public Texture2D tex_white_square;
    public Sprite sprite_white_square;
    public GameObject mouse_go;
    public Image mouse_indicator;


    public int g_size;

    public List<Animal> animals;


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
        this.animals = new List<Animal>();

        for (int i = 0; i < Content.n_animals; i++)
        {

            Animal shamoo = new Animal(i); 
            this.animals.Add(shamoo);
        }


    }


    void setup_gpu_stuff()
     {
         int total_size = Content.n_animals * Content.neurons_per_animal;
         int square_size = (int)(Mathf.Sqrt(total_size));
         this. g_size  = (int)(Mathf.Pow(2, Mathf.Ceil(Mathf.Log(square_size)/Mathf.Log(2)))); // https://stackoverflow.com/questions/466204/rounding-up-to-next-power-of-2
        // this.csr = new TemplateComputeShaderRunner(g_size);

        _renderTextureOutput = new RenderTexture(g_size, g_size, 24);
        _renderTextureOutput.filterMode = FilterMode.Point;
        _renderTextureOutput.enableRandomWrite = true;
        _renderTextureOutput.Create();


        this.bdg= new GameObject();
        this.bdg.transform.SetParent(this.canvas.transform);

        this.bdg.transform.position = new Vector3 (   500.0f, 400.0f, 0.0f   ) ;  
        this.bdg.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        this.bdi = this.bdg.AddComponent<Image>();
        
        // this. moocow_brown = new Texture2D(g_size, g_size);


        this. cstex_1 = new Texture2D(g_size, g_size);
        this. cstex_2 = new Texture2D(g_size, g_size);
        this. cstex_3 = new Texture2D(g_size, g_size);
        this. cstex_4 = new Texture2D(g_size, g_size);
        this. cstex_5 = new Texture2D(g_size, g_size);

        this.bds = Sprite.Create(cstex_1, new Rect(0.0f, 0.0f, g_size, g_size), new Vector2(g_size/2, g_size/2), 1.0f);
        this.bdi.sprite = this.bds;



        for(int i = 0; i < this.g_size; i++)
        {
            for(int j = 0; j < this.g_size; j++)
            {
              
                    // initial_conditions.SetPixel(i, j, new Color(0.5f, 0.5f, 0.5f, 1.0f));
                    // initial_conditions.SetPixel(i, j, new Color(0.0f, 0.0f, 0.0f, 1.0f));
                




                    // for airflow.
                    // if (i > 300 && i < 400)
                    // {
                    //     if (j > 300 && j < 400)
                    //     {
                    //         initial_conditions.SetPixel(i, j, new Color(0.5f, 0.5f, 1000000.0f, 1.0f));
                    //     }
                    // }


/*


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

                    cstex_1.SetPixel(i, j, new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.Range(0,8), (UnityEngine.Random.value - 0.5f) * 20.0f));
                    cstex_2.SetPixel(i, j, new Color(UnityEngine.Random.Range(0,Content.neurons_per_animal), (UnityEngine.Random.value - 0.5f) * 20.0f, UnityEngine.Random.Range(0,Content.neurons_per_animal), (UnityEngine.Random.value - 0.5f) * 20.0f));
                    cstex_3.SetPixel(i, j, new Color(UnityEngine.Random.Range(0,Content.neurons_per_animal), (UnityEngine.Random.value - 0.5f) * 20.0f, UnityEngine.Random.Range(0,Content.neurons_per_animal), (UnityEngine.Random.value - 0.5f) * 20.0f));
                    cstex_4.SetPixel(i, j, new Color(UnityEngine.Random.Range(0,Content.neurons_per_animal), (UnityEngine.Random.value - 0.5f) * 20.0f, UnityEngine.Random.Range(0,Content.neurons_per_animal), (UnityEngine.Random.value - 0.5f) * 20.0f));
                    cstex_5.SetPixel(i, j, new Color(UnityEngine.Random.Range(0,Content.neurons_per_animal), (UnityEngine.Random.value - 0.5f) * 20.0f, UnityEngine.Random.Range(0,Content.neurons_per_animal), (UnityEngine.Random.value - 0.5f) * 20.0f));



            }
        }
        cstex_1.Apply();
        cstex_2.Apply();
        cstex_3.Apply();
        cstex_4.Apply();
        cstex_5.Apply();

    }
     
    // Start is called before the first frame update
    void Start()
    {

        this.textures = new List<Texture2D> { this.cstex_1,this.cstex_2,this.cstex_3,this.cstex_4,this.cstex_5  } ;

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
        // this.csr.run(shader, this.textures);




   for (int i = 0; i < Content.n_animals; i++)
        {


            int index = i * Content.neurons_per_animal;

             for (int j = 0; j < Content.segments_per_animal; j++)
            {
                int here = index + j + Content.segments_per_animal;

                int x = here % this.g_size;
                int y = here / this.g_size;

                Color pixel = this.cstex_1.GetPixel(x,y);

                this.cstex_1.SetPixel(x,y, new Color(  this.animals[i].segments[j].ea.sensation,  pixel.g, pixel.b, pixel.a  )  );

            }

        }
        this.cstex_1.Apply();




             _computeShader.SetFloat("_Resolution", _renderTextureOutput.width);

        var main = _computeShader.FindKernel("NeuralNet");

        _computeShader.SetFloat( "nn_rand", UnityEngine.Random.value);
        _computeShader.SetTexture(main, "nn_1", this.cstex_1);
        _computeShader.SetTexture(main, "nn_2", this.cstex_2);
        _computeShader.SetTexture(main, "nn_3", this.cstex_3);
        _computeShader.SetTexture(main, "nn_4", this.cstex_4);
        _computeShader.SetTexture(main, "nn_5", this.cstex_5);
        _computeShader.SetTexture(main, "nn_out", _renderTextureOutput);

        _computeShader.GetKernelThreadGroupSizes(main, out uint xGroupSize, out uint yGroupSize, out uint zGroupSize);
        _computeShader.Dispatch(main, _renderTextureOutput.width / (int) xGroupSize, _renderTextureOutput.height / (int) yGroupSize,
            1);



        ToTexture2D( this._renderTextureOutput, this.cstex_1);


        for (int i = 0; i < Content.n_animals; i++)
        {

            int index = i * Content.neurons_per_animal;

             for (int j = 0; j < Content.segments_per_animal; j++)
            {
                int here = index + j;

                int x = here % this.g_size;
                int y = here / this.g_size;

                Color pixel = this.cstex_1.GetPixel(x,y);

                JointMotor2D jmo = new JointMotor2D();
                jmo.maxMotorTorque = 1000.0f;
                jmo.motorSpeed = pixel.r * 360.0f;
                
                this.animals[i].segments[j].joint.motor = jmo;


            }


        }




    }
}
