using UnityEngine;

/// <summary>
/// RuntimeTunnelSegmentFactory
/// Purpose: Build a stylized runtime tunnel segment for the bootstrap scene.
/// Responsibilities:
/// - Create a tunnel that is 2x wider/taller for more flight space.
/// - Add industrial visual detail while keeping gameplay colliders readable.
/// - Configure lighting/fog mood elements for a hostile megastructure escape.
/// </summary>
public static class RuntimeTunnelSegmentFactory
{
    public static GameObject CreateSegmentPrefab()
    {
        const float segmentLength = 25f;
        const float wallDistance = 10f;
        const float wallThickness = 0.5f;
        const float tunnelHeight = 20f;
        const float tunnelWidth = 20f;

        GameObject segment = new("RuntimeSegmentPrefab");
        Material wallMaterial = CreateWallMaterial();
        Material ribMaterial = CreateRibMaterial();
        Material pipeMaterial = CreatePipeMaterial();
        Material warningMaterial = CreateWarningMaterial();
        Material neonMaterial = CreateNeonMaterial();

        BuildWall("LeftWall", segment.transform, new Vector3(-wallDistance, 0f, 0f), new Vector3(wallThickness, tunnelHeight, segmentLength), wallMaterial);
        BuildWall("RightWall", segment.transform, new Vector3(wallDistance, 0f, 0f), new Vector3(wallThickness, tunnelHeight, segmentLength), wallMaterial);
        BuildWall("TopWall", segment.transform, new Vector3(0f, wallDistance, 0f), new Vector3(tunnelWidth, wallThickness, segmentLength), wallMaterial);
        BuildWall("BottomWall", segment.transform, new Vector3(0f, -wallDistance, 0f), new Vector3(tunnelWidth, wallThickness, segmentLength), wallMaterial);

        BuildStructuralRibs(segment.transform, ribMaterial, warningMaterial);
        BuildExposedPipes(segment.transform, pipeMaterial);
        BuildRotatingMachinery(segment.transform, ribMaterial);
        BuildNeonStrips(segment.transform, neonMaterial);
        BuildAtmosphereLighting(segment.transform);

        segment.SetActive(false);
        return segment;
    }

    private static void BuildWall(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material wallMaterial)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.localPosition = localPosition;
        wall.transform.localScale = localScale;
        ApplyMaterial(wall, wallMaterial);

        Collider collider = wall.GetComponent<Collider>();
        if (collider is BoxCollider boxCollider)
        {
            boxCollider.isTrigger = false;
        }
    }

    private static void BuildStructuralRibs(Transform parent, Material ribMaterial, Material warningMaterial)
    {
        const int ribCount = 6;
        const float zStart = -10f;
        const float zStep = 5f;
        const float frameThickness = 0.35f;
        const float tunnelHalfSize = 10f;
        const float frameSpan = 20f;

        for (int i = 0; i < ribCount; i++)
        {
            float z = zStart + (i * zStep);
            Transform rib = new GameObject($"StructuralRib_{i:00}").transform;
            rib.SetParent(parent, false);
            rib.localPosition = new Vector3(0f, 0f, z);

            BuildDecorCube("RibLeft", rib, new Vector3(-tunnelHalfSize, 0f, 0f), new Vector3(frameThickness, frameSpan, frameThickness), ribMaterial);
            BuildDecorCube("RibRight", rib, new Vector3(tunnelHalfSize, 0f, 0f), new Vector3(frameThickness, frameSpan, frameThickness), ribMaterial);
            BuildDecorCube("RibTop", rib, new Vector3(0f, tunnelHalfSize, 0f), new Vector3(frameSpan, frameThickness, frameThickness), ribMaterial);
            BuildDecorCube("RibBottom", rib, new Vector3(0f, -tunnelHalfSize, 0f), new Vector3(frameSpan, frameThickness, frameThickness), ribMaterial);

            if (i % 2 == 0)
            {
                BuildDecorCube("HazardBand", rib, new Vector3(0f, -9.2f, 0f), new Vector3(7f, 0.2f, frameThickness), warningMaterial);
            }
        }
    }

    private static void BuildExposedPipes(Transform parent, Material pipeMaterial)
    {
        BuildPipe("PipeTopLeft", parent, new Vector3(-8.25f, 8.25f, 0f), new Vector3(0f, 0f, 90f), pipeMaterial);
        BuildPipe("PipeTopRight", parent, new Vector3(8.25f, 8.25f, 0f), new Vector3(0f, 0f, 90f), pipeMaterial);
        BuildPipe("PipeBottomLeft", parent, new Vector3(-8.25f, -8.25f, 0f), new Vector3(0f, 0f, 90f), pipeMaterial);
        BuildPipe("PipeBottomRight", parent, new Vector3(8.25f, -8.25f, 0f), new Vector3(0f, 0f, 90f), pipeMaterial);
    }

    private static void BuildPipe(string name, Transform parent, Vector3 localPosition, Vector3 eulerAngles, Material material)
    {
        GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pipe.name = name;
        pipe.transform.SetParent(parent, false);
        pipe.transform.localPosition = localPosition;
        pipe.transform.localEulerAngles = eulerAngles;
        pipe.transform.localScale = new Vector3(0.3f, 12.5f, 0.3f);
        ApplyMaterial(pipe, material);
        DisableCollider(pipe);
    }

    private static void BuildRotatingMachinery(Transform parent, Material material)
    {
        float[] fanZPositions = { -8.5f, -1.5f, 6.5f };
        for (int i = 0; i < fanZPositions.Length; i++)
        {
            GameObject hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = $"Rotor_{i:00}";
            hub.transform.SetParent(parent, false);
            hub.transform.localPosition = new Vector3(0f, 0f, fanZPositions[i]);
            hub.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
            hub.transform.localScale = new Vector3(0.8f, 0.18f, 0.8f);
            ApplyMaterial(hub, material);
            DisableCollider(hub);

            RuntimeMechanicalRotator rotator = hub.AddComponent<RuntimeMechanicalRotator>();
            rotator.SetRotationAxisAndSpeed(Vector3.forward, i % 2 == 0 ? 210f : -195f);

            BuildDecorCube("BladeA", hub.transform, Vector3.zero, new Vector3(0.15f, 5.5f, 0.45f), material);
            BuildDecorCube("BladeB", hub.transform, Vector3.zero, new Vector3(5.5f, 0.15f, 0.45f), material);
        }
    }

    private static void BuildNeonStrips(Transform parent, Material neonMaterial)
    {
        BuildDecorCube("NeonStripLeft", parent, new Vector3(-9.4f, 0f, 0f), new Vector3(0.08f, 3.5f, 23f), neonMaterial);
        BuildDecorCube("NeonStripRight", parent, new Vector3(9.4f, 0f, 0f), new Vector3(0.08f, 3.5f, 23f), neonMaterial);
    }

    private static void BuildAtmosphereLighting(Transform parent)
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.06f, 0.02f, 0.02f, 1f);
        RenderSettings.fogDensity = 0.028f;

        GameObject strobe = new("EmergencyStrobe");
        strobe.transform.SetParent(parent, false);
        strobe.transform.localPosition = new Vector3(0f, 8f, -5f);
        Light strobeLight = strobe.AddComponent<Light>();
        strobeLight.type = LightType.Point;
        strobeLight.color = new Color(1f, 0.12f, 0.09f);
        strobeLight.intensity = 0.1f;
        strobeLight.range = 28f;
        RuntimeEmergencyStrobe emergencyStrobe = strobe.AddComponent<RuntimeEmergencyStrobe>();
        emergencyStrobe.Configure(0.06f, 0.55f, 9.5f);

        GameObject shaftLight = new("VentShaftLight");
        shaftLight.transform.SetParent(parent, false);
        shaftLight.transform.localPosition = new Vector3(0f, 9f, 1f);
        shaftLight.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        Light ventLight = shaftLight.AddComponent<Light>();
        ventLight.type = LightType.Spot;
        ventLight.color = new Color(0.48f, 0.07f, 0.06f);
        ventLight.spotAngle = 42f;
        ventLight.innerSpotAngle = 18f;
        ventLight.intensity = 2.5f;
        ventLight.range = 22f;

        BuildSparkEmitter(parent, new Vector3(-9.2f, 9f, -7f));
        BuildSparkEmitter(parent, new Vector3(9.2f, -9f, 4f));
    }

    private static void BuildSparkEmitter(Transform parent, Vector3 localPosition)
    {
        GameObject sparks = new("Sparks");
        sparks.transform.SetParent(parent, false);
        sparks.transform.localPosition = localPosition;

        ParticleSystem particleSystem = sparks.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particleSystem.main;
        main.startLifetime = 0.18f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.8f, 7.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.065f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.75f, 0.2f), new Color(1f, 0.25f, 0.05f));
        main.maxParticles = 16;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 2, 4, 1, 0.42f) });

        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 14f;
        shape.radius = 0.02f;

        ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private static void BuildDecorCube(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localScale = localScale;
        ApplyMaterial(cube, material);
        DisableCollider(cube);
    }

    private static Material CreateWallMaterial()
    {
        Material material = new(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = new Color(0.1f, 0.1f, 0.11f);
        material.SetFloat("_Metallic", 0.92f);
        material.SetFloat("_Smoothness", 0.15f);
        return material;
    }

    private static Material CreateRibMaterial()
    {
        Material material = new(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = new Color(0.18f, 0.16f, 0.15f);
        material.SetFloat("_Metallic", 0.8f);
        material.SetFloat("_Smoothness", 0.22f);
        return material;
    }

    private static Material CreatePipeMaterial()
    {
        Material material = new(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = new Color(0.27f, 0.2f, 0.16f);
        material.SetFloat("_Metallic", 0.78f);
        material.SetFloat("_Smoothness", 0.2f);
        return material;
    }

    private static Material CreateWarningMaterial()
    {
        Material material = new(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = new Color(0.8f, 0.63f, 0.11f);
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", new Color(0.22f, 0.16f, 0.01f));
        return material;
    }

    private static Material CreateNeonMaterial()
    {
        Material material = new(Shader.Find("Universal Render Pipeline/Lit"));
        Color neonColor = new(0.76f, 0.04f, 0.07f);
        material.color = neonColor;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", neonColor * 2f);
        return material;
    }

    private static void ApplyMaterial(GameObject target, Material material)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void DisableCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }
}
