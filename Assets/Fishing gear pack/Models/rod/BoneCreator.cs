using UnityEngine;

public class BoneCreator : MonoBehaviour
{
  public Transform rodMesh;  // Drag your rod mesh here
    public int boneCount = 5;
    public bool createInEditMode = true;

    [ContextMenu("Align Bones")]
    void AlignBonesAlongRod()
    {
        // Ensure we have a rod mesh reference
        if (rodMesh == null)
        {
            Debug.LogError("Please assign the rod mesh in the inspector!");
            return;
        }

        // Create or get the root armature
        Transform armature = transform.Find("FishingRod_Armature");
        if (armature == null)
        {
            GameObject armatureObj = new GameObject("FishingRod_Armature");
            armature = armatureObj.transform;
            armature.SetParent(transform);
        }

        // Clear existing children
        foreach (Transform child in armature)
        {
            DestroyImmediate(child.gameObject);
        }

        // Calculate rod length
        Renderer rodRenderer = rodMesh.GetComponent<Renderer>();
        float rodLength = rodRenderer != null 
            ? rodRenderer.bounds.size.y 
            : 1f;

        // Create bones
        Transform previousBone = null;
        for (int i = 0; i < boneCount; i++)
        {
            GameObject boneObj = new GameObject($"Rod_Bone_{i:00}");
            Transform boneTrans = boneObj.transform;
            
            // Position bones along rod length
            float t = (float)i / (boneCount - 1);
            boneTrans.position = rodMesh.position + Vector3.up * (t * rodLength);
            
            // Set parent hierarchy
            if (previousBone == null)
            {
                boneTrans.SetParent(armature);
            }
            else
            {
                boneTrans.SetParent(previousBone);
            }
            
            previousBone = boneTrans;
        }
    }

    void OnValidate()
    {
        if (createInEditMode)
        {
            AlignBonesAlongRod();
        }
    }
}