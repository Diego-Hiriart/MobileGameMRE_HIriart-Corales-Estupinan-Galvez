using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MicroRuleEngine;
using Newtonsoft.Json;
using System.IO;
using System;

public class PlayerController : MonoBehaviour
{
    private Material original;
    private Rigidbody rb;
    private Vector3 direction;
    private Player playerInfo;
    private float threshold = 0.2f;
    private Func<Player, bool> healthRule;
    private Func<Player, bool> speedRule;
    private Func<Player, bool> healRule;

    // Start is called before the first frame update
    void Start()
    {
        this.playerInfo = new Player();
        this.direction = Vector3.zero;
        this.rb = this.GetComponent<Rigidbody>();
        this.original = this.GetComponent<Renderer>().material;
        this.CompileRules();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.acceleration.x;
        float z = Input.acceleration.z;

        if (x > threshold)
        {
            direction -= Vector3.forward;
        }

        if (x < -threshold)
        {
            direction += Vector3.forward;
        }

        direction = direction.normalized;
        rb.AddTorque(direction * this.playerInfo.GetSpeed());

        if (this.playerInfo.GetHealth()<=0)
        {
            this.playerInfo.SetLives(this.playerInfo.GetLives()-1);
            this.playerInfo.SetHealth(100);
            this.Respawn();
        }

        if (this.healthRule(this.playerInfo))//Change color to red
        {
            this.GetComponent<Material>().color = Color.red;
        }
        else//Reset color if healed
        {
            this.GetComponent<Renderer>().material = this.original;//Reset color
        }
    }

    public Player GetPlayerInfo()
    {
        return this.playerInfo;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Spike"))
        {
            this.playerInfo.SetHealth(this.playerInfo.GetHealth()-20f);
        }

        if (collision.gameObject.CompareTag("PickUp"))
        {
            this.playerInfo.SetPickUpCount(this.playerInfo.GetPickUpCount()+1);
            if (this.speedRule(this.playerInfo))//Add speed if the rule is fulfilled
            {
                this.playerInfo.SetSpeed(this.playerInfo.GetSpeed()+0.5f);
            }
            if (this.healRule(this.playerInfo))//Heal if the conditions are met
            {
                this.playerInfo.SetHealth(this.playerInfo.GetHealth() + 10f);
            }
            Destroy(collision.gameObject);//Remove pick up
        }
    }

    private void Respawn()
    {
        this.transform.position = new Vector3(8.5f, 0.5f, -8.8f);//Put back in starting position
        this.rb.velocity = Vector3.zero;//Remove all movement
        this.rb.angularVelocity = Vector3.zero;
        this.GetComponent<Renderer>().material = this.original;//Reset color
    }

    private void CompileRules()
    {
        string jsonContents = File.ReadAllText(@".\Assets\Scripts\PlayerRules.json");
        List<Rule> rulesList = JsonConvert.DeserializeObject<List<Rule>>(jsonContents);
        MRE engine = new MRE();
        this.healthRule = engine.CompileRule<Player>(rulesList[0]);
        this.speedRule = engine.CompileRule<Player>(rulesList[1]);
        this.healRule = engine.CompileRule<Player>(rulesList[2]);
    }
}
