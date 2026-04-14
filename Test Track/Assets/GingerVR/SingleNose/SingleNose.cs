using UnityEngine;

[ExecuteInEditMode]
public class SingleNose : MonoBehaviour
{
    [Header("Position")]
    public Vector3 localPosition = new Vector3(0f, 0f, 0.3f);

    [Header("Scale")]
    public Vector3 localScale = new Vector3(0.03f, 0.02f, 0.02f);

    [Header("Appearance")]
    public Color noseColor = Color.yellow;

    void Update()
    {
        ApplyTransform();
        ApplyMaterial();
    }

    void ApplyTransform()
    {
        transform.localPosition = localPosition;
        transform.localScale = localScale;
    }

    void ApplyMaterial()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.sharedMaterial.color = noseColor;
        }
    }
}