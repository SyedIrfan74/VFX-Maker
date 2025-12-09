using System;
using System.Collections.Generic;
using UnityEngine;

public class AizawaAttractor : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material mat;
    [SerializeField] private Material trailMat;
    [SerializeField] private int numObjects;
    [SerializeField] private float scale;

    [Header("Aizawa Variables")]
    [SerializeField] private float a;
    [SerializeField] private float b;
    [SerializeField] private float c;
    [SerializeField] private float d;
    [SerializeField] private float e;
    [SerializeField] private float f;
    [SerializeField] private double dt;

    [Header("Lorenz Variables")]
    [SerializeField] private float g;
    [SerializeField] private float h;
    [SerializeField] private float i;

    private List<GameObject> points = new List<GameObject>();

    void Start()
    {
        InitPoints();
    }

    void Update()
    {
        //Aizawa();

        Lorenz();
    }

    public void Lorenz()
    {
        foreach (GameObject p in points)
        {
            double x = p.transform.position.x, y = p.transform.position.y, z = p.transform.position.z;

            double x1 = g * (y - x);

            double y1 = x * (h - z) - y;

            double z1 = x * y - i * z;

            Vector3 haha = new Vector3((float)(x + dt * x1), (float)(y + dt * y1), (float)(z + dt * z1));

            p.transform.position = haha;
        }
    }

    public void Aizawa()
    {
        foreach (GameObject p in points)
        {
            double x = p.transform.position.x, y = p.transform.position.y, z = p.transform.position.z;

            float x1 = (p.transform.position.z - b) * p.transform.position.x - d * p.transform.position.y;

            float y1 = d * p.transform.position.x + (p.transform.position.z - b) * p.transform.position.y; ;

            double z1 = c + a * p.transform.position.z - (Math.Pow(p.transform.position.z, 3) / 3d) -
                (Math.Pow(p.transform.position.x, 2) + Math.Pow(p.transform.position.y, 2)) *
                (1 + e * p.transform.position.z) + f * p.transform.position.z * (Math.Pow(p.transform.position.x, 3));

            Vector3 haha = new Vector3((float)(x + dt * x1), (float)(y + dt * y1), (float)(z + dt * z1));

            p.transform.position = haha;
        }
    }

    public void InitPoints()
    {
        points.Clear();

        for (int i = 0; i < numObjects; i++)
        {
            GameObject GO = new GameObject();
            GO.AddComponent<MeshFilter>();
            GO.AddComponent<MeshRenderer>();
            GO.AddComponent<TrailRenderer>();

            GO.GetComponent<MeshFilter>().mesh = mesh;
            GO.GetComponent<MeshRenderer>().material = mat;
            GO.GetComponent<TrailRenderer>().material = trailMat;
            GO.GetComponent<TrailRenderer>().time = 1;
            GO.GetComponent<TrailRenderer>().widthMultiplier = 0.1f;

            Vector3 rando = UnityEngine.Random.insideUnitSphere;
            GO.GetComponent<Transform>().position = rando;
            GO.GetComponent<Transform>().localScale = new Vector3(scale, scale, scale);

            points.Add(GO);
        }
    }
}