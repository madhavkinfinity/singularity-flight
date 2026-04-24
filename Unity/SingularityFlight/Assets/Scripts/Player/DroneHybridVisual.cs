using UnityEngine;

/// <summary>
/// DroneHybridVisual
/// Purpose: Build and maintain a lightweight hybrid drone visual inspired by Tron light cycles and pod racers.
/// Responsibilities:
/// - Procedurally build a modular drone body from primitive meshes.
/// - Configure emissive engine pods and a rear energy trail.
/// - Animate thruster glow intensity without gameplay coupling.
/// </summary>
[DisallowMultipleComponent]
public sealed class DroneHybridVisual : MonoBehaviour
{
    [Header("Palette")]
    [SerializeField] private Color hullColor = new(0.18f, 0.2f, 0.26f);
    [SerializeField] private Color accentColor = new(0.2f, 0.85f, 1f);
    [SerializeField] private Color engineGlowColor = new(1f, 0.42f, 0.12f);

    [Header("Sizing")]
    [SerializeField, Min(0.2f)] private float bodyLength = 1.8f;
    [SerializeField, Min(0.2f)] private float bodyWidth = 0.6f;
    [SerializeField, Min(0.1f)] private float bodyHeight = 0.35f;
    [SerializeField, Min(0.25f)] private float podOffset = 0.95f;

    [Header("Trail")]
    [SerializeField, Min(0.2f)] private float trailLifetime = 0.55f;
    [SerializeField, Min(0.02f)] private float trailWidth = 0.2f;

    [Header("Thruster Pulse")]
    [SerializeField, Min(0f)] private float pulseSpeed = 6f;
    [SerializeField, Min(0f)] private float minEmission = 1.8f;
    [SerializeField, Min(0f)] private float maxEmission = 3.4f;

    private const string RootName = "DroneVisualRoot";

    private Transform visualRoot;
    private Renderer[] animatedGlowRenderers;
    private MaterialPropertyBlock glowPropertyBlock;

    private void Awake()
    {
        EnsureVisualBuilt();
    }

    private void Update()
    {
        if (animatedGlowRenderers == null || animatedGlowRenderers.Length == 0)
        {
            return;
        }

        float pulse = Mathf.Lerp(minEmission, maxEmission, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
        Color emissive = engineGlowColor * pulse;

        glowPropertyBlock ??= new MaterialPropertyBlock();
        glowPropertyBlock.SetColor("_EmissionColor", emissive);

        for (int index = 0; index < animatedGlowRenderers.Length; index++)
        {
            animatedGlowRenderers[index].SetPropertyBlock(glowPropertyBlock);
        }
    }

    [ContextMenu("Rebuild Drone Visual")]
    public void Rebuild()
    {
        CleanupPreviousVisual();
        EnsureVisualBuilt();
    }

    private void EnsureVisualBuilt()
    {
        visualRoot = transform.Find(RootName);
        if (visualRoot != null && visualRoot.childCount > 0)
        {
            CacheGlowRenderersFromExistingVisual();
            return;
        }

        if (visualRoot == null)
        {
            visualRoot = CreateChild(RootName, Vector3.zero, Vector3.zero, Vector3.one).transform;
            visualRoot.SetParent(transform, false);
        }

        BuildBody(visualRoot);
        BuildPods(visualRoot);
        BuildCanopy(visualRoot);
        BuildDetailing(visualRoot);
        BuildTrail(visualRoot);
    }

    private void CacheGlowRenderersFromExistingVisual()
    {
        Transform leftEngine = visualRoot.Find("LeftPod/Engine");
        Transform rightEngine = visualRoot.Find("RightPod/Engine");

        if (leftEngine == null || rightEngine == null)
        {
            animatedGlowRenderers = null;
            return;
        }

        Renderer leftRenderer = leftEngine.GetComponent<Renderer>();
        Renderer rightRenderer = rightEngine.GetComponent<Renderer>();
        if (leftRenderer == null || rightRenderer == null)
        {
            animatedGlowRenderers = null;
            return;
        }

        animatedGlowRenderers = new[] { leftRenderer, rightRenderer };
    }

    private void BuildBody(Transform parent)
    {
        GameObject hull = CreatePrimitive("Hull", PrimitiveType.Cube, parent, new Vector3(0f, 0f, 0f), Vector3.zero, new Vector3(bodyWidth, bodyHeight, bodyLength));
        ApplyMaterial(hull.GetComponent<Renderer>(), hullColor, 0f);

        GameObject keel = CreatePrimitive("Keel", PrimitiveType.Cube, parent, new Vector3(0f, -bodyHeight * 0.35f, bodyLength * 0.1f), Vector3.zero, new Vector3(bodyWidth * 0.45f, bodyHeight * 0.35f, bodyLength * 1.15f));
        ApplyMaterial(keel.GetComponent<Renderer>(), accentColor, 0.5f);
    }

    private void BuildPods(Transform parent)
    {
        Renderer left = BuildPod("LeftPod", parent, -podOffset);
        Renderer right = BuildPod("RightPod", parent, podOffset);
        animatedGlowRenderers = new[] { left, right };
    }

    private Renderer BuildPod(string podName, Transform parent, float sideOffset)
    {
        GameObject podRoot = CreateChild(podName, new Vector3(sideOffset, 0f, 0.1f), Vector3.zero, Vector3.one);
        podRoot.transform.SetParent(parent, false);

        GameObject podShell = CreatePrimitive("Shell", PrimitiveType.Cylinder, podRoot.transform, Vector3.zero, new Vector3(90f, 0f, 0f), new Vector3(bodyHeight * 0.75f, bodyLength * 0.35f, bodyHeight * 0.75f));
        ApplyMaterial(podShell.GetComponent<Renderer>(), hullColor, 0f);

        GameObject podEngine = CreatePrimitive("Engine", PrimitiveType.Cylinder, podRoot.transform, new Vector3(0f, 0f, -bodyLength * 0.28f), new Vector3(90f, 0f, 0f), new Vector3(bodyHeight * 0.46f, bodyLength * 0.2f, bodyHeight * 0.46f));
        Renderer engineRenderer = podEngine.GetComponent<Renderer>();
        ApplyMaterial(engineRenderer, engineGlowColor, maxEmission);
        return engineRenderer;
    }


    private void BuildDetailing(Transform parent)
    {
        BuildWing("LeftWing", parent, -1f);
        BuildWing("RightWing", parent, 1f);

        GameObject spine = CreatePrimitive("Spine", PrimitiveType.Cube, parent, new Vector3(0f, bodyHeight * 0.28f, -bodyLength * 0.05f), Vector3.zero, new Vector3(bodyWidth * 0.18f, bodyHeight * 0.28f, bodyLength * 0.95f));
        ApplyMaterial(spine.GetComponent<Renderer>(), accentColor, 0.45f);

        GameObject noseEmitter = CreatePrimitive("NoseEmitter", PrimitiveType.Sphere, parent, new Vector3(0f, bodyHeight * 0.02f, bodyLength * 0.56f), Vector3.zero, new Vector3(bodyWidth * 0.2f, bodyHeight * 0.2f, bodyWidth * 0.2f));
        ApplyMaterial(noseEmitter.GetComponent<Renderer>(), accentColor, 1.5f);

        GameObject stabilizer = CreatePrimitive("RearStabilizer", PrimitiveType.Cube, parent, new Vector3(0f, bodyHeight * 0.36f, -bodyLength * 0.46f), Vector3.zero, new Vector3(bodyWidth * 0.3f, bodyHeight * 0.2f, bodyLength * 0.25f));
        ApplyMaterial(stabilizer.GetComponent<Renderer>(), hullColor, 0f);
    }

    private void BuildWing(string wingName, Transform parent, float sideSign)
    {
        GameObject wing = CreateChild(wingName, new Vector3(sideSign * (podOffset * 0.68f), -bodyHeight * 0.08f, bodyLength * 0.04f), new Vector3(0f, 0f, sideSign * 10f), Vector3.one);
        wing.transform.SetParent(parent, false);

        GameObject strut = CreatePrimitive("Strut", PrimitiveType.Cube, wing.transform, new Vector3(0f, 0f, 0f), Vector3.zero, new Vector3(bodyWidth * 0.08f, bodyHeight * 0.2f, bodyLength * 1.05f));
        ApplyMaterial(strut.GetComponent<Renderer>(), hullColor, 0f);

        GameObject fin = CreatePrimitive("Fin", PrimitiveType.Cube, wing.transform, new Vector3(sideSign * bodyWidth * 0.08f, bodyHeight * 0.08f, bodyLength * 0.16f), new Vector3(20f, 0f, sideSign * 18f), new Vector3(bodyWidth * 0.12f, bodyHeight * 0.14f, bodyLength * 0.4f));
        ApplyMaterial(fin.GetComponent<Renderer>(), accentColor, 0.6f);
    }
    private void BuildCanopy(Transform parent)
    {
        GameObject canopy = CreatePrimitive("Canopy", PrimitiveType.Sphere, parent, new Vector3(0f, bodyHeight * 0.28f, bodyLength * 0.18f), Vector3.zero, new Vector3(bodyWidth * 0.65f, bodyHeight * 0.65f, bodyLength * 0.25f));
        ApplyMaterial(canopy.GetComponent<Renderer>(), accentColor, 0.65f);
    }

    private void BuildTrail(Transform parent)
    {
        GameObject trailAnchor = CreateChild("TrailAnchor", new Vector3(0f, -bodyHeight * 0.2f, -bodyLength * 0.65f), Vector3.zero, Vector3.one);
        trailAnchor.transform.SetParent(parent, false);

        TrailRenderer trailRenderer = trailAnchor.GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = trailAnchor.AddComponent<TrailRenderer>();
        }

        trailRenderer.time = trailLifetime;
        trailRenderer.minVertexDistance = 0.08f;
        trailRenderer.widthMultiplier = trailWidth;
        trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trailRenderer.receiveShadows = false;
        trailRenderer.alignment = LineAlignment.View;
        trailRenderer.numCapVertices = 2;
        trailRenderer.textureMode = LineTextureMode.Stretch;

        Gradient gradient = new();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(accentColor, 0f),
                new GradientColorKey(engineGlowColor, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.95f, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        trailRenderer.colorGradient = gradient;

        AnimationCurve widthCurve = new();
        widthCurve.AddKey(0f, 1f);
        widthCurve.AddKey(1f, 0f);
        trailRenderer.widthCurve = widthCurve;

        Material trailMaterial = new(Shader.Find("Universal Render Pipeline/Unlit"));
        trailMaterial.color = accentColor;
        trailMaterial.EnableKeyword("_EMISSION");
        trailMaterial.SetColor("_EmissionColor", accentColor * 2.2f);
        trailRenderer.material = trailMaterial;
    }

    private static GameObject CreatePrimitive(string objectName, PrimitiveType primitiveType, Transform parent, Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale)
    {
        GameObject primitive = GameObject.CreatePrimitive(primitiveType);
        primitive.name = objectName;
        primitive.transform.SetParent(parent, false);
        primitive.transform.localPosition = localPosition;
        primitive.transform.localEulerAngles = localEulerAngles;
        primitive.transform.localScale = localScale;

        Collider primitiveCollider = primitive.GetComponent<Collider>();
        if (primitiveCollider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(primitiveCollider);
            }
            else
            {
                DestroyImmediate(primitiveCollider);
            }
        }

        return primitive;
    }

    private static GameObject CreateChild(string objectName, Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale)
    {
        GameObject child = new(objectName);
        child.transform.localPosition = localPosition;
        child.transform.localEulerAngles = localEulerAngles;
        child.transform.localScale = localScale;
        return child;
    }

    private static void ApplyMaterial(Renderer renderer, Color baseColor, float emissionIntensity)
    {
        Material material = new(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = baseColor;

        if (emissionIntensity > 0f)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", baseColor * emissionIntensity);
        }

        renderer.sharedMaterial = material;
    }

    private void CleanupPreviousVisual()
    {
        Transform oldRoot = transform.Find(RootName);
        if (oldRoot != null)
        {
            DestroyImmediate(oldRoot.gameObject);
        }

        animatedGlowRenderers = null;
        visualRoot = null;
    }
}
