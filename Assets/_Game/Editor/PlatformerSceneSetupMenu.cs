using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class PlatformerSceneSetupMenu
{
    private const float DefaultRestartDelay = 0.35f;
    private const float DefaultCameraOrthographicSize = 5f;
    private const float DefaultPlayerGravityScale = 4f;
    private const float DefaultSawColliderRadius = 0.45f;
    private const int BackgroundSortingOrder = -2;
    private const int GroundSortingOrder = 0;
    private const int DecorationSortingOrder = 2;
    private const int PlatformSortingOrder = 3;
    private const int SawSortingOrder = 4;
    private const int PlayerSortingOrder = 5;

    private static readonly Vector3 PlayerSpawnPosition = Vector3.zero;
    private static readonly Vector3 GroundCheckLocalPosition = new Vector3(0f, -0.55f, 0f);
    private static readonly Vector3 WallCheckLocalPosition = new Vector3(0.45f, 0f, 0f);
    private static readonly Vector2 PlayerColliderSize = new Vector2(0.7f, 1.1f);
    private static readonly Vector3 DeathZonePosition = new Vector3(0f, -6f, 0f);
    private static readonly Vector2 DeathZoneSize = new Vector2(40f, 4f);
    private static readonly Vector3 SawPointAPosition = new Vector3(4f, 1f, 0f);
    private static readonly Vector3 SawPointBPosition = new Vector3(8f, 2f, 0f);
    private static readonly Vector3 FallingPlatformPosition = new Vector3(2f, 3f, 0f);
    private static readonly Vector2 FallingPlatformSize = new Vector2(2f, 0.4f);

    [MenuItem("Tools/Create Platformer Scene Setup")]
    private static void CreatePlatformerSceneSetup()
    {
        GameObject levelRoot = GetOrCreateRootObject("LevelRoot", out _);

        Transform systemsRoot = GetOrCreateChild(levelRoot.transform, "Systems", out _);
        GameObject restarterObject = GetOrCreateChildObject(systemsRoot, "LevelRestarter", out _);
        LevelRestarter levelRestarter = GetOrCreateComponent<LevelRestarter>(restarterObject, out bool createdLevelRestarter);

        if (createdLevelRestarter)
        {
            SetFloatField(levelRestarter, "_restartDelay", DefaultRestartDelay);
        }

        GameObject gridObject = CreateGrid(levelRoot.transform);
        CreateTilemapLayer(gridObject.transform, "BackgroundTilemap", BackgroundSortingOrder, false);
        CreateTilemapLayer(gridObject.transform, "GroundTilemap", GroundSortingOrder, true);
        CreateTilemapLayer(gridObject.transform, "DecorationTilemap", DecorationSortingOrder, false);

        Transform dynamicRoot = GetOrCreateChild(levelRoot.transform, "Dynamic", out _);
        Transform enemiesRoot = GetOrCreateChild(dynamicRoot, "Enemies", out _);
        Transform platformsRoot = GetOrCreateChild(dynamicRoot, "Platforms", out _);
        Transform zonesRoot = GetOrCreateChild(levelRoot.transform, "Zones", out _);
        Transform pointsRoot = GetOrCreateChild(levelRoot.transform, "Points", out _);

        Transform playerSpawn = GetOrCreateChild(pointsRoot, "PlayerSpawn", out bool createdPlayerSpawn);

        if (createdPlayerSpawn)
        {
            playerSpawn.position = PlayerSpawnPosition;
        }

        GameObject player = CreatePlayer(levelRoot.transform, levelRestarter, out bool createdPlayer);

        if (createdPlayer || createdPlayerSpawn)
        {
            player.transform.position = playerSpawn.position;
        }

        CreateDeathZone(zonesRoot);
        CreateExampleSaw(enemiesRoot);
        CreateExampleFallingPlatform(platformsRoot);
        SetupCamera();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = levelRoot;
    }

    private static GameObject CreateGrid(Transform parent)
    {
        GameObject gridObject = GetOrCreateChildObject(parent, "Grid", out bool createdGrid);

        if (createdGrid)
        {
            gridObject.transform.localPosition = Vector3.zero;
        }

        GetOrCreateComponent<Grid>(gridObject, out _);
        return gridObject;
    }

    private static void CreateTilemapLayer(Transform parent, string name, int sortingOrder, bool withCollision)
    {
        GameObject tilemapObject = GetOrCreateChildObject(parent, name, out bool createdTilemapObject);
        GetOrCreateComponent<Tilemap>(tilemapObject, out _);
        TilemapRenderer tilemapRenderer = GetOrCreateComponent<TilemapRenderer>(tilemapObject, out bool createdRenderer);

        if (createdTilemapObject)
        {
            tilemapObject.transform.localPosition = Vector3.zero;
        }

        if (createdRenderer)
        {
            tilemapRenderer.sortingOrder = sortingOrder;
        }

        if (withCollision == false)
        {
            return;
        }

        TilemapCollider2D tilemapCollider = GetOrCreateComponent<TilemapCollider2D>(tilemapObject, out bool createdTilemapCollider);
        CompositeCollider2D compositeCollider = GetOrCreateComponent<CompositeCollider2D>(tilemapObject, out bool createdCompositeCollider);
        Rigidbody2D rigidbody2D = GetOrCreateComponent<Rigidbody2D>(tilemapObject, out bool createdRigidbody2D);

        if (createdTilemapCollider)
        {
            tilemapCollider.usedByComposite = true;
        }

        if (createdCompositeCollider)
        {
            compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
        }

        if (createdRigidbody2D)
        {
            rigidbody2D.bodyType = RigidbodyType2D.Static;
            rigidbody2D.simulated = true;
        }
    }

    private static GameObject CreatePlayer(Transform parent, LevelRestarter levelRestarter, out bool createdPlayer)
    {
        GameObject player = GetOrCreateChildObject(parent, "Player", out createdPlayer);
        GameObject visual = GetOrCreateChildObject(player.transform, "Visual", out bool createdVisual);
        Transform groundCheck = GetOrCreateChild(player.transform, "GroundCheck", out bool createdGroundCheck);
        Transform wallCheck = GetOrCreateChild(player.transform, "WallCheck", out bool createdWallCheck);

        Rigidbody2D rigidbody2D = GetOrCreateComponent<Rigidbody2D>(player, out bool createdRigidbody2D);
        CapsuleCollider2D capsuleCollider2D = GetOrCreateComponent<CapsuleCollider2D>(player, out bool createdCapsuleCollider2D);
        SpriteRenderer spriteRenderer = GetOrCreateComponent<SpriteRenderer>(visual, out bool createdSpriteRenderer);
        Animator animator = GetOrCreateComponent<Animator>(visual, out _);
        PlayerSurfaceDetector surfaceDetector = GetOrCreateComponent<PlayerSurfaceDetector>(player, out _);
        PlayerMovement playerMovement = GetOrCreateComponent<PlayerMovement>(player, out _);
        PlayerAnimationSync playerAnimationSync = GetOrCreateComponent<PlayerAnimationSync>(player, out _);
        PlayerDeathHandler playerDeathHandler = GetOrCreateComponent<PlayerDeathHandler>(player, out _);

        if (createdPlayer)
        {
            player.transform.position = PlayerSpawnPosition;
        }

        if (createdVisual)
        {
            visual.transform.localPosition = Vector3.zero;
        }

        if (createdGroundCheck)
        {
            groundCheck.localPosition = GroundCheckLocalPosition;
        }

        if (createdWallCheck)
        {
            wallCheck.localPosition = WallCheckLocalPosition;
        }

        if (createdRigidbody2D)
        {
            rigidbody2D.gravityScale = DefaultPlayerGravityScale;
            rigidbody2D.freezeRotation = true;
            rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
            rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (createdCapsuleCollider2D)
        {
            capsuleCollider2D.size = PlayerColliderSize;
            capsuleCollider2D.direction = CapsuleDirection2D.Vertical;
        }

        if (createdSpriteRenderer)
        {
            spriteRenderer.sortingOrder = PlayerSortingOrder;
        }

        SetObjectField(surfaceDetector, "_groundCheck", groundCheck);
        SetObjectField(surfaceDetector, "_wallCheck", wallCheck);
        SetObjectField(playerMovement, "_rigidbody2D", rigidbody2D);
        SetObjectField(playerMovement, "_surfaceDetector", surfaceDetector);
        SetObjectField(playerMovement, "_visualRoot", visual.transform);
        SetObjectField(playerAnimationSync, "_movement", playerMovement);
        SetObjectField(playerAnimationSync, "_animator", animator);
        SetObjectField(playerDeathHandler, "_movement", playerMovement);
        SetObjectField(playerDeathHandler, "_animationSync", playerAnimationSync);
        SetObjectField(playerDeathHandler, "_rigidbody2D", rigidbody2D);
        SetObjectField(playerDeathHandler, "_collider2D", capsuleCollider2D);
        SetObjectField(playerDeathHandler, "_levelRestarter", levelRestarter);

        return player;
    }

    private static void CreateDeathZone(Transform parent)
    {
        GameObject deathZoneObject = GetOrCreateChildObject(parent, "DeathZone", out bool createdDeathZone);
        BoxCollider2D boxCollider2D = GetOrCreateComponent<BoxCollider2D>(deathZoneObject, out bool createdCollider);
        DeathZone deathZone = GetOrCreateComponent<DeathZone>(deathZoneObject, out _);

        if (createdDeathZone)
        {
            deathZoneObject.transform.position = DeathZonePosition;
        }

        if (createdCollider)
        {
            boxCollider2D.size = DeathZoneSize;
            boxCollider2D.isTrigger = true;
        }

        SetObjectField(deathZone, "_triggerCollider", boxCollider2D);
    }

    private static void CreateExampleSaw(Transform parent)
    {
        Transform patrolPointsRoot = GetOrCreateChild(parent, "PatrolPoints", out _);
        GameObject sawObject = GetOrCreateChildObject(parent, "SawEnemy_01", out bool createdSaw);
        Transform pointA = GetOrCreateChild(patrolPointsRoot, "SawEnemy_01_PointA", out bool createdPointA);
        Transform pointB = GetOrCreateChild(patrolPointsRoot, "SawEnemy_01_PointB", out bool createdPointB);

        Rigidbody2D rigidbody2D = GetOrCreateComponent<Rigidbody2D>(sawObject, out bool createdRigidbody2D);
        CircleCollider2D circleCollider2D = GetOrCreateComponent<CircleCollider2D>(sawObject, out bool createdCircleCollider2D);
        SpriteRenderer spriteRenderer = GetOrCreateComponent<SpriteRenderer>(sawObject, out bool createdSpriteRenderer);
        HazardPatrol hazardPatrol = GetOrCreateComponent<HazardPatrol>(sawObject, out _);
        GetOrCreateComponent<HazardKillOnTouch>(sawObject, out _);

        if (createdSaw)
        {
            sawObject.transform.position = SawPointAPosition;
        }

        if (createdPointA)
        {
            pointA.position = SawPointAPosition;
        }

        if (createdPointB)
        {
            pointB.position = SawPointBPosition;
        }

        if (createdRigidbody2D)
        {
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            rigidbody2D.gravityScale = 0f;
            rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        if (createdCircleCollider2D)
        {
            circleCollider2D.radius = DefaultSawColliderRadius;
        }

        if (createdSpriteRenderer)
        {
            spriteRenderer.sortingOrder = SawSortingOrder;
        }

        SetObjectField(hazardPatrol, "_rigidbody2D", rigidbody2D);
        SetObjectField(hazardPatrol, "_pointA", pointA);
        SetObjectField(hazardPatrol, "_pointB", pointB);
        SetObjectField(hazardPatrol, "_visualRoot", sawObject.transform);
    }

    private static void CreateExampleFallingPlatform(Transform parent)
    {
        GameObject platformObject = GetOrCreateChildObject(parent, "FallingPlatform_01", out bool createdPlatform);
        Rigidbody2D rigidbody2D = GetOrCreateComponent<Rigidbody2D>(platformObject, out bool createdRigidbody2D);
        BoxCollider2D boxCollider2D = GetOrCreateComponent<BoxCollider2D>(platformObject, out bool createdBoxCollider2D);
        SpriteRenderer spriteRenderer = GetOrCreateComponent<SpriteRenderer>(platformObject, out bool createdSpriteRenderer);
        FallingPlatform fallingPlatform = GetOrCreateComponent<FallingPlatform>(platformObject, out _);

        if (createdPlatform)
        {
            platformObject.transform.position = FallingPlatformPosition;
        }

        if (createdRigidbody2D)
        {
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            rigidbody2D.gravityScale = 0f;
            rigidbody2D.freezeRotation = true;
            rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        if (createdBoxCollider2D)
        {
            boxCollider2D.size = FallingPlatformSize;
        }

        if (createdSpriteRenderer)
        {
            spriteRenderer.sortingOrder = PlatformSortingOrder;
        }

        SetObjectField(fallingPlatform, "_rigidbody2D", rigidbody2D);
        SetObjectField(fallingPlatform, "_platformCollider", boxCollider2D);
    }

    private static void SetupCamera()
    {
        Camera cameraComponent = Object.FindFirstObjectByType<Camera>();
        GameObject cameraObject;

        if (cameraComponent == null)
        {
            cameraObject = new GameObject("Main Camera");
            cameraComponent = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
        }
        else
        {
            cameraObject = cameraComponent.gameObject;
        }

        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = DefaultCameraOrthographicSize;

        if (cameraObject.transform.position.z == 0f)
        {
            cameraObject.transform.position = new Vector3(cameraObject.transform.position.x, cameraObject.transform.position.y, -10f);
        }
    }

    private static GameObject GetOrCreateRootObject(string name, out bool created)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            if (rootObject.name == name)
            {
                created = false;
                return rootObject;
            }
        }

        created = true;
        return new GameObject(name);
    }

    private static Transform GetOrCreateChild(Transform parent, string name, out bool created)
    {
        GameObject childObject = GetOrCreateChildObject(parent, name, out created);
        return childObject.transform;
    }

    private static GameObject GetOrCreateChildObject(Transform parent, string name, out bool created)
    {
        Transform existingChild = FindDirectChild(parent, name);

        if (existingChild != null)
        {
            created = false;
            return existingChild.gameObject;
        }

        created = true;

        GameObject childObject = new GameObject(name);
        childObject.transform.SetParent(parent);
        childObject.transform.localPosition = Vector3.zero;
        childObject.transform.localRotation = Quaternion.identity;
        childObject.transform.localScale = Vector3.one;
        return childObject;
    }

    private static Transform FindDirectChild(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == name)
            {
                return child;
            }
        }

        return null;
    }

    private static T GetOrCreateComponent<T>(GameObject targetObject, out bool created) where T : Component
    {
        T component = targetObject.GetComponent<T>();

        if (component != null)
        {
            created = false;
            return component;
        }

        created = true;
        return targetObject.AddComponent<T>();
    }

    private static void SetObjectField(Object targetObject, string fieldName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(targetObject);
        SerializedProperty property = serializedObject.FindProperty(fieldName);

        if (property == null)
        {
            return;
        }

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloatField(Object targetObject, string fieldName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(targetObject);
        SerializedProperty property = serializedObject.FindProperty(fieldName);

        if (property == null)
        {
            return;
        }

        property.floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}
