using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Data;
using UnityEngine.Rendering;
using Npgsql;


public class StatsGatherer : MonoBehaviour
{
    private string connstring = String.Format("Server={0};Port={1};" + "User Id={2};Password={3};Database={4};", "localhost", 5432, "postgres", "15082000", "StatsGatherer");
    private NpgsqlConnection conn;
    private string sql;
    private NpgsqlCommand cmd;
    private DataTable dt;
    public GameObject Player_obj;
    float player_pos_x = 0;
    float player_pos_y = 0;
    float player_pos_z = 0;
    float camera_x;
    float camera_y;
    int screenres_x;
    int screenres_y;
    float result = 0;
    bool send_data = true;
    int gold, score, amount_of_jumps;
    public static int kills = 0;
    public static int deaths = 0;
    string level_time;
    private static StatsGatherer instance;
    private Camera camera;
    private bool takeScreenshotOnNextFrame;
    public UnityEditor.MonoScript Script1;
    public string Name1;
    public UnityEditor.MonoScript Script2;
    public string Name2;
    public UnityEditor.MonoScript Script3;
    public string Name3;
    public UnityEditor.MonoScript Script4;
    public string Name4;
    public UnityEditor.MonoScript Script5;
    public string Name5;
    
    public UnityEditor.MonoScript Script6;
    public string Name6;
    public GameObject left_lvl_border;
    public GameObject right_lvl_border;
    public GameObject up_lvl_border;
    public GameObject down_lvl_border;
    private bool res;
    private void Awake()
    {
        instance = this;
        camera = gameObject.GetComponent<Camera>();
        camera_x = left_lvl_border.transform.position.x < 0 ? (right_lvl_border.transform.position.x - left_lvl_border.transform.position.x) / 2
        : (right_lvl_border.transform.position.x + left_lvl_border.transform.position.x) / 2;
        camera_y = down_lvl_border.transform.position.y < 0 ? (up_lvl_border.transform.position.y - down_lvl_border.transform.position.y) / 2
        : (up_lvl_border.transform.position.y + down_lvl_border.transform.position.y) / 2;
        screenres_x = (int)(left_lvl_border.transform.position.x < 0 ? (right_lvl_border.transform.position.x - left_lvl_border.transform.position.x)
        : (right_lvl_border.transform.position.x + left_lvl_border.transform.position.x));
        screenres_y = (int)(down_lvl_border.transform.position.y < 0 ? (up_lvl_border.transform.position.y - down_lvl_border.transform.position.y)
        : (up_lvl_border.transform.position.y + down_lvl_border.transform.position.y));
        
        TakeScreenshot_static(screenres_x*16, screenres_y*16);
      
    }

    void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
    }
    void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
    }
    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        OnPostRender();
    }

    private void OnPostRender()
    {          
        if (takeScreenshotOnNextFrame)
        {
            takeScreenshotOnNextFrame = false;
            
            RenderTexture renderTexture = camera.targetTexture;
            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0,0,renderTexture.width,renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);
            byte[] byteArray = renderResult.EncodeToPNG();
           
            string lvlname = "lvl1";
            sql = $@"select exists(select 1 from level_picture where imgname = '{lvlname}')";
            cmd = new NpgsqlCommand(sql, conn);
            res = (bool)cmd.ExecuteScalar();
           // Debug.Log("RES" + res);
            if (!res)
            {
                sql = $@"SELECT * FROM level_picture_insert(:imgname,:img)";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("imgname", lvlname);
                cmd.Parameters.AddWithValue("img", byteArray);
                cmd.ExecuteScalar();
            }
          
            System.IO.File.WriteAllBytes(Application.dataPath + "/CameraScreenshot.png",byteArray);
            Debug.Log("Screenshot taken");
            RenderTexture.ReleaseTemporary(renderTexture);
            camera.targetTexture = null;
        }
    }
    private void TakeScreenshot(int width, int height)
    {     
        this.transform.position = new Vector3(camera_x, camera_y, -36);
        camera.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        takeScreenshotOnNextFrame = true;

    }
    public static void TakeScreenshot_static(int width,int height)
    {
        instance.TakeScreenshot(width,height);
    }
    void Start()
    {
        conn = new NpgsqlConnection(connstring);
 
        conn.Open();

       

        sql = $@"UPDATE level_border SET leftb = {left_lvl_border.transform.position.x},rightb = {right_lvl_border.transform.position.x},
        upb = {up_lvl_border.transform.position.y},downb ={down_lvl_border.transform.position.y}";
        cmd = new NpgsqlCommand(sql, conn);
        cmd.ExecuteScalar();

        sql = $@"select exists(select 1 from player_movement_tracker where x = {left_lvl_border.transform.position.x})";
        cmd = new NpgsqlCommand(sql, conn);
        res = (bool)cmd.ExecuteScalar();
     
        if (!res)
        {       
                sql = @"SELECT * FROM Player_movement_tracker_insert(:x,:y,:z)";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("x", left_lvl_border.transform.position.x);
                cmd.Parameters.AddWithValue("y", left_lvl_border.transform.position.y);
                cmd.Parameters.AddWithValue("z", 0);
                cmd.ExecuteScalar();           
        }

        sql = $@"select exists(select 1 from player_movement_tracker where x = {right_lvl_border.transform.position.x})";
        cmd = new NpgsqlCommand(sql, conn);
        res = (bool)cmd.ExecuteScalar();
        if (!res)
        {
            sql = @"SELECT * FROM Player_movement_tracker_insert(:x,:y,:z)";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("x", right_lvl_border.transform.position.x);
            cmd.Parameters.AddWithValue("y", right_lvl_border.transform.position.y);
            cmd.Parameters.AddWithValue("z", 0);
            cmd.ExecuteScalar();
        }

        sql = $@"select exists(select 1 from player_movement_tracker where y = {up_lvl_border.transform.position.y})";
        cmd = new NpgsqlCommand(sql, conn);
        res = (bool)cmd.ExecuteScalar();
        if (!res)
        {
            sql = @"SELECT * FROM Player_movement_tracker_insert(:x,:y,:z)";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("x", up_lvl_border.transform.position.x);
            cmd.Parameters.AddWithValue("y", up_lvl_border.transform.position.y);
            cmd.Parameters.AddWithValue("z", 0);
            cmd.ExecuteScalar();
        }


        sql = $@"select exists(select 1 from player_movement_tracker where y = {down_lvl_border.transform.position.y})";
        cmd = new NpgsqlCommand(sql, conn);
        res = (bool)cmd.ExecuteScalar();
        if (!res)
        {
            sql = @"SELECT * FROM Player_movement_tracker_insert(:x,:y,:z)";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("x", down_lvl_border.transform.position.x);
            cmd.Parameters.AddWithValue("y", down_lvl_border.transform.position.y);
            cmd.Parameters.AddWithValue("z", 0);
            cmd.ExecuteScalar();
        }



        InvokeRepeating("Gather", 1.0f, 0.3f);
        sql = $@"CREATE TABLE player_stats (
        id serial,
	    {Name1} integer NOT NULL,
	    {Name2} integer NOT NULL,
	    level_time varchar(255) NOT NULL,
        amount_of_jumps integer NOT NULL,
	    {Name3} integer NOT NULL,
	    {Name4} integer NOT NULL,
	    CONSTRAINT Player_stats_pk PRIMARY KEY(id)
        ) WITH(
        OIDS = FALSE
        ); ";
        cmd = new NpgsqlCommand(sql, conn);
        cmd.ExecuteScalar();
    
        sql = $@"create function Player_stats_insert(_{Name1} integer,_{Name2} integer,_level_time VARCHAR(255),_amount_of_jumps integer,_{Name3} integer,_{Name4} integer)
        returns int as
        $$
        begin
        insert into player_stats({Name1},{Name2},level_time,amount_of_jumps,{Name3},{Name4})
        values(_{Name1},_{Name2},_level_time,_amount_of_jumps,_{Name3},_{Name4});
        if found then return 1;
        else return 0;
        end if;
        end 
        $$
        language plpgsql";
        cmd = new NpgsqlCommand(sql, conn);
        cmd.ExecuteScalar();
 
    }

    void Gather()
    {

        player_pos_x = GameObject.Find(Player_obj.name).transform.position.x;
        player_pos_y = GameObject.Find(Player_obj.name).transform.position.y;
        player_pos_z = GameObject.Find(Player_obj.name).transform.position.z;


        sql = @"SELECT * FROM player_movement_tracker_insert(:x,:y,:z)";
        cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("x", player_pos_x);
        cmd.Parameters.AddWithValue("y", player_pos_y);
        cmd.Parameters.AddWithValue("z", player_pos_z);
        result = (int)cmd.ExecuteScalar();
       
        if (result == 1)
        {
            Debug.Log("Data about position collected");

        }
        else
        {
            Debug.Log("Failed to collect information about position");
        }


    }

   
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.tag == "Finish")
        {            
            sql = $@"SELECT * FROM Player_stats_insert(:{Name1},:{Name2},:level_time,:amount_of_jumps,:{Name3},:{Name4})";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue(Name1, Script1.GetClass().GetField(Name1).GetValue(Name1));
            cmd.Parameters.AddWithValue(Name2, Script2.GetClass().GetField(Name2).GetValue(Name2));
            cmd.Parameters.AddWithValue("level_time", Time.time.ToString());
            cmd.Parameters.AddWithValue("amount_of_jumps", amount_of_jumps);
            cmd.Parameters.AddWithValue(Name3, Script3.GetClass().GetField(Name3).GetValue(Name3));
            cmd.Parameters.AddWithValue(Name4, Script4.GetClass().GetField(Name4).GetValue(Name4));
            result = (int)cmd.ExecuteScalar();
            conn.Close();
            // Debug.Log(result);
            if (result == 1)
            {
                Debug.Log("Level stats gathered");

            }
            else
            {
                Debug.Log("Error during level stats gathering");
            }
            //FindObjectOfType<GameManager>().EndGame();
            send_data = false;
            conn.Close();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        send_data = true;
    }
    // Update is called once per frame
    private void followPlayer()
    {
        this.transform.position = new Vector2(Player_obj.transform.position.x, Player_obj.transform.position.y);
    }
    void FixedUpdate()
    {
        InvokeRepeating("followPlayer", 3.0f, 0.1f);
        if (Input.GetButtonDown("Jump"))
        {
            amount_of_jumps++;
           // Debug.Log("Jumped " + amount_of_jumps);
        }
        Debug.Log("Coins : " + Script1.GetClass().GetField(Name1).GetValue(Name1));
        Debug.Log("Score : " + Script2.GetClass().GetField(Name2).GetValue(Name2));
        Debug.Log("Kills : " + Script3.GetClass().GetField(Name3).GetValue(Name3));
        Debug.Log("Deaths : " + Script4.GetClass().GetField(Name4).GetValue(Name4));
    }
}
