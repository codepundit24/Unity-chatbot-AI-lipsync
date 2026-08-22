using UnityEngine;

public class DynamicJawSync : MonoBehaviour
{
    [Header("Assign Your Meshes Here")]
    public SkinnedMeshRenderer headMesh;
    [Tooltip("Drag the AvatarTeethDown object here")]
    public SkinnedMeshRenderer bottomTeethMesh; // Added the teeth mesh

    [Header("Jaw Settings")]
    public string jawOpenBlendshapeName = "jawOpen";

    [Range(0.2f, 1.5f)]
    public float jawMultiplier = 1.2f;

    private readonly int aa_Index = 59;
    private readonly int E_Index = 53;
    private readonly int oh_Index = 63;

    private int headJawOpenIndex = -1;
    private int teethJawOpenIndex = -1; // Index for the teeth's blendshape

    void Start()
    {
        if (headMesh != null)
        {
            headJawOpenIndex = headMesh.sharedMesh.GetBlendShapeIndex(jawOpenBlendshapeName);
        }

        // Find the matching jawOpen shape on the teeth mesh
        if (bottomTeethMesh != null)
        {
            teethJawOpenIndex = bottomTeethMesh.sharedMesh.GetBlendShapeIndex(jawOpenBlendshapeName);
        }
    }

    void LateUpdate()
    {
        if (headMesh == null || headJawOpenIndex == -1) return;

        // 1. Read the open-mouth weights from the Head
        float aa_Weight = headMesh.GetBlendShapeWeight(aa_Index);
        float E_Weight = headMesh.GetBlendShapeWeight(E_Index);
        float oh_Weight = headMesh.GetBlendShapeWeight(oh_Index);

        // 2. Calculate how wide the mouth is opening
        float maxViseme = Mathf.Max(aa_Weight, E_Weight, oh_Weight);
        float finalJawWeight = maxViseme * jawMultiplier;

        // 3. Move the Head's jaw
        headMesh.SetBlendShapeWeight(headJawOpenIndex, finalJawWeight);

        // 4. Move the Teeth's jaw to match perfectly
        if (bottomTeethMesh != null && teethJawOpenIndex != -1)
        {
            bottomTeethMesh.SetBlendShapeWeight(teethJawOpenIndex, finalJawWeight);
        }
    }
}