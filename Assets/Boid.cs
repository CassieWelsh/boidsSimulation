using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Set Dynamically")]
    public Rigidbody rigid; //a
    private Neighborhood neighborhood;

    void Awake()
    {
        neighborhood = GetComponent<Neighborhood>();
        rigid = GetComponent<Rigidbody>(); //a

        pos = Random.insideUnitSphere * Spawner.S.spawnRadius; //b

        Vector3 vel = Random.onUnitSphere * Spawner.S.velocity; //c
        rigid.velocity = vel; 

        LookAhead(); //d

        Color randColor = Color.black; //e
        while (randColor.r + randColor.g + randColor.b < 1.0f)
            randColor = new Color(Random.value, Random.value, Random.value);
        
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>(); //f
        foreach (Renderer r in rends)
            r.material.color = randColor;
        TrailRenderer tRend = GetComponent<TrailRenderer>();
        tRend.material.SetColor("_TintColor", randColor);
    }

    void LookAhead() //d
    {
        transform.LookAt(pos + rigid.velocity);
    }

    
    public Vector3 pos //b //a
    {
        get {return transform.position;}
        set {transform.position = value;}
    }

    void FixedUpdate()
    {
        Vector3 vel = rigid.velocity; //b
        Spawner spn = Spawner.S; //c

        //Предотвращение столкновений
        Vector3 velAvoid = Vector3.zero;
        Vector3 tooClosePos = neighborhood.avgClosePos;
        if (tooClosePos != Vector3.zero)
        {
            velAvoid = pos - tooClosePos;
            velAvoid.Normalize();
            velAvoid *= spn.velocity;
        }

        //Согласование скорости
        Vector3 velAlign = neighborhood.avgVel;
        //Согласование требуется, только если velAlign не равно Vector3.zero
        if (velAlign != Vector3.zero)
        {
            velAlign.Normalize();
            velAlign *= spn.velocity;
        }

        //Концентрация соседей
        Vector3 velCenter = neighborhood.avgPos;
        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= spn.velocity;
        }

        //Притяжение
        Vector3 delta = Attractor.POS - pos; //d
        //Проверить, куда двигаться, в сторону Attractor или от него
        bool attracted = (delta.magnitude > spn.velocity); 
        Vector3 velAttract = delta.normalized * spn.velocity; //e

        //Применить все скорости
        float fdt = Time.fixedDeltaTime;
        if (velAvoid != Vector3.zero)
            vel = Vector3.Lerp(vel, velAvoid, spn.collAvoid * fdt);
        else
        {
            if (velAlign != Vector3.zero)
                vel = Vector3.Lerp(vel, velAlign, spn.velMatching * fdt);
            if (velCenter != Vector3.zero) 
                vel = Vector3.Lerp(vel, velAlign, spn.flockCentering * fdt);
            if (velAttract != Vector3.zero)
                if (attracted)
                    vel = Vector3.Lerp(vel, velAttract, spn.attractPull * fdt);
                else
                    vel = Vector3.Lerp(vel, -velAttract, spn.attractPush * fdt);
        }

        vel = vel.normalized * spn.velocity;
        rigid.velocity = vel;
        LookAhead();
    }
}
