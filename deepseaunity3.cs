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
    // public float sensation = 0.0f;
    public int animal_index = 0;
    public int segment_index = 0;
    public deepseaunity3 world;

    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     // this.sensation = 1.0f;
    //     // EA ea = collision.gameObject.GetComponent<EA>();
    //     // ea.sensation = 1.0f;
    // }

    // void OnCollisionLeave2D(Collision2D collision)
    // {
    //     this.sensation = 0.0f;
    //     EA ea = collision.gameObject.GetComponent<EA>();
    //     ea.sensation = 0.0f;
    // }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // this.sensation = 1.0f;
        EA other_ea = collision.gameObject.GetComponent<EA>();
        // other_ea.sensation = 1.0f;
        if (this.segment_index == 7)
        {
            if (other_ea.segment_index == 0)
            {
                if (other_ea.animal_index != this.animal_index)
                {
                    for(int i = 0; i < Content.segments_per_animal; i++)
                    {
                        this.world. animals[ other_ea.animal_index ] .segments[i].duplicate_from(this.world.animals[this.animal_index].segments[i]);
                    }
                    this.world.duplicate_brain( other_ea.animal_index, this.animal_index);
                    for(int i = 0; i < Content.segments_per_animal; i++)
                    {
                        this.world. animals[ other_ea.animal_index ] .segments[i].update_from_parameters(i);
                    }
                    this.world.mutate_animal(other_ea.animal_index);
                }
            }
        }
    }

    // void OnTriggerLeave2D(Collider2D collision)
    // {
    //     this.sensation = 0.0f;
    //     EA ea = collision.gameObject.GetComponent<EA>();
    //     ea.sensation = 0.0f;
    // }
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
        this.length = other.length;
        this.width = other.width;
        this.color = other.color;
        this.connectedTo = other.connectedTo;
    }

    public void update_from_parameters(int segment_index)
    {
        if (segment_index != 0)
        {
        this.joint.enableCollision = false;
        this.joint.breakForce = Mathf.Infinity;
        this.joint.autoConfigureConnectedAnchor = false;
        this.joint.connectedBody =  this.ea.world.animals[ this.ea.animal_index ] .segments[ this.connectedTo ].rb;
        this.joint.anchor = new Vector2(0.0f, (this.go.transform.localScale.x/2 ));
        this.joint.connectedAnchor = new Vector2(0.0f, -(this.ea.world.animals[ this.ea.animal_index ] .segments[ this.connectedTo ].go.transform.localScale.x/2));
        this.joint.useMotor = true;
        this.joint.useLimits = false;
        }
        this.go.transform.localScale = new Vector3(this.width, this.length, 1.0f);
        this.sr.color = this.color;
        this.rb.mass = this.length * this.width;
    }

    public void flight_model()
    {
        float rotation_rad = this.rb.rotation * Content.pi_on_180;
        float movement_angle = Mathf.Atan2(this.rb.velocity.y, this.rb.velocity.x);
        float angle_of_incidence = Mathf.Atan2(Mathf.Sin(rotation_rad - movement_angle ), Mathf.Cos(rotation_rad - movement_angle));
        float reactive_force = Mathf.Sin( angle_of_incidence)  * this.rb.velocity.magnitude * Content.drag_coeff * (this.length * this.width);
        float face_normal = rotation_rad + (Mathf.PI * 0.5f);
        this.rb.AddForce( new Vector2(Mathf.Cos(face_normal) * reactive_force,Mathf.Sin(face_normal) * reactive_force));
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

        if (segment_index == 0 || segment_index == 7)
        {
            this.col = this.go.AddComponent<BoxCollider2D>();
            if (segment_index == 7)
            {
                this.col.isTrigger = true;
            }
            else
            {
                this.col.isTrigger = false;
            }
        }

        if (segment_index != 0)
        {
            this.joint = this.go.AddComponent<HingeJoint2D>();
        }
        this.go.transform.position = pos;
        this.length = UnityEngine.Random.value * 2.0f;
        this.width = UnityEngine.Random.value * 2.0f;
        this.connectedTo = UnityEngine.Random.Range(0,segment_index);
        this.color = new Color(UnityEngine.Random.value,UnityEngine.Random.value,UnityEngine.Random.value,1.0f);


        this.rb.AddForce( new Vector2(UnityEngine.Random.value * 200.0f,UnityEngine.Random.value * 200.0f));
        
    }
}

public class Animal
{
    public Segment [] segments = new Segment [Content.segments_per_animal];
    public int animal_index;

    public Animal(int new_index)
    {
        this.animal_index = new_index;        
        // Vector3 pos = new Vector3( (UnityEngine.Random.value * Content.arena_size * 2.0f) - (0.5f * Content.arena_size)  ,  (UnityEngine.Random.value * Content.arena_size* 2.0f) - (0.5f * Content.arena_size) , 0.0f);
       
       Vector3 pos = new Vector3(

        ((UnityEngine.Random.value - 0.5f) * 2.0f) * Content.arena_size,
        ((UnityEngine.Random.value - 0.5f) * 2.0f) * Content.arena_size,
        0.0f

       );
       
       
        for(int i = 0; i<Content.segments_per_animal;i++)
        {
            this.segments[i] = new Segment(pos, i, this.animal_index);
        }
    }

    public void restrain()
    {
        bool adjust = false;
        Vector2 pos = this.segments[0].go.transform.position;
        Vector3 new_pos = pos;
        if (pos.x > Content.arena_size)
        {
            new_pos.x = Content.arena_size;
            adjust = true;
        }
        else if (pos.x < -Content.arena_size)
        {
            new_pos.x = -Content.arena_size;
            adjust = true;
        }
        if (pos.y > Content.arena_size)
        {
            new_pos.y = Content.arena_size;
            adjust = true;
        }
        else if (pos.y < -Content.arena_size)
        {
            new_pos.y = -Content.arena_size;
            adjust = true;
        }
        if (adjust)
        {
            for(int i = 0; i < Content.segments_per_animal; i++)
            {
                this.segments[i].go.transform.position = new_pos;
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
    public static int segments_per_animal    ; 
    public static int neurons_per_animal     ; 
    public static int n_animals              ; 
    public static float default_segment_size ; 
    public static float arena_size           ; 
    public static int total_size             ; 
    public static int square_size            ; 
    public static int g_size                 ; 
    public static float drag_coeff ;
    public static float pi_on_180;
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
            for(int jj = 0; jj < Content.segments_per_animal; jj+=7)
            {
                this.animals[i].segments[jj].update_from_parameters(jj);
                for(int jq = 0; jq < Content.segments_per_animal; jq+=7)
                {
                    Physics2D.IgnoreCollision(this.animals[i].segments[jj ].col, this.animals[i].segments[jq].col)   ;
                }
            }
        }
    }



    public void body_mutation(int animal_index)
    {
            // body mutation
            int mutated_segment = UnityEngine.Random.Range(0,8);
            int mutation_type = UnityEngine.Random.Range(0,3);

            if (mutation_type == 0)
            {
                // segment width
                this.animals[animal_index].segments[mutated_segment].width += (UnityEngine.Random.value) - 0.5f;
            }
            else if (mutation_type == 1)
            {
                // segment length
                this.animals[animal_index].segments[mutated_segment].length += (UnityEngine.Random.value) - 0.5f;
            }
            else if (mutation_type == 2)
            {
                // segment connection
                int nrs = (UnityEngine.Random.Range(0,mutated_segment));

                if (mutated_segment != 0)
                {
                    if (nrs == mutated_segment)
                    {
                        nrs -= 1;
                    }
                }


                this.animals[animal_index].segments[mutated_segment].connectedTo = nrs;
            }
    }


    public void mutate_connection_weight(int x, int y, int random_connection)
    {
 // connection weight
                switch(random_connection)
                {
                    case 0:
                    {
                        Color temp = this.cstex_1.GetPixel(x,y);
                        temp.a += UnityEngine.Random.value - 0.5f;
                        this.cstex_1.SetPixel(x,y,temp);
                        break;
                    }
                    case 1:
                    {
                        Color temp = this.cstex_2.GetPixel(x,y);
                        temp.g += UnityEngine.Random.value - 0.5f;
                        this.cstex_2.SetPixel(x,y,temp);
                        break;
                    }
                    case 2:
                    {
                        Color temp = this.cstex_2.GetPixel(x,y);
                        temp.a += UnityEngine.Random.value - 0.5f;
                        this.cstex_2.SetPixel(x,y,temp);
                        break;
                    }
                    case 3:
                    {
                        Color temp = this.cstex_3.GetPixel(x,y);
                        temp.g += UnityEngine.Random.value - 0.5f;
                        this.cstex_3.SetPixel(x,y,temp);
                        break;
                    }
                    case 4:
                    {
                        Color temp = this.cstex_3.GetPixel(x,y);
                        temp.a += UnityEngine.Random.value - 0.5f;
                        this.cstex_3.SetPixel(x,y,temp);
                        break;
                    }
                    case 5:
                    {
                        Color temp = this.cstex_4.GetPixel(x,y);
                        temp.g += UnityEngine.Random.value - 0.5f;
                        this.cstex_4.SetPixel(x,y,temp);
                        break;
                    }
                    case 6:
                    {
                        Color temp = this.cstex_4.GetPixel(x,y);
                        temp.a += UnityEngine.Random.value - 0.5f;
                        this.cstex_4.SetPixel(x,y,temp);
                        break;
                    }
                    case 7:
                    {
                        Color temp = this.cstex_5.GetPixel(x,y);
                        temp.g += UnityEngine.Random.value - 0.5f;
                        this.cstex_5.SetPixel(x,y,temp);
                        break;
                    }
                }
    }



    public void mutate_bias(int x, int y)
    {
 // bias mutation
                Color temp = this.cstex_1.GetPixel(x,y);
                temp.g += (UnityEngine.Random.value - 0.5f);
                this.cstex_1.SetPixel(x,y,temp);
    }


    public void mutate_connection_address(int x, int y, int random_connection)
    {
 // connection addresses
                switch(random_connection)
                {
                    case 0:
                    {
                        Color temp = this.cstex_1.GetPixel(x,y);
                        temp.b = UnityEngine.Random.Range(0,Content.neurons_per_animal);
                        this.cstex_1.SetPixel(x,y,temp);
                        break;
                    }
                    case 1:
                    {
                        Color temp = this.cstex_2.GetPixel(x,y);
                        temp.r = UnityEngine.Random.Range(0,Content.neurons_per_animal);
                        this.cstex_2.SetPixel(x,y,temp);
                        break;
                    }
                    case 2:
                    {
                        Color temp = this.cstex_2.GetPixel(x,y);
                        temp.b = UnityEngine.Random.Range(0,Content.neurons_per_animal);
                        this.cstex_2.SetPixel(x,y,temp);
                        break;
                    }
                    case 3:
                    {
                        Color temp = this.cstex_3.GetPixel(x,y);
                        temp.r = UnityEngine.Random.Range(0,Content.neurons_per_animal);
                        this.cstex_3.SetPixel(x,y,temp);
                        break;
                    }
                    case 4:
                    {
                        Color temp = this.cstex_3.GetPixel(x,y);
                        temp.b = UnityEngine.Random.Range(0,Content.neurons_per_animal);
                        this.cstex_3.SetPixel(x,y,temp);
                        break;
                    }
                    case 5:
                    {
                        Color temp = this.cstex_4.GetPixel(x,y);
                        temp.r = UnityEngine.Random.Range(0,Content.neurons_per_animal);
                        this.cstex_4.SetPixel(x,y,temp);
                        break;
                    }
                    case 6:
                    {
                        Color temp = this.cstex_4.GetPixel(x,y);
                        temp.b = UnityEngine.Random.Range(0,Content.neurons_per_animal);
                        this.cstex_4.SetPixel(x,y,temp);
                        break;
                    }
                    case 7:
                    {
                        Color temp = this.cstex_5.GetPixel(x,y);
                        temp.r = UnityEngine.Random.Range(0,Content.neurons_per_animal);
                        this.cstex_5.SetPixel(x,y,temp);
                        break;
                    }
                }
    }


    public void mental_mutation(int animal_index)
    {
 // mental mutation
            int mutation_type = UnityEngine.Random.Range(0,2);
            int mutated_neuron = UnityEngine.Random.Range(0, Content.neurons_per_animal);

            int baselel = animal_index * Content.neurons_per_animal;

            int here = baselel + mutated_neuron;
            int x = here % Content.g_size;
            int y = here / Content.g_size;

            int random_connection = UnityEngine.Random.Range(0,8);

            // if (mutation_type == 0)
            // {
            //     this.mutate_bias();

            // }
            // else if (mutation_type == 1)
            // {
            //    this.

            // }
            // else if (mutation_type == 2)
            // {
               

            // }

            this.mutate_connection_weight(x,y,random_connection);

            if (UnityEngine.Random.value < 0.5f)
            {
                this.mutate_bias(x,y);
            }
            else
            {
                this.mutate_connection_address(x,y,random_connection);
            }

    }


    public void mutate_color(int animal_index)
    {
        for(int i = 0; i<Content.segments_per_animal;i++)
        {
            const float color_mutation_rate = 0.35f;
            Color old =  this.animals[animal_index].segments[i].color ;
            this.animals[animal_index].segments[i].color = new Color(
                old.r + ((UnityEngine.Random.value - 0.5f) * color_mutation_rate),
                old.g + ((UnityEngine.Random.value - 0.5f) * color_mutation_rate),
                old.b + ((UnityEngine.Random.value - 0.5f) * color_mutation_rate),
                1.0f


            );
        }
    }


    public void mutate_animal(int animal_index)
    {
     
        this.mental_mutation(animal_index);
        this.mutate_color(animal_index);

        if (UnityEngine.Random.value < 0.5f)
        {
            this.body_mutation(animal_index);
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
        this.bdg.transform.localScale = new Vector3(8.0f, 8.0f, 8.0f);

        this.bdi = this.bdg.AddComponent<Image>();

        this. cstex_1 = new Texture2D(Content.g_size, Content.g_size);
        this. cstex_2 = new Texture2D(Content.g_size, Content.g_size);
        this. cstex_3 = new Texture2D(Content.g_size, Content.g_size);
        this. cstex_4 = new Texture2D(Content.g_size, Content.g_size);
        this. cstex_5 = new Texture2D(Content.g_size, Content.g_size);

        this.bds = Sprite.Create(cstex_1, new Rect(0.0f, 0.0f, Content.g_size, Content.g_size), new Vector2(Content.g_size/2, Content.g_size/2), 1.0f);
        this.bdi.sprite = this.bds;

        for(int y = 0; y < Content.g_size; y++)
        {
            for(int x = 0; x < Content.g_size; x++)
            {
                int addr_here = (y * Content.g_size) + x;

                int animal_index = addr_here / Content.neurons_per_animal;
                int neuron_index = addr_here % Content.neurons_per_animal;
                int layer_index = neuron_index / Content.segments_per_animal;


                // cstex_1.SetPixel(x, y, new Color((UnityEngine.Random.value-0.5f)*20.0f, (UnityEngine.Random.value-0.5f)*20.0f, UnityEngine.Random.Range(0,8), (UnityEngine.Random.value - 0.5f) * 20.0f));
                // cstex_2.SetPixel(x, y, new Color(UnityEngine.Random.Range(0,neuron_index), (UnityEngine.Random.value - 0.5f) * 20.0f, UnityEngine.Random.Range(0,neuron_index), (UnityEngine.Random.value - 0.5f) * 20.0f));
                // cstex_3.SetPixel(x, y, new Color(UnityEngine.Random.Range(0,neuron_index), (UnityEngine.Random.value - 0.5f) * 20.0f, UnityEngine.Random.Range(0,neuron_index), (UnityEngine.Random.value - 0.5f) * 20.0f));
                // cstex_4.SetPixel(x, y, new Color(UnityEngine.Random.Range(0,Content.neurons_per_animal), (UnityEngine.Random.value - 0.5f) * 20.0f, UnityEngine.Random.Range(0,neuron_index), (UnityEngine.Random.value - 0.5f) * 20.0f));
                // cstex_5.SetPixel(x, y, new Color(UnityEngine.Random.Range(0,Content.neurons_per_animal), (UnityEngine.Random.value - 0.5f) * 20.0f, UnityEngine.Random.Range(0,neuron_index), (UnityEngine.Random.value - 0.5f) * 20.0f));
            
                float val = 10.0f;
                if (layer_index == 1)
                {
                    val = -10.0f;
                }

                cstex_1.SetPixel(x, y, new Color((UnityEngine.Random.value-0.5f)*20.0f, (UnityEngine.Random.value-0.5f)*20.0f, UnityEngine.Random.Range(0,neuron_index), (UnityEngine.Random.value - 0.5f) * 20.0f));
                cstex_2.SetPixel(x, y, new Color(UnityEngine.Random.Range(0,neuron_index), val, UnityEngine.Random.Range(0,neuron_index), layer_index));
                cstex_3.SetPixel(x, y, new Color(UnityEngine.Random.Range(0,neuron_index), val, UnityEngine.Random.Range(0,neuron_index), layer_index));
                cstex_4.SetPixel(x, y, new Color(UnityEngine.Random.Range(0,Content.neurons_per_animal), val, UnityEngine.Random.Range(0,neuron_index), val));
                cstex_5.SetPixel(x, y, new Color(UnityEngine.Random.Range(0,Content.neurons_per_animal), val, UnityEngine.Random.Range(0,neuron_index), val));
           

            
            }
        }
        cstex_1.Apply();
        cstex_2.Apply();
        cstex_3.Apply();
        cstex_4.Apply();
        cstex_5.Apply();



        _computeShader.SetFloat("_Resolution", _renderTextureOutput.width);

        var main = _computeShader.FindKernel("NeuralNet");

        _computeShader.SetFloat( "nn_rand", UnityEngine.Random.value);
        _computeShader.SetTexture(main, "nn_1", this.cstex_1);
        _computeShader.SetTexture(main, "nn_2", this.cstex_2);
        _computeShader.SetTexture(main, "nn_3", this.cstex_3);
        _computeShader.SetTexture(main, "nn_4", this.cstex_4);
        _computeShader.SetTexture(main, "nn_5", this.cstex_5);
        _computeShader.SetTexture(main, "nn_out", _renderTextureOutput);
    }
     
    // Start is called before the first frame update
    void Start()
    {
        this.prepared = false;
        Content.segments_per_animal = 8;
        Content.neurons_per_animal = 24;
        Content.n_animals = 512;
        Content.default_segment_size = 2.0f;
        Content.arena_size = 100;
        Content.total_size = Content.n_animals * Content.neurons_per_animal;
        Content.square_size = (int)(Mathf.Sqrt(Content.total_size));
        Content.g_size = (int)(Mathf.Pow(2, Mathf.Ceil(Mathf.Log(Content.square_size)/Mathf.Log(2)))); // https://stackoverflow.com/questions/466204/rounding-up-to-next-power-of-2
        Content.drag_coeff = 1.0f;
        Content.pi_on_180 = Mathf.PI / 180.0f;
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

    void scramble()
    {
        for(int i = 0; i < Content.g_size; i++)
        {
            int x2 = i % Content.g_size;
            int y2 = i / Content.g_size;
            Color pixel1 = this.cstex_1.GetPixel(x2,y2);

            this.cstex_1.SetPixel(x2,y2, new Color(  pixel1.r + ((UnityEngine.Random.value-0.5f)*20.0f),  pixel1.g, pixel1.b, pixel1.a  )  );



        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.prepared)
        {

             if (Input.GetKey("d"))
            {
                // print("up arrow key is held down");
                this.scramble();
            }



             var main = _computeShader.FindKernel("NeuralNet");

            _computeShader.GetKernelThreadGroupSizes(main, out uint xGroupSize, out uint yGroupSize, out uint zGroupSize);
            _computeShader.Dispatch(main, _renderTextureOutput.width / (int) xGroupSize, _renderTextureOutput.height / (int) yGroupSize,
                1);

            ToTexture2D( this._renderTextureOutput, this.cstex_1);


            for (int animal_index = 0; animal_index < Content.n_animals; animal_index++)
            {
                int animal_base = animal_index * Content.neurons_per_animal;
                for (int segment_index = 0; segment_index < Content.segments_per_animal; segment_index++)
                {

                    int here2 = animal_base + segment_index + (Content.neurons_per_animal - Content.segments_per_animal);
                    int x2 = here2 % Content.g_size;
                    int y2 = here2 / Content.g_size;
                    Color pixel2 = this.cstex_1.GetPixel(x2,y2);

                    float goatation = this.animals[animal_index].segments[segment_index].rb.rotation * Content.pi_on_180;
                    float sublimit = (goatation - pixel2.r) * 0.5f;
                    JointMotor2D jmo = new JointMotor2D();
                    jmo.maxMotorTorque = this.animals[animal_index].segments[segment_index].length * this.animals[animal_index].segments[segment_index].width;

                    if (sublimit > 10.0f)
                    {
                        sublimit = 10.0f;
                    }
                    else if (sublimit < -10.0f)
                    {
                        sublimit = -10.0f;
                    }

                    jmo.motorSpeed = sublimit;
                    if (segment_index != 0)
                    {
                        this.animals[animal_index].segments[segment_index].joint.motor = jmo;    
                    }


                    int here1 = animal_base + segment_index;
                    int x1 = here1 % Content.g_size;
                    int y1 = here1 / Content.g_size;
                    Color pixel1 = this.cstex_1.GetPixel(x1,y1);
                    float noise = UnityEngine.Random.value * 0.1f;

                    // this.animals[animal_index].segments[segment_index].ea.sensation =  this.animals[animal_index].segments[segment_index].rb.rotation * Content.pi_on_180;

                    this.cstex_1.SetPixel(x1,y1, new Color(  (this.animals[animal_index].segments[segment_index].rb.rotation * Content.pi_on_180) + noise,  pixel1.g, pixel1.b, pixel1.a  )  );


                    this.animals[animal_index].segments[segment_index].flight_model();
                    this.animals[animal_index].restrain();

                 
                }
            }
            this.cstex_1.Apply();


           
            // for (int i = 0; i < Content.n_animals; i++)
            // {
            //     int index = i * Content.neurons_per_animal;

            //     for (int j = 0; j < Content.segments_per_animal; j++)
            //     {
            //         int here = index + j + Content.segments_per_animal;
            //         int x = here % Content.g_size;
            //         int y = here / Content.g_size;
            //         Color pixel = this.cstex_1.GetPixel(x,y);
            //         float goatation = this.animals[i].segments[j].rb.rotation * Content.pi_on_180;
            //         float sublimit = (goatation - pixel.r) * 0.5f;
            //         JointMotor2D jmo = new JointMotor2D();
            //         jmo.maxMotorTorque = 1000.0f;
            //         jmo.motorSpeed = sublimit;
            //         this.animals[i].segments[j].joint.motor = jmo;
            //         this.animals[i].segments[j].flight_model();
            //         this.animals[i].restrain();
            //     }
            // }
        }
    }
}
