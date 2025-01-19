using UnityEngine;

public static class GameObjectCloner
{
    /// <summary>
    /// Duplicates <paramref name="original"/> at the scene root so it visually matches
    /// the original's world position, rotation, and scale, 
    /// but only keeps Transform, MeshRenderer, SkinnedMeshRenderer, and MeshFilter.
    /// </summary>
    public static GameObject DuplicateAndStrip(GameObject original)
    {
        // 1) Record the original’s world transform
        Vector3 worldPos      = original.transform.position;
        Quaternion worldRot   = original.transform.rotation;
        Vector3 originalScale = original.transform.lossyScale;

        // 2) Instantiate at the root (no parent)
        GameObject clone = Object.Instantiate(original, null);

        // 3) Remove unwanted components (two-pass to bypass [RequireComponent])
        StripComponentsExceptMesh(clone);

        // 4) Force the clone to match the original’s world position/rotation
        clone.transform.position = worldPos;
        clone.transform.rotation = worldRot;

        // 5) Reset local scale to 1, then set to the original's world scale 
        //    so it "looks" the same size in the scene.
        clone.transform.localScale = Vector3.one; // zero out any odd local scale
        clone.transform.localScale = originalScale; 

        return clone;
    }

    /// <summary>
    /// Removes every component except Transform, MeshRenderer, 
    /// SkinnedMeshRenderer, and MeshFilter, in two passes.
    /// </summary>
    static void StripComponentsExceptMesh(GameObject root)
    {
        // Pass 1: Remove MonoBehaviours first
        var allComponents = root.GetComponentsInChildren<Component>(true);
        foreach (var comp in allComponents)
        {
            if (IsKeepComponent(comp))
                continue;
            if (comp is MonoBehaviour)
                Object.Destroy(comp);
        }

        // Pass 2: Remove remaining unwanted components
        allComponents = root.GetComponentsInChildren<Component>(true);
        foreach (var comp in allComponents)
        {
            if (IsKeepComponent(comp))
                continue;
            Object.Destroy(comp);
        }
    }

    static bool IsKeepComponent(Component comp)
    {
        return comp is Transform 
            || comp is MeshRenderer 
            || comp is SkinnedMeshRenderer 
            || comp is MeshFilter;
    }
}
