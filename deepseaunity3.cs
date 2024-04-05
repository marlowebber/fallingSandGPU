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

public class EA : MonoBehaviour
{
    // i don't remember what EA stands for but it serves to link all the different parts together.

    public float sensation = 0.0f;
    public int animal_index = 0;  // this is the index of the animal this EA belongs to.
    public int segment_index = 0;
    public deepseaunity3 world;

    void OnCollisionEnter2D(Collision2D collision)
    {
        this.sensation = 1.0f;
        EA ea = collision.gameObject.GetComponent<EA>();
        ea.sensation = 1.0f;
    }

    void OnCollisionLeave2D(Collision2D collision)
    {
        this.sensation = 0.0f;
        EA ea = collision.gameObject.GetComponent<EA>();
        ea.sensation = 0.0f;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        this.sensation = 1.0f;
        EA other_ea = collision.gameObject.GetComponent<EA>();
        other_ea.sensation = 1.0f;

        if (this.segment_index == 7)
        {
            if (other_ea.animal_index != this.animal_index)
            {
               // the collider 'heart' of the other animal has been 'stung' by your segment 8, which means you killed it and can transform it into one of your children.
                // Debug.Log("animal " + this.animal_index.ToString() + " stinged " +  other_ea.animal_index.ToString());
                for(int i = 0; i < Content.segments_per_animal; i++)
                {
                this.world. animals[ other_ea.animal_index ] .segments[i].duplicate_from(this.world.animals[this.animal_index].segments[i]);
                }
                this.world.duplicate_brain( other_ea.animal_index, this.animal_index);

                for(int i = 0; i < Content.segments_per_animal; i++)
                {
                    this.world. animals[ other_ea.animal_index ] .segments[i].update_from_parameters();
                }
            }
        }
    }

    void OnTriggerLeave2D(Collider2D collision)
    {
        this.sensation = 0.0f;
        EA ea = collision.gameObject.GetComponent<EA>();
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
    public BoxCollider2D col;
    public EA ea;

    public int connectedTo;
    public float length;
    public float width;
    public Color color;

    public void duplicate_from(Segment other)
    {
        // what parts must be duplicated?

        // the length, width, color, and attachment point

        this.length = other.length;
        this.width = other.width;
        this.color = other.color;
        this.connectedTo = other.connectedTo;
    }

    public void update_from_parameters()
    {
        this.joint.enableCollision = false;
        this.joint.breakForce = Mathf.Infinity;
        this.joint.autoConfigureConnectedAnchor = false;
        this.joint.connectedBody =  this.ea.world.animals[ this.ea.animal_index ] .segments[ this.connectedTo ].rb;
        this.joint.anchor = new Vector2(0.0f, (this.go.transform.localScale.x/2 ));
        this.joint.connectedAnchor = new Vector2(0.0f, -(this.ea.world.animals[ this.ea.animal_index ] .segments[ this.connectedTo ].go.transform.localScale.x/2));
        this.joint.useMotor = true;
        this.joint.useLimits = false;

        this.go.transform.localScale = new Vector3(this.width, this.length, 1.0f);

        this.sr.color = this.color;
       

    }

      public void flight_model()
    {
        

            // s.last_world_pos.x = s.world_pos.x ;
            // s.last_world_pos.y = s.world_pos.y ;
            // s.world_pos.x =  s.center_pos.x - this.center_of_mass.x    +  this.world_position.x;
            // s.world_pos.y =  s.center_pos.y - this.center_of_mass.y    +  this.world_position.y;
            // Vector2 segment_velocity = s.velocity();


            float rotation_rad = this.rb.rotation * (Mathf.PI / 180.0f);


            float movement_angle = Mathf.Atan2(this.rb.velocity.y, this.rb.velocity.x);
            float angle_of_incidence = Mathf.Atan2(Mathf.Sin(rotation_rad - movement_angle ), Mathf.Cos(rotation_rad - movement_angle));
            float reactive_force = Mathf.Sin( angle_of_incidence)  * this.rb.velocity.magnitude * Content.drag_coeff * (this.length * this.width);
            float face_normal = rotation_rad + (Mathf.PI * 0.5f);
            this.rb.AddForce(
                        // new Vector2(s.world_pos.x, s.world_pos.y),


                        // face_normal, reactive_force
                        new Vector2(
                        Mathf.Cos(face_normal) * reactive_force,
                        Mathf.Sin(face_normal) * reactive_force
                        )
                        
                        );



            // float distance_to_com =  Mathf.Sqrt((s.center_pos.x  * s.center_pos.x ) + (s.center_pos.y * s.center_pos.y));
        
    }

    public Segment(Vector2 pos, int segment_index, int animal_index)
    {
        int psize = 1;
        this.tex = new Texture2D(psize, psize);
        this.sprite = Sprite.Create(this.tex, new Rect(0.0f, 0.0f, psize, psize), new Vector2(psize/2, psize/2), 1);
        this.go = new GameObject();
        this.sr =  this.go.AddComponent<SpriteRenderer>();
        this.sr.sprite = this.sprite;
        this.rb = this.go.AddComponent<Rigidbody2D>();
        this.rb.gravityScale = 0.0f;
        this.ea = this.go.AddComponent<EA>();
        this.ea.animal_index = animal_index;
        this.ea.segment_index = segment_index;
    
        this.col = this.go.AddComponent<BoxCollider2D>();
        // this.go.transform.localScale = new Vector3( UnityEngine.Random.value * Content.default_segment_size, UnityEngine.Random.value * Content.default_segment_size, this.go.transform.localScale.z);
        this.joint = this.go.AddComponent<HingeJoint2D>();
        this.go.transform.position = pos;

        this.length = UnityEngine.Random.value * 2.0f;
        this.width = UnityEngine.Random.value * 2.0f;
        this.connectedTo = UnityEngine.Random.Range(0,segment_index);
        this.color = new Color(UnityEngine.Random.value,UnityEngine.Random.value,UnityEngine.Random.value,1.0f);

        // this.update_from_parameters();

        if (segment_index == 7)
        {
            this.col.isTrigger = true;
        }
        else
        {
            this.col.isTrigger = false;

        }
    }
}




public class Animal
{
    public Segment [] segments = new Segment [Content.segments_per_animal];
    public int animal_index;

    public Animal(int new_index)
    {
        this.animal_index = new_index;
        
        Vector3 pos = new Vector3( (UnityEngine.Random.value * Content.arena_size) - (0.5f * Content.arena_size)  ,  (UnityEngine.Random.value * Content.arena_size) - (0.5f * Content.arena_size) , 0.0f);


        for(int i = 0; i<Content.segments_per_animal;i++)
        {
            this.segments[i] = new Segment(pos, i, this.animal_index);
        }

       
     
    }


    public void restrain()
    {

        float xadjust = 0.0f;
        float yadjust = 0.0f;
    
        if (this.segments[0].go.transform.position.x > Content.arena_size )
        {   
            xadjust = (this.segments[0].go.transform.position.x  - Content.arena_size) * -1.0f;
        }
        else            if (this.segments[0].go.transform.position.x < -Content.arena_size )
        {   
            xadjust = (this.segments[0].go.transform.position.x  - (-Content.arena_size)) * -1.0f;
        }

        if (this.segments[0].go.transform.position.y > Content.arena_size )
        {   
            yadjust = (this.segments[0].go.transform.position.y  - Content.arena_size) * -1.0f;
        }
        else            if (this.segments[0].go.transform.position.y < -Content.arena_size )
        {   
            yadjust = (this.segments[0].go.transform.position.y  - (-Content.arena_size)) * -1.0f;
        }

        if (xadjust != 0.0f || yadjust != 0.0f)
        {

            for (int j = 0; j < Content.segments_per_animal; j++)
            {
                this.segments[j].go.transform.position = 
                new Vector3(
                    this.segments[j].go.transform.position.x + xadjust,
                    this.segments[j].go.transform.position.y + yadjust,
                    0.0f
                )
                ;
            }
        }

    }
}

/*
    the network is made of 5 textures using their RGBA channels to store data.
    each pixel represents 1 neuron.
    connection address is from 0 to neurons_per_animal.

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
    output texture with updated values which replaces texture 1 next round. Includes repeats of the connection address 1 and weight 1.
*/


public static class Content
{
    public static int segments_per_animal    ; //8;
    public static int neurons_per_animal     ; //64;
    public static int n_animals              ; //1024;
    public static float default_segment_size ; //2.0f;
    public static float arena_size           ; //100;
    public static int total_size             ; //Content.n_animals * Content.neurons_per_animal;
    public static int square_size            ; //(int)(Mathf.Sqrt(total_size));
    public static int g_size                 ; //(int)(Mathf.Pow(2, Mathf.Ceil(Mathf.Log(square_size)/Mathf.Log(2)))); // https://stackoverflow.com/questions/466204/rounding-up-to-next-power-of-2
    public static float drag_coeff ;
}


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
    public Texture2D cstex_2 ;
    public Texture2D cstex_3 ;
    public Texture2D cstex_4 ;
    public Texture2D cstex_5 ;

    public List<Texture2D> textures;

    public Texture2D tex_white_square;
    public Sprite sprite_white_square;
    public GameObject mouse_go;
    public Image mouse_indicator;

    public bool prepared;

    // public int g_size;

    public List<Animal> animals;

    void ToTexture2D( RenderTexture rTex, Texture2D tex)
    {
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
    }

    void setup_fishies()
    {
        this.animals = new List<Animal>();
        for (int i = 0; i < Content.n_animals; i++)
        {
            Animal shamoo = new Animal(i); 

              for(int jj = 0; jj < Content.segments_per_animal; jj++)
            {
                shamoo.segments[jj].ea.world = this;
            }


            this.animals.Add(shamoo);

          
        }



        for (int i = 0; i < Content.n_animals; i++)
        {

            for(int jj = 0; jj < Content.segments_per_animal; jj++)
            {
                this.animals[i].segments[jj].update_from_parameters();

                for(int jq = 0; jq < Content.segments_per_animal; jq++)
                {
                    Physics2D.IgnoreCollision(this.animals[i].segments[jj ].col, this.animals[i].segments[jq].col)   ;
                }
            }


        }
    }



    public void duplicate_brain(int to, int from)
    {


        Color [] texture_part_1 = new Color [Content.neurons_per_animal];
        Color [] texture_part_2 = new Color [Content.neurons_per_animal];
        Color [] texture_part_3 = new Color [Content.neurons_per_animal];
        Color [] texture_part_4 = new Color [Content.neurons_per_animal];
        Color [] texture_part_5 = new Color [Content.neurons_per_animal];

        int ruubase = from * Content.neurons_per_animal;

        for(int i = 0; i < Content.neurons_per_animal; i++)
        {
            int here = ruubase + i;
            int x = here % Content.g_size;
            int y = here / Content.g_size;
            texture_part_1[i] = this.cstex_1.GetPixel(x,y);
            texture_part_2[i] = this.cstex_2.GetPixel(x,y);
            texture_part_3[i] = this.cstex_3.GetPixel(x,y);
            texture_part_4[i] = this.cstex_4.GetPixel(x,y);
            texture_part_5[i] = this.cstex_5.GetPixel(x,y);
        }


        int penbase = to * Content.neurons_per_animal;
         for(int i = 0; i < Content.neurons_per_animal; i++)
        {
            int here = penbase + i;
            int x = here % Content.g_size;
            int y = here / Content.g_size;

            this.cstex_1.SetPixel(x,y,texture_part_1[i]);
            this.cstex_2.SetPixel(x,y,texture_part_2[i]);
            this.cstex_3.SetPixel(x,y,texture_part_3[i]);
            this.cstex_4.SetPixel(x,y,texture_part_4[i]);
            this.cstex_5.SetPixel(x,y,texture_part_5[i]);


        }

        this.cstex_1.Apply();
        this.cstex_2.Apply();
        this.cstex_3.Apply();
        this.cstex_4.Apply();
        this.cstex_5.Apply();

    }


    void setup_gpu_stuff()
     {
        
        _renderTextureOutput = new RenderTexture(Content.g_size, Content.g_size, 24);
        _renderTextureOutput.filterMode = FilterMode.Point;
        _renderTextureOutput.enableRandomWrite = true;
        _renderTextureOutput.Create();

        this.bdg= new GameObject();
        this.bdg.transform.SetParent(this.canvas.transform);

        this.bdg.transform.position = new Vector3 (   500.0f, 400.0f, 0.0f   ) ;  
        this.bdg.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        this.bdi = this.bdg.AddComponent<Image>();

        this. cstex_1 = new Texture2D(Content.g_size, Content.g_size);
        this. cstex_2 = new Texture2D(Content.g_size, Content.g_size);
        this. cstex_3 = new Texture2D(Content.g_size, Content.g_size);
        this. cstex_4 = new Texture2D(Content.g_size, Content.g_size);
        this. cstex_5 = new Texture2D(Content.g_size, Content.g_size);

        this.bds = Sprite.Create(cstex_1, new Rect(0.0f, 0.0f, Content.g_size, Content.g_size), new Vector2(Content.g_size/2, Content.g_size/2), 1.0f);
        this.bdi.sprite = this.bds;

        for(int i = 0; i < Content.g_size; i++)
        {
            for(int j = 0; j < Content.g_size; j++)
            {
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

        this.prepared = false;

    Content.segments_per_animal = 8;
    Content.neurons_per_animal = 64;
    Content.n_animals = 256;
    Content.default_segment_size = 2.0f;
    Content.arena_size = 100;
    Content.total_size = Content.n_animals * Content.neurons_per_animal;
    Content.square_size = (int)(Mathf.Sqrt(Content.total_size));
    Content.g_size = (int)(Mathf.Pow(2, Mathf.Ceil(Mathf.Log(Content.square_size)/Mathf.Log(2)))); // https://stackoverflow.com/questions/466204/rounding-up-to-next-power-of-2
    Content.drag_coeff = 0.5f;

      this.go = new GameObject();
        this.canvas = this.go.AddComponent<Canvas>();
        this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;


        setup_gpu_stuff();

        this.textures = new List<Texture2D> { this.cstex_1,this.cstex_2,this.cstex_3,this.cstex_4,this.cstex_5  } ;

  
        this.tex_white_square = new Texture2D(1, 1);
        this.sprite_white_square = Sprite.Create(tex_white_square, new Rect(0.0f, 0.0f, this.tex_white_square.width, this.tex_white_square.height), new Vector2(this.tex_white_square.width/2, this.tex_white_square.height/2), 1.0f);
        this.mouse_go = new GameObject();
        this.mouse_go.transform.SetParent(this.canvas.transform);
        this.mouse_indicator = this.mouse_go.AddComponent<Image>();
        this.mouse_indicator.sprite = this.sprite_white_square;
        this.mouse_indicator.transform.localScale  = new Vector3(1.0f, 0.1f, 1.0f);

        

        setup_fishies();



        this.prepared = true;

    }

    // Update is called once per frame
    void Update()
    {
        if (this.prepared)
        {

        for (int i = 0; i < Content.n_animals; i++)
        {
            int index = i * Content.neurons_per_animal;

            for (int j = 0; j < Content.segments_per_animal; j++)
            {
                int here = index + j;;


                int x = here % Content.g_size;
                int y = here / Content.g_size;

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
                int here = index + j + Content.segments_per_animal;

                int x = here % Content.g_size;
                int y = here / Content.g_size;

                Color pixel = this.cstex_1.GetPixel(x,y);



                    // float final_speed = 0.0f;
                    // float gyro_force = 10.0f; // max limb speed

                    // const float kp = 1.0f; // makes the animals point in a direction instead of spin like a motor. But unity wants to know spin speed. so this is a control loop to drive the segments.
                    // const float kd = -0.3f;

                    // float p = pixel.r - this.animals[i].segments[j].rb.rotation;
                    // float d = this.animals[i].segments[j].rb.angularVelocity;
                    // float sum = (p * kp) + (d * kd);
                    // if (Mathf.Abs(sum) > gyro_force)
                    // {
                    //     if (sum < 0.0f)
                    //     {
                    //        final_speed = gyro_force * -1.0f;
                    //     }
                    //     else
                    //     {
                    //         final_speed = gyro_force;
                    //     }
                    // }
                    // else
                    // {
                    //     final_speed = sum;
                    // }
                

                float goatation = this.animals[i].segments[j].rb.rotation * (Mathf.PI / 180.0f);

                float sublimit = (goatation - pixel.r) * 0.5f;


                JointMotor2D jmo = new JointMotor2D();
                jmo.maxMotorTorque = 1000.0f;
                jmo.motorSpeed = sublimit;//0.0f;// final_speed * 360.0f;
                
                this.animals[i].segments[j].joint.motor = jmo;

                this.animals[i].segments[j].flight_model();
            }

           
        }
        }
    }
}
