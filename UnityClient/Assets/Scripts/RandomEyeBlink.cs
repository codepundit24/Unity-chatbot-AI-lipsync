using System.Collections;
using UnityEngine;

public class RandomEyeBlink : MonoBehaviour
{
    [Header("Assign All Blinking Meshes (Head, Eyelashes, etc.)")]
    public SkinnedMeshRenderer[] targetMeshes;

    private float nextBlinkTime, weight;

    void Start()
    {
        nextBlinkTime = Time.time + Random.Range(2f, 6f);
    }

    void Update()
    {
        bool isBlinking = Time.time > nextBlinkTime && Time.time < nextBlinkTime + 0.15f;
        weight = Mathf.MoveTowards(weight, isBlinking ? 100f : 0f, Time.deltaTime * 1000f);

        // Apply weight to all assigned meshes simultaneously
        foreach (var mesh in targetMeshes)
        {
            if (mesh == null) continue;

            int l_Idx = mesh.sharedMesh.GetBlendShapeIndex("eyeBlinkLeft");
            int r_Idx = mesh.sharedMesh.GetBlendShapeIndex("eyeBlinkRight");

            if (l_Idx != -1) mesh.SetBlendShapeWeight(l_Idx, weight);
            if (r_Idx != -1) mesh.SetBlendShapeWeight(r_Idx, weight);
        }

        if (Time.time > nextBlinkTime + 0.15f)
            nextBlinkTime = Time.time + Random.Range(2f, 6f);
    }
}